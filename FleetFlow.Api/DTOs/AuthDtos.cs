using FleetFlow.Api.Models;

namespace FleetFlow.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(FleetFlowDbContext context)
    {
        if (context.Companies.Any())
        {
            return;
        }

        var company = new Company
        {
            Name = "Northwind Fleet",
            Address = "123 Fleet Lane, Dallas, TX",
            Phone = "(214) 555-0101",
            SubscriptionStatus = "Active",
            SubscriptionPlan = "Growth"
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var adminUser = new User
        {
            CompanyId = company.CompanyId,
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@fleetflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Admin"
        };

        var managerUser = new User
        {
            CompanyId = company.CompanyId,
            FirstName = "Maria",
            LastName = "Garcia",
            Email = "manager@fleetflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
            Role = "Manager"
        };

        var technicianUser = new User
        {
            CompanyId = company.CompanyId,
            FirstName = "Luis",
            LastName = "Mendez",
            Email = "tech@fleetflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tech123!"),
            Role = "Technician"
        };

        context.Users.AddRange(adminUser, managerUser, technicianUser);
        await context.SaveChangesAsync();

        var vehicles = new[]
        {
            new Vehicle { CompanyId = company.CompanyId, VehicleNumber = "VF-101", VIN = "1HGBH41JXMN109186", Year = 2022, Make = "Ford", Model = "Transit", LicensePlate = "TX-1014", CurrentMileage = 42000, Status = "Active", AssignedDriver = "A. Wilson" },
            new Vehicle { CompanyId = company.CompanyId, VehicleNumber = "VF-102", VIN = "2T2BK1BA9KC134589", Year = 2021, Make = "Chevrolet", Model = "Silverado", LicensePlate = "TX-2087", CurrentMileage = 78000, Status = "In Service", AssignedDriver = "M. Ross" },
            new Vehicle { CompanyId = company.CompanyId, VehicleNumber = "VF-103", VIN = "3VW2B7AJXKM203919", Year = 2023, Make = "Mercedes", Model = "Sprinter", LicensePlate = "TX-4478", CurrentMileage = 26000, Status = "Active", AssignedDriver = "T. Payne" }
        };

        context.Vehicles.AddRange(vehicles);
        await context.SaveChangesAsync();

        var serviceTypes = new[]
        {
            new ServiceType { CompanyId = company.CompanyId, Name = "Oil Change", IntervalMiles = 7500, IntervalDays = 180 },
            new ServiceType { CompanyId = company.CompanyId, Name = "Brake Inspection", IntervalMiles = 25000, IntervalDays = 365 },
            new ServiceType { CompanyId = company.CompanyId, Name = "Tire Rotation", IntervalMiles = 12000, IntervalDays = 180 },
            new ServiceType { CompanyId = company.CompanyId, Name = "Transmission Service", IntervalMiles = 40000, IntervalDays = 365 }
        };

        context.ServiceTypes.AddRange(serviceTypes);
        await context.SaveChangesAsync();

        var schedules = new[]
        {
            new MaintenanceSchedule { VehicleId = vehicles[0].VehicleId, ServiceTypeId = serviceTypes[0].ServiceTypeId, LastServiceMileage = 35000, LastServiceDate = DateTime.UtcNow.AddDays(-70), NextDueMileage = 42500, NextDueDate = DateTime.UtcNow.AddDays(30) },
            new MaintenanceSchedule { VehicleId = vehicles[1].VehicleId, ServiceTypeId = serviceTypes[1].ServiceTypeId, LastServiceMileage = 68000, LastServiceDate = DateTime.UtcNow.AddDays(-120), NextDueMileage = 70000, NextDueDate = DateTime.UtcNow.AddDays(-5) },
            new MaintenanceSchedule { VehicleId = vehicles[2].VehicleId, ServiceTypeId = serviceTypes[2].ServiceTypeId, LastServiceMileage = 21000, LastServiceDate = DateTime.UtcNow.AddDays(-40), NextDueMileage = 33000, NextDueDate = DateTime.UtcNow.AddDays(15) }
        };

        context.MaintenanceSchedules.AddRange(schedules);
        await context.SaveChangesAsync();

        var workOrders = new[]
        {
            new WorkOrder { VehicleId = vehicles[0].VehicleId, AssignedUserId = technicianUser.UserId, Title = "Engine diagnostics", Description = "Check check-engine warning and perform diagnostics.", Priority = "High", Status = "Open", LaborCost = 235.50m, PartsCost = 95.00m, TotalCost = 330.50m },
            new WorkOrder { VehicleId = vehicles[1].VehicleId, AssignedUserId = technicianUser.UserId, Title = "Brake inspection", Description = "Inspect and replace front pads if needed.", Priority = "Medium", Status = "In Progress", LaborCost = 180.00m, PartsCost = 210.00m, TotalCost = 390.00m },
            new WorkOrder { VehicleId = vehicles[2].VehicleId, AssignedUserId = technicianUser.UserId, Title = "Tire rotation", Description = "Seasonal tire rotation and balancing.", Priority = "Low", Status = "Completed", LaborCost = 120.00m, PartsCost = 45.00m, TotalCost = 165.00m, CompletedDate = DateTime.UtcNow.AddDays(-2) }
        };

        context.WorkOrders.AddRange(workOrders);
        await context.SaveChangesAsync();

        var serviceRecords = new[]
        {
            new ServiceRecord { VehicleId = vehicles[0].VehicleId, WorkOrderId = workOrders[0].WorkOrderId, ServiceTypeId = serviceTypes[0].ServiceTypeId, ServiceDate = DateTime.UtcNow.AddDays(-30), Mileage = 36000, Cost = 260.00m, Notes = "Performed oil and filter change" },
            new ServiceRecord { VehicleId = vehicles[1].VehicleId, WorkOrderId = workOrders[1].WorkOrderId, ServiceTypeId = serviceTypes[1].ServiceTypeId, ServiceDate = DateTime.UtcNow.AddDays(-12), Mileage = 71000, Cost = 375.00m, Notes = "Brake inspection completed" },
            new ServiceRecord { VehicleId = vehicles[2].VehicleId, WorkOrderId = workOrders[2].WorkOrderId, ServiceTypeId = serviceTypes[2].ServiceTypeId, ServiceDate = DateTime.UtcNow.AddDays(-2), Mileage = 24500, Cost = 165.00m, Notes = "Tire rotation and balancing" }
        };

        context.ServiceRecords.AddRange(serviceRecords);
        context.AuditLogs.Add(new AuditLog
        {
            CompanyId = company.CompanyId,
            UserId = adminUser.UserId,
            EntityType = "Company",
            EntityId = company.CompanyId,
            Action = "Seed",
            Details = "Initial FleetFlow seed data created."
        });

        await context.SaveChangesAsync();
    }
}

