using Microsoft.AspNetCore.Identity;
using SideReport.Domain.Entities;

namespace SideReport.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity 사용자 (Infrastructure 레이어)
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>사용자 이름 (실명)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>가입 시각</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Refresh Token 목록</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>복용약 목록</summary>
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();

    /// <summary>부작용 보고 목록</summary>
    public ICollection<AdverseReport> AdverseReports { get; set; } = new List<AdverseReport>();

    /// <summary>OCR 이미지 목록</summary>
    public ICollection<OcrImage> OcrImages { get; set; } = new List<OcrImage>();
}
