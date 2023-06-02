using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoneyTransfer.Model
{
    public class Payment_Details
    {
        public int Id { get; set; }
        public string Full_Name { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string address_type { get; set; }
    }
}