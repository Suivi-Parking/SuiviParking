namespace Parkini.Application.DTOs;

public class OccupationJourDto
{
	public DateTime Date { get; set; }
	public double TauxOccupation { get; set; }
}

public class OccupationParkingDto
{
	public int ParkingId { get; set; }
	public string ParkingNom { get; set; } = string.Empty;
	public int Capacite { get; set; }
	public List<OccupationJourDto> Historique { get; set; } = new();
}