using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MoneyTransfer.Model
{
    public class Data_Class
    {
        public int ID;
        public string Amount;
        public long Trans_ID;

        public Data_Class(int id, string amount, long trans_ID)
        {
            ID = id;
            Amount = amount;
            Trans_ID = trans_ID;
        }
    }
}