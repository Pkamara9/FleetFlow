{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FleetFlowDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "FleetFlowDevelopmentSecretKeyLongEnough123!",
    "Issuer": "FleetFlow",
    "Audience": "FleetFlowClients",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Stripe": {
    "ApiKey": "sk_test_1234567890",
    "WebhookSecret": "whsec_1234567890"
  },
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=fleetflowstorage;AccountKey=example;EndpointSuffix=core.windows.net",
    "ContainerName": "fleetflow-documents"
  }
}

