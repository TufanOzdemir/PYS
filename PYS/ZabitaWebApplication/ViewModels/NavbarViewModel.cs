using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EBS.Data;

namespace ZabitaWebApplication.ViewModels
{
    public class NavbarViewModel
    {
        public List<Notification> Notifications { get; set; }

        public User user { get; set; }
    }
}