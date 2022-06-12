using System.ComponentModel.DataAnnotations;

namespace TerralinkTest.Models
{
    public class Document
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Body { get; set; }
    }
}
