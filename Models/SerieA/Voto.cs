using System;
using System.Collections.Generic;
using System.Text;

namespace Models.SerieA
{
    public class Voto
    {
        public int Id { get; set; }
        public int GiocatoreId { get; set; }
        public int GiornataId { get; set; }

        public double Votazione { get; set; }
        public double Fantavoto { get; set; }
        public int GolFatti { get; set; }
        public int GolSubiti { get; set; }
        public int Autogol { get; set; }
        public int Assist { get; set; }
        public int Ammonizione { get; set; }
        public int Espulsione { get; set; }
        public int RigoriSbagliati { get; set; }
        public int RigoriTrasformati { get; set; }
        public int RigoriParati { get; set; }
        public int RigoriSubiti { get; set; }
    }
}
