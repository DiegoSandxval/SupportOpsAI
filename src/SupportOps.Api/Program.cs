#pragma warning disable OPENAI001
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SupportOps.Api.Authorization;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Abstractions.Security;
using SupportOps.Application.Auth.Login;
using SupportOps.Application.Auth.Register;
using SupportOps.Application.Common.Exceptions;
using SupportOps.Application.Tickets.Comments;
using SupportOps.Application.Tickets.Create;
using SupportOps.Application.Tickets.GetAll;
using SupportOps.Application.Tickets.GetHistory;
using SupportOps.Application.Tickets.GetTicketById;
using SupportOps.Application.Tickets.Update;
using SupportOps.Application.Users.Create;
using SupportOps.Domain.Entities;
using SupportOps.Domain.Enums;
using SupportOps.Infrastructure;
using System.Security.Claims;
using System.Text.Json.Serialization;
using SupportOps.Application.Tickets.Analyze;
using OpenAI.Responses;
using SupportOps.Application.Abstractions.AI;
using SupportOps.Infrastructure.AI;
using SupportOps.Application.Users.GetAgents;

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured.");
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddOpenApi();
builder.Services.AddScoped<GetTicketsHandler>();
builder.Services.AddScoped<GetTicketByIdHandler>();
builder.Services.AddScoped<UpdateTicketHandler>();
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetTicketHistoryHandler>();
builder.Services.AddScoped<GetAgentsHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "SupportOpsFront",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });

    options.AddPolicy(
        AuthorizationPolicies.SupportStaff,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Agent", "Admin");
        });
});
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddScoped<CreateTicketHandler>();
builder.Services.AddScoped<GetTicketsHandler>();
builder.Services.AddScoped<AddTicketCommentHandler>();
builder.Services.AddScoped<GetTicketCommentsHandler>();



var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var userRepository =
        scope.ServiceProvider.GetRequiredService<IUserRepository>();

    var passwordHasher =
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    var unitOfWork =
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

    const string adminEmail = "admin@supportops.com";
    const string adminPassword = "Admin123!";

    var adminExists =
        await userRepository.ExistsByEmailAsync(adminEmail);

    if (!adminExists)
    {
        var passwordHash =
            passwordHasher.Hash(adminPassword);

        var admin = new User(
            "SupportOps",
            "Admin",
            adminEmail,
            passwordHash,
            UserRole.Admin
        );

        await userRepository.AddAsync(admin);

        await unitOfWork.SaveChangesAsync();
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("SupportOpsFront");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGet(
    "/api/auth/support-area",
    () =>
    {
        return Results.Ok(new
        {
            message = "You have support staff access."
        });
    })
    .RequireAuthorization(
        AuthorizationPolicies.SupportStaff
    )
    .WithTags("Auth");
app.MapGet(
    "/api/auth/admin-area",
    () =>
    {
        return Results.Ok(new
        {
            message = "You have administrator access."
        });
    })
    .RequireAuthorization(
        AuthorizationPolicies.AdminOnly
    )
    .WithTags("Auth");
app.MapPost(
    "/api/tickets",
    async (
        CreateTicketRequest request,
        CreateTicketHandler handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        try
        {
            var response =
                await handler.HandleAsync(
                    userId,
                    request,
                    cancellationToken
                );

            return Results.Created(
                $"/api/tickets/{response.Id}",
                response
            );
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new
            {
                message = exception.Message
            });
        }
    })
    .RequireAuthorization()
    .WithName("CreateTicket")
    .WithTags("Tickets");

app.MapGet(
    "/api/tickets",
    async (
        ClaimsPrincipal user,
        GetTicketsHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var roleValue =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        var tickets =
            await handler.HandleAsync(
                userId,
                role,
                cancellationToken
            );

        return Results.Ok(tickets);
    })
    .RequireAuthorization()
    .WithName("GetTickets")
    .WithTags("Tickets");

app.MapPatch(
    "/api/tickets/{id:guid}",
    async (
        Guid id,
        UpdateTicketRequest request,
        ClaimsPrincipal user,
        UpdateTicketHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue = user.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        var roleValue = user.FindFirstValue(
            ClaimTypes.Role
        );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        var command = new UpdateTicketCommand(
            id,
            request.Priority,
            request.Category,
            request.AssignedAgentId,
            request.Status
        );

        try
        {
            var updated = await handler.HandleAsync(
                command,
                userId,
                role,
                cancellationToken
            );

            if (!updated)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    })
    .RequireAuthorization()
    .WithName("UpdateTicket")
    .WithTags("Tickets");
app.MapGet(
    "/api/tickets/{id:guid}",
    async (
        Guid id,
        ClaimsPrincipal user,
        GetTicketByIdHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var roleValue =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        var query =
            new GetTicketByIdQuery(id);

        var ticket =
            await handler.HandleAsync(
                query,
                userId,
                role,
                cancellationToken
            );

        if (ticket is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ticket);
    })
    .RequireAuthorization()
    .WithName("GetTicketById")
    .WithTags("Tickets");

app.MapPost(
    "/api/users",
    async (
        CreateUserRequest request,
        CreateUserHandler handler,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var response =
                await handler.HandleAsync(
                    request,
                    cancellationToken
                );

            return Results.Created(
                $"/api/users/{response.Id}",
                response
            );
        }
        catch (DuplicateEmailException exception)
        {
            return Results.Conflict(new
            {
                message = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new
            {
                message = exception.Message
            });
        }
    })
    .RequireAuthorization(
        AuthorizationPolicies.AdminOnly
    )
    .WithName("CreateUser")
    .WithTags("Users");

app.MapGet(
    "/api/tickets/{id:guid}/history",
    async (
        Guid id,
        ClaimsPrincipal user,
        GetTicketHistoryHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var roleValue =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        var history =
            await handler.HandleAsync(
                id,
                userId,
                role,
                cancellationToken
            );

        if (history is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(history);
    })
    .RequireAuthorization()
    .WithName("GetTicketHistory")
    .WithTags("Tickets");

app.MapPost(
    "/api/tickets/{id:guid}/comments",
    async (
        Guid id,
        AddTicketCommentRequest request,
        ClaimsPrincipal user,
        AddTicketCommentHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var roleValue =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        try
        {
            var response =
                await handler.HandleAsync(
                    id,
                    userId,
                    role,
                    request,
                    cancellationToken
                );

            if (response is null)
            {
                return Results.NotFound();
            }

            return Results.Created(
                $"/api/tickets/{id}/comments/{response.Id}",
                response
            );
        }
        catch (UnauthorizedAccessException exception)
        {
            return Results.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new
            {
                message = exception.Message
            });
        }
    })
    .RequireAuthorization()
    .WithName("AddTicketComment")
    .WithTags("Tickets");

app.MapGet(
    "/api/tickets/{id:guid}/comments",
    async (
        Guid id,
        ClaimsPrincipal user,
        GetTicketCommentsHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var roleValue =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        try
        {
            var comments =
                await handler.HandleAsync(
                    id,
                    userId,
                    role,
                    cancellationToken
                );

            if (comments is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(comments);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Results.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
    })
    .RequireAuthorization()
    .WithName("GetTicketComments")
    .WithTags("Tickets");

app.MapPost(
    "/api/tickets/{id:guid}/ai-analysis",
    async (
        Guid id,
        ClaimsPrincipal user,
        AnalyzeTicketHandler handler,
        CancellationToken cancellationToken) =>
    {
        var userIdValue =
            user.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var roleValue =
            user.FindFirstValue(
                ClaimTypes.Role
            );

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<UserRole>(
            roleValue,
            ignoreCase: true,
            out var role))
        {
            return Results.Unauthorized();
        }

        try
        {
            var analysis =
                await handler.HandleAsync(
                    id,
                    userId,
                    role,
                    cancellationToken
                );

            if (analysis is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(analysis);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Results.Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
    })
    .RequireAuthorization(
        AuthorizationPolicies.SupportStaff
    )
    .WithName("AnalyzeTicket")
    .WithTags("AI");
app.MapGet(
    "/api/users/agents",
    async (
        GetAgentsHandler handler,
        CancellationToken cancellationToken) =>
    {
        var agents =
            await handler.HandleAsync(
                cancellationToken
            );

        return Results.Ok(agents);
    })
    .RequireAuthorization(
        AuthorizationPolicies.SupportStaff
    )
    .WithName("GetActiveAgents")
    .WithTags("Users");
app.Run();


static class EndpointMappings
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Auth");

        auth.MapPost("/register", RegisterUser)
            .WithName("RegisterUser");

        auth.MapPost("/login", Login)
            .WithName("Login");

        auth.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .WithName("GetCurrentUser");
    }

    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        RegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await handler.HandleAsync(request, cancellationToken);
            return Results.Created($"/api/users/{response.Id}", response);
        }
        catch (DuplicateEmailException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        LoginHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await handler.HandleAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (InvalidCredentialsException exception)
        {
            return Results.Json(
                new { message = exception.Message },
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (InactiveUserException exception)
        {
            return Results.Json(
                new { message = exception.Message },
                statusCode: StatusCodes.Status403Forbidden);
        }
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user)
    {
        return Results.Ok(new
        {
            id = user.FindFirstValue(ClaimTypes.NameIdentifier),
            name = user.FindFirstValue(ClaimTypes.Name),
            email = user.FindFirstValue(ClaimTypes.Email),
            role = user.FindFirstValue(ClaimTypes.Role)
        });
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
