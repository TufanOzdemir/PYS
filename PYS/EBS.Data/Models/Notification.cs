namespace EBS.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Notification")]
    public partial class Notification
    {
        public int ID { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime Date { get; set; }

        public int UserID { get; set; }

        public bool isRead { get; set; }

        public virtual User User { get; set; }
    }
}
