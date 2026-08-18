using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Parkini.Infrastructure.Data;
using Parkini.Infrastructure.Security;
using Hangfire;
using Hangfire.PostgreSql;
using Parkini.Infrastructure.Notifications;
using Parkini.Infrastructure.Vlm;
using Parkini.Infrastructure.Calibration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString)
               .UseSnakeCaseNamingConvention());
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emetteur"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Cle"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();

// ===== Notifications / Rappels =====
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<RappelsJobs>();

// ===== VLM (SCRUM-22 : analyse d'occupation par vision) =====
builder.Services.AddHttpClient<IVlmOccupationService, GeminiVlmOccupationService>();
builder.Services.AddScoped<IOccupationAnalyseService, OccupationAnalyseService>();
builder.Services.AddScoped<OccupationJobs>();

// ===== Calibration (SCRUM-21 : recuperation image + zones de calibration) =====
builder.Services.AddHttpClient<IImageParkingService, ImageParkingService>();

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddHangfire(config => config
        .UsePostgreSqlStorage(connectionString));
    builder.Services.AddHangfireServer();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("LocalDev");
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ===== Hangfire : dashboard + jobs recurrents =====
if (!string.IsNullOrEmpty(connectionString))
{
    app.UseHangfireDashboard("/hangfire"); // optionnel : interface web pour voir les jobs

    RecurringJob.AddOrUpdate<RappelsJobs>(
        "verifier-renouvellements-abonnements",
        job => job.VerifierRenouvellementsAbonnements(),
        "0 6 * * *");

    RecurringJob.AddOrUpdate<RappelsJobs>(
        "verifier-saturation-parkings",
        job => job.VerifierSaturationParkings(),
        "0 6 * * *");
    RecurringJob.AddOrUpdate<RappelsJobs>(
        "verifier-echeances-paiements",
        job => job.VerifierEcheancesPaiements(),
        "0 6 * * *");
    RecurringJob.AddOrUpdate<RappelsJobs>(
        "verifier-mise-a-jour-infos",
        job => job.VerifierMiseAJourInfos(),
        "0 6 * * *");

    // ===== SCRUM-22 : analyse VLM automatique de tous les parkings =====
    // Toutes les 30 minutes. A ajuster selon le quota VLM disponible (le tier
    // gratuit Gemini a des limites de requetes/minute assez basses) et le
    // besoin metier reel de fraicheur des donnees d'occupation.
    RecurringJob.AddOrUpdate<OccupationJobs>(
        "analyser-occupation-parkings",
        job => job.AnalyserTousLesParkings(),
        "*/30 * * * *");

}

app.Run();