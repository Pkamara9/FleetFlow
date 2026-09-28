namespace FleetFlow.Api.DTOs;

public class VehicleDto
{
    public int VehicleId { get; set; }
    public int CompanyId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? LicensePlate { get; set; }
    public int CurrentMileage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedDriver { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateVehicleRequest
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? LicensePlate { get; set; }
    public int CurrentMileage { get; set; }
    public string Status { get; set; } = "Active";
    public string? AssignedDriver { get; set; }
}

