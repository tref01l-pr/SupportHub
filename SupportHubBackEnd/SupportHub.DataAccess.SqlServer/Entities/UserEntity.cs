using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportHub.DataAccess.SqlServer.Entities.Email;

namespace SupportHub.DataAccess.SqlServer.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public int CompanyId { get; set; }
    public CompanyEntity CompanyEntity { get; set; }
    public virtual ICollection<EmailMessageEntity> EmailMessages { get; set; }
}

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(x => x.CompanyId)
            .IsRequired(true);

        builder.HasOne( u => u.CompanyEntity)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}