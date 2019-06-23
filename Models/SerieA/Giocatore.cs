using System;
using System.Collections.Generic;
using System.Text;

namespace Models.SerieA
{
    public class Giocatore
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }

        public string GazzettaId { get; set; }
    }
}
