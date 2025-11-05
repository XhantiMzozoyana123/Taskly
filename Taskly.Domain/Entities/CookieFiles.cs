using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskly.Domain.Entities
{
    public class CookieFiles 
    {
        [Key]
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public bool Remote { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
