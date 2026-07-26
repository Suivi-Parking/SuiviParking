namespace Parkini.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Canal { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public string Destinataire { get; set; } = string.Empty;
    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public int? EntrepriseId { get; set; }
    public string? EntrepriseNom { get; set; }
    public int? AbonnementId { get; set; }
    public int? ParkingId { get; set; }
    public int? AccesId { get; set; }
    public int? PaiementId { get; set; }
    public int? EmployeId { get; set; }
    public int? VehiculeId { get; set; }

    public DateTime DateCreation { get; set; }
    public DateTime? DateEnvoi { get; set; }
    public string? ErreurDetail { get; set; }
}

public class NotificationListeResponseDto
{
    public List<NotificationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}