using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Fantacalcio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Nome { get; set; }

        public List<Fantasquadra> Fantasquadre { get; set; }

        public List<Competizione> Competizioni { get; set; }

    }
}
