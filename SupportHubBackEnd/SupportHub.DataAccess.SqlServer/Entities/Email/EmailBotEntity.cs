using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SupportHub.DataAccess.SqlServer.Entities.Email;

public class EmailBotEntity
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpHost { get; set; }
    public int ImapPort { get; set; }
    public string ImapHost { get; set; }
    public bool IsDeleted { get; set; }
    public DateOnly? DeletedOn { get; set; }
    public CompanyEntity Company { get; set; }
    public virtual ICollection<EmailConversationEntity> EmailConversations { get; set; }
}

public class EmailBotEntityConfiguration : IEntityTypeConfiguration<EmailBotEntity>
{
    public void Configure(EntityTypeBuilder<EmailBotEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompanyId)
            .IsRequired(true);

        builder.Property(x => x.Email)
            .IsRequired(true);

        builder.Property(x => x.Password)
            .IsRequired(true);

        builder.Property(x => x.SmtpPort)
            .IsRequired(true);

        builder.Property(x => x.SmtpHost)
            .IsRequired(true);

        builder.Property(x => x.ImapPort)
            .IsRequired(true);

        builder.Property(x => x.ImapHost)
            .IsRequired(true);
        
        builder.Property(x => x.IsDeleted)
            .IsRequired(true);
        
        builder.Property(x => x.DeletedOn)
            .IsRequired(false);
        
        builder.HasIndex(msg => msg.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(eb => eb.Company)
            .WithMany(c => c.EmailBots)
            .HasForeignKey(eb => eb.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}