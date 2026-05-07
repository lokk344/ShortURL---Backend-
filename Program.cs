using System.Text;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//
// DATABASE
//
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

//
// SERVICES
//
builder.Services.AddScoped<JwtService>();

//
// CONTROLLERS
//
builder.Services.AddControllers();

//
// SWAGGER
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//
// JWT AUTHENTICATION
//
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!
                    )
                )
            };
    });

//
// CORS
//
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin(); // temporary for deployment
        });
});

var app = builder.Build();

//
// SWAGGER
//
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//
// HTTPS
//
app.UseHttpsRedirection();

//
// CORS
//
app.UseCors("AllowFrontend");

//
// AUTH
//
app.UseAuthentication();
app.UseAuthorization();

//
// CONTROLLERS
//
app.MapControllers();

app.Run();