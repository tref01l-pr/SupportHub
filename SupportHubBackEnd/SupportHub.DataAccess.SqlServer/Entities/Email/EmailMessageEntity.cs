using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Entities.Email;

public class EmailMessageEntity
{
    public int Id { get; set; }
    public int EmailConversationId { get; set; }
    public int EmailRequesterId { get; set; }
    public Guid? UserId { get; set; }
    public string MessageId { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
    public MessageTypes MessageType { get; set; }
    public EmailConversationEntity EmailConversation { get; set; }
    public EmailRequesterEntity EmailRequester { get; set; }
    public UserEntity User { get; set; }
}

public class EmailMessageEntityConfiguration : IEntityTypeConfiguration<EmailMessageEntity>
{
    public void Configure(EntityTypeBuilder<EmailMessageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmailConversationId)
            .IsRequired(true);

        builder.Property(x => x.EmailRequesterId)
            .IsRequired(true);

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.MessageId)
            .IsRequired(true);

        builder.Property(x => x.Subject)
            .IsRequired(false);

        builder.Property(x => x.Body)
            .IsRequired(true);

        builder.Property(x => x.Date)
            .IsRequired(true);

        //TODO add unique for one bot
        builder.HasIndex(msg => msg.MessageId)
            .IsUnique();

        builder.HasOne(em => em.EmailConversation)
            .WithMany(ec => ec.EmailMessages)
            .HasForeignKey(em => em.EmailConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(em => em.EmailRequester)
            .WithMany(er => er.EmailMessages)
            .HasForeignKey(em => em.EmailRequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(em => em.User)
            .WithMany(u => u.EmailMessages)
            .HasForeignKey(em => em.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}