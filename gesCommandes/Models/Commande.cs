namespace gesCommandes.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Commande : AbstractEntity
{
    public double montantCommande { get; set; }

    public Client client { get; set; }
    public int clientId { get; set; }

    public Livreur? livreur { get; set; }
    public int? livreurId { get; set; } // Nullable foreign key

    [NotMapped]
    public ICollection<DetailCommande> detailCommandes { get; set; }

    public Paiement? paiement { get; set; }
   public int? paiementId { get; set; } 
}
