using IdentityAndSerilog.Common;
using IdentityAndSerilog.Data;
using IdentityAndSerilog.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IdentityAndSerilog.Application.Features.Authentication.UserRegister;
using IdentityAndSerilog.Application.Features.Authentication.UserLogin;
using IdentityAndSerilog.Application.Features.Authentication.RefreshToken;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


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

builder.Services.AddScoped<JwtHelper>();

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

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

UserRegisterEndPoint.MapUserRegisterEndPoint(app);
UserLoginEndPoint.MapUserLoginEndpoint(app);
RefreshTokenEndPoint.MapRefreshTokenEndPoint(app);



app.Run();
