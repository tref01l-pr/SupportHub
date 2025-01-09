using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SupportHub.DataAccess.SqlServer.Entities.Email;

public class EmailConversationEntity
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int EmailBotId { get; set; }
    public int EmailRequesterId { get; set; }
    public string MsgId { get; set; }
    public string Subject { get; set; }
    public DateTimeOffset LastUpdateDate { get; set; }
    public CompanyEntity Company { get; set; }
    public EmailBotEntity EmailBot { get; set; }
    public EmailRequesterEntity EmailRequester { get; set; }
    public virtual ICollection<EmailMessageEntity> EmailMessages { get; set; }
}

public class EmailConversationEntityConfiguration : IEntityTypeConfiguration<EmailConversationEntity>
{
    public void Configure(EntityTypeBuilder<EmailConversationEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyId)
            .IsRequired(true);

        builder.Property(x => x.EmailBotId)
            .IsRequired(true);

        builder.Property(x => x.EmailRequesterId)
            .IsRequired(true);

        builder.Property(x => x.MsgId)
            .IsRequired(true);
        
        builder.Property(x => x.Subject)
            .IsRequired(true);
        
        builder.Property(x => x.LastUpdateDate)
            .IsRequired(true);

        builder.HasOne(ec => ec.Company)
            .WithMany(c => c.EmailConversations)
            .HasForeignKey(ec => ec.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.EmailBot)
            .WithMany(eb => eb.EmailConversations)
            .HasForeignKey(ec => ec.EmailBotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.EmailRequester)
            .WithMany(er => er.EmailConversations)
            .HasForeignKey(ec => ec.EmailRequesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}