using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SideReport.Application.Interfaces;
using SideReport.Infrastructure.Identity;
using SideReport.Infrastructure.Persistence;
using SideReport.Infrastructure.Services;

namespace SideReport.Infrastructure;

/// <summary>
/// Infrastructure 레이어 DI 등록 확장 메서드
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Auth Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // ─── OCR 서비스 (환경변수로 Mock/Real 전환) ────────────────────────────
        var useOcrMock = configuration.GetValue<bool>("Ocr:UseMock", defaultValue: true);
        if (useOcrMock)
        {
            services.AddScoped<IOcrService, MockOcrService>();
        }
        else
        {
            services.AddHttpClient<GoogleVisionOcrService>();
            services.AddScoped<IOcrService, GoogleVisionOcrService>();
        }

        // ─── 이미지 저장 서비스 ────────────────────────────────────────────────
        services.AddScoped<IImageStorageService, LocalImageStorageService>();

        // ─── 약품 파서 서비스 ──────────────────────────────────────────────────
        services.AddScoped<IDrugParserService, DrugTextParserService>();

        // ─── 약품 정보 서비스 (환경변수로 Mock/Real 전환) ──────────────────────
        var useDrugInfoMock = configuration.GetValue<bool>("DrugInfo:UseMock", defaultValue: true);
        if (useDrugInfoMock)
        {
            services.AddScoped<IDrugInfoService, MockDrugInfoService>();
        }
        else
        {
            services.AddHttpClient<MfdsDrugInfoService>();
            services.AddScoped<IDrugInfoService, MfdsDrugInfoService>();
        }

        // ─── OCR 파이프라인 오케스트레이터 ────────────────────────────────────
        services.AddScoped<IOcrPipelineService, OcrPipelineService>();

        // 메모리 캐시 (약품 정보 캐싱 등)
        services.AddMemoryCache();

        return services;
    }
}
