using MoneyTransfer.Database;
using MoneyTransfer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MoneyTransfer.Controllers
{
    public class ECommerceController : Controller
    {
        MoneyGatewaydbEntities11 dbobj = new MoneyGatewaydbEntities11();
        // GET: ECommerce
        public ActionResult index()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult products()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult household()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult vegetables()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult kitchen()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult short_codes()
        {
            return View();
        }
        public ActionResult drinks()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult pet()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult frozen()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }
        public ActionResult bread()
        {
            if (Session["cart"] != null)
            {
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }

        public string back()
        {
            var y = Session["total"];
            return y.ToString();
        }

        public ActionResult add(Cart e)
        {
            List<Cart> AddSingleEntry = new List<Cart>();
            List<Cart> FilledCart = (List<Cart>)Session["cart"];

            e.Quantity = e.Quantity + 1;
            e.Price = e.Quantity * e._Rate;

            //If record already exists or not
            if (Session["cart"] != null)
            {
                for (int k = 0; k < FilledCart.Count; k++)
                {
                    if (e.Name == FilledCart[k].Name)
                    {
                        e.cart = FilledCart.Count() + 1;
                        FilledCart[k].Quantity = FilledCart[k].Quantity + 1;
                        FilledCart[k].Price = FilledCart[k]._Rate * FilledCart[k].Quantity;
                        var pervious = Session["total"];
                        var currentTotal = Convert.ToInt32(pervious) + Convert.ToInt32(e._Rate.ToString());
                        Session["total"] = currentTotal;
                        return RedirectToAction("bread");
                    }
                }
            }
            //If cart is null
            if (Session["cart"] == null)
            {
                e.cart = 1;
                AddSingleEntry.Add(e);
                Session["cart"] = AddSingleEntry;
                Session["total"] = e._Rate;
            }
            //If cart is not null
            else
            {
                e.cart = FilledCart.Count() + 1;
                FilledCart.Add(e);
                Session["cart"] = FilledCart;
                var pervious = Session["total"];
                var currentTotal = Convert.ToInt32(pervious) + Convert.ToInt32(e._Rate.ToString());
                Session["total"] = currentTotal;
            }

            List<Cart> Extra = (List<Cart>)Session["cart"];
            var p = Session["total"];

            //return RedirectToAction("bread");
            return RedirectToAction("back");
        }

        public ActionResult checkout()
        {
            if (Session["cart"] != null)
            {
                ViewBag.TotalBill = Session["total"];
                return View((List<Cart>)Session["cart"]);
            }
            return View();
        }

        public ActionResult Remove(int id)
        {
            int minus = 0;
            List<Cart> li = (List<Cart>)Session["cart"];
            var total = Session["total"];
            if (id == 1 && li.Count == 1)
            {
                li.Remove(li[0]);
                for (int i = 0; i < li.Count; i++)
                {
                    li[i].ID = i;
                }
                Session["cart"] = li;
                List<Cart> l = (List<Cart>)Session["cart"];
            }
            else
            {
                minus = Convert.ToInt32(li[id].Price);
                li.Remove(li[id]);

                for (int i = 0; i < li.Count; i++)
                {
                    Session["total"] = Convert.ToInt32(total) - minus;
                }

                for (int i = 0; i < li.Count; i++)
                {
                    li[i].ID = i;
                    li[i].cart = i + 1;
                }

                Session["cart"] = li;
                List<Cart> l = (List<Cart>)Session["cart"];
            }
            return RedirectToAction("checkout");
        }
        public ActionResult saveData(List<string> countData, string name_name, string number_name, string email_name, string city_name, string adress_name)
        {
            //----- cart data start -----
            List<Cart> li = (List<Cart>)Session["cart"];
            if (li != null)
            {
                if (countData != null)
                {
                    for (int i = 0; i < li.Count; i++)
                    {
                        li[i].Quantity = Convert.ToInt32(countData[i]);
                        //li[i].Total = li[i]._Rate * li[i].Quantity;
                    }
                }
                Session["cart"] = li;
                List<Cart> ok = (List<Cart>)Session["cart"];
                //----- end

                //----- address data -----
                Payment_Details data = new Payment_Details();
                data.Id = 1;
                data.Full_Name = name_name;
                data.phone_number = number_name;
                data.email = email_name;
                data.city = city_name;
                data.address_type = adress_name;

                List<Payment_Details> data1 = new List<Payment_Details>();
                data1.Add(data);

                Session["address_details"] = data1;
                List<Payment_Details> p_details = (List<Payment_Details>)Session["address_details"];
                //end

                return RedirectToAction("SendMoney", "Home");
            }

            TempData["emptyCart"] = "Your cart can not be empty!";
            return RedirectToAction("checkout");
        }
        public ActionResult abc(List<string> countData)
        {

            return View();
        }
    }
}