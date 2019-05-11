using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace FutsalHoi.Models
{
    public class PenggunaModel
    {
        public int IDAkun { get; set; }
        public string JenisAkun { get; set; }
        [DisplayName("Nama Depan")]
        public string NamaDepan { get; set; }
        [DisplayName("Nama Belakang")]
        public string NamaBelakang { get; set; }
        [DisplayName("Tanggal Lahir")]
        public DateTime TanggalLahir { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        [DisplayName("Kata Sandi")]
        public string KataSandi { get; set; }
    }
}