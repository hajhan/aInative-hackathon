using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SideReport.Api.Middleware;
using SideReport.Domain.Entities;
using SideReport.Infrastructure;
using SideReport.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ─── Infrastructure (EF Core, Identity, Services) ────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey가 설정되지 않았습니다.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SideReport";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SideReportUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─── 요청 크기 제한 증가 (이미지 업로드 — 최대 10MB) ─────────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// ─── Controllers & Swagger ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SideReport API",
        Version = "v1",
        Description = "약물 부작용 보고 서비스 API"
    });

    // JWT Bearer 인증 헤더 입력 가능하도록 설정
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT 토큰을 입력하세요. 예: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML 주석 포함
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // CORS_ORIGINS 환경 변수로 추가 도메인 주입 가능 (쉼표 구분)
        // 예: https://sidereport.vercel.app,https://sidereport-frontend.up.railway.app
        var extraOrigins = (builder.Configuration["CORS_ORIGINS"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var origins = new[]
        {
            "http://localhost:3000",
            "http://localhost:3100"
        }.Concat(extraOrigins).Distinct().ToArray();

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ─── Auto-migrate on startup ──────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // 마이그레이션 실행 (기존 테이블이 있으면 히스토리만 기록)
    try
    {
        db.Database.Migrate();
        startupLogger.LogInformation("DB 마이그레이션 완료");
    }
    catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // relation already exists
    {
        startupLogger.LogWarning("기존 테이블 감지. 마이그레이션 히스토리를 동기화합니다.");
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260314014437_AddOcrAndDrugEntities', '8.0.11')
            ON CONFLICT DO NOTHING;
            """);
        startupLogger.LogInformation("마이그레이션 히스토리 동기화 완료");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "DB 마이그레이션 중 오류가 발생했습니다.");
    }

    // 마이그레이션 결과와 무관하게 누락된 Sprint 2 테이블/컬럼을 IF NOT EXISTS로 보정
    try
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "KnownSideEffects" (
                "Id" bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL,
                "DrugName" character varying(200) NOT NULL,
                "SymptomName" character varying(200) NOT NULL,
                "FrequencyRank" integer NOT NULL,
                "Source" character varying(50) NOT NULL DEFAULT 'KAERS',
                "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
                CONSTRAINT "PK_KnownSideEffects" PRIMARY KEY ("Id")
            );
            CREATE INDEX IF NOT EXISTS "idx_known_side_effects_drug_name" ON "KnownSideEffects" ("DrugName");

            CREATE TABLE IF NOT EXISTS "DrugCaches" (
                "Id" bigint GENERATED BY DEFAULT AS IDENTITY NOT NULL,
                "DrugName" character varying(200) NOT NULL,
                "OfficialName" character varying(300) NOT NULL,
                "Ingredient" text,
                "Efficacy" text,
                "UsageInfo" text,
                "SideEffects" text,
                "ApiSource" character varying(50) NOT NULL DEFAULT 'mfds',
                "CachedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
                "ExpiresAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DrugCaches" PRIMARY KEY ("Id")
            );
            CREATE INDEX IF NOT EXISTS "idx_drug_caches_drug_name_expires" ON "DrugCaches" ("DrugName", "ExpiresAt");

            ALTER TABLE IF EXISTS "OcrImages" ADD COLUMN IF NOT EXISTS "RawOcrText" text;
            ALTER TABLE IF EXISTS "OcrImages" ADD COLUMN IF NOT EXISTS "ParsedResultJson" text;
            ALTER TABLE IF EXISTS "OcrImages" ADD COLUMN IF NOT EXISTS "Status" character varying(20) NOT NULL DEFAULT 'Pending';
            ALTER TABLE IF EXISTS "OcrImages" ADD COLUMN IF NOT EXISTS "ProcessedAt" timestamp with time zone;
            ALTER TABLE IF EXISTS "OcrImages" ADD COLUMN IF NOT EXISTS "DeleteAfterProcessing" boolean NOT NULL DEFAULT true;
            CREATE INDEX IF NOT EXISTS "idx_ocr_images_user_id" ON "OcrImages" ("UserId");
            """);
        startupLogger.LogInformation("스키마 보정 완료");
        await SeedKaersDataAsync(db, app.Environment, startupLogger);
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "스키마 보정 중 오류가 발생했습니다.");
    }
}

// ─── 정적 파일 (업로드 이미지 서빙) ──────────────────────────────────────────
app.UseStaticFiles();

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SideReport API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ─── KAERS 시드 헬퍼 ─────────────────────────────────────────────────────────
static async Task SeedKaersDataAsync(ApplicationDbContext db, IWebHostEnvironment env, ILogger logger)
{
    if (await db.KnownSideEffects.AnyAsync())
        return;

    // 시드 파일 탐색 (로컬 개발, Docker 환경 모두 지원)
    var searchPaths = new[]
    {
        Path.Combine(env.ContentRootPath, "..", "..", "data", "kaers-seed.json"),
        Path.Combine(AppContext.BaseDirectory, "data", "kaers-seed.json"),
        Path.Combine(env.ContentRootPath, "data", "kaers-seed.json")
    };

    var seedPath = searchPaths.FirstOrDefault(File.Exists);
    if (seedPath == null)
    {
        logger.LogWarning("KAERS 시드 파일을 찾을 수 없습니다.");
        return;
    }

    try
    {
        var json = await File.ReadAllTextAsync(seedPath);
        var items = JsonSerializer.Deserialize<List<KaersSeedItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (items == null || items.Count == 0) return;

        var entities = items.Select(i => new KnownSideEffect
        {
            DrugName = i.DrugName,
            SymptomName = i.SymptomName,
            FrequencyRank = i.FrequencyRank,
            Source = i.Source ?? "KAERS"
        }).ToList();

        db.KnownSideEffects.AddRange(entities);
        await db.SaveChangesAsync();
        logger.LogInformation("KAERS 시드 데이터 {Count}건 적재 완료", entities.Count);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "KAERS 시드 데이터 적재 중 오류");
    }
}

// Make Program accessible for integration tests
public partial class Program { }

/// <summary>KAERS JSON 시드 항목</summary>
internal record KaersSeedItem(string DrugName, string SymptomName, int FrequencyRank, string? Source);
