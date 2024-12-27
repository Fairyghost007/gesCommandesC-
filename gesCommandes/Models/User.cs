using System.ComponentModel.DataAnnotations.Schema;
using gesCommandes.Enums;

namespace gesCommandes.Models;
public class User : Personne
{
    public string Login { get; set; }
    public string Password { get; set; }

    public Role Role { get; set; }

    [NotMapped]
    public IFormFile? ImgFile { get; set; }
    public string? ImgPath { get; set; }
}
