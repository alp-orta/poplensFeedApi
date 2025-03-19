using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using poplensFeedApi.Contracts;
using poplensFeedApi.Services;
using System.Text;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IUserProfileApiProxyService, UserProfileApiProxyService>();
builder.Services.AddScoped<IMediaApiProxyService, MediaApiProxyService>();

builder.Services.AddHttpClient();

string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
string issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
string audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer!,
            ValidAudience = audience!,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

// Add controllers and other necessary services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
