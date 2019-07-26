using System;
using System.Collections.Generic;
using System.Text;

namespace Models.SerieA
{
    public class Giornata
    {
        public int Id { get; set; }
        public string Descrizione { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }

        public int StagioneId { get; set; }

        public Giornata(int id, string descrizione, DateTime dataInizio, DateTime dataFine, int stagioneId)
        {
            this.Id = id;
            this.Descrizione = descrizione;
            this.DataInizio = dataInizio;
            this.DataFine = dataFine;
            this.StagioneId = stagioneId;
        }
    }
}
