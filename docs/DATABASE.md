# FleetFlow Database Design

## Core schema

```sql
CREATE TABLE Companies (
    CompanyId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500),
    Phone NVARCHAR(50),
    SubscriptionStatus NVARCHAR(50) NOT NULL DEFAULT 'Trial',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyId INT NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE Vehicles (
    VehicleId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyId INT NOT NULL,
    VehicleNumber NVARCHAR(50) NOT NULL,
    VIN NVARCHAR(50) NOT NULL,
    Year INT NOT NULL,
    Make NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    LicensePlate NVARCHAR(50),
    CurrentMileage INT NOT NULL DEFAULT 0,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    AssignedDriver NVARCHAR(200),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Vehicles_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE ServiceTypes (
    ServiceTypeId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    IntervalMiles INT,
    IntervalDays INT,
    CONSTRAINT FK_ServiceTypes_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

CREATE TABLE MaintenanceSchedules (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId INT NOT NULL,
    ServiceTypeId INT NOT NULL,
    LastServiceMileage INT NOT NULL DEFAULT 0,
    LastServiceDate DATETIME2,
    NextDueMileage INT,
    NextDueDate DATETIME2,
    CONSTRAINT FK_MaintenanceSchedules_Vehicles FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
    CONSTRAINT FK_MaintenanceSchedules_ServiceTypes FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(ServiceTypeId)
);

CREATE TABLE WorkOrders (
    WorkOrderId INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId INT NOT NULL,
    AssignedUserId INT,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Priority NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Open',
    LaborCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    PartsCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalCost DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CompletedDate DATETIME2,
    CONSTRAINT FK_WorkOrders_Vehicles FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
    CONSTRAINT FK_WorkOrders_Users FOREIGN KEY (AssignedUserId) REFERENCES Users(UserId)
);

CREATE TABLE ServiceRecords (
    ServiceRecordId INT IDENTITY(1,1) PRIMARY KEY,
    VehicleId INT NOT NULL,
    WorkOrderId INT,
    ServiceTypeId INT NOT NULL,
    ServiceDate DATETIME2 NOT NULL,
    Mileage INT NOT NULL,
    Cost DECIMAL(18,2) NOT NULL DEFAULT 0,
    Notes NVARCHAR(MAX),
    CONSTRAINT FK_ServiceRecords_Vehicles FOREIGN KEY (VehicleId) REFERENCES Vehicles(VehicleId),
    CONSTRAINT FK_ServiceRecords_WorkOrders FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(WorkOrderId),
    CONSTRAINT FK_ServiceRecords_ServiceTypes FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(ServiceTypeId)
);

CREATE TABLE AuditLogs (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    CompanyId INT,
    UserId INT,
    EntityType NVARCHAR(100),
    EntityId INT,
    Action NVARCHAR(100) NOT NULL,
    Details NVARCHAR(MAX),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

## Indexes

```sql
CREATE INDEX IX_Vehicles_CompanyId ON Vehicles(CompanyId);
CREATE INDEX IX_Users_CompanyId ON Users(CompanyId);
CREATE INDEX IX_WorkOrders_VehicleId ON WorkOrders(VehicleId);
CREATE INDEX IX_WorkOrders_Status ON WorkOrders(Status);
CREATE INDEX IX_ServiceRecords_VehicleId ON ServiceRecords(VehicleId);
CREATE INDEX IX_MaintenanceSchedules_VehicleId ON MaintenanceSchedules(VehicleId);
```
