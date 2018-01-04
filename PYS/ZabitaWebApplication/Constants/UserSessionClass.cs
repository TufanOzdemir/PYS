using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EBS.Data;

namespace PYSInonu.Constants
{
    public class UserSessionClass
    {
        ModelContext db = new ModelContext();
        private static string logFile = @"C:\Users\Tufan\Desktop\bit\PYS\ZabitaWebApplication-branch\ZabitaWebApplication\SystemLogs\Logs.txt";
        TraceListener listener = new DelimitedListTraceListener(logFile);
        
        public enum Yetkiler
        {
            Uye_Ekleyebilir,
            Rol_Olusturabilir,
            Uye_Engelleyebilir,
            Uye_Guncelleyebilir,
            Gorev_Olusturabilir,
            Rol_Guncelleyebilir,
            Rol_Silebilir,
            Proje_Olusturabilir,
            Proje_Guncelleyebilir,
            Proje_Silebilir,
            Proje_eleman_ekleyebilir,
            Gorev_Guncelleyebilir,
            Gorev_Silebilir,
            Gorev_Atayabilir
        };

        //Yetki olup olmadığını kontrol eder.
        public bool YetkiKontrol(User kullanici,Yetkiler yetkim)
        {
            var yetkiVarmi = kullanici.Role.Auth.Where(k => k.Name == YetkiEnumToString(yetkim)).SingleOrDefault();

            if (yetkiVarmi == null)
            {
                return false;
            }

            return true;
        }

        //İsim enum çevrimi yapar.
        private string YetkiEnumToString(Yetkiler model)
        {
            if(model == Yetkiler.Rol_Olusturabilir)
            {
                return "Rol Oluşturabilir";
            }
            else if(model == Yetkiler.Uye_Ekleyebilir)
            {
                return "Üye Ekleyebilir";
            }
            else if (model == Yetkiler.Uye_Engelleyebilir)
            {
                return "Üye Engelleyebilir";
            }
            else if (model == Yetkiler.Uye_Guncelleyebilir)
            {
                return "Üye Güncelleyebilir";
            }
            else if (model == Yetkiler.Gorev_Olusturabilir)
            {
                return "Görev Oluşturabilir";
            }
            else if (model == Yetkiler.Rol_Guncelleyebilir)
            {
                return "Rol Güncelleyebilir";
            }
            else if (model == Yetkiler.Rol_Silebilir)
            {
                return "Rol Silebilir";
            }
            else if (model == Yetkiler.Proje_Olusturabilir)
            {
                return "Proje Oluşturabilir";
            }
            else if (model == Yetkiler.Proje_Guncelleyebilir)
            {
                return "Proje Güncelleyebilir";
            }
            else if (model == Yetkiler.Proje_Silebilir)
            {
                return "Proje Silebilir";
            }
            else if (model == Yetkiler.Proje_eleman_ekleyebilir)
            {
                return "Projeye Eleman Ekleyebilir";
            }
            else if (model == Yetkiler.Gorev_Guncelleyebilir)
            {
                return "Görev Güncelleyebilir";
            }
            else if (model == Yetkiler.Gorev_Silebilir)
            {
                return "Görev Silebilir";
            }
            else if (model == Yetkiler.Gorev_Atayabilir)
            {
                return "Görev Atayabilir";
            }
            return null;
        }

        //Kriptolama yapar.
        string hash = "PYS";
        public string Encrypt(string password)
        {
            string encrypted = null;
            byte[] data = UTF8Encoding.UTF8.GetBytes(password);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = tripDes.CreateEncryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                    encrypted = Convert.ToBase64String(result, 0, result.Length);
                }
            }
            return encrypted;
        }

        //Kripto çözer.
        public string Decrypt(string encryptedPassword)
        {
            string decrypted;
            byte[] data = Convert.FromBase64String(encryptedPassword);
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));
                using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = tripDes.CreateDecryptor();
                    byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                    decrypted = UTF8Encoding.UTF8.GetString(result);
                }
            }
            return decrypted;
        }

        //Bildirim oluşturur.Bildirim içeriği ve bildirimin gideceği kişinin id sini girmek yeterlidir.
        public void BildirimOlustur(string icerik, int kisiid)
        {
            try
            {
                Notification bildirim = new Notification();
                bildirim.Description = icerik;
                bildirim.Date = DateTime.Now;
                bildirim.UserID = kisiid;
                bildirim.isRead = false;
                db.Notification.Add(bildirim);
                db.SaveChanges();
            }
            catch
            {
                return;
            }
        }

        //Yeni bir log oluşturur.
        public void CreateActionLog(string userId, string optionalString, string action)
        {
            // Add listener.
            Debug.Listeners.Add(listener);
            // Write and flush.
            Debug.WriteLine(DateTime.Now + "  -"+userId +", id numaralı kullanıcı,"+ optionalString + ", id numaralı"+action);
            Debug.Flush();
        }

        //Mevcut bir logu günceller
        public void UpdateActionLog(string userId, string optionalId, string action)
        {
            // Add listener.
            Debug.Listeners.Add(listener);
            // Write and flush.
            Debug.WriteLine(DateTime.Now+"  -"+userId + ", id numaralı kullanıcı ," + optionalId + ", id numaralı"+action);
            Debug.Flush();
        }
       
        //Log değişimi yapar.
        public void ChangeActionLog(string userId, string optionalIdOld, string optionalIdNew, string action)
        {
            // Add listener.
            Debug.Listeners.Add(listener);
            // Write and flush.
            Debug.WriteLine(DateTime.Now + "  -"+userId + ", id numaralı kullanıcı, " + optionalIdOld + " --> "+optionalIdNew + action);
            Debug.Flush();
        }

    }
}
