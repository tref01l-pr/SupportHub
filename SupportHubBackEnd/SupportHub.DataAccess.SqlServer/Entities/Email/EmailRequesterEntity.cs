using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SupportHub.DataAccess.SqlServer.Entities.Email;

public class EmailRequesterEntity
{
    public int Id { get; set; }
    public string Email { get; set; }
    public virtual ICollection<EmailConversationEntity> EmailConversations { get; set; }
    public virtual ICollection<EmailMessageEntity> EmailMessages { get; set; }
}

public class EmailRequesterEntityConfiguration : IEntityTypeConfiguration<EmailRequesterEntity>
{
    public void Configure(EntityTypeBuilder<EmailRequesterEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired(true);
    }
}