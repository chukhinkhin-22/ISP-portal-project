using Microsoft.EntityFrameworkCore;
using ISP_Portal.API.Models;

namespace ISP_Portal.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<NetworkNode> NetworkNodes => Set<NetworkNode>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // table names with underscores don't match EF's default naming, map explicitly
        modelBuilder.Entity<NetworkNode>().ToTable("Network_Nodes");
        modelBuilder.Entity<SupportTicket>().ToTable("Support_Tickets");
        modelBuilder.Entity<TicketComment>().ToTable("Ticket_Comments");

        // the actual schema uses "ID" not "Id" on every key column, so map those by hand
        modelBuilder.Entity<User>().Property(u => u.UserId).HasColumnName("UserID");
        modelBuilder.Entity<Plan>().Property(p => p.PlanId).HasColumnName("PlanID");
        modelBuilder.Entity<NetworkNode>().Property(n => n.NodeId).HasColumnName("NodeID");
        modelBuilder.Entity<Department>().Property(d => d.DepartmentId).HasColumnName("DepartmentID");

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.Property(s => s.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(s => s.UserId).HasColumnName("UserID");
            entity.Property(s => s.PlanId).HasColumnName("PlanID");
            entity.Property(s => s.NodeId).HasColumnName("NodeID");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.Property(t => t.TicketId).HasColumnName("TicketID");
            entity.Property(t => t.UserId).HasColumnName("UserID");
            entity.Property(t => t.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(t => t.DepartmentId).HasColumnName("DepartmentID");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.Property(c => c.CommentId).HasColumnName("CommentID");
            entity.Property(c => c.TicketId).HasColumnName("TicketID");
            entity.Property(c => c.UserId).HasColumnName("UserID");
        });

        // store enums as the real MySQL ENUM strings instead of integers
        modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
        modelBuilder.Entity<NetworkNode>().Property(n => n.Status).HasConversion<string>();
        modelBuilder.Entity<Subscription>().Property(s => s.Status).HasConversion<string>();
        modelBuilder.Entity<SupportTicket>().Property(t => t.Priority).HasConversion<string>();
        modelBuilder.Entity<SupportTicket>().Property(t => t.Status).HasConversion<string>();

        // these two are nullable FKs, so don't cascade delete - just clear the reference
        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.Subscription)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Tickets)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // comments are meaningless without their ticket, so cascade here is fine
        modelBuilder.Entity<TicketComment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
