using IzgodnoUserService.Data;
using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.Data.Repositories.Interfaces;
using IzgodnoUserService.Data.Repositories;
using IzgodnoUserService.Services.UserServices;
using IzgodnoUserService.Services.UserServices.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IzgodnoUserService.Services.AuthenticationServices.Interfaces;
using IzgodnoUserService.Services.AuthenticationServices;

namespace IzgodnoUserService.API
{
    public static class StartupHelperExtension
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            // --- Database ---
            builder.Services.AddDbContext<IzgodnoUserServiceDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // --- Identity ---
            builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<IzgodnoUserServiceDbContext>()
                .AddDefaultTokenProviders();

            // --- JWT Authentication ---
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")))
                };

                // Allow receiving tokens via cookies
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt"];
                        return Task.CompletedTask;
                    }
                };
            });

            // --- Google OAuth ---
            builder.Services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.ClientId = builder.Configuration["GoogleAuthSettings:ClientId"]!;
                    options.ClientSecret = builder.Configuration["GoogleAuthSettings:ClientSecret"]!;
                });

            // --- Services ---
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            return builder;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
