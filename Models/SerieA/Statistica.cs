using System;
using System.Collections.Generic;
using System.Text;

namespace Models.SerieA
{
    public class Statistica
    {
        public int Id { get; set; }
        public int GiocatoreId { get; set; }
        public int StagioneId { get; set; }
        public int Presenze { get; set; }
        public int Giocabili { get; set; }
        public double MediaVoto { get; set; }
        public double Fantamedia { get; set; }
        public int GolFatti { get; set; }
        public int GolSubiti { get; set; }
        public int Autogol { get; set; }
        public int Assist { get; set; }
        public int Ammonizioni { get; set; }
        public int Espulsioni { get; set; }
        public int RigoriSbagliati { get; set; }
        public int RigoriTrasformati { get; set; }
        public int RigoriParati { get; set; }
        public int RigoriSubiti { get; set; }
    }
}
