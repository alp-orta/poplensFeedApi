using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using poplensFeedApi.Contracts;
using poplensFeedApi.Data;
using poplensFeedApi.Services;
using System.Text;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Add DbContext to the services
builder.Services.AddDbContext<FeedDbContext>(options =>
    options.UseNpgsql("Host=postgresFeed;Port=5432;Username=postgre;Password=postgre;Database=Feed"));


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
            ValidIssuer = "YourIssuer",
            ValidAudience = "YourAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("moresimplekeyrightherefolkssssssssssssss"))
        };
    });

// Add controllers and other necessary services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//using (var scope = app.Services.CreateScope()) {
//    var dbContext = scope.ServiceProvider.GetRequiredService<FeedDbContext>();
//    dbContext.Database.Migrate(); // This applies any pending migrations
//}
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
