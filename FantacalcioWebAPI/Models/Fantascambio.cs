using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Fantascambio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public Fantasquadra Squadra1 { get; set; }

        public List<FantaAcquisto> GiocatoriSquadra1 { get; set; }

        public int CreditiSquadra1 { get; set; }

        public Fantasquadra Squadra2 { get; set; }

        public List<Acquisto> GiocatoriSquadra2 { get; set; }

        public int CreditiSquadra2 { get; set; }

        public DateTime Data { get; set; }
    }
}
