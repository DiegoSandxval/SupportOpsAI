#pragma warning disable OPENAI001

using OpenAI.Responses;
using SupportOps.Application.Abstractions.AI;
using SupportOps.Domain.Enums;
using System.Text.Json;

namespace SupportOps.Infrastructure.AI;

public sealed class OpenAiTicketAnalyzer
    : ITicketAiAnalyzer
{
    private readonly ResponsesClient _client;
    private readonly string _model;

    public OpenAiTicketAnalyzer(
        ResponsesClient client,
        string model)
    {
        _client = client;
        _model = model;
    }

    public async Task<TicketAiAnalysisResult> AnalyzeAsync(
        string title,
        string description,
        CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
        You are an AI support ticket triage assistant.

        Analyze the following support ticket.

        Title:
        {{title}}

        Description:
        {{description}}

        You must classify the ticket using ONLY one of these categories:

        General
        Hardware
        Software
        Network
        Security
        Account
        Api
        Database
        Other

        You must classify priority using ONLY one of:

        Low
        Medium
        High
        Critical

        Return ONLY valid JSON.

        Do not include markdown.
        Do not include code fences.
        Do not include additional text.

        Return exactly this structure:

        {
          "suggestedCategory": "Api",
          "suggestedPriority": "High",
          "summary": "Short summary of the issue.",
          "reason": "Short explanation of why this category and priority were selected."
        }
        """;

        ResponseResult response =
            await _client.CreateResponseAsync(
                _model,
                prompt,
                cancellationToken: cancellationToken
            );

        var output = response
            .GetOutputText()
            .Trim();

        AiAnalysisPayload? payload;

        try
        {
            payload =
                JsonSerializer.Deserialize<AiAnalysisPayload>(
                    output,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "AI returned an invalid response.",
                exception
            );
        }

        if (payload is null)
        {
            throw new InvalidOperationException(
                "AI returned an empty response."
            );
        }

        if (!Enum.TryParse<TicketCategory>(
            payload.SuggestedCategory,
            ignoreCase: true,
            out var category))
        {
            throw new InvalidOperationException(
                "AI returned an invalid ticket category."
            );
        }

        if (!Enum.TryParse<TicketPriority>(
            payload.SuggestedPriority,
            ignoreCase: true,
            out var priority))
        {
            throw new InvalidOperationException(
                "AI returned an invalid ticket priority."
            );
        }

        return new TicketAiAnalysisResult(
            category,
            priority,
            payload.Summary,
            payload.Reason
        );
    }

    private sealed record AiAnalysisPayload(
        string SuggestedCategory,
        string SuggestedPriority,
        string Summary,
        string Reason
    );
}