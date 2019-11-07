using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using prjShoppingCar.Models;

namespace prjShoppingCar.Controllers
{
    public class HomeController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        // GET: Home
        public ActionResult Index()
        {
            var products = db.tProduct.ToList();
            if(Session["Member"] == null)
            {
                return View("Index", "_layout", products);
            }
            return View("Index", "_layoutMember", products);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string fUserId, string fPwd)
        {
            var member = db.tMember
                         .Where(m => m.fUserId == fUserId && m.fPwd == fPwd).FirstOrDefault();
            if (member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();
            }
            Session["WelCome"] = member.fName + "歡迎光臨";
            Session["Member"] = member;
            return RedirectToAction("Index");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            if(ModelState.IsValid == false)
            {
                return View();
            }
            var member = db.tMember
                         .Where(m => m.fUserId == pMember.fUserId).FirstOrDefault();
            if (member == null)
            {
                db.tMember.Add(pMember);
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號已有人使用，註冊失敗";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        public ActionResult ShoppingCar()
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var orderDetails = db.tOrderDetail.Where(m => m.fUserId == fUserId && m.fIsApproved == "否").ToList();
            return View("ShoppingCar", "_layoutMember", orderDetails);
        }

        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver, string fEmail, string fAddress)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            string guid = Guid.NewGuid().ToString();
            db.tOrder.Add(new tOrder { fOrderGuid = guid, fUserId = fUserId, fReceiver = fReceiver, fEmail = fEmail, fAddress = fAddress, fDate = DateTime.Now });
            var carList = db.tOrderDetail
                          .Where(m => m.fIsApproved == "否" && m.fUserId == fUserId).ToList();
            foreach(var item in carList)
            {
                item.fOrderGuid = guid;
                item.fIsApproved = "是";
            }
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }

        public ActionResult AddCar(string fPId)
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var currentCar = db.tOrderDetail
                             .Where(m => m.fPId == fPId && m.fIsApproved == "否" && m.fUserId == fUserId).FirstOrDefault();
            if (currentCar == null)
            {
                var product = db.tProduct
                              .Where(m => m.fPId == fPId).FirstOrDefault();
                /*tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUserId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = product.fPrice;
                orderDetail.fQty = 1;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);*/
                db.tOrderDetail.Add(new tOrderDetail { fUserId = fUserId, fPId = product.fPId, fName = product.fName, fPrice = product.fPrice, fQty = 1, fIsApproved = "否" });
            }
            else
            {
                currentCar.fQty += 1;
            }
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult DeleteCar(int fId)
        {
            var orderDetail = db.tOrderDetail.Where(m => m.fId == fId).FirstOrDefault();
            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }

        public ActionResult OrderList()
        {
            string fUserId = (Session["Member"] as tMember).fUserId;
            var orders = db.tOrder.Where(m => m.fUserId == fUserId).OrderByDescending(m => m.fDate).ToList();
            return View("OrderList", "_layoutMember", orders);
        }
        
        public ActionResult OrderDetail(string forderguid)
        {
            var orderDetails = db.tOrderDetail.Where(m => m.fOrderGuid == forderguid).ToList();
            return View("OrderDetail", "_layoutMember", orderDetails);
        }
    }
}