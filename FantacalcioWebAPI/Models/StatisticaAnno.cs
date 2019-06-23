using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class StatisticaAnno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int Anno { get; set; }

        [ForeignKey("GiocatoreForeignKey")]
        public SquadraArchivio Squadra { get; set; }

        [ForeignKey("GiocatoreForeignKey")]
        public Giocatore Giocatore { get; set; }

    }
}
