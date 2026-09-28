using FleetFlow.Api.Data;
using FleetFlow.Api.DTOs;
using FleetFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
}

public class AuthService : IAuthService
{
    private readonly FleetFlowDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;

    public AuthService(FleetFlowDbContext context, IConfiguration configuration, ITokenService tokenService)
    {
        _context = context;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Email and password are required.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email.Trim()))
        {
            throw new InvalidOperationException("User already exists.");
        }

        var company = new Company
        {
            Name = request.CompanyName,
            SubscriptionStatus = "Trial",
            SubscriptionPlan = "Starter"
        };

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        var user = new User
        {
            CompanyId = company.CompanyId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(
            user,
            _configuration["JwtSettings:Issuer"]!,
            _configuration["JwtSettings:Audience"]!,
            _configuration["JwtSettings:Secret"]!,
            int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60"));

        var refreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7"))
        });

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
            User = new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                CompanyName = company.Name
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim());

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _tokenService.GenerateAccessToken(
            user,
            _configuration["JwtSettings:Issuer"]!,
            _configuration["JwtSettings:Audience"]!,
            _configuration["JwtSettings:Secret"]!,
            int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60"));

        var refreshToken = _tokenService.GenerateRefreshToken();
        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7"))
        });

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
            User = new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                CompanyName = user.Company?.Name ?? ""
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var stored = await _context.RefreshTokens
            .Include(r => r.User)
            .ThenInclude(u => u.Company)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

        if (stored is null || stored.User is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var accessToken = _tokenService.GenerateAccessToken(
            stored.User,
            _configuration["JwtSettings:Issuer"]!,
            _configuration["JwtSettings:Audience"]!,
            _configuration["JwtSettings:Secret"]!,
            int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60"));

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = request.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
            User = new UserDto
            {
                UserId = stored.User.UserId,
                FirstName = stored.User.FirstName,
                LastName = stored.User.LastName,
                Email = stored.User.Email,
                Role = stored.User.Role,
                CompanyName = stored.User.Company?.Name ?? ""
            }
        };
    }
}

