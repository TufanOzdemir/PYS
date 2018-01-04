using PYSInonu.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZabitaWebApplication.Base_Controller;
using EBS.Data;
using ZabitaWebApplication.ViewModels;

namespace ZabitaWebApplication.Controllers
{
    public class RoleController : BaseSessionController
    {
        private ModelContext db = new ModelContext();
        private UserSessionClass usc = new UserSessionClass();

        // Tüm rolleri listeler
        public ActionResult Index()
        {
            return View(db.Role.ToList());
        }

        // Rolün yetkilerini sıralar.
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role rol = db.Role.Find(id);
            if (rol == null)
            {
                return HttpNotFound();
            }

            return View(rol.Auth.ToList());
        }

        // Rol oluşturmaya yarar.
        public ActionResult Create()
        {
            string username = Session["username"].ToString();
            var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

            if (!usc.YetkiKontrol(kullanici,UserSessionClass.Yetkiler.Rol_Olusturabilir))
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }

            List<CheckBoxModel> cbm = new List<CheckBoxModel>();
            foreach (var i in db.Auth.ToList())
            {
                cbm.Add(new CheckBoxModel { Checked = false, ID = i.ID, Name = i.Name });
            }
            Rol_SubRol_ViewModel rsv = new Rol_SubRol_ViewModel() { SubList = cbm };
            return View(rsv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Rol_SubRol_ViewModel cbm)
        {
            try
            {
                Role rol = new Role();
                rol.Name = cbm.RolAd;

                foreach (var i in cbm.SubList)
                {
                    if (i.Checked)
                    {
                        rol.Auth.Add(db.Auth.Where(k => k.ID == i.ID).SingleOrDefault());
                    }
                }

                if (rol.Name == "Default")
                {
                    return RedirectToAction("ErrorPage", "Home", new { error = "Bu İsimde Bir Rol Oluşturamazsınız!" });
                }
                db.Role.Add(rol);
                db.SaveChanges();

                usc.CreateActionLog(Session["userId"].ToString(), cbm.RolAd.ToString(), " adlı rolü ekledi.");
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Hay Aksi!" });
            }
        }
        
        //Rol güncellemeyi sağlar.
        public ActionResult Edit(int? id)
        {
            string username = Session["username"].ToString();
            var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

            if (!usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Rol_Guncelleyebilir))
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role rol = db.Role.Find(id);
            if (rol == null)
            {
                return HttpNotFound();
            }
            if (rol.Name == "Default")
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu Rolü Değiştirme Yetkiniz Bulunmamaktadır!" });
            }

            List<CheckBoxModel> cbm = new List<CheckBoxModel>();

            bool a = false;
            foreach (var i in db.Auth.ToList())
            {
                foreach (var k in rol.Auth.ToList())
                {
                    if (i == k)
                    {
                        cbm.Add(new CheckBoxModel { Checked = true, ID = i.ID, Name = i.Name });
                        a = true;
                        break;
                    }
                }
                if (!a)
                    cbm.Add(new CheckBoxModel { Checked = false, ID = i.ID, Name = i.Name });
                a = false;
            }
            Rol_SubRol_ViewModel rsv = new Rol_SubRol_ViewModel() { SubList = cbm };
            rsv.RolAd = rol.Name;
            return View(rsv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Rol_SubRol_ViewModel model)
        {
            try
            {
                var rol = db.Role.Where(i => i.ID == id).SingleOrDefault();

                foreach (var i in rol.Auth.ToList())
                {
                    rol.Auth.Remove(i);
                }

                foreach (var i in model.SubList)
                {
                    if (i.Checked)
                    {
                        rol.Auth.Add(db.Auth.Where(k => k.ID == i.ID).SingleOrDefault());
                    }
                }
                rol.Name = model.RolAd;

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Hay Aksi!" });
            }

        }

        // Rol silmeyi sağlar.
        public ActionResult Delete(int? id)
        {

            string username = Session["username"].ToString();
            var kullanici = db.User.Where(i => i.Username == username).SingleOrDefault();

            if (!usc.YetkiKontrol(kullanici, UserSessionClass.Yetkiler.Rol_Silebilir))
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu işlem için gerekli yetkiniz bulunmamaktadır!" });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role rol = db.Role.Find(id);
            if (rol == null)
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Rol Bulunamadı!" });
            }
            if (rol.Name == "Default")
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bu Rolü Silme Yetkiniz Bulunmamaktadır!" });
            }
            return View(rol);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Role rol = db.Role.Find(id);
                foreach (var i in rol.Auth.ToList())
                {
                    rol.Auth.Remove(i);
                }

                var defaultRol = db.Role.Where(i => i.Name == "Default").SingleOrDefault();
                foreach (var i in rol.User.ToList())
                {
                    i.Role = defaultRol;
                }
                db.Role.Remove(rol);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Hay Aksi!" });
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