using SupportHub.Application.Services.Email;
using SupportHub.Domain.Interfaces.Application.Email;

namespace SupportHub.API;

using SupportHub.Domain.Interfaces.DataAccess.Email;
using System.Text;
using Application.Services;
using DataAccess.SqlServer;
using DataAccess.SqlServer.Entities;
using DataAccess.SqlServer.Repositories;
using Domain.Interfaces;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Interfaces.Infrastructure;
using Domain.Options;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        DotNetEnv.Env.Load("../.env");

        /*builder.Services.Configure<JWTSecretOptions>(
            builder.Configuration.GetSection(JWTSecretOptions.JWTSecret));
        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.Smtp));
        builder.Services.Configure<ImapOptions>(
            builder.Configuration.GetSection(ImapOptions.Imap));*/

        builder.Services.Configure<JWTSecretOptions>(options =>
        {
            options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new Exception("JWT_SECRET is not set");
        });
        builder.Services.Configure<SmtpOptions>(options =>
        {
            options.User = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new Exception("SMTP_USER is not set");
            options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new Exception("SMTP_PASSWORD is not set");
            options.Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? throw new Exception("SMTP_HOST is not set");
            options.Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? throw new Exception("SMTP_PORT is not set"));
        });
        builder.Services.Configure<ImapOptions>(options =>
        {
            options.User = Environment.GetEnvironmentVariable("IMAP_USER") ?? throw new Exception("IMAP_USER is not set");
            options.Password = Environment.GetEnvironmentVariable("IMAP_PASSWORD") ?? throw new Exception("IMAP_PASSWORD is not set");
            options.Host = Environment.GetEnvironmentVariable("IMAP_HOST") ?? throw new Exception("IMAP_HOST is not set");
            options.Port = int.Parse(Environment.GetEnvironmentVariable("IMAP_PORT") ?? throw new Exception("IMAP_PORT is not set"));
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddHttpContextAccessor();

        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });

        builder.Services.AddDbContext<SupportHubDbContext>(options =>
            options.UseNpgsql(
                Environment.GetEnvironmentVariable("DATABASE_URL"),
                x => x.MigrationsAssembly("SupportHub.DataAccess.SqlServer")));
        
        /*builder.Services.AddDbContext<SupportHubDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("SupportHubDbContext"),
                x => x.MigrationsAssembly("SupportHub.DataAccess.SqlServer")));*/


        builder.Services
            .AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<SupportHubDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });

        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile<ApiMappingProfile>();
            config.AddProfile<DataAccessMappingProfile>();
        });

        builder.Services.AddTransient<Seed>();
        builder.Services.AddScoped<IEmailImapService, EmailImapService>();
        builder.Services.AddScoped<IEmailSmtpService, EmailSmtpService>();

        builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
        builder.Services.AddScoped<IEmailMessagesRepository, EmailMessagesRepository>();
        builder.Services.AddScoped<ISessionsRepository, SessionsRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        //builder.Services.AddScoped<ICacheRepository, CacheRepository>();
        builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();
        builder.Services.AddScoped<IEmailRequestersRepository, EmailRequestersRepository>();
        builder.Services.AddScoped<IEmailConversationsRepository, EmailConversationsRepository>();
        builder.Services.AddScoped<IEmailBotsRepository, EmailBotsRepository>();

        builder.Services.AddScoped<IMessagesService, MessagesService>();
        builder.Services.AddScoped<IUsersService, UsersService>();
        builder.Services.AddScoped<IClientMessagesService, ClientsService>();
        builder.Services.AddScoped<IMessagesService, MessagesService>();
        builder.Services.AddScoped<ICompaniesService, CompaniesService>();
        builder.Services.AddScoped<ISystemAdminsService, SystemAdminsService>();
        builder.Services.AddScoped<IEmailBotsService, EmailBotsService>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSecret:Secret").Value!)),
                };
            });

        builder.Services.AddAuthorization();


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Any", corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .WithOrigins(new[]
                    {
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "http://localhost:5174",
                        "https://mail-front-end-2vl6-f61l90s8c-anatoliis-projects-f2a5aed8.vercel.app",
                        "https://mail-front-end-2vl6.vercel.app/",
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


        var app = builder.Build();

        if (args.Length == 1 && args[0].ToLower() == "--seeddata")
            await SeedData(app);

        async Task SeedData(IHost app)
        {
            var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopedFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<Seed>();
                await service.SeedDataContextAsync();
            }
        }

        /*if (app.Environment.IsDevelopment())
        {*/
        app.UseSwagger();
        app.UseSwaggerUI();
        /*}*/

        /*using (var scope = app.Services.CreateScope())
        {
            var messagesService = scope.ServiceProvider.GetRequiredService<IMessagesService>();
            var result = await messagesService.RemoveKeyAsync();
            if (result.IsSuccess)
            {
                Console.WriteLine("Cache key deleted successfully");
            }
            else
            {
                Console.WriteLine($"Failed to delete cache key: {result.Error}");
            }
        }*/

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors("Any");

        app.MapControllers();

        app.Run();
    }
}