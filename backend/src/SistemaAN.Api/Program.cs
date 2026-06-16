using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using SistemaAN.Api.Middleware;
using SistemaAN.Application;
using SistemaAN.Infrastructure;
using SistemaAN.Infrastructure.Persistence;
using SistemaAN.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logs (Serilog) — configurado a partir do appsettings (seção "Serilog").
// ---------------------------------------------------------------------------
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// ---------------------------------------------------------------------------
// Camadas da aplicação (Clean Architecture).
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------------------------------------------------------------------------
// Tratamento global de erros (ProblemDetails / RFC 7807).
// ---------------------------------------------------------------------------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------------------------------------------------------------------------
// API + Documentação (Swagger/OpenAPI) com suporte a JWT.
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SistemaAN API",
        Version = "v1",
        Description = "API do sistema de gestão de alimentação natural para cães.",
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu token}",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer",
        },
    };

    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() },
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ---------------------------------------------------------------------------
// Health checks (inclui verificação de conectividade com o banco).
// ---------------------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

// ---------------------------------------------------------------------------
// CORS para o frontend Angular.
// ---------------------------------------------------------------------------
const string corsPolicy = "frontend";
builder.Services.AddCors(options => options.AddPolicy(corsPolicy, policy =>
    policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// Aplica migrations pendentes e executa o seed inicial (papel + admin).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
    await seeder.SeedAsync();

    var catalogSeeder = scope.ServiceProvider.GetRequiredService<CatalogDataSeeder>();
    await catalogSeeder.SeedAsync();

    var consumoSeeder = scope.ServiceProvider.GetRequiredService<ConsumoDataSeeder>();
    await consumoSeeder.SeedAsync();

    var frequenciasSeeder = scope.ServiceProvider.GetRequiredService<FrequenciasDataSeeder>();
    await frequenciasSeeder.SeedAsync();

    var pacotesSeeder = scope.ServiceProvider.GetRequiredService<PacotesDataSeeder>();
    await pacotesSeeder.SeedAsync();

    // Massa de dados de demonstração/teste — só quando Seed:DemoData = true
    // (ligado apenas no docker-compose.test.yml; o ambiente original nunca recebe).
    if (app.Configuration.GetValue<bool>("Seed:DemoData"))
    {
        var demoSeeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await demoSeeder.SeedAsync();
    }
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "SistemaAN API v1"));
}

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
