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
    public class PenggunaController : Controller
    {
        string connectionString = @"Data Source = PROMETHEUS; Initial Catalog = FutsalDB; Integrated Security=True";
        [HttpGet]
        // GET: Pengguna
        public ActionResult Index()
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Masuk", "Pengguna");
            }
            else if (Session["type"].ToString() != ("Admin"))
            {
                return RedirectToAction("ErrorAuth", "Home");
            }

            //Get Data from database
            DataTable dtblPengguna = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT * FROM Pengguna WHERE JenisAkun = 'Pelanggan'", sqlCon);
                sqlDa.Fill(dtblPengguna);
            }
            return View(dtblPengguna);
        }

        public ActionResult Blokir(int id)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pengguna SET Status = 'Tidak Aktif' WHERE IDAkun = @IDAkun";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDAkun", id);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "Pengguna");
        }

        public ActionResult Unblok(int id)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "UPDATE Pengguna SET Status = 'Aktif' WHERE IDAkun = @IDAkun";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@IDAkun", id);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index", "Pengguna");
        }

        [HttpGet]
        // GET: Pengguna/Create
        public ActionResult Daftar()
        {
            return View(new PenggunaModel());
        }

        // POST: Pengguna/Create
        [HttpPost]
        public ActionResult Daftar(PenggunaModel penggunaModel)
        {
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "INSERT INTO Pengguna(JenisAkun, NamaDepan, NamaBelakang, TanggalLahir, Status, Email, KataSandi)"+
                    "VALUES ('Pelanggan', @NamaDepan, @NamaBelakang, @TanggalLahir, 'Aktif', @Email, HASHBYTES('SHA2_512',@KataSandi))";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.Parameters.AddWithValue("@NamaDepan", penggunaModel.NamaDepan);
                sqlCmd.Parameters.AddWithValue("@NamaBelakang", penggunaModel.NamaBelakang);
                sqlCmd.Parameters.AddWithValue("@TanggalLahir", penggunaModel.TanggalLahir);
                sqlCmd.Parameters.AddWithValue("@Email", penggunaModel.Email);
                sqlCmd.Parameters.AddWithValue("@KataSandi", penggunaModel.KataSandi);
                sqlCmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Masuk()
        {
            //TempData["msg"] = 0;
            return View();
        }

        [HttpPost]
        public ActionResult Masuk(PenggunaModel penggunaModel)
        {
           
            DataTable dtblPengguna = new DataTable();
            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                sqlCon.Open();
                string query = "SELECT * FROM Pengguna Where Email = @Email AND KataSandi=HASHBYTES('SHA2_512',@KataSandi) AND Status = 'Aktif'";
                SqlDataAdapter sqlDa = new SqlDataAdapter(query, sqlCon);
                sqlDa.SelectCommand.Parameters.AddWithValue("@Email", penggunaModel.Email);
                sqlDa.SelectCommand.Parameters.AddWithValue("@KataSandi", penggunaModel.KataSandi);
                sqlDa.Fill(dtblPengguna);
            }
            if (dtblPengguna.Rows.Count == 1)
            {
                Session["id"] = Convert.ToInt32(dtblPengguna.Rows[0][0].ToString());
                Session["name"] = dtblPengguna.Rows[0][2].ToString()+" "+ dtblPengguna.Rows[0][3].ToString();
                Session["type"] = dtblPengguna.Rows[0][1].ToString();
                return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("Masuk");
        }

        [HttpGet]
        public ActionResult Keluar()
        {
            Session.Remove("id");
            Session.Remove("name");
            Session.Remove("type");
            //Session.Remove("ErrorMessage");
            return RedirectToAction("Index", "Home");
        }

        // GET: Pengguna/Delete/5
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
            return View();
        }

        
    }
}
