namespace FleetFlow.Api.Models;

public class Company
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string SubscriptionStatus { get; set; } = "Trial";
    public string? SubscriptionPlan { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}

