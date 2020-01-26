using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FakturiSecond.Models
{
    public class Article
    {
        public int Article_ID { get; set; }
        public int Product_ID { get; set; }
        public string Product_Name { get; set; }
        public int Quantity { get; set; }
        public double Article_Price { get; set; }
        public double Article_Price_DDV { get; set; }
        public double Article_DDV { get; set; }        
    }
}