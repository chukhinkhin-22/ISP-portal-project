{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=isp_portal;User=root;Password=yourpassword;"
  },
  "Jwt": {
    "Key": "replace-this-with-a-long-random-secret-at-least-32-characters",
    "Issuer": "ISPPortalAPI",
    "Audience": "ISPPortalClient",
    "ExpiryMinutes": "120"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
