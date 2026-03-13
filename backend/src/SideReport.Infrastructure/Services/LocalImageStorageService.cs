using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SideReport.Application.Interfaces;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// 로컬 파일시스템 이미지 저장 서비스 (개발/해커톤용)
/// </summary>
public class LocalImageStorageService : IImageStorageService
{
    private readonly string _uploadBasePath;
    private readonly ILogger<LocalImageStorageService> _logger;

    public LocalImageStorageService(
        IWebHostEnvironment env,
        ILogger<LocalImageStorageService> logger)
    {
        _uploadBasePath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads", "ocr");
        Directory.CreateDirectory(_uploadBasePath);
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream imageStream, string fileName)
    {
        // 안전한 파일명 생성 (경로 탐색 방지)
        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_uploadBasePath, safeFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await imageStream.CopyToAsync(fileStream);

        _logger.LogInformation("이미지 저장 완료: {Path}", fullPath);
        return fullPath;
    }

    public Task DeleteAsync(string storagePath)
    {
        try
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
                _logger.LogInformation("이미지 삭제 완료: {Path}", storagePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "이미지 삭제 실패 (무시): {Path}", storagePath);
        }
        return Task.CompletedTask;
    }
}
