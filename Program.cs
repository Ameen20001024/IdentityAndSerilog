using IdentityAndSerilog.Application.Features.Authentication.RefreshToken;
using IdentityAndSerilog.Application.Features.Authentication.UserLogin;
using IdentityAndSerilog.Application.Features.Authentication.UserRegister;
using IdentityAndSerilog.Application.Features.DummyFeatures.DummyFeatureOne;
using IdentityAndSerilog.Common;
using IdentityAndSerilog.Data;
using IdentityAndSerilog.Domain.Models;
using IdentityAndSerilog.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;


Log.Logger = new LoggerConfiguration()
    .ConfigureApplicationLogging()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //builder.Host.UseSerilog((context, services, configuration) =>
    //{
    //    configuration
    //        .ReadFrom.Configuration(context.Configuration)
    //        .ReadFrom.Services(services)
    //        .Enrich.FromLogContext()
    //        .WriteTo.Console()
    //        .WriteTo.File(
    //            path: "Logs/app-.log",
    //            rollingInterval: RollingInterval.Day,
    //            retainedFileCountLimit: 30,
    //            fileSizeLimitBytes: 10_000_000,
    //            rollOnFileSizeLimit: true,
    //            shared: true);
    //});

    // Add services to the container.


    builder.Host.UseSerilog();


    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();


    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("Default")));

    builder.Services
        .AddIdentity<User, IdentityRole<int>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddScoped<JwtHelper>();

    builder.Services
        .AddOptions<JwtOptions>()
        .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
        .Validate(options => !string.IsNullOrWhiteSpace(options.Key),
            "JWT Key is required.")
        .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer),
            "JWT Issuer is required.")
        .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
            "JWT Audience is required.")
        .Validate(options => options.ExpirationMinutes > 0,
            "JWT expiration must be greater than 0.")
        .Validate(options => options.RefreshTokenExpirationDays > 0,
            "Refresh token expiration must be greater than 0.")
        .ValidateOnStart();

    

    builder.Services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssembly(
            typeof(UserLoginHandler).Assembly);
    });

    var jwtOptions = builder.Configuration
        .GetSection(JwtOptions.SectionName)
        .Get<JwtOptions>()
        ?? throw new InvalidOperationException("JWT configuration is missing.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
            };
        });

    builder.Services.AddAuthorization(x => x.AddPolicy(Policies.AdminOnly, policy=> policy.RequireRole("Admin")));


    var app = builder.Build();

    Log.ForContext<Program>()
       .ForSystem()
       .Information("Application started.");

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }
    
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();


    app.UseAuthentication();
    app.UseAuthorization();

    UserRegisterEndPoint.MapUserRegisterEndPoint(app);
    UserLoginEndPoint.MapUserLoginEndpoint(app);
    RefreshTokenEndPoint.MapRefreshTokenEndPoint(app);
    DummyFeatureEndPoint.MapDummyFeatureEndPoint(app);



    app.Run();
}
catch (Exception ex)
{
    Log.ForContext<Program>()
   .ForSystem()
   .Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
