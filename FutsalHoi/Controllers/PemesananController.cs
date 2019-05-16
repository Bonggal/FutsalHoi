using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using FutsalHoi.Models;
using OrderNumberGenerator;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace FutsalHoi.Controllers
{
    public class PemesananController : Controller
    {
        
        string connectionString = @"Data Source = PROMETHEUS; Initial Catalog = FutsalDB; Integrated Security=True";
        [HttpGet]
        // GET: Pemesanan
        public ActionResult Index()
        {
            // Login check
            DataTable dtblPemesanan = new DataTable();
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            //If role is admin
            else if (Session["type"].ToString() == ("Admin"))
            {

                var result = new twoDatatable();

                DataTable request = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE Status = 'Req' order by TanggalPemesanan DESC", sqlCon);
                    sqlDa.Fill(request);
                }

                DataTable accept = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE Status = 'Acc' order by TanggalPemesanan DESC", sqlCon);
                    sqlDa.Fill(accept);
                }

                DataTable cancel = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE Status = 'Can' OR Status = 'Rej' order by TanggalPemesanan DESC", sqlCon);
                    sqlDa.Fill(cancel);
                }

                result.First = request;
                result.Second = accept;
                result.Third = cancel;

            return View(result);
            }

            // If role is pelanggan
            else
            {
                DataTable all = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE tanggal >= GETDATE() AND (Status = 'Acc' OR Status = 'Req') order by Tanggal ASC", sqlCon);
                    sqlDa.Fill(all);
                }
                
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE IDAkun = @idPengguna order by TanggalPemesanan DESC", sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@idPengguna", Convert.ToInt32(Session["id"]));
                    sqlDa.Fill(dtblPemesanan);
                }

                var myObject = new twoDatatable();
                myObject.First = dtblPemesanan;
                myObject.Second = all;

                return View(myObject);
            }
            
        }

        // GET: Pemesanan/Details/5
        [HttpGet]
        public ActionResult Detail(int id)
        {
            DataTable dtblPemesanan = new DataTable();
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            else if (Session["type"].ToString() == ("Admin"))
            {
                //Get Data from database

                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE IDPemesanan=@IDPemesanan", sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@IDPemesanan", id);
                    sqlDa.Fill(dtblPemesanan);
                }   
                return View(dtblPemesanan);
            }
            else
            {

                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM PemesananLapangan WHERE IDPemesanan=@IDPemesanan AND IDAkun = @idPengguna", sqlCon);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@IDPemesanan", id);
                    sqlDa.SelectCommand.Parameters.AddWithValue("@idPengguna", Convert.ToInt32(Session["id"]));
                    sqlDa.Fill(dtblPemesanan);
                }
                return View(dtblPemesanan);
            }
        }

        // GET: Pemesanan/Create
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            // Untuk data pada dropdown field
            DataTable lapangan = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Lapangan", sqlCon);
                sqlDa.Fill(lapangan);

                ViewBag.LapanganList = new SelectList(lapangan.AsDataView(),"IDLapangan", "NamaLapangan");
            }
            
            return View(new PemesananModel());
        }

        // POST: Pemesanan/Create
        [HttpPost]
        public ActionResult Create(PemesananModel pemesananModel)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            NumberGenerator generator = new NumberGenerator();
            string number = generator.GenerateNumber("FT");

            // Validasi => Tanggal Pemakaian Lapangan
            if (pemesananModel.Tanggal < DateTime.Now)
            {
                TempData["Error"] = "Tanggal pemakaian lapangan harus lebih besar dari tanggal sekarang";
                return RedirectToAction("Create", "Pemesanan");
            }

            // Validasi => Jam Pemesanan
            if (pemesananModel.JamSelesai <= pemesananModel.JamMulai ) {
                TempData["Error"] = "Jam selesai harus lebih besar dari jam mulai";
                return RedirectToAction("Create", "Pemesanan");
            }
            
            DataTable exist = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Pemesanan WHERE Tanggal = @Tanggal AND IDLapangan = @Lapangan AND format(JamSelesai, 'hh:mm') > format(@JamMulai, 'hh:mm') AND format(JamMulai, 'hh:mm') < format(@JamSelesai, 'hh:mm') AND (STATUS = 'Req' OR STATUS='Acc')", sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Tanggal", pemesananModel.Tanggal);
                sqlDa.SelectCommand.Parameters.AddWithValue("@JamMulai", pemesananModel.JamMulai);
                sqlDa.SelectCommand.Parameters.AddWithValue("@JamSelesai", pemesananModel.JamSelesai);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Lapangan", pemesananModel.IDLapangan);
                sqlDa.Fill(exist);
            }
            
            // Validasi lapangan telah dipesan
            if(exist.Rows.Count > 0)
            {
                TempData["Error"] = "Lapangan telah dipesan";
                return RedirectToAction("Create", "Pemesanan");
            }

            // Membuat pesanan
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "INSERT INTO Pemesanan(Tanggal, JamMulai, JamSelesai, IDLapangan, IDPelanggan,Status,NomorPemesanan,TanggalPemesanan) "+
                    "VALUES (@Tanggal, @JamMulai,@JamSelesai,@IDLapangan,@IDPelanggan, 'Req', @NomorPemesanan,GETDATE())";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@Tanggal", pemesananModel.Tanggal);
                sqlCmd.Parameters.AddWithValue("@JamMulai", pemesananModel.JamMulai);
                sqlCmd.Parameters.AddWithValue("@JamSelesai", pemesananModel.JamSelesai);
                sqlCmd.Parameters.AddWithValue("@IDLapangan", pemesananModel.IDLapangan);
                sqlCmd.Parameters.AddWithValue("@IDPelanggan", Convert.ToInt32(Session["id"]));
                sqlCmd.Parameters.AddWithValue("@NomorPemesanan", number+Session["id"].ToString());
                sqlCmd.ExecuteNonQuery();
            }
            //Session.Remove("Error");
            return RedirectToAction("Index");
        }

        // GET: Pemesanan/Edit/5
        public ActionResult Edit(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            PemesananModel pemesananModel = new PemesananModel();
            DataTable dtblPemesanan = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT * FROM Pemesanan Where IDPemesanan = @IDPemesanan AND IDPelanggan = @IDPelanggan";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDPemesanan", id);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDPelanggan", Session["id"].ToString());
                sqlDa.Fill(dtblPemesanan);
            }

            if (dtblPemesanan.Rows.Count == 1 && dtblPemesanan.Rows[0]["Bukti"].ToString() == "")
            {
                pemesananModel.IDPemesanan = Convert.ToInt32(dtblPemesanan.Rows[0][0].ToString());
                pemesananModel.IDPelanggan = Convert.ToInt32(dtblPemesanan.Rows[0][5].ToString());
                pemesananModel.Tanggal = Convert.ToDateTime(dtblPemesanan.Rows[0][1].ToString());
                pemesananModel.JamMulai = Convert.ToDateTime(dtblPemesanan.Rows[0][2].ToString());
                pemesananModel.JamSelesai = Convert.ToDateTime(dtblPemesanan.Rows[0][3].ToString());
                pemesananModel.IDLapangan = Convert.ToInt32(dtblPemesanan.Rows[0][4].ToString());

                DataTable lapangan = new DataTable();
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();
                    SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Lapangan", sqlCon);
                    sqlDa.Fill(lapangan);

                    ViewBag.LapanganList = new SelectList(lapangan.AsDataView(), "IDLapangan", "NamaLapangan");
                }
                return View(pemesananModel);
            }
            else
                return RedirectToAction("Detail","Pemesanan", new { @id = id});
        }

        // POST: Pemesanan/Edit/5
        [HttpPost]
        public ActionResult Edit(PemesananModel pemesananModel)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            // Validasi => Tanggal Pemakaian Lapangan
            if (pemesananModel.Tanggal < DateTime.Now)
            {
                TempData["Error"] = "Tanggal pemakaian lapangan harus lebih besar dari tanggal sekarang";
                return RedirectToAction("Edit", "Pemesanan", pemesananModel.IDPemesanan);
            }

            // Validasi => Jam Pemesanan
            if (pemesananModel.JamSelesai <= pemesananModel.JamMulai)
            {
                TempData["Error"] = "Jam selesai harus lebih besar dari jam mulai";
                return RedirectToAction("Edit", "Pemesanan", pemesananModel.IDPemesanan);
            }

            DataTable exist = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Pemesanan WHERE Tanggal = @Tanggal AND IDLapangan = @Lapangan AND format(JamSelesai, 'hh:mm') > format(@JamMulai, 'hh:mm') AND format(JamMulai, 'hh:mm') < format(@JamSelesai, 'hh:mm') AND IDPemesanan != @IDPemesanan AND (STATUS = 'Req' OR STATUS='Acc')", sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Tanggal", pemesananModel.Tanggal);
                sqlDa.SelectCommand.Parameters.AddWithValue("@JamMulai", pemesananModel.JamMulai);
                sqlDa.SelectCommand.Parameters.AddWithValue("@JamSelesai", pemesananModel.JamSelesai);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Lapangan", pemesananModel.IDLapangan);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDPemesanan", pemesananModel.IDPemesanan);
                sqlDa.Fill(exist);
            }

            // Validasi lapangan telah dipesan
            if (exist.Rows.Count > 0)
            {
                TempData["Error"] = "Lapangan telah dipesan";
                return RedirectToAction("Edit", "Pemesanan", pemesananModel.IDPemesanan);
            }

            // Memperbaharui pesanan
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pemesanan SET Tanggal = @Tanggal, JamMulai = @JamMulai, JamSelesai = @JamSelesai , IDLapangan = @IDLapangan, TanggalPemesanan = GETDATE() WHERE IDPemesanan = @IDPemesanan AND IDPelanggan = @IDPelanggan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDPemesanan",pemesananModel.IDPemesanan );
                sqlCmd.Parameters.AddWithValue("@IDPelanggan", Convert.ToInt32(Session["id"]));
                sqlCmd.Parameters.AddWithValue("@Tanggal", pemesananModel.Tanggal);
                sqlCmd.Parameters.AddWithValue("@JamMulai", pemesananModel.JamMulai);
                sqlCmd.Parameters.AddWithValue("@JamSelesai", pemesananModel.JamSelesai);
                sqlCmd.Parameters.AddWithValue("@IDLapangan", pemesananModel.IDLapangan);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Bukti(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            PemesananModel pemesananModel = new PemesananModel();
            DataTable dtblPemesanan = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT * FROM Pemesanan Where IDPemesanan = @IDPemesanan AND IDPelanggan = @IDPelanggan";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDPemesanan", id);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDPelanggan", Session["id"].ToString());
                sqlDa.Fill(dtblPemesanan);
            }
            if (dtblPemesanan.Rows.Count == 1)
            {
                pemesananModel.IDPemesanan = Convert.ToInt32(dtblPemesanan.Rows[0][0].ToString());
                pemesananModel.IDPelanggan = Convert.ToInt32(dtblPemesanan.Rows[0][5].ToString());
                pemesananModel.Bukti = dtblPemesanan.Rows[0][7].ToString();
                TempData["IDPemesanan"] = pemesananModel.IDPemesanan;
                return View(pemesananModel);
            }
            else
                return RedirectToAction("Index");
            //return View(new PemesananModel());
        }

        [HttpPost]
        public ActionResult Bukti(HttpPostedFileBase buktiPembayaran, string IDPemesanan)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }

            int IDPemesanans = Convert.ToInt32(IDPemesanan);
            string filename = IDPemesanan+"-"+Session["id"].ToString()+Path.GetExtension(buktiPembayaran.FileName);
            string path = Path.Combine(Server.MapPath("~/Bukti"), filename);
            buktiPembayaran.SaveAs(path);
            

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pemesanan SET Bukti = @Bukti WHERE IDPemesanan = @IDPemesanan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@Bukti", filename);
                sqlCmd.Parameters.AddWithValue("@IDPemesanan", IDPemesanans);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Detail","Pemesanan", new { @id = IDPemesanans });
        }

        public ActionResult Konfirmasi(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            if(Session["type"].ToString() != "Admin")
            {
                return RedirectToAction("ErrorAuth","Home");
            }
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pemesanan SET Status = 'Acc', IDAdmin = @IDAdmin WHERE IDPemesanan = @IDPemesanan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDPemesanan", id);
                sqlCmd.Parameters.AddWithValue("@IDAdmin", Convert.ToInt32(Session["id"].ToString()));
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Detail","Pemesanan", new { @id = id });
        }

        public ActionResult Tolak(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            if (Session["type"].ToString() != "Admin")
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pemesanan SET Status = 'Rej', IDAdmin = @IDAdmin WHERE IDPemesanan = @IDPemesanan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDPemesanan", id);
                sqlCmd.Parameters.AddWithValue("@IDAdmin", Convert.ToInt32(Session["id"].ToString()));
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Detail", "Pemesanan", new { @id = id });
        }

        public ActionResult Batalkan(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            if (Session["type"].ToString() != "Pelanggan")
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pemesanan SET Status = 'Can' WHERE IDPemesanan = @IDPemesanan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDPemesanan", id);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Detail", "Pemesanan", new { @id = id });
        }

        // Export PDF
        public ActionResult printPDF(int id)
        {
            DataTable data = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT * FROM PemesananLapangan Where IDPemesanan = @IDPemesanan";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDPemesanan", id);
                sqlDa.Fill(data);
            }

            var harga = Convert.ToInt32(Convert.ToInt32(data.Rows[0]["Durasi"]) * Convert.ToInt32(data.Rows[0]["Harga"])) / 60;

            // Create Document
            string filepath = Server.MapPath("~");
            string filename = "Order-" + id + "-" + data.Rows[0]["NomorPemesanan"] + ".pdf";
            Document document = new Document(PageSize.A4, 70, 70, 70, 70);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filepath + "PDF Form Pemesanan\\" + filename, FileMode.Create));
            
            // Fonts
            var titleFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
            var boldTableFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
            var boldTableFontSmall = FontFactory.GetFont("Arial", 8, Font.BOLD);
            var TableFontSmall = FontFactory.GetFont("Arial", 8, Font.NORMAL);
            var bodyFont = FontFactory.GetFont("Arial", 10, Font.NORMAL);
            Rectangle pageSize = writer.PageSize;

            document.Open();

            PdfPTable headertable = new PdfPTable(3);
            headertable.HorizontalAlignment = 0;
            headertable.WidthPercentage = 100;
            headertable.SetWidths(new float[] { 4, 3, 3 });
            headertable.DefaultCell.Border = Rectangle.NO_BORDER;
            headertable.SpacingAfter = 30;

            PdfPTable nested = new PdfPTable(1);
            nested.DefaultCell.Border = Rectangle.BOX;

            PdfPCell nextPostCell1 = new PdfPCell(new Phrase("FutsalHoi Futsal", boldTableFont));
            nextPostCell1.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            nested.AddCell(nextPostCell1);

            PdfPCell nextPostCell2 = new PdfPCell(new Phrase("Jl. Karbela Timur No.1A, Kec. Setiabudi", bodyFont));
            nextPostCell2.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            nested.AddCell(nextPostCell2);

            PdfPCell nextPostCell3 = new PdfPCell(new Phrase("South Jakarta 12920", bodyFont));
            nextPostCell3.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            nested.AddCell(nextPostCell3);

            PdfPCell nextPostCell4 = new PdfPCell(new Phrase("DKI Jakarta", bodyFont));
            nextPostCell4.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            nested.AddCell(nextPostCell4);

            PdfPCell nesthousing = new PdfPCell(nested);
            nesthousing.Rowspan = 4;
            nesthousing.Padding = 0f;
            headertable.AddCell(nesthousing);
            
            headertable.AddCell("");

            PdfPCell invoiceCell = new PdfPCell(new Phrase("BUKTI PEMBAYARAN", titleFont));
            invoiceCell.HorizontalAlignment = 2;
            invoiceCell.Border = Rectangle.NO_BORDER;
            headertable.AddCell(invoiceCell);
            PdfPCell noCell = new PdfPCell(new Phrase("No. Pemesanan :", bodyFont));
            noCell.HorizontalAlignment = 2;
            noCell.Border = Rectangle.NO_BORDER;
            headertable.AddCell(noCell);
            headertable.AddCell(new Phrase(data.Rows[0]["NomorPemesanan"].ToString(), bodyFont));
            PdfPCell dateCell = new PdfPCell(new Phrase("Tanggal Pemesanan :", bodyFont));
            dateCell.HorizontalAlignment = 2;
            dateCell.Border = Rectangle.NO_BORDER;
            headertable.AddCell(dateCell);
            headertable.AddCell(new Phrase(Convert.ToDateTime(data.Rows[0]["TanggalPemesanan"]).ToString("dd MMMM yyyy"), bodyFont));
            PdfPCell billCell = new PdfPCell(new Phrase("Pemesan :", bodyFont));
            billCell.HorizontalAlignment = 2;
            billCell.Border = Rectangle.NO_BORDER;
            headertable.AddCell(billCell);
            headertable.AddCell(new Phrase(data.Rows[0]["NamaDepan"].ToString()+" "+data.Rows[0]["NamaBelakang"].ToString() + "\n" + data.Rows[0]["Email"].ToString(), bodyFont));

            document.Add(headertable);

            // Table content
            PdfPTable itemTable = new PdfPTable(4);
            itemTable.HorizontalAlignment = 0;
            itemTable.WidthPercentage = 100;
            itemTable.SetWidths(new float[] { 25, 20, 35, 20 });  // then set the column's __relative__ widths
            itemTable.SpacingAfter = 40;
            itemTable.DefaultCell.Border = Rectangle.BOX;

            //table header
            PdfPCell cell1 = new PdfPCell(new Phrase("Lapangan", boldTableFontSmall));
            cell1.HorizontalAlignment = 1;
            itemTable.AddCell(cell1);
            PdfPCell cell2 = new PdfPCell(new Phrase("Harga Lapangan", boldTableFontSmall));
            cell2.HorizontalAlignment = 1;
            itemTable.AddCell(cell2);
            PdfPCell cell3 = new PdfPCell(new Phrase("Waktu Pemakaian", boldTableFontSmall));
            cell3.HorizontalAlignment = 1;
            itemTable.AddCell(cell3);
            PdfPCell cell4 = new PdfPCell(new Phrase("Durasi", boldTableFontSmall));
            cell4.HorizontalAlignment = 1;
            itemTable.AddCell(cell4);

            //main information
            PdfPCell numberCell = new PdfPCell(new Phrase(data.Rows[0]["NamaLapangan"].ToString(), TableFontSmall));
            numberCell.HorizontalAlignment = 1;
            //numberCell.PaddingLeft = 10f;
            numberCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            itemTable.AddCell(numberCell);

            PdfPCell descCell = new PdfPCell(new Phrase("Rp "+data.Rows[0]["Harga"].ToString()+",00/jam", TableFontSmall));
            descCell.HorizontalAlignment = 1;
            //descCell.PaddingLeft = 10f;
            descCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            itemTable.AddCell(descCell);

            PdfPCell qtyCell = new PdfPCell(new Phrase(Convert.ToDateTime(data.Rows[0]["Tanggal"]).ToString("dd MMMM yyyy")+
                ", pukul "+ Convert.ToDateTime(data.Rows[0]["JamMulai"]).ToString("HH:mm")+"-"+ Convert.ToDateTime(data.Rows[0]["JamSelesai"]).ToString("HH:mm")+" WIB", TableFontSmall));
            qtyCell.HorizontalAlignment = 1;
            //qtyCell.PaddingLeft = 10f;
            qtyCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            itemTable.AddCell(qtyCell);

            PdfPCell amtCell = new PdfPCell(new Phrase(data.Rows[0]["Durasi"].ToString()+" menit", TableFontSmall));
            amtCell.HorizontalAlignment = 1;
            amtCell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;
            itemTable.AddCell(amtCell);
            


            PdfPCell totalAmtCell1 = new PdfPCell(new Phrase(""));
            totalAmtCell1.Border = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER;
            itemTable.AddCell(totalAmtCell1);
            PdfPCell totalAmtCell2 = new PdfPCell(new Phrase(""));
            totalAmtCell2.Border = Rectangle.TOP_BORDER; //Rectangle.NO_BORDER; //Rectangle.TOP_BORDER;
            itemTable.AddCell(totalAmtCell2);
            PdfPCell totalAmtStrCell = new PdfPCell(new Phrase("Total Harga", boldTableFont));
            totalAmtStrCell.Border = Rectangle.TOP_BORDER;   //Rectangle.NO_BORDER; //Rectangle.TOP_BORDER;
            totalAmtStrCell.HorizontalAlignment = 1;
            itemTable.AddCell(totalAmtStrCell);
            PdfPCell totalAmtCell = new PdfPCell(new Phrase("Rp "+harga+",00", boldTableFont));
            totalAmtCell.HorizontalAlignment = 1;
            itemTable.AddCell(totalAmtCell);

            PdfPCell cell = new PdfPCell(new Phrase("*** Pastikan Anda membawa form ini sebagai bukti pembayaran ***", TableFontSmall));
            cell.Colspan = 4;
            cell.HorizontalAlignment = 1;
            itemTable.AddCell(cell);
            document.Add(itemTable);
            // end of main table info
                        
            //Approved by
            PdfContentByte cb = new PdfContentByte(writer);
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, true);
            cb = writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(bf, 10);
            cb.SetTextMatrix(pageSize.GetLeft(70), 200);
            cb.ShowText("Disetujui Oleh, "+ data.Rows[0]["AdminDepan"].ToString()+" "+ data.Rows[0]["AdminBelakang"].ToString());
            cb.EndText();



            cb = new PdfContentByte(writer);
            bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, true);
            cb = writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(bf, 10);
            cb.SetTextMatrix(pageSize.GetLeft(70), 100);
            cb.ShowText("Terimakasih atas kepercayaan Anda!");
            cb.EndText();

            cb.BeginText();
            cb.SetFontAndSize(bf, 10);
            cb.SetTextMatrix(pageSize.GetLeft(70), 120);
            cb.ShowText("Jika Anda memiliki pertanyaan, hubungi kami melalui (021) 5290 3131 / 5290 3204.");
            cb.EndText();


            writer.CloseStream = true;

            document.Close();
            TempData["status"] = "Berkas pemesanan ("+filename+") telah berhasil disimpan.";
            return RedirectToAction("Detail", "Pemesanan", new { @id = id });
        }

        // End of Controller
        }

}
