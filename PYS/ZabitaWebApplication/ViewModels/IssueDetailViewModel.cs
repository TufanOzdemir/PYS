using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EBS.Data;

namespace ZabitaWebApplication.ViewModels
{
    public class IssueDetailViewModel
    {
        public Issue issue{ get; set; }

        public List<Status> status { get; set; }
    }
}