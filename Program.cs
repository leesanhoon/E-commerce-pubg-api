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
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
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

app.UseCors("AllowAll"); // Di chuyển CORS trước UseHttpsRedirection
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        logger.LogInformation("Starting database initialization at {Time}", DateTimeOffset.Now);
        
        // Attempt to connect to database
        if (await context.Database.CanConnectAsync())
        {
            logger.LogInformation("Successfully connected to database");
            
            // Apply any pending migrations
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully");
        }
        else
        {
            logger.LogInformation("Could not connect to database. Creating new database...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database created and migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while setting up the database");
        throw; // Crash the app if database isn't available
    }
}

app.Run();
