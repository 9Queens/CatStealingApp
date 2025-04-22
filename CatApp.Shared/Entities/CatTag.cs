using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CatApp.Shared.Entities
{

    public class CatTag
    {

        [ForeignKey(nameof(Cat))]
        public int CatId { get; set; }

        [JsonIgnore]
        public virtual Cat Cat { get; set; } = null!;

        [ForeignKey(nameof(Tag))]
        public int TagId { get; set; }

        [JsonIgnore]
        public virtual Tag Tag { get; set; } = null!;
 
    }
}
