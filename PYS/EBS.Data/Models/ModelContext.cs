namespace EBS.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ModelContext : DbContext
    {
        public ModelContext()
            : base("name=ModelContext")
        {
        }

        public virtual DbSet<Auth> Auth { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<File> File { get; set; }
        public virtual DbSet<Issue> Issue { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<Priority> Priority { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auth>()
                .HasMany(e => e.Role)
                .WithMany(e => e.Auth)
                .Map(m => m.ToTable("Role_Authorization").MapLeftKey("AuthorizationID").MapRightKey("RoleID"));

            modelBuilder.Entity<Issue>()
                .HasMany(e => e.Comment)
                .WithRequired(e => e.Issue)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Issue>()
                .HasMany(e => e.File)
                .WithRequired(e => e.Issue)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Issue>()
                .HasMany(e => e.Issue1)
                .WithMany(e => e.Issue2)
                .Map(m => m.ToTable("Task_SubTask").MapLeftKey("TaskID").MapRightKey("SubTaskID"));

            modelBuilder.Entity<Priority>()
                .HasMany(e => e.Issue)
                .WithRequired(e => e.Priority)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>()
                .HasMany(e => e.Issue)
                .WithRequired(e => e.Project)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>()
                .HasMany(e => e.User1)
                .WithMany(e => e.Project1)
                .Map(m => m.ToTable("Project_Employe").MapLeftKey("ProjectID").MapRightKey("UserID"));

            modelBuilder.Entity<Role>()
                .HasMany(e => e.User)
                .WithRequired(e => e.Role)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Status>()
                .HasMany(e => e.Issue)
                .WithRequired(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Comment)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.File)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Issue)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.ReporterID);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Message)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.PostUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Message1)
                .WithRequired(e => e.User1)
                .HasForeignKey(e => e.GetUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Notification)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Project)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.CreatedUserID)
                .WillCascadeOnDelete(false);
        }
    }
}
