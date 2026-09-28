using FleetFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Api.Data;

public class FleetFlowDbContext : DbContext
{
    public FleetFlowDbContext(DbContextOptions<FleetFlowDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<MaintenanceSchedule> MaintenanceSchedules => Set<MaintenanceSchedule>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.CompanyId);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.SubscriptionStatus).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Company).WithMany(c => c.Users).HasForeignKey(u => u.CompanyId);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.VehicleId);
            entity.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(50);
            entity.Property(v => v.VIN).IsRequired().HasMaxLength(50);
            entity.HasOne(v => v.Company).WithMany(c => c.Vehicles).HasForeignKey(v => v.CompanyId);
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(s => s.ServiceTypeId);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(s => s.Company).WithMany(c => c.ServiceTypes).HasForeignKey(s => s.CompanyId);
        });

        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(m => m.ScheduleId);
            entity.HasOne(m => m.Vehicle).WithMany(v => v.MaintenanceSchedules).HasForeignKey(m => m.VehicleId);
            entity.HasOne(m => m.ServiceType).WithMany(s => s.MaintenanceSchedules).HasForeignKey(m => m.ServiceTypeId);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(w => w.WorkOrderId);
            entity.Property(w => w.Title).IsRequired().HasMaxLength(200);
            entity.HasOne(w => w.Vehicle).WithMany(v => v.WorkOrders).HasForeignKey(w => w.VehicleId);
            entity.HasOne(w => w.AssignedUser).WithMany(u => u.WorkOrders).HasForeignKey(w => w.AssignedUserId);
        });

        modelBuilder.Entity<ServiceRecord>(entity =>
        {
            entity.HasKey(s => s.ServiceRecordId);
            entity.HasOne(s => s.Vehicle).WithMany(v => v.ServiceRecords).HasForeignKey(s => s.VehicleId);
            entity.HasOne(s => s.WorkOrder).WithMany(w => w.ServiceRecords).HasForeignKey(s => s.WorkOrderId);
            entity.HasOne(s => s.ServiceType).WithMany(t => t.ServiceRecords).HasForeignKey(s => s.ServiceTypeId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.RefreshTokenId);
            entity.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
        });

        base.OnModelCreating(modelBuilder);
    }
}

