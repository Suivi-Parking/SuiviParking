namespace Parkini.Application.DTOs;

public class DashboardDto
{
    public int TotalEntreprises { get; set; }
    public int EntreprisesActives { get; set; }
    public int EntreprisesInactives { get; set; }

    public int AbonnementsActifs { get; set; }
    public int AbonnementsExpires { get; set; }

    public int TotalEmployes { get; set; }
    public int TotalVehicules { get; set; }

    public int TotalParkings { get; set; }
    public double TauxOccupationGlobal { get; set; }

    public decimal RevenuTotal { get; set; }
    public decimal RevenuMensuel { get; set; }
    public decimal MontantImpaye { get; set; }
}