using EBS.Data;
using EBS.Extensions;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace ZabitaWebApplication.Controllers
{
    public class HomeController : Controller
    {
        //Database delegesi
        ModelContext db = new ModelContext();
        UserSessionClass usc = new UserSessionClass();

        //Giriş yaparken çağrılır.
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login formundan gelen verileri işler ve giriş yapıp yapmayacağına karar verir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model)
        {
            try
            {
                if(Session["username"] != null)
                {
                    return RedirectToAction("Index","User");
                }
                var user = db.User.FirstOrDefault(i => i.Username == model.Username);

                if (user == null)
                {
                    return View(model);
                }
                var parola = usc.Decrypt(user.Password);

                if (parola != model.Password)
                {
                    return View(model);
                }

                Session["userId"] = user.ID;
                Session["userRole"] = user.RoleID;
                Session["username"] = user.Username;
                if(user.PicturePath == null)
                {
                    Session["userPic"] = "admin.ico";
                }else
                {
                    Session["userPic"] = user.PicturePath;
                }
                usc.CreateActionLog(Session["userId"].ToString(), "", " giriş yaptı.");
                return RedirectToAction("Index", "User");
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Home", new { error = "Bir Hata Oldu!" });
            }
        }

        //E maile parola yollar
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //Form bilgilerini kontrol eder.
        [HttpPost]
        public ActionResult ForgotPassword(string KullaniciAdi, string posta)
        {
            try
            {
                    var kisi = db.User.FirstOrDefault(i => i.Username == KullaniciAdi);
                    if (kisi == null)
                    {
                        return View();
                    }

                    MailMessage ePosta = new MailMessage();
                    ePosta.From = new MailAddress("tufan.ozdemir@ebirdsoftware.com");
                    ePosta.To.Add(kisi.Email);
                    ePosta.Subject = "Proje Yönetim Sistemi Parolanız";
                    ePosta.Body = usc.Decrypt(kisi.Password);
                    SmtpClient smtp = new SmtpClient();
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("amonisal4@hotmail.com", "tfngkhn44");
                    smtp.Port = 587;
                    smtp.Host = "smtp-mail.outlook.com";
                    smtp.EnableSsl = true;

                    smtp.Send(ePosta);

                    return RedirectToAction("Login", "Home");
                
            }
            catch
            {
                return View();
            }
        }

        //Hata fonksiyonlarının genelinde verilen hata kodu ve hata açıklaması parametreleriyle kullanılır.
        public ActionResult ErrorPage(string error,int errorCode = 404)
        {
            ViewBag.error = error;
            ViewBag.errorCode = errorCode;
            return View();
        }

        //Çıkış yapmak için kullanılır.(Session boşaltır)
        public ActionResult Logout()
        {

            usc.CreateActionLog(Session["userId"].ToString(), "", " çıkış yaptı.");
            Session["userId"] = null;
            Session["username"] = null;
            Session["rol"] = null;
            return RedirectToAction("Login", "Home");
        }
    }
}