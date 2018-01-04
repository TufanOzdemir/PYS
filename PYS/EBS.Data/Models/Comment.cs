namespace EBS.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Comment")]
    public partial class Comment
    {
        public int ID { get; set; }

        public int UserID { get; set; }

        public int IssueID { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string Description { get; set; }

        public virtual Issue Issue { get; set; }

        public virtual User User { get; set; }
    }
}
