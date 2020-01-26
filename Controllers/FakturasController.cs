using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FakturiSecond;
using iTextSharp.text;
using iTextSharp.text.pdf;
using FakturiSecond.Models;
using System.Globalization;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
using System.IO;
//using EASendMail;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web.Helpers;

namespace FakturiSecond.Controllers
{
    public class FakturasController : Controller
    {
        private MoiFakturiEntities db = new MoiFakturiEntities();
        private Font arial;

        // GET: Fakturas
        public ActionResult Index()
        {
            if (Session["Firm_ID"] != null)
            {
                int firm_id = (int)Session["Firm_ID"];
                var faktura = db.Faktura.Include(f => f.Clients).Include(f => f.Firm).Where(f => f.Firm_ID == firm_id)
                    .OrderByDescending(f => f.Faktura_DatumIzdavanje);
                return View(faktura.ToList());
            }
            return HttpNotFound();

        }

        // GET: Fakturas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faktura faktura = db.Faktura.Find(id);
            if (faktura == null)
            {
                return HttpNotFound();
            }
            var path = Path.Combine(Server.MapPath("~/PDFs/"), faktura.Firm_ID.ToString() + "\\" + faktura.Faktura_ID.ToString() + ".pdf");
            if (System.IO.File.Exists(path))
            {
                byte[] FileBytes = System.IO.File.ReadAllBytes(path);
                return File(FileBytes, "application/pdf");
            }
            return HttpNotFound();
        }

        // GET: Fakturas/Create
        public ActionResult Create()
        {
            var model = new FakturaView();
            using (MoiFakturiEntities entities = new MoiFakturiEntities())
            {
                //ke treba samo != namesto ==
                if (Session["Firm_ID"] != null)
                {
                    int firm_id = (int)Session["Firm_ID"];
                    var query = from p in db.Products
                                where p.Firm_ID == firm_id
                                select p;
                    var dbData = query.ToList();
                    model.Products = GetSelectListItems(dbData);
                    if (Session["articles"] != null)
                    {
                        List<Article> artikli = new List<Article>();
                        artikli = (List<Article>)Session["articles"];
                        model.Articles = artikli;
                    }
                    ViewBag.Client_ID = new SelectList(db.Clients, "Client_ID", "Client_Name");
                    ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name");
                    return View(model);

                }
            }

            return View(model);

        }

        private IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<Products> elements)
        {
            var selectList = new List<SelectListItem>();
            foreach (var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Product_ID.ToString(),
                    Text = element.Product_Name
                });
            }
            return selectList;
        }

        [HttpPost]
        public ActionResult AddProduct()
        {
            var productID = Request.Form["ddlProduct"].ToString();
            if (productID != "")
            {
                int pr_id = Int32.Parse(productID);
                var query = db.Products.Where(p => p.Product_ID == pr_id);
                var item = query.FirstOrDefault();
                Products product = query.FirstOrDefault();
                int quantity = Int32.Parse(Request["quantity"]);
                Article article = new Article();
                article.Product_ID = product.Product_ID;
                article.Product_Name = product.Product_Name;
                article.Quantity = quantity;
                article.Article_Price = (double)product.Product_Price * quantity;
                article.Article_Price_DDV = (double)product.Product_Price_with_DDV * quantity;
                List<Article> artikliToAdd = new List<Article>();
                if (Session["articles"] != null)
                {
                    List<Article> artikliSaved = (List<Article>)Session["articles"];
                    artikliToAdd = artikliSaved;
                }
                artikliToAdd.Add(article);
                Session["articles"] = artikliToAdd;

            }

            return RedirectToAction("Create");

        }


        // POST: Fakturas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Faktura_ID,Firm_ID,Client_ID,Faktura_Status,Faktura_Suma,Faktura_DatumIzdavanje,Faktura_DatumDospevanje,Faktura_TotalDDV,Faktura_Zabeleska")] Faktura faktura)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
                int uniqueId = (int)(zeroDate.Ticks / 10000);
                faktura.Faktura_ID = uniqueId;
                faktura.Faktura_DatumIzdavanje = DateTime.Now;
                if (Session["articles"] != null && Session["Firm_ID"] != null)
                {
                    List<Article> articles = new List<Article>();
                    articles = (List<Article>)Session["articles"];
                    double suma = 0.0, sumaDDV = 0.0;
                    foreach (var item in articles)
                    {
                        suma += item.Article_Price;
                        sumaDDV += item.Article_Price_DDV;
                    }
                    faktura.Faktura_Suma = sumaDDV;
                    faktura.Faktura_TotalDDV = sumaDDV - suma;
                    faktura.Firm_ID = (int)Session["Firm_ID"];
                }
                faktura.Faktura_Status = 1;

                db.Faktura.Add(faktura);
                db.SaveChanges();
                List<Article> articlesForPDF = new List<Article>();
                articlesForPDF = (List<Article>)Session["articles"];
                SaveAsPDF(faktura, articlesForPDF);
                Session["articles"] = null;


                return RedirectToAction("Index");
            }

            ViewBag.Client_ID = new SelectList(db.Clients, "Client_ID", "Client_Name", faktura.Client_ID);
            ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name", faktura.Firm_ID);
            return View(faktura);
        }

        private void SaveAsPDF(Faktura faktura, List<Article> articles)
        {
            var path = Path.Combine(Server.MapPath("~/PDFs/"), faktura.Firm_ID.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Document document = new Document(PageSize.A4);
            //naoganje na firma iklient soodvetno so id
            Firm firm = db.Firm.Find(faktura.Firm_ID);
            Clients client = db.Clients.Find(faktura.Client_ID);
            try
            {
                // step 2:
                // we create a writer that listens to the document
                // and directs a PDF-stream to a file
                path = Path.Combine(Server.MapPath("~/PDFs/"), faktura.Firm_ID.ToString() + "/");
                var pdfWriter = PdfWriter.GetInstance(document, new FileStream(path + String.Format("{0}.pdf", faktura.Faktura_ID), FileMode.Create));

                // step 3: we open the document
                document.Open();



                //dodavanje pozadina
                Image background = Image.GetInstance(AppDomain.CurrentDomain.BaseDirectory + "/images/fakturaTemplate.png");
                background.ScaleAbsolute(PageSize.A4.Width, PageSize.A4.Height);
                background.SetAbsolutePosition(0, 0);
                background.Alignment = Image.UNDERLYING;
                document.Add(background);

                // the pdf content
                PdfContentByte cb = pdfWriter.DirectContent;

                // select the font properties               

                BaseFont baseFontArial = BaseFont.CreateFont(AppDomain.CurrentDomain.BaseDirectory + "Fonts/arial.ttf", "Cp1251", BaseFont.EMBEDDED);
                cb.SetColorFill(BaseColor.DARK_GRAY);
                cb.SetFontAndSize(baseFontArial, 22);
                // write the text in the pdf content
                cb.BeginText();
                // Koordinati na Ime na firmata
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_Name, 50, 700, 0);
                cb.EndText();
                cb.SetFontAndSize(baseFontArial, 16);
                // Koordinati na Telefon na firma
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_PhoneNumber, 50, 677, 0);
                cb.EndText();
                // Koordinati na Email na firma 
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_Email, 50, 663, 0);
                cb.EndText();
                //Koordinati na Adresa na firma
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_Address, 50, 647, 0);
                cb.EndText();
                //Koordinati na Postenski broj, Grad
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, String.Format("{0}, {1}", firm.Firm_CityPostCode, firm.Firm_City), 50, 633, 0);
                cb.EndText();
                //Koordinati na Drzava na firma
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_State, 50, 619, 0);
                cb.EndText();
                //Koordinati na Web-strana na firma
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_WebPage, 50, 605, 0);
                cb.EndText();

                //Detaljiza faktura: desno-gore
                //Koordinati na ID na faktura
                cb.SetFontAndSize(baseFontArial, 20);
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, faktura.Faktura_ID.ToString(), 488, 788, 0);
                cb.EndText();
                //Koordinati na Data Izdavanje
                string tmp1 = faktura.Faktura_DatumIzdavanje.ToString();
                string[] datum1 = tmp1.Split(' ');
                cb.SetFontAndSize(baseFontArial, 16);
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_CENTER, datum1[0], 530, 753, 0);
                cb.EndText();
                //Koordinati na Data Dospevanje   
                string tmp2 = faktura.Faktura_DatumDospevanje.ToString();
                string[] datum2 = tmp2.Split(' ');
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_CENTER, datum2[0], 530, 737, 0);
                cb.EndText();

                //Detalji za klient
                cb.SetFontAndSize(baseFontArial, 22);
                cb.BeginText();
                // Koordinati na Ime na klient
                cb.ShowTextAligned(Element.ALIGN_RIGHT, client.Client_Name, 568, 670, 0);
                cb.EndText();
                cb.SetFontAndSize(baseFontArial, 14);
                // Koordinati na Email na klient
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, client.Client_Email, 568, 651, 0);
                cb.EndText();
                //Koordinati na Adresa na klient
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, client.Client_Address, 568, 636, 0);
                cb.EndText();
                //Koordinati na Postenski broj, Grad na klient
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, String.Format("{0}, {1}", client.Client_CityPostCode, client.Client_City), 568, 622, 0);
                cb.EndText();
                //Koordinati na Drzava na klient
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, client.Client_State, 568, 610, 0);
                cb.EndText();
                //Koordinati na TFN na klient
                if (client.Client_TFN != null)
                {
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_RIGHT, "Даночен број: " + client.Client_TFN, 568, 596, 0);
                    cb.EndText();
                }
                int redenBroj = 1;
                int redenX = 65, redenY = 557;
                int artiklX = 89, artiklY = 557;
                int kolicinaX = 345, kolicinaY = 557;
                int cenaX = 408, cenaY = 557;
                int ddvX = 472, ddvY = 557;
                int iznosX = 532, iznosY = 557;
                //Koordinati na artiklite
                foreach (var article in articles)
                {
                    // Koordinati na Redenbroj
                    redenY -= 26;
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_CENTER, redenBroj.ToString(), redenX, redenY, 0);
                    cb.EndText();
                    redenBroj++;
                    // Koordinati na Artikl
                    artiklY -= 26;
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_LEFT, article.Product_Name, artiklX, artiklY, 0);
                    cb.EndText();
                    //Koordinati na Kolicina
                    kolicinaY -= 26;
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_CENTER, article.Quantity.ToString(), kolicinaX, kolicinaY, 0);
                    cb.EndText();
                    //Koordinati na Cena
                    cenaY -= 26;
                    Products product = db.Products.Find(article.Product_ID);
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_CENTER, product.Product_Price.ToString() + "МКД", cenaX, cenaY, 0);
                    cb.EndText();
                    //Koordinati na DDV
                    ddvY -= 26;
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_CENTER, product.Product_DDV_Percent.ToString() + "%", ddvX, ddvY, 0);
                    cb.EndText();
                    //Koordinati na Iznos
                    iznosY -= 26;
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_CENTER, article.Article_Price.ToString() + "МКД", iznosX, iznosY, 0);
                    cb.EndText();
                }

                //Koordinati za TOTAL sumi
                //Koordinati za Cena bezDDV
                cb.SetFontAndSize(baseFontArial, 16);
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, (faktura.Faktura_Suma - faktura.Faktura_TotalDDV).ToString() + " МКД", 564, 178, 0);
                cb.EndText();
                //Koordinati za DDV
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, faktura.Faktura_TotalDDV.ToString() + " МКД", 564, 158, 0);
                cb.EndText();
                //Koordinati za Iznos so DDV
                cb.SetColorFill(new BaseColor(0, 153, 92));
                cb.SetFontAndSize(baseFontArial, 13);
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, "МКД", 564, 107, 0);
                cb.EndText();
                cb.SetFontAndSize(baseFontArial, 24);
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_RIGHT, faktura.Faktura_Suma.ToString(), 564, 122, 0);
                cb.EndText();

                //Koordinati za Payment del
                cb.SetColorFill(BaseColor.DARK_GRAY);
                cb.SetFontAndSize(baseFontArial, 10);
                //Koordinati za Ziro-smetka
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, "Жиро-сметка #", 45, 130, 0);
                cb.EndText();
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_TransactionAccount, 115, 130, 0);
                cb.EndText();
                //Koordinati za TFN
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, "Даноченброј:", 45, 120, 0);
                cb.EndText();
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_TFN, 115, 120, 0);
                cb.EndText();
                //Koordinati za Banka
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, "Банка:", 45, 110, 0);
                cb.EndText();
                cb.BeginText();
                cb.ShowTextAligned(Element.ALIGN_LEFT, firm.Firm_Bank, 115, 110, 0);
                cb.EndText();

                //Koordinati za Zabeleska
                if (faktura.Faktura_Zabeleska != null)
                {
                    cb.BeginText();
                    cb.ShowTextAligned(Element.ALIGN_LEFT, faktura.Faktura_Zabeleska, 48, 60, 0);
                    cb.EndText();
                }

                //Koordinati za potpisi
                if (firm.Firm_Signature != null)
                {
                    Image signatureFirm = Image.GetInstance(AppDomain.CurrentDomain.BaseDirectory + "/images/FirmSignature/" + firm.Firm_Signature);
                    signatureFirm.ScaleToFit(120f, 120f);
                    signatureFirm.SetAbsolutePosition(255, 5);

                    document.Add(signatureFirm);
                }
                if (client.Client_Signature != null)
                {
                    Image signatureClient = Image.GetInstance(AppDomain.CurrentDomain.BaseDirectory + "/images/ClientSignature/" + client.Client_Signature);
                    signatureClient.ScaleToFit(120f, 120f);
                    signatureClient.SetAbsolutePosition(415, 5);

                    document.Add(signatureClient);
                }



            }
            catch (DocumentException de)
            {
                Console.Error.WriteLine(de.Message);
            }
            catch (IOException ioe)
            {
                Console.Error.WriteLine(ioe.Message);
            }

            // step 5: we close the document
            document.Close();
            SendViaMail(firm, faktura, client);
        }

        private void SendViaMail(Firm firm, Faktura faktura, Clients client)
        {
            var path = Path.Combine(Server.MapPath("~/PDFs/"), faktura.Firm_ID.ToString() + "\\" + faktura.Faktura_ID.ToString() + ".pdf");

            if (ModelState.IsValid)
            {
                string from = "moi.fakturi@gmail.com";
                string to = client.Client_Email;

                using (MailMessage mail = new MailMessage(from, to))
                {
                    try
                    {

                        mail.Subject = String.Format("Фактура број #{0}", faktura.Faktura_ID);
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<p><font size=\"3.5\">Почитувани,</font></p>");                        
                        sb.AppendFormat("<p><font size=\"3.5\">Во прилог на овој меил Ви ја испраќаме фактурата со број <strong>#{0}</strong> креирана од фирмата <i>{1}</i> со датум на издавање: {2} и датум на доспевање: {3}.</font></p>", faktura.Faktura_ID, firm.Firm_Name, faktura.Faktura_DatumIzdavanje, faktura.Faktura_DatumDospevanje);                        
                        sb.AppendFormat("<p><font size=\"3.5\">Истата изнесува:</font><font size=\"5\" color=#228B22><b>{0}  МКД</b></font></p>", faktura.Faktura_Suma);                       
                        sb.Append("<p><font size=\"3.5\">Со почит,<br><b>Мои фактури</b></font></p>");                        
                        mail.Body = sb.ToString();
                        mail.IsBodyHtml = true;
                        mail.BodyEncoding = UTF8Encoding.UTF8;                        

                        SmtpClient SmtpMail = new SmtpClient();
                        SmtpMail.Host = "smtp.gmail.com";//name or IP-Address of Host used for SMTP transactions  
                        SmtpMail.Port = 587;//Port for sending the mail 
                        SmtpMail.EnableSsl = true;
                        SmtpMail.Timeout = 100000;
                        SmtpMail.DeliveryMethod = SmtpDeliveryMethod.Network;
                        SmtpMail.UseDefaultCredentials = true;
                        SmtpMail.Credentials = new NetworkCredential("moi.fakturi@gmail.com", "taki-dimi319");//username/password of network, if apply  

                        mail.Attachments.Add(new Attachment(path));

                        SmtpMail.Send(mail); //Smtpclient to send the mail message                          
                        Response.Write("Email has been sent");
                    }
                    catch (Exception ex)
                    {
                        Response.Write("Failed");
                    }
                }
            }
            else
            {

            }

        }

        // GET: Fakturas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faktura faktura = db.Faktura.Find(id);
            int status = (int)faktura.Faktura_Status;
            if (status == 1)
            {
                faktura.Faktura_Status = 0;
            }
            else
            {
                faktura.Faktura_Status = 1;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
            //if (faktura == null)
            //{
            //    return HttpNotFound();
            //}

            //ViewBag.Client_ID = new SelectList(db.Clients, "Client_ID", "Client_Name", faktura.Client_ID);
            //ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name", faktura.Firm_ID);
            //return View(faktura);
        }

        // POST: Fakturas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Faktura_ID,Firm_ID,Client_ID,Faktura_Status,Faktura_Suma,Faktura_DatumIzdavanje,Faktura_DatumDospevanje,Faktura_TotalDDV,Faktura_Zabeleska")] Faktura faktura)
        {
            if (ModelState.IsValid)
            {
                db.Entry(faktura).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Client_ID = new SelectList(db.Clients, "Client_ID", "Client_Name", faktura.Client_ID);
            ViewBag.Firm_ID = new SelectList(db.Firm, "Firm_ID", "Firm_Name", faktura.Firm_ID);
            return View(faktura);
        }

        // GET: Fakturas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Faktura faktura = db.Faktura.Find(id);
            if (faktura == null)
            {
                return HttpNotFound();
            }
            return View(faktura);
        }

        // POST: Fakturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Faktura faktura = db.Faktura.Find(id);
            db.Faktura.Remove(faktura);
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
