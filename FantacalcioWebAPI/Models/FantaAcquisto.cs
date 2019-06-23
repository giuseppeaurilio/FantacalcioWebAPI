using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class FantaAcquisto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Giocatore Giocatore { get; set; }

        public Fantasquadra Squadra { get; set; }

        public int Crediti { get; set; }

        public DateTime Dal { get; set; }

        public DateTime Al { get; set; }
    }
}
