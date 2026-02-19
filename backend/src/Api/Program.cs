using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Host.UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
{
    loggerConfiguration.ConfigureBaseLogging("realworldDotnet");
    loggerConfiguration.AddApplicationInsightsLogging(services, hostBuilderContext.Configuration);
});

// Setup database connection (SQLite In-Memory)
const string connectionString = "Filename=:memory:";
var connection = new SqliteConnection(connectionString);
connection.Open();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "realworlddotnet", Version = "v1" });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressInferBindingSourcesForParameters = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin() // For development only!
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// --- CUSTOM SERVICES & HANDLERS ---
builder.Services.AddScoped<IConduitRepository, ConduitRepository>();
builder.Services.AddScoped<IUserHandler, UserHandler>();
builder.Services.AddScoped<IArticlesHandler, ArticlesHandler>();
builder.Services.AddScoped<IProfilesHandler, ProfilesHandler>();

// --- IDENTITY & SEEDING REGISTRATION ---
// Required for hashing passwords in the seeder
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Register the Database Seeder
builder.Services.AddScoped<IDatabaseSeeder>(provider =>
    new DatabaseSeeder(
        provider.GetRequiredService<ConduitContext>(),
        provider.GetRequiredService<ILogger<DatabaseSeeder>>(),
        provider.GetRequiredService<IPasswordHasher<User>>(),
        builder.Environment.ContentRootPath
    ));

builder.Services.AddSingleton<CertificateProvider>();
builder.Services.AddSingleton<ITokenGenerator>(container =>
{
    var logger = container.GetRequiredService<ILogger<CertificateProvider>>();
    var certificateProvider = new CertificateProvider(logger);
    var cert = certificateProvider.LoadFromFile("certificate.pfx", "password");
    return new TokenGenerator(cert);
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<ILogger<CertificateProvider>>((o, logger) =>
    {
        var certificateProvider = new CertificateProvider(logger);
        var cert = certificateProvider.LoadFromFile("certificate.pfx", "password");

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = new RsaSecurityKey(cert.GetRSAPublicKey())
        };
        o.Events = new JwtBearerEvents { OnMessageReceived = CustomOnMessageReceivedHandler.OnMessageReceived };
    });

// DbContext configuration
builder.Services.AddDbContext<ConduitContext>(options => { options.UseSqlite(connection); });

// Error handling
// Note: Ensure your local project has the ProblemDetails extension methods available
// ProblemDetailsExtensions.AddProblemDetails(builder.Services); 
builder.Services.ConfigureOptions<ProblemDetailsLogging>();

var app = builder.Build();

// --- DATABASE INITIALIZATION & SEEDING ---
Log.Information("Start configuring http request pipeline");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ConduitContext>();

    // Create tables in memory
    context.Database.EnsureCreated();

    // Execute Seeder
    var seeder = services.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseCors("CorsPolicy");

// Middleware pipeline
app.UseSerilogRequestLogging();
// app.UseProblemDetails(); // Ensure extension is available before uncommenting
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "realworlddotnet v1"));

try
{
    Log.Information("Starting web host");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    connection.Close();
    Log.CloseAndFlush();
    Thread.Sleep(2000);
}