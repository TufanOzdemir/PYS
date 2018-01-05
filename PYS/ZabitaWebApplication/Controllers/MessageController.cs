using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ZabitaWebApplication.Base_Controller;
using EBS.Data;
using EBS.Extensions;
using EBS.Services;

namespace ZabitaWebApplication.Controllers
{
    public class MessageController : BaseSessionController
    {
        private ModelContext db = new ModelContext();
        private UserSessionClass usc = new UserSessionClass();
        private NotificationService nfc = new NotificationService();

        // Mesajlaşılan kişileri listeler.
        public ActionResult Index(int mesajlasilanID = 0)
        {
            List<int> userIdList = new List<int>();

            int userid = Int32.Parse(Session["userId"].ToString());

            var messages = db.Message.Where(i=> i.GetUserID == userid || i.PostUserID == userid);

            foreach (var item in messages)
            {
                userIdList.Add(item.GetUserID);
                userIdList.Add(item.PostUserID);
            }
            userIdList= userIdList.Distinct().ToList();
            userIdList.Remove(userid);

            var allUsers = db.User.ToList();

            List<User> userListVm = new List<User>();
            User pivotUser;
            for (int i = 0; i < userIdList.Count; i++)
            {
                pivotUser = new User();
                pivotUser = allUsers.Where(w => w.ID == userIdList[i]).FirstOrDefault();
                if (pivotUser != null)
                {
                    userListVm.Add(pivotUser);
                }
            }
            if(mesajlasilanID == 0)
            {
                ViewBag.mesajGetir = 0;
            }else
            {
                ViewBag.mesajGetir = mesajlasilanID;
            }
            
            return View(userListVm);
        }

        // Son 20 mesaj bilgisini döndürür.
        public PartialViewResult Details(int? id)
        {
            if (id == 0)
            {
                ViewBag.partialVeri = "Mesajlaşmak için kişi seçin!";
                return PartialView("BosView");
            }
            ViewBag.karsiID = id;
            int userid = Int32.Parse(Session["userId"].ToString());

            var messages = db.Message.Where(i => (i.GetUserID == userid || i.PostUserID == userid) && (i.GetUserID == id || i.PostUserID == id)).OrderBy(i=>i.Date).Take(20).ToList();
            return PartialView(messages);
        }

        //Mesaj oluşturur.
        [HttpPost]
        public ActionResult Create(string yorumyap, string idDonucu)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                Message mesaj = new Message();
                int aid = Int32.Parse(idDonucu);

                mesaj.PostUserID = kullanici.ID;
                mesaj.GetUserID = aid;
                mesaj.MessageContent = yorumyap;
                mesaj.Date = DateTime.Now;

                db.Message.Add(mesaj);
                db.SaveChanges();

                nfc.Create(kullanici.Username + " kişisinden size "+mesaj.MessageContent + " yazdı.",aid);
                return RedirectToAction("Index", new { mesajlasilanID = idDonucu });
            }
            catch
            {
                return RedirectToAction("Index", new { mesajlasilanID = idDonucu });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
