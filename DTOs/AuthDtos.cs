namespace Parkini.Application.DTOs;

public record LoginRequest(string Email, string MotDePasse);

public record LoginResponse(string Token, DateTime ExpireLe, string Email, string Role);