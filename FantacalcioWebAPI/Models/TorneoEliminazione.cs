using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Models
{
    public class TorneoEliminazione : Competizione
    {
        public List<Giornata> Giornate { get; set; }
    }
}
