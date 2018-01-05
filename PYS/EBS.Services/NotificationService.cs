using EBS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBS.Services
{
    public class NotificationService
    {
        //Bildirim oluşturur.Bildirim içeriği ve bildirimin gideceği kişinin id sini girmek yeterlidir.
        public void Create(string icerik, int kisiid)
        {
            try
            {
                using (ModelContext db = new ModelContext())
                {
                    Notification bildirim = new Notification();
                    bildirim.Description = icerik;
                    bildirim.Date = DateTime.Now;
                    bildirim.UserID = kisiid;
                    bildirim.isRead = false;
                    db.Notification.Add(bildirim);
                    db.SaveChanges();
                }
            }
            catch
            {
                return;
            }
        }
    }
}
