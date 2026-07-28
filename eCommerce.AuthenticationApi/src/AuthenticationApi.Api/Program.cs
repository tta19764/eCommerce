using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var authGroup = app.MapGroup("api/v1/auth")
    .WithTags("Authentication");

authGroup.MapPost("token", (TokenRequest request, IConfiguration configuration) =>
{
    var options = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiresAt = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, request.ClientId.ToString()),
        new(JwtRegisteredClaimNames.Email, request.Email),
        new(ClaimTypes.NameIdentifier, request.ClientId.ToString())
    };

    claims.AddRange(request.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var token = new JwtSecurityToken(
        issuer: options.Issuer,
        audience: options.Audience,
        claims: claims,
        expires: expiresAt,
        signingCredentials: credentials);

    return Results.Ok(new TokenResponse(
        new JwtSecurityTokenHandler().WriteToken(token),
        expiresAt));
})
.WithName("CreateToken")
.WithSummary("Create a local development JWT");

app.Run();

public sealed record TokenRequest(Guid ClientId, string Email, IReadOnlyCollection<string> Roles);

public sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc);

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "ecommerce-auth";

    public string Audience { get; init; } = "ecommerce";

    public string Secret { get; init; } = "local-development-secret-key-with-at-least-32-characters";

    public int ExpiryMinutes { get; init; } = 60;
}
