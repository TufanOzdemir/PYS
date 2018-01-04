using PYSInonu.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZabitaWebApplication.ViewModels
{
    public class Rol_SubRol_ViewModel
    {
        public string RolAd { get; set; }

        public List<CheckBoxModel> SubList { get; set; }
    }
}