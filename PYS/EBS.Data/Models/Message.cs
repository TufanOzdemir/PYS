namespace EBS.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Message")]
    public partial class Message
    {
        public int ID { get; set; }

        [Required]
        public string MessageContent { get; set; }

        public DateTime Date { get; set; }

        public int PostUserID { get; set; }

        public int GetUserID { get; set; }

        public virtual User User { get; set; }

        public virtual User User1 { get; set; }
    }
}
