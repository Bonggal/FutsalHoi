using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using FutsalHoi.Models;

namespace FutsalHoi.Controllers
{
    
    public class LapanganController : Controller
    {
        
        string connectionString = @"Data Source = PROMETHEUS; Initial Catalog = FutsalDB; Integrated Security=True";
        [HttpGet]
        // GET: Lapangan
        public ActionResult Index()
        {
            DataTable dtblLapangan = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Lapangan",sqlCon);
                sqlDa.Fill(dtblLapangan);
            }
                return View(dtblLapangan);
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            LapanganModel lapanganModel = new LapanganModel();
            DataTable dtblLapangan = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT * FROM Lapangan Where IDLapangan = @IDLapangan";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDLapangan", id);
                sqlDa.Fill(dtblLapangan);
            }
            if (dtblLapangan.Rows.Count == 1)
            {
                lapanganModel.IDLapangan = Convert.ToInt32(dtblLapangan.Rows[0][0].ToString());
                lapanganModel.NamaLapangan = dtblLapangan.Rows[0][1].ToString();
                lapanganModel.Harga = Convert.ToInt32(dtblLapangan.Rows[0][2].ToString());
                lapanganModel.Panjang = Convert.ToDouble(dtblLapangan.Rows[0][3].ToString());
                lapanganModel.Lebar = Convert.ToDouble(dtblLapangan.Rows[0][4].ToString());
                lapanganModel.Keterangan = dtblLapangan.Rows[0][5].ToString();
                return View(lapanganModel);
            }
            else
                return RedirectToAction("Index");
        }
        
        [HttpGet]
        // GET: Lapangan/Create
        public ActionResult Create()
        {
            if (Session["id"] == null) {
                return RedirectToAction("Masuk","Pengguna");
            }
            else if (Session["type"].ToString() != ("Admin"))
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            return View(new LapanganModel());
        }

        // POST: Lapangan/Create
        [HttpPost]
        public ActionResult Create(LapanganModel lapanganModel)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            else if (Session["type"].ToString() != ("Admin"))
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "INSERT INTO Lapangan(NamaLapangan, Harga, Panjang, Lebar, Keterangan) VALUES (@NamaLapangan, @Harga,@Panjang,@Lebar,@Keterangan)";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@NamaLapangan", lapanganModel.NamaLapangan);
                sqlCmd.Parameters.AddWithValue("@Harga", lapanganModel.Harga);
                sqlCmd.Parameters.AddWithValue("@Panjang", lapanganModel.Panjang);
                sqlCmd.Parameters.AddWithValue("@Lebar", lapanganModel.Lebar);
                sqlCmd.Parameters.AddWithValue("@Keterangan", lapanganModel.Keterangan);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // GET: Lapangan/Edit/5
        public ActionResult Edit(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            else if (Session["type"].ToString() != ("Admin"))
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            LapanganModel lapanganModel = new LapanganModel();
            DataTable dtblLapangan = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT * FROM Lapangan Where IDLapangan = @IDLapangan";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@IDLapangan", id);
                sqlDa.Fill(dtblLapangan);
            }
            if (dtblLapangan.Rows.Count == 1)
            {
                lapanganModel.IDLapangan = Convert.ToInt32(dtblLapangan.Rows[0][0].ToString());
                lapanganModel.NamaLapangan = dtblLapangan.Rows[0][1].ToString();
                lapanganModel.Harga = Convert.ToInt32(dtblLapangan.Rows[0][2].ToString());
                lapanganModel.Panjang = Convert.ToDouble(dtblLapangan.Rows[0][3].ToString());
                lapanganModel.Lebar = Convert.ToDouble(dtblLapangan.Rows[0][4].ToString());
                lapanganModel.Keterangan = dtblLapangan.Rows[0][5].ToString();
                return View(lapanganModel);
            }
            else
                return RedirectToAction("Index");
        }

        // POST: Lapangan/Edit/5
        [HttpPost]
        public ActionResult Edit(LapanganModel lapanganModel)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            else if (Session["type"].ToString() != ("Admin"))
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Lapangan SET NamaLapangan = @NamaLapangan, Harga = @Harga, Keterangan = @Keterangan, Panjang = @Panjang, Lebar = @Lebar WHERE IDLapangan = @IDLapangan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDLapangan", lapanganModel.IDLapangan);
                sqlCmd.Parameters.AddWithValue("@NamaLapangan", lapanganModel.NamaLapangan);
                sqlCmd.Parameters.AddWithValue("@Harga", lapanganModel.Harga);
                sqlCmd.Parameters.AddWithValue("@Panjang", lapanganModel.Panjang);
                sqlCmd.Parameters.AddWithValue("@Lebar", lapanganModel.Lebar);
                sqlCmd.Parameters.AddWithValue("@Keterangan", lapanganModel.Keterangan);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // GET: Lapangan/Delete/5
        public ActionResult Delete(int id)
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            else if (Session["type"].ToString() != ("Admin"))
            {
                return RedirectToAction("ErrorAuth", "Home");
            }
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "DELETE FROM Lapangan WHERE IDLapangan = @IDLapangan";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDLapangan", id);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
        
    }
}
