using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FakturiSecond;

namespace FakturiSecond.Controllers
{
    public class ProductsController : Controller
    {
        private MoiFakturiEntities db = new MoiFakturiEntities();

        // GET: Products
        public ActionResult Index()
        {
            if (Session["Firm_ID"] != null)
            {
                int firm_id = (int)Session["Firm_ID"];
                var products = db.Products.Include(p => p.Firm).Where(p => p.Firm_ID == firm_id);
                return View(products.ToList());
            }
            return View("~/Views/Account/Error.cshtml");

        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Product_ID,Product_Name,Product_Price,Product_DDV_Percent")] Products products)
        {
            if (ModelState.IsValid)
            {
                var firm_id = Session["Firm_ID"];
                products.Product_Price_with_DDV = products.Product_Price + (products.Product_Price * (products.Product_DDV_Percent / 100.0));
                products.Firm_ID = (int)firm_id;
                db.Products.Add(products);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name", products.Firm_ID);
            return View(products);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name", products.Firm_ID);
            return View(products);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Product_ID,Product_Name,Product_Price,Product_DDV_Percent,Product_Price_with_DDV,Firm_ID")] Products products)
        {
            if (ModelState.IsValid)
            {
                products.Product_Price_with_DDV = products.Product_Price + (products.Product_Price * (products.Product_DDV_Percent / 100.0));
                db.Entry(products).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name", products.Firm_ID);
            return View(products);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Products products = db.Products.Find(id);
            db.Products.Remove(products);
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
