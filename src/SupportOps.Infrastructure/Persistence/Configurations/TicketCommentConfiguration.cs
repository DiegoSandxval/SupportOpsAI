using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportOps.Domain.Entities;

namespace SupportOps.Infrastructure.Persistence.Configurations;

public class TicketCommentConfiguration
    : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("TicketComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.IsInternal)
            .IsRequired();

        builder.HasIndex(x => x.TicketId);

        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}