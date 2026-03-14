using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SideReport.Application.Auth.Commands;
using SideReport.Application.Auth.Results;
using SideReport.Application.Interfaces;
using SideReport.Domain.Entities;
using SideReport.Domain.Exceptions;
using SideReport.Infrastructure.Identity;
using SideReport.Infrastructure.Persistence;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// 인증 서비스 구현체
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly int _refreshTokenExpiryDays;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _refreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 7;
    }

    /// <inheritdoc />
    public async Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        // 이메일 중복 확인
        var existing = await _userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            throw new ValidationException("이미 사용 중인 이메일입니다.");

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            Name = command.Name,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new ValidationException(errors);
        }

        return new RegisterResult
        {
            UserId = user.Id,
            Email = user.Email!,
            Name = user.Name
        };
    }

    /// <inheritdoc />
    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            throw new UnauthorizedException("이메일 또는 비밀번호가 올바르지 않습니다.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!isPasswordValid)
            throw new UnauthorizedException("이메일 또는 비밀번호가 올바르지 않습니다.");

        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.Name);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        // Refresh Token DB 저장
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            Expires = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var mins) ? mins * 60 : 900
        };
    }

    /// <inheritdoc />
    public async Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedException("유효하지 않거나 만료된 Refresh Token입니다.");

        var tokenUser = await _userManager.FindByIdAsync(storedToken.UserId);
        if (tokenUser is null)
            throw new UnauthorizedException("사용자를 찾을 수 없습니다.");

        // 기존 토큰 폐기 + 새 토큰 발급을 트랜잭션으로 묶어 부분 실패 방지
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        storedToken.IsRevoked = true;

        var newAccessToken = _jwtTokenService.GenerateAccessToken(tokenUser.Id, tokenUser.Email ?? string.Empty, tokenUser.Name);
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshTokenValue,
            Expires = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RefreshTokenResult
        {
            AccessToken = newAccessToken,
            ExpiresIn = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var mins) ? mins * 60 : 900
        };
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (storedToken is not null && !storedToken.IsRevoked)
        {
            storedToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
