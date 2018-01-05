using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ZabitaWebApplication.Base_Controller;
using EBS.Data;
using ZabitaWebApplication.ViewModels;
using EBS.Extensions;

namespace ZabitaWebApplication.Controllers
{
    public class UserController : BaseSessionController
    {
        ModelContext db = new ModelContext();
        UserSessionClass usc = new UserSessionClass();
        NavbarViewModel navbarvm = new NavbarViewModel();

        //Anasayfa dashboard viewmodel bilgisi taşır.
        public ActionResult Index()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);
                if (kullanici.Name == null && kullanici.Surname == null)
                {
                    TempData["msg"] = "<script>alert('Lütfen Profil Bilgilerinizi Güncelleyiniz.');</script>";

                    return RedirectToAction("ProfilGuncelle", "user");
                }
                return View(kullanici);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Kullanıcı kaydıdır. Proje kotrollerinden buraya gelinebilir.
        public ActionResult SignUp(int projeId, string email)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (!usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Uye_Ekleyebilir))
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
                }
                ViewBag.RoleID = new SelectList(db.Role, "ID", "Name");
                return View();
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        public ActionResult SignUp(User model, int projeId, string email)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                var varmi = db.User.Any(i => i.Username == model.Username || i.Email == email);
                if (varmi)
                {
                    return View(model);
                }
                var role = model.RoleID;
                if (kullanici.Role.Auth.Count < db.Role.FirstOrDefault(i => i.ID == role).Auth.Count)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Kendi yetkinizden daha fazla yetkisi olan bir rol ekleyemezsiniz!" });
                }
                model.RegisterDate = DateTime.Now;
                model.Email = email;
                var parola = usc.Encrypt(model.Password.Trim());
                model.Password = parola;
                db.User.Add(model);
                var proje = db.Project.SingleOrDefault(i => i.ID == projeId);
                proje.User1.Add(model);
                db.SaveChanges();
                usc.ChangeActionLog(Session["userId"].ToString(), projeId.ToString(), model.ID.ToString(), " kişiyi projeye ekledi.");
                return RedirectToAction("Index");
            }
            catch
            {
                ViewBag.RoleID = new SelectList(db.Role, "ID", "Name");
                return View(model);
            }
        }

        //Profil bilgilerini döner.
        public ActionResult Profil()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (string.IsNullOrEmpty(kullanici.Name) && string.IsNullOrEmpty(kullanici.Surname))
                {
                    return RedirectToAction("ProfilGuncelle", "User");
                }

                return View(kullanici);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Profil güncellemek için kullanılır.
        public ActionResult ProfilGuncelle()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();
                UserSessionClass usc = new UserSessionClass();
                kullanici.Password = usc.Decrypt(kullanici.Password);
                return View(kullanici);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        public ActionResult ProfilGuncelle(User model)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                kullanici.Name = model.Name;
                kullanici.Surname = model.Surname;
                kullanici.Email = model.Email;
                kullanici.Username = model.Username;
                kullanici.Password = usc.Encrypt(model.Password);

                if (Request.Files.Count > 0)
                {
                    var PicturePath = Request.Files[0];


                    if (PicturePath != null && PicturePath.ContentLength > 0)
                    {

                        var fotoadi = Path.GetFileName(PicturePath.FileName);
                        var fotoyolu = Path.Combine(Server.MapPath("~/Content/ProfilResimler/"), fotoadi);
                        kullanici.PicturePath = fotoadi;


                        PicturePath.SaveAs(fotoyolu);
                    }
                }

                db.SaveChanges();
                Session["userPic"] = kullanici.PicturePath;
                Session["username"] = model.Username;

                usc.CreateActionLog(Session["userId"].ToString(), Session["userId"].ToString(), " profilini güncelledi.");
                return RedirectToAction("Profil", "User");
            }
            catch(Exception ex)
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Herhangi bir kullanıcının bilgilerini döner
        public ActionResult KullaniciDetay(int id)
        {
            try
            {
                var kullanici = db.User.FirstOrDefault(i => i.ID == id);
                return View(kullanici);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Kişi kendi görevlerini listeler.
        public ActionResult Gorevlerim()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                return View(kullanici.Issue.ToList());
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Görev detay sayfasına köprü bağlantısı kurar.
        public ActionResult GorevDetay(int id,int projeID)
        {
            return RedirectToAction("Details", "Project", new { id = projeID, issueID = id});
        }

        //Kişi yetkilerini listeler.
        public ActionResult YetkileriniGoruntule()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                return View(kullanici.Role.Auth.ToList());
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Navbar bildirim ve temel bilgileri taşıyan viewmodel döner.
        public PartialViewResult Navbar()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);
                navbarvm.Notifications = kullanici.Notification.OrderByDescending(i => i.Date).Take(10).ToList();
                navbarvm.user = kullanici;

                return PartialView(navbarvm);
            }
            catch (Exception)
            {
                ViewBag.hataMesaji = "asd";
                return PartialView("BosView");
            }
            
        }

        //Bildirimlerin okundu olmasını sağlar.
        public ActionResult BildirimOku()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                foreach (var i in kullanici.Notification)
                {
                    i.isRead = true;
                }
                db.SaveChanges();
                return Json("1");
            }
            catch (Exception ex)
            {
                return Json("0");
            }
            
        }

    }
}