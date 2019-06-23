using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Formazione
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public List<Giocatore> Titolari { get; set; }

        public List<Giocatore> Riserve { get; set; }

        public Fantasquadra Fantasquadra { get; set; }

        public Giornata Giornata { get; set; }
    }
}
