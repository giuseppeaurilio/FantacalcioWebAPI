using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Giocatore
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        //[Required(ErrorMessage = "Nome Must be provided")]
        //[StringLength(50, MinimumLength = 2)]
        public string Nome { get; set; }

        //[Required(ErrorMessage = "Cognome Must be provided")]
        //[StringLength(50, MinimumLength = 2)]
        public string Cognome { get; set; }


        public List<VotoGiornata> Voti { get; set; }


        public List<StatisticaAnno> Statistica { get; set; }
    }
}
