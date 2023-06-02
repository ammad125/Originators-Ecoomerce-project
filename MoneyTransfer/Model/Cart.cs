using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoneyTransfer.Model
{
    public class Cart
    {
        public int ID { get; set; }
        public int Price { get; set; }
        public string Name { get; set; }
        public string _Product { get; set; }
        public int Quantity { get; set; }
        public int _Rate { get; set; }
        public int cart { get; set; }
    }
}