using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Models.SerieA
{
    public class Stagione
    {
        public int Id { get; set; }
        public string Descrizione { get; set; }

        public Stagione(int id, string descrizione)
        {
            this.Id = id;
            this.Descrizione = descrizione;
        }
    }
}
