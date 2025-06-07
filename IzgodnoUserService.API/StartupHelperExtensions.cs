using IzgodnoUserService.Data;
using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.Services.UserServices;
using IzgodnoUserService.Services.UserServices.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", options =>
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
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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
            builder.Services.AddScoped<IUserService, UserService>();

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
