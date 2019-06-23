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

        public Stagione(DataRow dr)
        {
            this.Id = int.Parse(dr["Id"].ToString());
            this.Descrizione = dr["Descrizione"].ToString();
        }
    }
}
