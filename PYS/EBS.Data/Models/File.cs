namespace EBS.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("File")]
    public partial class File
    {
        public int ID { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int IssueID { get; set; }

        public int UserID { get; set; }

        public DateTime UploadDate { get; set; }

        public virtual Issue Issue { get; set; }

        public virtual User User { get; set; }
    }
}
