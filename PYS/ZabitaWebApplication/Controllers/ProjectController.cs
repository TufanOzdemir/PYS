using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ZabitaWebApplication.Base_Controller;
using EBS.Data;
using EBS.Extensions;
using EBS.Services.Service;

namespace ZabitaWebApplication.Controllers
{
    public class ProjectController : BaseSessionController
    {
        ModelContext db = new ModelContext();
        private UserSessionClass usc = new UserSessionClass();
        private NotificationService nfc = new NotificationService();

        //Projeleri listeler.
        public ActionResult Index()
        {
            string username = Session["username"].ToString();
            var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();
            var list = kullanici.Project1.ToList();
            return View(list);
        }

        //Proje altındaki tüm görevleri listeler.Bir çok sayfa ile etkileşim halindedir ve üzerinde çok sayıda partial view vardır.
        public ActionResult Details(int id, int issueID = 0, int sorguID = 0)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                var proje = kullanici.Project1.FirstOrDefault(i => i.ID == id);
                List<Issue> gorevler = new List<Issue>();
                if (proje == null)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Yetkiniz Bulunmamaktadır!" });
                }

                gorevler = proje.Issue.ToList();

                if (sorguID == 1)
                {
                    gorevler = proje.Issue.Where(i => i.Status.Priority == 3).ToList();
                }
                else if (sorguID == 2)
                {
                    gorevler = proje.Issue.Where(i => i.Status.Priority != 3).ToList();
                }
                else if (sorguID == 3)
                {
                    gorevler = kullanici.Issue.Where(i => i.ProjectID == proje.ID).ToList();
                }

                ViewBag.ProjeAdi = proje.Name;
                ViewBag.ProjeId = proje.ID;
                if (issueID == 0)
                {
                    if (proje.Issue.Count == 0)
                    {
                        ViewBag.gorevGetir = 0;
                    }
                    else
                    {
                        ViewBag.gorevGetir = proje.Issue.First().ID;
                    }
                }
                else
                {
                    ViewBag.gorevGetir = issueID;
                }

                return View(gorevler);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }

        }

        //Proje oluşturur.
        public ActionResult Create()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (!usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Proje_Olusturabilir))
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
                }
                return View();
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        public ActionResult Create(Project model)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                model.CreatedUserID = kullanici.ID;
                db.Project.Add(model);
                model.User1.Add(kullanici);
                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), model.ID.ToString(), " projeyi oluşturdu.");
                return new RedirectResult(Url.Action("Index"));
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Proje günceller.
        public ActionResult Update(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Proje_Guncelleyebilir))
                {

                    var proje = kullanici.Project.SingleOrDefault(i => i.ID == id);
                    if (proje == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için proje yöneticisi olmalısınız!" });
                    }
                    return View(proje);
                }
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        public ActionResult Update(Project model, int id)
        {
            try
            {
                var projem = db.Project.SingleOrDefault(i => i.ID == id);
                string ad = projem.Name;
                projem.Name = model.Name;
                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), id.ToString(), " projeyi güncelledi.");
                foreach (var i in projem.User1)
                {
                    nfc.Create("Bulunduğunuz " + ad + " isimli projeniz" + projem.Name + " olarak değiştirildi!", i.ID);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Projeye eleman ekler. Yeni eleman buradan eklenir.
        public ActionResult ProjectAddEmployee(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Proje_eleman_ekleyebilir))
                {

                    var proje = kullanici.Project1.SingleOrDefault(i => i.ID == id);
                    if (proje == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Projede bulunmuyorsunuz!" });
                    }

                    return View(proje);
                }
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        public ActionResult ProjectAddEmployee(int id, string email)
        {
            try
            {
                var kisi = db.User.SingleOrDefault(i => i.Email == email);
                if (kisi == null)
                {
                    return RedirectToAction("SignUp", "User", new { projeId = id, email = email });
                }
                var projem = db.Project.SingleOrDefault(i => i.ID == id);
                projem.User1.Add(kisi);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Proje ve altındaki tüm görevleri siler
        public ActionResult Delete(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Proje_Silebilir))
                {

                    var proje = kullanici.Project.SingleOrDefault(i => i.ID == id);
                    if (proje == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için proje yöneticisi olmalısınız!" });
                    }
                    return View(proje);
                }
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        public ActionResult DeleteConfirmed(int id)
        {
            var proje = db.Project.SingleOrDefault(i => i.ID == id);

            if (proje == null)
            {
                return View();
            }
            try
            {
                foreach (var i in proje.User1)
                {
                    nfc.Create("Bulunduğunuz " + proje.Name + " isimli projeniz silinmiş olabilir!", i.ID);
                }

                proje.User1.Clear();
                List<Issue> issueList = new List<Issue>();

                foreach (var i in proje.Issue.Where(k => !k.isSubTask).ToList())
                {
                    issueList.Add(i);
                }

                int ctm = issueList.Count;
                for (int i = 0; i < ctm; i++)
                {
                    GorevSil(issueList[i]);
                }

                db.Project.Remove(proje);
                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), id.ToString(), " projeyi sildi.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                int aid = Int32.Parse(Session["userId"].ToString());
                nfc.Create(proje.Name + " isimli proje silme işlemi başarısız oldu.", aid);
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Projeye görev ekler
        public ActionResult ProjectAddIssues(int id)
        {
            return RedirectToAction("Create", "Issue", new { projeId = id, issueId = 0 });
        }

        //Issue Details sayfasına köprü kurar.
        public ActionResult IssueDetails(int id)
        {
            return RedirectToAction("Details", "Issue", new { id = id });
        }

        //Görev silen motor fonksiyondur.
        private void GorevSil(Issue i)
        {
            List<Issue> altgorev = new List<Issue>();
            try
            {
                foreach (var item in i.Issue1)
                {
                    altgorev.Add(item);
                }

                int ctm = i.Issue1.Count;
                for (int k = 0; k < ctm; k++)
                {
                    GorevSil(altgorev[k]);
                }

                int ct = i.Comment.ToList().Count;
                var yeni = i.Comment.ToList();
                for (int k = 0; k < ct; k++)
                {
                    db.Comment.Remove(yeni[k]);
                }

                i.Issue2 = null;
                db.Issue.Remove(i);

                db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }
    }
}