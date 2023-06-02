using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoneyTransfer.Model
{
    public class StoreSignUp
    {
        public int id { get; set; }
        public string fullname { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string conf_password { get; set; }
        public string account_no { get; set; }
    }
}