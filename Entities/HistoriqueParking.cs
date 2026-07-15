namespace Parkini.Entities;

public class HistoriqueParking
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }

    public string Action { get; set; } = string.Empty; // ex: "Archivage"
    public DateTime DateAction { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}
