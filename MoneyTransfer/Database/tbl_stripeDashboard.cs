//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MoneyTransfer.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_stripeDashboard
    {
        public int ID { get; set; }
        public Nullable<long> trans_id { get; set; }
        public string Balnc_ID { get; set; }
        public Nullable<decimal> amount { get; set; }
        public string currency { get; set; }
        public string description { get; set; }
        public string email { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public string username { get; set; }
        public string risk { get; set; }
        public string risk_level { get; set; }
        public string Brand { get; set; }
        public string status { get; set; }
        public string fingerprint { get; set; }
        public Nullable<long> last4 { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
    }
}