using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gesCommandes.Models
{
    public class Client :AbstractEntity
    {
        public double Solde { get; set; }
        public string Adresse { get; set; }
        [NotMapped]
        public ICollection<Commande> Commandes { get; set; } = new List<Commande>();
        public User? user  {get;set;}
        public int? userId { get; set; }

    }
}

