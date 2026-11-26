using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TblStatus")]
public class TblStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // change to Identity if StatusID is auto-increment
    public int StatusID { get; set; }

    [MaxLength(50)]
    public string Status { get; set; }

    [MaxLength(50)]
    public string Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string DomainID { get; set; } = "001";       // matches the DB default
}