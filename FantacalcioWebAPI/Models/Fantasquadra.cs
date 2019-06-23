using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Fantasquadra
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Nome { get; set; }

        public Fantallenatore Allenatore { get; set; }

        public List<FantaAcquisto> Giocatori { get; set; }

        public Fantacalcio Fantacalcio { get; set; }

    }
}
