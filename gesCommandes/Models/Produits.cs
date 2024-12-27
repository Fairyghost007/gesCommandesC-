using System.ComponentModel.DataAnnotations.Schema;

namespace gesCommandes.Models
{
    public class Produit: AbstractEntity
    {
        // public int Id { get; set; }
        public string Libelle { get; set; }
        public double Prix { get; set; }
        public int QteStock { get; set; }
        [NotMapped]
        public IFormFile? ImgFile { get; set; }
        
        public string? ImgPath { get; set; }

    }
}
