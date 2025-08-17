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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.Mapping;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Repositories;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;
using TayNinhTourApi.DataAccessLayer.SeedData;
using TayNinhTourApi.DataAccessLayer.UnitOfWork;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Vietnam timezone and culture
var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
var vietnamCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = vietnamCulture;
CultureInfo.DefaultThreadCurrentUICulture = vietnamCulture;

// Configure JSON serialization for Vietnam timezone
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Configure DateTime serialization to handle Vietnam timezone
        options.JsonSerializerOptions.Converters.Add(new VietnamDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new VietnamNullableDateTimeConverter());
    });
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

// Register DbContext with MySQL (Pomelo provider) with retry policy and extended timeouts
builder.Services.AddDbContext<TayNinhTouApiDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection")!,
        new MySqlServerVersion(new Version(8, 0, 21)),
        mySqlOptions =>
        {
            mySqlOptions.CommandTimeout(300); // Tăng command timeout lên 5 phút
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

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
// builder.Services.Configure<ForwardedHeadersOptions>(options =>
// {
//     options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
//     options.KnownNetworks.Clear();
//     options.KnownProxies.Clear();
// });

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

// Configure AI settings
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAISettings"));

// Register HttpClient for Gemini API với timeout và retry policy
builder.Services.AddHttpClient<IGeminiAIService, GeminiAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // Tăng timeout lên 30s cho Gemini API
    client.DefaultRequestHeaders.Add("User-Agent", "TayNinhTourAPI/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // Tắt automatic decompression để tránh conflict
    handler.AutomaticDecompression = System.Net.DecompressionMethods.None;
    return handler;
});

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
builder.Services.AddScoped<ITourSlotService, TourSlotService>();
builder.Services.AddScoped<ITourMigrationService, TourMigrationService>();
builder.Services.AddScoped<ITourOperationService, TourOperationService>();
builder.Services.AddScoped<ITourBookingService, TourBookingService>();
builder.Services.AddScoped<IBlogCommentService, BlogCommentService>();

// Tour Booking System Services
builder.Services.AddScoped<IUserTourBookingService, UserTourBookingService>();
builder.Services.AddScoped<ITourPricingService, TourPricingService>();
builder.Services.AddScoped<ITourRevenueService, TourRevenueService>();
builder.Services.AddScoped<ITourCompanyNotificationService, TourCompanyNotificationService>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPayOsService, PayOsService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ISpecialtyShopApplicationService, SpecialtyShopApplicationService>();
builder.Services.AddScoped<ISpecialtyShopService, SpecialtyShopService>();

// Wallet Service - for managing Tour Company and Specialty Shop wallets
builder.Services.AddScoped<IWalletService, WalletService>();

// Withdrawal System Services - for managing bank accounts and withdrawal requests
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IWithdrawalRequestService, WithdrawalRequestService>();

// File Storage Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// TourGuide Invitation Workflow Services
builder.Services.AddScoped<ITourGuideInvitationService, TourGuideInvitationService>();

// Skill Management Services
builder.Services.AddScoped<ISkillManagementService, SkillManagementService>();

// Data Migration Services
builder.Services.AddScoped<DataMigrationService>();

// AI Chat Services
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();
builder.Services.AddScoped<IAIChatService, AIChatService>();
builder.Services.AddScoped<IAITourDataService, AITourDataService>();
builder.Services.AddScoped<IAIProductDataService, AIProductDataService>();
builder.Services.AddScoped<IAISpecializedChatService, AISpecializedChatService>();

// Notification Services
builder.Services.AddScoped<INotificationService, NotificationService>();

// QR Code Services
builder.Services.AddScoped<IQRCodeService, QRCodeService>();

// Timeline Progress Services
builder.Services.AddScoped<ITourGuideTimelineService, TourGuideTimelineService>();

// Register repositories layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITourTemplateRepository, TourTemplateRepository>();
// Shop repository removed - merged into SpecialtyShopRepository
builder.Services.AddScoped<ITourSlotRepository, TourSlotRepository>();
builder.Services.AddScoped<ITourDetailsRepository, TourDetailsRepository>();
builder.Services.AddScoped<ITourDetailsSpecialtyShopRepository, TourDetailsSpecialtyShopRepository>();
builder.Services.AddScoped<ITourOperationRepository, TourOperationRepository>();
builder.Services.AddScoped<ITourSlotTimelineProgressRepository, TourSlotTimelineProgressRepository>();
builder.Services.AddScoped<ITourBookingRepository, TourBookingRepository>();
builder.Services.AddScoped<ITourCompanyRepository, TourCompanyRepository>();

builder.Services.AddScoped<ITimelineItemRepository, TimelineItemRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketCommentRepository, SupportTicketCommentRepository>();
builder.Services.AddScoped<ITourGuideApplicationRepository, TourGuideApplicationRepository>();
builder.Services.AddScoped<ITourGuideRepository, TourGuideRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IBlogImageRepository, BlogImageRepository>();
builder.Services.AddScoped<IBlogReactionRepository, BlogReactionRepository>();
builder.Services.AddScoped<IBlogCommentRepository, BlogCommentRepository>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IProductRatingRepository, ProductRatingRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();

builder.Services.AddScoped<ISpecialtyShopApplicationRepository, SpecialtyShopApplicationRepository>();
builder.Services.AddScoped<ISpecialtyShopRepository, SpecialtyShopRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// TourGuide Invitation Workflow Repositories
builder.Services.AddScoped<ITourGuideInvitationRepository, TourGuideInvitationRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IVoucherCodeRepository, VoucherCodeRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();


// Notification Repository
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAdminSettingDiscountRepository, AdminSettingDiscountRepository>();

// AI Chat Repositories
builder.Services.AddScoped<IAIChatSessionRepository, AIChatSessionRepository>();
builder.Services.AddScoped<IAIChatMessageRepository, AIChatMessageRepository>();

// Tour Booking Refund Repositories
builder.Services.AddScoped<ITourBookingRefundRepository, TourBookingRefundRepository>();
builder.Services.AddScoped<IRefundPolicyRepository, RefundPolicyRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<ITourFeedbackRepository, TourFeedbackRepository>();

// Payment System Repositories
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();


// Tour Booking Refund Services
builder.Services.AddScoped<IRefundPolicyService, RefundPolicyService>();
builder.Services.AddScoped<ITourBookingRefundService, TourBookingRefundService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITourFeedbackService, TourFeedbackService>();

// Register utilities
builder.Services.AddScoped<BcryptUtility>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<JwtUtility>();
builder.Services.AddScoped<EmailSender>();

// TourGuide Invitation Workflow Utilities (Static utility - no registration needed for SkillsMatchingUtility)

// Register Background Job Service as Hosted Service
builder.Services.AddHostedService<BackgroundJobService>();
builder.Services.AddHostedService<TourAutoCancelService>();
builder.Services.AddHostedService<TourBookingCleanupService>();
builder.Services.AddHostedService<TourRevenueTransferService>(); // NEW: Automated revenue transfer service
builder.Services.AddHostedService<TourReminderService>(); // NEW: Tour reminder email service

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

    // Specific policy for PayOS callback
    options.AddPolicy("PayOSCallback", policy =>
    {
        policy.WithOrigins("https://api-merchant.payos.vn", "https://payos.vn")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Initialize database and seed data with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<TayNinhTouApiDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Attempting to connect to database...");

        // Test connection first
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogWarning("Cannot connect to database. API will start without database initialization.");
        }
        else
        {
            logger.LogInformation("Database connection successful. Initializing...");

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database ensured created.");

            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedDataAsync();
            logger.LogInformation("Database seeding completed.");
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the database. API will start without database initialization.");
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();
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
// app.UseForwardedHeaders(new ForwardedHeadersOptions
// {
//     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
// });

app.UseAuthentication();
app.UseAuthorization();
// Use CORS policy
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
