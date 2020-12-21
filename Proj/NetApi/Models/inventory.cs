using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetApi.Models
{
    public partial class inventory
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string InName { get; set; }
        [Column(TypeName = "timestamp")]
        public DateTime? LastEdit { get; set; }
    }
}
