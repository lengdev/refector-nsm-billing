using System.ComponentModel.DataAnnotations;
using System.Web;

namespace NSMSOLUTION.Models
{
    public class ExcelUploadFile
    {
        [Required]
        public HttpPostedFileBase ExcelFile { get; set; }
    }
}