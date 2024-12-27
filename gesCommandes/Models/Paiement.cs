using gesCommandes.Enums;
namespace gesCommandes.Models
{
    public class Paiement: AbstractEntity
    {
        // public DateTime Date { get; set; }
        public TypePaiement TypePaiement { get; set; }
        public Commande commande { get; set; }
    }
}
