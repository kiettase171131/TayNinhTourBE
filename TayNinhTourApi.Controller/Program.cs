using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Mapping;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.SeedData;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.UnitOfWork;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MemoryHotelApi", Version = "v1" });

    // Define Bearer Auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using Bearer scheme. Example: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Register DbContext with MySQL (Pomelo provider)
builder.Services.AddDbContext<TayNinhTouApiDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")!, new MySqlServerVersion(new Version(8, 0, 21))));

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    // Policy �ExcludeAdmin�: cho ph�p m?i user ?� auth, mi?n tr? role = "Admin"
    options.AddPolicy("ExcludeAdmin", policy =>
        policy.RequireAssertion(context =>
            // user ph?i authenticated v� KH�NG c� role "Admin"
            context.User.Identity != null
            && context.User.Identity.IsAuthenticated
            && !context.User.IsInRole("Admin")
        )
    );
});


// Config Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure Kestrel to allow large request bodies
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Validate AutoMapper configuration in development
// TEMPORARILY DISABLED: Causing deadlock during startup
// TODO: Re-enable after fixing mapping configuration issues
/*
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton(provider =>
    {
        var mapper = provider.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        return mapper.ConfigurationProvider;
    });
}
*/

// Register services layer
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICmsService, CmsService>();
builder.Services.AddScoped<ITourCompanyService, TourCompanyService>();
builder.Services.AddScoped<ITourTemplateService, EnhancedTourTemplateService>();

builder.Services.AddScoped<ITourDetailsService, TourDetailsService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<ITourGuideApplicationService, TourGuideApplicationService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IBlogReactionService, BlogReactionService>();
// Shop service removed - merged into SpecialtyShopService
builder.Services.AddScoped<ISchedulingService, SchedulingService>();
builder.Services.AddScoped<ITourMigrationService, TourMigrationService>();
builder.Services.AddScoped<ITourOperationService, TourOperationService>();
builder.Services.AddScoped<IBlogCommentService, BlogCommentService>();
builder.Services.AddScoped<IShopApplicationService, ShopApplicationService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ISpecialtyShopApplicationService, SpecialtyShopApplicationService>();
builder.Services.AddScoped<ISpecialtyShopService, SpecialtyShopService>();

// TourGuide Invitation Workflow Services
builder.Services.AddScoped<ITourGuideInvitationService, TourGuideInvitationService>();



// Register repositories layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITourTemplateRepository, TourTemplateRepository>();
// Shop repository removed - merged into SpecialtyShopRepository
builder.Services.AddScoped<ITourSlotRepository, TourSlotRepository>();
builder.Services.AddScoped<ITourDetailsRepository, TourDetailsRepository>();
builder.Services.AddScoped<ITourOperationRepository, TourOperationRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketCommentRepository, SupportTicketCommentRepository>();
builder.Services.AddScoped<ITourGuideApplicationRepository, TourGuideApplicationRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IBlogImageRepository, BlogImageRepository>();
builder.Services.AddScoped<IBlogReactionRepository, BlogReactionRepository>();
builder.Services.AddScoped<IBlogCommentRepository, BlogCommentRepository>();
builder.Services.AddScoped<IShopApplicationRepository, ShopApplicationRepository>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();

builder.Services.AddScoped<ISpecialtyShopApplicationRepository, SpecialtyShopApplicationRepository>();
builder.Services.AddScoped<ISpecialtyShopRepository, SpecialtyShopRepository>();

// TourGuide Invitation Workflow Repositories
builder.Services.AddScoped<ITourGuideInvitationRepository, TourGuideInvitationRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();


// Register utilities
builder.Services.AddScoped<BcryptUtility>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<JwtUtility>();
builder.Services.AddScoped<EmailSender>();

// TourGuide Invitation Workflow Utilities (Static utility - no registration needed for SkillsMatchingUtility)

// Register Background Job Service as Hosted Service
builder.Services.AddHostedService<BackgroundJobService>();

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register IMemoryCache
builder.Services.AddMemoryCache();

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});





var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TayNinhTouApiDbContext>();

    // Ensure database is created
    await context.Database.EnsureCreatedAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedDataAsync();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
// Serve static files
// Create the Images directory if it doesn't exist
string imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/Images"
});


// Enable Forwarded Headers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();
// Use CORS policy
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
