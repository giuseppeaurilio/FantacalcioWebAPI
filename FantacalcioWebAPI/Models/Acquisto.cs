using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Acquisto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Giocatore Giocatore { get; set; }

        public Squadra Squadra { get; set; }

        public DateTime Dal { get; set; }

        public DateTime Al { get; set; }
    }
}
