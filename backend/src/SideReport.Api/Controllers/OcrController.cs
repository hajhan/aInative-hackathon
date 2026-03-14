using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideReport.Application.Interfaces;
using SideReport.Application.Ocr.Commands;
using SideReport.Application.Ocr.Results;
using SideReport.Domain.Exceptions;

namespace SideReport.Api.Controllers;

/// <summary>
/// 약봉투 OCR 인식 및 약품 정보 파싱 API
/// </summary>
[ApiController]
[Route("api/ocr")]
[Authorize]
public class OcrController : ControllerBase
{
    private readonly IOcrPipelineService _pipeline;
    private readonly ILogger<OcrController> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/heic"
    };

    public OcrController(IOcrPipelineService pipeline, ILogger<OcrController> logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    /// <summary>
    /// 약봉투 이미지 업로드 → OCR → 약품 파싱 결과 반환
    /// </summary>
    /// <param name="image">이미지 파일 (multipart/form-data)</param>
    /// <param name="deleteAfterProcessing">처리 후 이미지 삭제 여부 (기본: true)</param>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(OcrUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile image,
        [FromForm] bool deleteAfterProcessing = true)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { message = "이미지 파일이 필요합니다." });

        if (!AllowedContentTypes.Contains(image.ContentType))
            return BadRequest(new { message = "지원하지 않는 이미지 형식입니다. (JPEG, PNG, WebP 지원)" });

        if (image.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "이미지 크기는 10MB 이하여야 합니다." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("사용자 인증 정보를 찾을 수 없습니다.");

        var command = new UploadOcrImageCommand
        {
            UserId = userId,
            ImageStream = image.OpenReadStream(),
            FileName = image.FileName,
            ContentType = image.ContentType,
            DeleteAfterProcessing = deleteAfterProcessing
        };

        _logger.LogInformation("OCR 업로드 요청 (UserId: {UserId}, FileName: {FileName})", userId, image.FileName);
        var result = await _pipeline.ProcessAsync(command);
        return Ok(result);
    }

    /// <summary>
    /// 이전 OCR 처리 결과 조회
    /// </summary>
    /// <param name="id">OCR 이미지 레코드 ID</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OcrUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResult(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("사용자 인증 정보를 찾을 수 없습니다.");

        var result = await _pipeline.GetResultAsync(id, userId);
        if (result == null)
            return NotFound(new { message = "OCR 결과를 찾을 수 없습니다." });

        return Ok(result);
    }

    /// <summary>
    /// 편집된 약품 목록을 Medications 테이블에 저장
    /// </summary>
    /// <param name="id">OCR 이미지 레코드 ID</param>
    /// <param name="drugs">저장할 약품 목록</param>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] List<ParsedDrugItem> drugs)
    {
        if (drugs == null || drugs.Count == 0)
            return BadRequest(new { message = "저장할 약품 목록이 없습니다." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("사용자 인증 정보를 찾을 수 없습니다.");

        await _pipeline.ConfirmMedicationsAsync(id, userId, drugs);
        return NoContent();
    }
}
