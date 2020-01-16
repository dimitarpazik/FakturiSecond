using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FakturiSecond;

namespace FakturiSecond.Controllers
{
    public class FirmsController : Controller
    {
        private MoiFakturiEntities db = new MoiFakturiEntities();

        // GET: Firms
        public ActionResult Index()
        {
            return View(db.Firm.ToList());
        }

        // GET: Firms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Firm firm = db.Firm.Find(id);
            if (firm == null)
            {
                return HttpNotFound();
            }
            return View(firm);
        }

        // GET: Firms/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Firms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Firm_ID,Firm_Name,Firm_Email,Firm_Password,Firm_Address,Firm_City,Firm_State,Firm_PhoneNumber,Firm_WebPage,Firm_TFN,Firm_TransactionAccount,Firm_Bank,Firm_CityPostCode,Firm_Logo,Firm_Signature")] Firm firm)
        {
            if (ModelState.IsValid)
            {
                db.Firm.Add(firm);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(firm);
        }

        // GET: Firms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Firm firm = db.Firm.Find(id);
            if (firm == null)
            {
                return HttpNotFound();
            }
            return View(firm);
        }

        // POST: Firms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Firm_ID,Firm_Name,Firm_Email,Firm_Password,Firm_Address,Firm_City,Firm_State,Firm_PhoneNumber,Firm_WebPage,Firm_TFN,Firm_TransactionAccount,Firm_Bank,Firm_CityPostCode,Firm_Logo,Firm_Signature")] Firm firm, HttpPostedFileBase fileLogo, HttpPostedFileBase fileSignature)
        {
            if (ModelState.IsValid)
            {
                string path = "-1";

                if (fileLogo != null && fileLogo.ContentLength > 0)
                {
                    string extension = Path.GetExtension(fileLogo.FileName);
                    if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                    {
                        try
                        {
                            path = Path.Combine(Server.MapPath("~/images/FirmLogo"), firm.Firm_Name + Path.GetExtension(fileLogo.FileName));
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                            fileLogo.SaveAs(path);
                            firm.Firm_Logo = firm.Firm_Name + Path.GetExtension(fileLogo.FileName);
                        }
                        catch (Exception ex)
                        {
                            path = "-1";
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert('Only jpg,jpeg or png formats areacceptable...!');</script>");
                    }
                }

                if (fileSignature != null && fileSignature.ContentLength > 0)
                {
                    string extension = Path.GetExtension(fileSignature.FileName);
                    if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                    {
                        try
                        {
                            path = Path.Combine(Server.MapPath("~/images/FirmSignature"), firm.Firm_Name + Path.GetExtension(fileSignature.FileName));
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                            fileSignature.SaveAs(path);
                            firm.Firm_Signature = firm.Firm_Name + Path.GetExtension(fileSignature.FileName);
                        }
                        catch (Exception ex)
                        {
                            path = "-1";
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert('Only jpg,jpeg or png formats areacceptable...!');</script>");
                    }
                }

                db.Entry(firm).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = firm.Firm_ID });
            }
            return View(firm);
        }

        // GET: Firms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Firm firm = db.Firm.Find(id);
            if (firm == null)
            {
                return HttpNotFound();
            }
            return View(firm);
        }

        // POST: Firms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Firm firm = db.Firm.Find(id);
            db.Firm.Remove(firm);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
