using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TourismReddit.Api.Data; // Using for ApplicationDbContext
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Microsoft.Extensions.FileProviders; // For static files

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;

// 1. Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Ensure configuration values exist before using them
    var jwtKey = Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
    var jwtIssuer = Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
    var jwtAudience = Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// 3. Configure CORS (Allow frontend dev server)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // React default port - Adjust if yours is different
              .AllowAnyHeader()
              .AllowAnyMethod();
        // In production, restrict to your actual frontend domain:
        // policy.WithOrigins("https://<your-username>.github.io")
        //       .AllowAnyHeader()
        //       .AllowAnyMethod();
    });
});

// Add Controllers Service
builder.Services.AddControllers();

// 4. Configure Swagger/OpenAPI for API testing and JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TourismReddit API", Version = "v1" });
    // Add JWT Authentication support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field (e.g., Bearer YOUR_TOKEN)",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"}
        },
        new string[] {}
    }});
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TourismReddit API V1");
        // Optionally configure Swagger UI settings here
    });
    app.UseDeveloperExceptionPage();
}

// Optional: Apply migrations automatically on startup (for dev)
/* // Uncomment if you want auto-migration on startup - Use with caution
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
        // Consider logging the error properly
    }
}
*/

// --- Add Static Files Middleware ---
// Define the path to the uploads folder relative to the content root
var uploadsFolderPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
// Ensure the directory exists (optional here, created on upload)
// Directory.CreateDirectory(uploadsFolderPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolderPath),
    RequestPath = "/uploads" // The URL path clients will use (e.g., http://localhost:5019/uploads/...)
});
// --- End Add Static Files ---

//For production, this is a dev build, so we don't need to use HTTPS redirection
//app.UseHttpsRedirection();

// Enable CORS - MUST be before UseAuthentication and UseAuthorization
app.UseCors("AllowReactApp");

app.UseAuthentication(); // Enable JWT token validation
app.UseAuthorization(); // Enable [Authorize] attribute checks

app.MapControllers(); // Map controller routes

app.Run();