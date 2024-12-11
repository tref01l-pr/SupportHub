using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities;
using SupportHub.DataAccess.SqlServer.Entities.Email;

namespace SupportHub.DataAccess.SqlServer;

public class SupportHubDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{
    public SupportHubDbContext(DbContextOptions<SupportHubDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<CompanyEntity> Companies { get; set; }
    public DbSet<EmailBotEntity> EmailBots { get; set; }
    public DbSet<EmailConversationEntity> EmailConversations { get; set; }
    public DbSet<EmailMessageEntity> EmailMessages { get; set; }
    public DbSet<EmailRequesterEntity> EmailRequesters { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SupportHubDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}