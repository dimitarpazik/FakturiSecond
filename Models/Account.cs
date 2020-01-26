using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FakturiSecond.Models
{
    public class Account
    {
        public int Firm_ID { get; set; }
        public string Firm_Name { get; set; }
        public string Firm_Password { get; set; }
        public string Firm_Logo { get; set; }
        public string Firm_Signature { get; set; }
        public string Firm_Address { get; set; }
        public string Firm_City { get; set; }
        public string Firm_CityPostCode { get; set; }
        public string Firm_State { get; set; }
        public string Firm_PhoneNumber { get; set; }
        public string Firm_Email { get; set; }
        public string Firm_WebPage { get; set; }
        public string Firm_TFN { get; set; }
        public string Firm_TransactionAccount { get; set; }
        public string Firm_Bank { get; set; }
    }
}