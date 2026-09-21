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
//using Serilog.Formatting.Compact;
using System.Text;


//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .Enrich.FromLogContext()
//    //.Enrich.WithMachineName()

//    // General application log — excludes security range (4000-4999)
//    .WriteTo.Logger(lc => lc
//        .Filter.ByExcluding(Serilog.Filters.Matching.WithProperty<int>(
//            "EventId.Id", id => id >= 4000 && id < 5000))
//        .WriteTo.Console()
//        .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day))

//    // Security log — only EventIds in the 4000-4999 range
//    .WriteTo.Logger(lc => lc
//        .Filter.ByIncludingOnly(Serilog.Filters.Matching.WithProperty<int>(
//            "EventId.Id", id => id >= 4000 && id < 5000))
//        .WriteTo.File(
//            new CompactJsonFormatter(),
//            "logs/security-.json",
//            rollingInterval: RollingInterval.Day))

//    .CreateLogger();

Log.Logger = new LoggerConfiguration()
    .ConfigureApplicationLogging()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

 

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

    //Log.ForContext<Program>()
    //   .ForSystem()
    //   .Information("Application started.");

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
    .Fatal(ex, "Server terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
