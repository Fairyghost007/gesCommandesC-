namespace gesCommandes.Models
{
    public class DetailCommande: AbstractEntity
    {
        // public int Id { get; set; }
        public double Prix { get; set; }
        public int qteCommande { get; set; }
        public virtual Commande Commande { get; set; }
        public int commandeId { get; set; }


        public virtual Produit Produit { get; set; }
        public int ProduitId { get; set; }
    }
}
