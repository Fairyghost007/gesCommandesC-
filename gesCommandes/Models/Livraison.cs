using System;
using System.ComponentModel.DataAnnotations;

namespace gesCommandes.Models
{
    public class Livraison : AbstractEntity
    {

        public string Adresse { get; set; }
        public Commande Commande { get; set; }
        public int CommandeId { get; set; }

        public Livreur Livreur { get; set; }
        public int LivreurId { get; set; }

    }
}