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
using IzgodnoUserService.Services.MessageQueueService.Interfaces;
using IzgodnoUserService.Services.MessageQueueService;
using StackExchange.Redis;
using MassTransit;
using IzgodnoUserService.API.Consumers;
using IzgodnoUserService.API.Hubs;

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

            builder.Services.AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    IConfigurationSection googleConf = builder.Configuration.GetSection("GoogleAuthSettings");
                    options.ClientId = googleConf["ClientId"]!;
                    options.ClientSecret = googleConf["ClientSecret"]!;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<ProductResultConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("product.result", e =>
                    {
                        e.ConfigureConsumer<ProductResultConsumer>(context);
                    });
                });
            });


            // --- Services ---
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect("localhost:6379"));

            builder.Services.AddScoped<RedisConnectionManager>();

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowChromeExtension", policy =>
                {
                    policy.WithOrigins(
                            "chrome-extension://jhjicmlogjaiglekmddiajgjplaolofg", // Replace with your actual extension ID
                            "http://127.0.0.1:5500/index.html",
                            "http://localhost:4200"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // Important for cookies
                });
            });

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

            app.UseCors("AllowChromeExtension");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notification")
                .RequireCors("AllowChromeExtension"); ;

            return app;
        }
    }
}
