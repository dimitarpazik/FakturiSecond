using FakturiSecond.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FakturiSecond.Controllers
{
    public class AccountController : Controller
    {
        private SqlConnection con = new SqlConnection();
        private SqlCommand com = new SqlCommand();
        private MoiFakturiEntities entities = new MoiFakturiEntities();

        

        // GET: Account
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        void connectionString()
        {
            con.ConnectionString = "data source=ASUS\\SQLEXPRESS; database=MoiFakturi; integrated security=SSPI";
        }
        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            var firm = entities.Firm.Where(f => f.Firm_Email == Email && f.Firm_Password == Password);
            var item = firm.FirstOrDefault();
            if (item != null)
            {

                Firm firma = (Firm)item;

                Session["Firm_ID"] = firma.Firm_ID;
                Session["Firm_Name"] = firma.Firm_Name;
                return RedirectToAction("Home", "Dashboard");

            }

            else
            {
                //Ne e najdena kompanijata
                //con.Close();
                return View("Login");
            }
        }
        public ActionResult Register(Account account)
        {
            Firm firm = new Firm();

            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);
            string name = Request["Name"];
            string email = Request["Email"];
            string password = Request["Password"];
            string potvrdiPassword = Request["ConfirmPassword"];
            if (password != potvrdiPassword)
            {
                return View("Login");
            }

            //firm.Firm_ID = uniqueId;
            //firm.Firm_Name = name;
            //firm.Firm_Email = email;
            //firm.Firm_Password = password;          

            connectionString();
            con.Open();
            com.Connection = con;
            
            com.CommandText = "insert into Firm values('" + uniqueId + "','" + name + "','" + email + "','" + password + "','" + "" + "','" + ""+ "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + ""+ "','" + "" + "','" + "" + "','" + "" + "')";
            try
            {
                com.ExecuteNonQuery();
                con.Close();
                Session["Firm_ID"] = uniqueId;
                Session["Firm_Name"] = name;
                return RedirectToAction("Home", "Dashboard");
                
            }
            catch (Exception e)
            {
                return View("Login");
            }
        }
    }
}