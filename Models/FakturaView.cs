using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FakturiSecond.Models
{
    public class FakturaView
    {
        public int Faktura_ID { get; set; }
        public Nullable<int> Firm_ID { get; set; }
        public Nullable<int> Client_ID { get; set; }
        public Nullable<int> Faktura_Status { get; set; }
        public Nullable<double> Faktura_Suma { get; set; }
        public Nullable<System.DateTime> Faktura_DatumIzdavanje { get; set; }
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> Faktura_DatumDospevanje { get; set; }
        public Nullable<double> Faktura_TotalDDV { get; set; }
        public string Faktura_Zabeleska { get; set; }
        public IEnumerable<Article> Articles { get; set; }
        [Display(Name ="Производ")]
        public IEnumerable<SelectListItem> Products { get; set; }

        public virtual Clients Clients { get; set; }        
        public virtual Firm Firm { get; set; }
    }
}