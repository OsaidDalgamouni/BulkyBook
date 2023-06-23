using System.ComponentModel.DataAnnotations;

namespace BulkyBookweb.Models
{
    public class CoverType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
