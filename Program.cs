using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using E_commerce_pubg_api.Infrastructure.Persistence;
using E_commerce_pubg_api.Application.Interfaces;
using E_commerce_pubg_api.Application.Services;
using E_commerce_pubg_api.Application.Validators;
using E_commerce_pubg_api.Infrastructure.ExternalServices.Cloudinary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();
builder.Services.AddScoped<CreateCategoryDtoValidator>();
builder.Services.AddScoped<UpdateCategoryDtoValidator>();
builder.Services.AddScoped<CreateProductDtoValidator>();

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
    });
});

// Configure Services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    { 
        Title = "PUBG E-commerce API", 
        Version = "v1",
        Description = "REST API for PUBG E-commerce system",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@pubg-ecommerce.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PUBG E-commerce API V1");
    c.RoutePrefix = string.Empty;
    c.DocumentTitle = "PUBG E-commerce API Documentation";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Starting database migration at {Time}", DateTimeOffset.Now);

        // Create database if not exists
        context.Database.EnsureCreated();
        logger.LogInformation("Database created successfully");

        // Apply migrations
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Found {Count} pending migrations", pendingMigrations.Count());
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("Applying migration: {Migration}", migration);
            }
            await context.Database.MigrateAsync();
            logger.LogInformation("All migrations applied successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations found");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw; // Crash the app if database isn't available
    }
}

app.Run();
