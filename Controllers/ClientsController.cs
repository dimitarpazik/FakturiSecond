using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class ClientsController : Controller
    {
        private MoiFakturiEntities db = new MoiFakturiEntities();        

        // GET: Clients
        public ActionResult Index()
        {
            return View(db.Clients.ToList());
        }

        // GET: Clients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clients clients = db.Clients.Find(id);
            if (clients == null)
            {
                return HttpNotFound();
            }
            return View(clients);
        }

        // GET: Clients/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Client_ID,Client_Name,Client_Email,Client_Address,Client_CityPostCode,Client_City,Client_State,Client_TFN,Client_Signature")] Clients clients, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {                
                string path = "-1";                
                if(file!=null && file.ContentLength > 0)
                {
                    string extension = Path.GetExtension(file.FileName);
                    if(extension.ToLower().Equals(".jpg")|| extension.ToLower().Equals(".jpeg")|| extension.ToLower().Equals(".png"))
                    {
                        try
                        {
                            path = Path.Combine(Server.MapPath("~/images/ClientSignature"),clients.Client_Name+Path.GetExtension(file.FileName));
                            file.SaveAs(path);
                            clients.Client_Signature = clients.Client_Name + Path.GetExtension(file.FileName);
                        }
                        catch(Exception ex)
                        {
                            path = "-1";
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert('Only jpg,jpeg or png formats areacceptable...!');</script>");
                    }
                }              

                var now = DateTime.Now;
                var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
                int uniqueId = (int)(zeroDate.Ticks / 10000);
                clients.Client_ID = uniqueId;
                db.Clients.Add(clients);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(clients);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clients clients = db.Clients.Find(id);
            if (clients == null)
            {
                return HttpNotFound();
            }
            return View(clients);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Client_ID,Client_Name,Client_Email,Client_TFN,Client_Address,Client_City,Client_State,Client_CityPostCode,Client_Signature")] Clients clients, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                
                string path = "-1";
               
                if (file != null && file.ContentLength > 0)
                {
                    string extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                    {
                        try
                        {                            
                            path = Path.Combine(Server.MapPath("~/images/ClientSignature"), clients.Client_Name + Path.GetExtension(file.FileName));
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }                            
                            file.SaveAs(path);
                            clients.Client_Signature = clients.Client_Name + Path.GetExtension(file.FileName);
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
                db.Entry(clients).State = EntityState.Modified;
                db.SaveChanges();
                ModelState.Clear();
                return RedirectToAction("Index");
            }
            return View(clients);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Clients clients = db.Clients.Find(id);
            if (clients == null)
            {
                return HttpNotFound();
            }
            return View(clients);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Clients clients = db.Clients.Find(id);
            db.Clients.Remove(clients);
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
