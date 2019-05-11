using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FutsalHoi.Models
{
    public class LapanganModel
    {
        public int IDLapangan { get; set; }
        [DisplayName("Nama Lapangan")]
        public string NamaLapangan { get; set; }
        public int Harga { get; set; }
        public double Panjang { get; set; }
        public double Lebar { get; set; }
        public string Keterangan { get; set; }
    }
}