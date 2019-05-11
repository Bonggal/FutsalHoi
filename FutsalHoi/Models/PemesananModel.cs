using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FutsalHoi.Models
{
    public class PemesananModel
    {
        private DateTime _createdOn = DateTime.MinValue;

        public int IDPemesanan { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Tanggal {
            get
            {
                return (_createdOn == DateTime.MinValue) ? DateTime.Now : _createdOn;
            }
            set { _createdOn = value; }
        }
        //public DateTime Tanggal { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        public DateTime JamMulai { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        public DateTime JamSelesai { get; set; }

        public int IDPelanggan { get; set; }

        public int IDLapangan { get; set; }

        public string Status { get; set; }

        public string Bukti { get; set; }

        public string NomorPemesanan { get; set; }

        public DateTime TanggalPemesanan { get; set; }

        public int IDAdmin { get; set; }
    }
}