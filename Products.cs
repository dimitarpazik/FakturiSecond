//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FakturiSecond
{
    using System;
    using System.Collections.Generic;
    
    public partial class Products
    {
        public int Product_ID { get; set; }
        public string Product_Name { get; set; }
        public Nullable<double> Product_Price { get; set; }
        public Nullable<int> Product_DDV_Percent { get; set; }
        public Nullable<double> Product_Price_with_DDV { get; set; }
        public Nullable<int> Firm_ID { get; set; }
    
        public virtual Firm Firm { get; set; }
    }
}
