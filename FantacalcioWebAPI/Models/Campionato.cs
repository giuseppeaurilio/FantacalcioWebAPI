using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class Campionato : Competizione
    {
        public List<Squadra> Squadre { get; set; }

        public List<Giornata> Giornate { get; set; }
    }
}
