using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using ZabitaWebApplication.Base_Controller;
using EBS.Data;
using ZabitaWebApplication.ViewModels;
using EBS.Extensions;
using EBS.Services.Service;

namespace ZabitaWebApplication.Controllers
{
    public class IssueController : BaseSessionController
    {
        private ModelContext db = new ModelContext();
        private UserSessionClass usc = new UserSessionClass();
        private NotificationService nfc = new NotificationService();

        //Görevi detaylıca verir.
        public PartialViewResult Details(int? id)
        {
            if (id == 0)
            {
                ViewBag.partialVeri = "Görev Bulunmamaktadır!";
                return PartialView("BosView");
            }
            IssueDetailViewModel vm = new IssueDetailViewModel();
            vm.issue = db.Issue.Find(id);
            vm.status = db.Status.ToList();
            return PartialView(vm);
        }

        //Alt görev oluştururken kullanılır.
        public ActionResult Create(int projeId, int issueId)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Gorev_Olusturabilir))
                {

                    var proje = kullanici.Project1.SingleOrDefault(i => i.ID == projeId);
                    if (proje == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
                    }
                    ViewBag.PriorityID = new SelectList(db.Priority, "ID", "Name");
                    return PartialView("CreateModal");
                }
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Hata!" });
            }
        }

        [HttpPost]
        public ActionResult Create(int projeId, int issueId, Issue issue)
        {
            try
            {
                issue.isSubTask = true;
                issue.Issue2.Add(db.Issue.FirstOrDefault(i => i.ID == issueId));
                int userid = Int32.Parse(Session["userId"].ToString());
                issue.ProjectID = projeId;
                issue.CreatedDate = DateTime.Now;
                issue.StatusID = db.Status.First().ID;
                db.Issue.Add(issue);
                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), issueId.ToString(), "issue ekledi");
                return RedirectToAction("Details", "Project", new { id = projeId, issueId = issue.ID });
            }
            catch
            {
                ViewBag.PriorityID = new SelectList(db.Priority, "ID", "Name", issue.PriorityID);
                return View(issue);
            }
        }

        //Ana görev oluştururken kullanılır.
        public PartialViewResult CreateModal(int projeId, int issueId = 0)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Gorev_Olusturabilir))
                {

                    var proje = kullanici.Project1.SingleOrDefault(i => i.ID == projeId);
                    if (proje == null)
                    {
                        ViewBag.partialVeri = "Bulunmadığınız projeye görev ekleyemezsiniz!";
                        return PartialView("BosView");
                    }
                    ViewBag.PriorityID = new SelectList(db.Priority, "ID", "Name");
                    return PartialView();
                }
                ViewBag.partialVeri = "Yetkiniz Bulunmamaktadır!";
                return PartialView("BosView");
            }
            catch
            {
                ViewBag.partialVeri = "Hata Oldu!";
                return PartialView("BosView");
            }
        }

        [HttpPost]
        public void CreateModal(int projeId, int issueId, Issue issue)
        {
            if (issueId != 0)
            {
                try
                {
                    issue.isSubTask = true;
                    issue.Issue2.Add(db.Issue.FirstOrDefault(i => i.ID == issueId));
                    int userid = Int32.Parse(Session["userId"].ToString());
                    issue.ProjectID = projeId;
                    issue.CreatedDate = DateTime.Now;
                    issue.StatusID = db.Status.First().ID;
                    db.Issue.Add(issue);
                    db.SaveChanges();
                    usc.CreateActionLog(Session["userId"].ToString(), issueId.ToString(), "issue ekledi");
                }
                catch
                {
                    return;
                }
            }
            else
            {
                try
                {
                    issue.isSubTask = false;
                    int userid = Int32.Parse(Session["userId"].ToString());
                    issue.ProjectID = projeId;
                    issue.CreatedDate = DateTime.Now;
                    issue.StatusID = db.Status.First().ID;
                    db.Issue.Add(issue);
                    db.SaveChanges();
                    usc.CreateActionLog(Session["userId"].ToString(), issue.Name, " görevini oluşturdu. ");
                    HttpContext.Response.Redirect("?id=" + projeId + "&issueID=" + issue.ID);
                }
                catch
                {
                    return;
                }
            }
        }

        //Görev güncellemek için kullanılır.
        public ActionResult Edit(int id, int projeId)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Gorev_Guncelleyebilir))
                {
                    /*var sorgu = (from gorevler in db.Issues join projeler in db.Projects on gorevler.ProjectID equals projeler.ID where gorevler.ProjectID==projeId select new { gorevler,projeler }).ToList();

                    var x = (from q in sorgu where q.projeler.Users.Contains(kullanici)select new {q}).ToList();*/
                    var proje = kullanici.Project1.SingleOrDefault(i => i.ID == projeId);
                    if (proje == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Bulunmadığınız projeye görev ekleyemezsiniz!" });
                    }
                    var gorev = proje.Issue.SingleOrDefault(i => i.ID == id);
                    if (gorev == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Bulunmadığınız projeye görev ekleyemezsiniz!" });
                    }
                    ViewBag.PriorityID = new SelectList(db.Priority, "ID", "Name");
                    ViewBag.StatusID = new SelectList(db.Status, "ID", "Name");
                    return View(gorev);
                }
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, int projeId, Issue model)
        {
            try
            {
                var gorev = db.Issue.SingleOrDefault(i => i.ID == id);
                gorev.Name = model.Name;
                gorev.Description = model.Description;
                gorev.PriorityID = model.PriorityID;
                gorev.StatusID = model.StatusID;
                db.SaveChanges();
                usc.UpdateActionLog(Session["userId"].ToString(), gorev.ID.ToString(), " görevi güncelledi ");
                return RedirectToAction("Details", "Product", new { id = projeId, issueID = id });
            }
            catch
            {
                ViewBag.PriorityID = new SelectList(db.Priority, "ID", "Name", model.PriorityID);
                ViewBag.Status = new SelectList(db.Status, "ID", "Name");
                return View(model);
            }

        }

        //Yorum oluşturmak için kullanılır.
        [HttpPost]
        public ActionResult YorumYap(string yorumyap, string idDonucu)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);
                int id = Int32.Parse(idDonucu);

                Comment yorum = new Comment();
                var gorev = db.Issue.FirstOrDefault(i => i.ID == id);

                yorum.UserID = kullanici.ID;
                yorum.IssueID = gorev.ID;
                yorum.Description = yorumyap;
                yorum.Date = DateTime.Now;

                db.Comment.Add(yorum);
                db.SaveChanges();

                usc.CreateActionLog(kullanici.ID.ToString(), gorev.ID.ToString(), "göreve yorum yaptı. ");
                return RedirectToAction("Details", "Project", new { id = gorev.ProjectID, issueID = idDonucu });
            }
            catch
            {
                return RedirectToAction("Detay", new { id = idDonucu });
            }

        }

        //Yorum güncellemek için kullanılır.
        public ActionResult YorumGuncelle(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                var yorum = kullanici.Comment.FirstOrDefault(i => i.ID == id);

                if (yorum == null)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Yetkisiz istek!" });
                }
                return View(yorum);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost]
        public ActionResult YorumGuncelle(int id, Comment model)
        {
            try
            {
                var yorum = db.Comment.FirstOrDefault(i => i.ID == id);
                yorum.Description = model.Description;
                db.SaveChanges();
                usc.UpdateActionLog(Session["userId"].ToString(), yorum.ID.ToString(), " yorumunu güncelledi.");
                return RedirectToAction("Details", "Project", new { id = yorum.Issue.ProjectID, issueID = yorum.IssueID });
            }
            catch
            {
                return View(model);
            }
        }

        //Görev statü değerlerini değiştiren fonksiyondur.
        public ActionResult GorevDurumDegis(int id, int statusID)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                var gorev = kullanici.Issue.FirstOrDefault(i => i.ID == id);

                if (gorev == null)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Yetkisiz istek!" });
                }

                gorev.StatusID = statusID;
                db.SaveChanges();
                usc.ChangeActionLog(kullanici.ID.ToString(), gorev.ID.ToString(), gorev.StatusID.ToString(), "görevin statü id numarasını değiştirdi ");
                nfc.Create(gorev.Project.Name + " isimli projenizin " + gorev.Name + " isimli görevinin durumu " + gorev.Status.Name + " olarak değiştirildi!", gorev.Project.CreatedUserID);
                return RedirectToAction("Details", "Project", new { id = gorev.ProjectID, issueID = gorev.ID });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Alt görevin detay bilgisini veren fonksiyondur.
        public ActionResult AltGorevGetir(int id, int issueId)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                return RedirectToAction("Details", "Project", new { id = id, issueID = issueId });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Kişinin yorumlarını tersine listeler.
        public ActionResult KisiYorumList()
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                var yorum = kullanici.Comment.ToList();

                if (yorum.Count == 0)
                {
                    ViewBag.yorumvarmi = "Yorum Bulunmamaktadır!";
                }
                else
                {
                    yorum.Reverse();
                }

                return View(yorum);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Yorum silme işlemini yapar.
        public ActionResult YorumSil(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                var yorum = kullanici.Comment.FirstOrDefault(i => i.ID == id);

                if (yorum == null)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Yetkisiz istek!" });
                }

                int gorevId = yorum.IssueID;
                int projeId = yorum.Issue.ProjectID;
                db.Comment.Remove(yorum);
                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), id.ToString(), "yorumu sildi.");
                return RedirectToAction("Details", "Project", new { id = projeId, issueID = gorevId });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Görevi üstlenirken kullanılır.
        public ActionResult GorevTalipOl(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                var proje = kullanici.Project1.FirstOrDefault(i => i.Issue.FirstOrDefault(k => k.ID == id) != null);

                var gorev = db.Issue.FirstOrDefault(i => i.ID == id);
                gorev.ReporterID = kullanici.ID;

                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), gorev.ID.ToString(), "  görevi üstlendi. ");
                nfc.Create(kullanici.Username + " kullanıcısı " + gorev.Name + " isimli göreve talip oldu!", proje.CreatedUserID);
                return RedirectToAction("Details", "Project", new { id = gorev.ProjectID, issueID = id });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Görev iptal ederken kullanılır.
        public ActionResult GorevIptalEt(int id)
        {
            try
            {
                int kisiId = Int32.Parse(Session["userId"].ToString());

                var gorev = db.Issue.FirstOrDefault(i => i.ID == id);

                if (gorev == null)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Sen Hayır mı?" });
                }

                if (gorev.ReporterID != kisiId)
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Yetkin Bulunmuyor!" });
                }

                gorev.ReporterID = null;
                db.SaveChanges();
                usc.CreateActionLog(Session["userId"].ToString(), gorev.ID.ToString(), " görevini iptal etti. ");
                nfc.Create(Session["username"].ToString() + " kullanıcısı " + gorev.Name + " isimli görevi iptal etti!", gorev.Project.CreatedUserID);
                return RedirectToAction("Details", "Project", new { id = gorev.ProjectID, issueID = id });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //Görevi bir başkasına atarken kullanılır.Görev atama yetkisi şartı aranır.
        public PartialViewResult GorevAta(int id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                if (!usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Gorev_Atayabilir))
                {
                    ViewBag.partialVeri = "Yetkisiz İşlem";
                    return PartialView("BosView");
                }

                var gorev = db.Issue.FirstOrDefault(i => i.ID == id);

                if (gorev == null)
                {
                    ViewBag.partialVeri = "Görev Bulunamadı!";
                    return PartialView("BosView");
                }

                var proje = gorev.Project;

                if (proje.User1.FirstOrDefault(i => i.ID == kullanici.ID) == null)
                {
                    ViewBag.partialVeri = "Kullanıcı Bulunamadı";
                    return PartialView("BosView");
                }

                ViewBag.gorevID = gorev.ID;
                return PartialView(proje.User1);
            }
            catch
            {
                //return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
                return PartialView("BosView");
            }
        }

        public ActionResult GorevAtaConfirmed(int id, int issueId)
        {
            try
            {
                var gorev = db.Issue.FirstOrDefault(i => i.ID == issueId);
                gorev.ReporterID = id;
                db.SaveChanges();
                usc.ChangeActionLog(Session["userId"].ToString(), id.ToString(), gorev.ID.ToString(), " görev atadı. ");
                nfc.Create(gorev.Name + " isimli görev size verildi!", id);
                return RedirectToAction("Details", "Project", new { id = gorev.ProjectID, issueID = issueId });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        // Görev silerken kullanılır
        public ActionResult Delete(int? id)
        {
            try
            {
                string username = Session["username"].ToString();
                var kullanici = db.User.FirstOrDefault(i => i.Username == username);

                if (usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Gorev_Silebilir))
                {
                    /*var sorgu = (from gorevler in db.Issues join projeler in db.Projects on gorevler.ProjectID equals projeler.ID where gorevler.ProjectID==projeId select new { gorevler,projeler }).ToList();

                    var x = (from q in sorgu where q.projeler.Users.Contains(kullanici)select new {q}).ToList();*/
                    var proje = kullanici.Project1.FirstOrDefault(i => i.Issue.Any(k => k.ID == id));
                    if (proje == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Bulunmadığınız projeden görev silemezsiniz!" });
                    }
                    var gorev = proje.Issue.FirstOrDefault(i => i.ID == id);
                    if (gorev == null)
                    {
                        return RedirectToAction("ErrorPage", "Home", new { error = "Görev Bulunamadı!" });
                    }
                    return View(gorev);
                }
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Issue issue = db.Issue.Find(id);
            int projId = issue.ProjectID;
            /*if (issue.AltGorevs.Count != 0)
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Alt Görevleri Olan Bir Görev Silinemez!" });
            }*/
            GorevSil(issue);
            usc.CreateActionLog(Session["userId"].ToString(), id.ToString(), " görevi sildi. ");
            return RedirectToAction("Details", "Project", new { id = projId });
        }

        //Recursive çalışan görev silme motor fonksiyonudur.
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

        //Bellek temizleyen fonksiyondur.
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
