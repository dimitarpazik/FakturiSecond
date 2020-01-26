using FakturiSecond.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FakturiSecond.Controllers
{
    public class DashboardController : Controller
    {
        MoiFakturiEntities entities = new MoiFakturiEntities();

        // GET: Dashboard
        public ActionResult Home()
        {
            if (Session["Firm_ID"] != null)
            {
                FirmDashboard model = new FirmDashboard();
                var firm_id = (int)Session["Firm_ID"];
                var firm_name = (string)Session["Firm_Name"];
                var fakturi = entities.Faktura
                    .Where(f => f.Firm_ID == firm_id)
                    .OrderByDescending(f=>f.Faktura_DatumIzdavanje) 
                    .Take(4);
                model.Fakturi = fakturi;
                fakturi = entities.Faktura
                    .Where(f => f.Firm_ID == firm_id);
                model.BrojFakturi = fakturi.Count();
                fakturi = entities.Faktura
                    .Where(f => f.Firm_ID == firm_id && f.Faktura_Status == 1);
                model.BrojAktivniFakturi = fakturi.Count();
                var produkti = entities.Products
                    .Where(p => p.Firm_ID == firm_id);
                model.BrojProizvodi = produkti.Count();
                var clients = entities.Clients;
                model.BrojKlienti = clients.Count();


                Session["Firm_ID"] = firm_id;
                Session["Firm_Name"] = firm_name;
                return View(model);
            }
            else
            {
                return View("~/Views/Account/Error");
            }
        }


        protected override void Dispose(bool disposing)
        {
            entities.Dispose();
            base.Dispose(disposing);
        }
    }
}