using System.ComponentModel.DataAnnotations.Schema;

namespace gesCommandes.Models
{
    public class Livreur : Personne
    {
        bool isDisponible { get; set; } = true;
        [NotMapped]
        public ICollection<Commande> Commandes { get; set; }
    }
}
