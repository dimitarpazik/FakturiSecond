using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FakturiSecond.Models
{
    public class FirmDashboard
    {
        public int BrojFakturi { get; set; }
        public int BrojAktivniFakturi { get; set; }
        public int BrojProizvodi { get; set; }
        public int BrojKlienti { get; set; }
        public IEnumerable<Faktura> Fakturi { get; set; }
    }
}