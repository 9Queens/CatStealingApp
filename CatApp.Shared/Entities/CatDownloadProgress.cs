using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatApp.Shared.Entities
{
    public class CatDownloadProgress
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int TotalCats { get; set; }
 
        public int CatsDownloaded { get; set; }

        public int DoublicatesOccured { get; set; }

        public int ErrorsOccured { get; set; }

        [Required]
        public DateTimeOffset StartedOn{ get; set; } = DateTime.UtcNow;
 
        public DateTimeOffset? CompletedOn { get; set; }

        public int? BatchFailures { get; set; }
        public Status? Status { get; set; }

        public string? Messages { get; set; }
    }
}
