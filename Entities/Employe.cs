namespace Parkini.Entities;

public class Employe
{
    public int Id { get; set; }
    public int EntrepriseId { get; set; }
    public Entreprise? Entreprise { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
}
