using EduAIAPI.Models;
using EduAIAPI.Data;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MongoDbContext as a service
builder.Services.AddSingleton<MongoDbContext>();

// Add JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Ensure the JWT Key is not null or empty
var key = jwtSettings["Key"];
if (string.IsNullOrEmpty(key))
{
    throw new InvalidOperationException("JWT Key is missing or empty in appsettings.json.");
}

var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing in appsettings.json.");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is missing in appsettings.json.");

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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Define your endpoints here
app.MapGet("/api/lectures", async (MongoDbContext context) =>
{
    var lectures = await context.Lectures.Find(_ => true).ToListAsync();
    return Results.Ok(lectures);
});

app.MapPost("/api/lectures", async (MongoDbContext context, Lecture lecture) =>
{
    await context.Lectures.InsertOneAsync(lecture);
    return Results.Created($"/api/lectures/{lecture.Id}", lecture);
});

// User login endpoint
app.MapPost("/api/login", async (MongoDbContext context, LoginRequest request) =>
{
    // Find the user by university number
    var user = await context.Users.Find(u => u.UniversityNumber == request.UniversityNumber).FirstOrDefaultAsync();
    if (user == null)
        return Results.BadRequest("Invalid university number or password.");

    // Verify the password (compare the provided password with the stored hash)
    var passwordHasher = new PasswordHasher<User>();
    var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
    if (result != PasswordVerificationResult.Success)
        return Results.BadRequest("Invalid university number or password.");

    // Ensure the JWT Key is not null or empty
    var key = jwtSettings["Key"];
    if (string.IsNullOrEmpty(key))
    {
        throw new InvalidOperationException("JWT Key is missing or empty in appsettings.json.");
    }

    // Generate a JWT
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.UniversityNumber),
            new Claim(ClaimTypes.Role, user.Role)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature),
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"]
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { Token = tokenString });
});

// Example of a secured endpoint
app.MapGet("/api/secure-lectures", [Authorize] async (MongoDbContext context) =>
{
    var lectures = await context.Lectures.Find(_ => true).ToListAsync();
    return Results.Ok(lectures);
});

app.Run();