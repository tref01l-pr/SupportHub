using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportHub.DataAccess.SqlServer.Entities.Email;

namespace SupportHub.DataAccess.SqlServer.Entities;

public class CompanyEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public virtual ICollection<EmailBotEntity> EmailBots { get; set; }
    public virtual ICollection<UserEntity> Users { get; set; }
    public virtual ICollection<EmailConversationEntity> EmailConversations { get; set; }
}

public class CompanyEntityConfiguration : IEntityTypeConfiguration<CompanyEntity>
{
    public void Configure(EntityTypeBuilder<CompanyEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired(true);

        builder.Property(x => x.Url)
            .IsRequired(true);
        
        builder.HasIndex(msg => msg.Url)
            .IsUnique();
    }
}