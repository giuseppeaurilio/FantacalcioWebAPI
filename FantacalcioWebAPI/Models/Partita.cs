using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Partita
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Squadra Squadra1 { get; set; }

        public Squadra Squadra2 { get; set; }

        public string Risultato { get; set; }

        public Giornata Giornata { get; set; }
    }
}
