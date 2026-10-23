# Manage Your Education and Skills Funding Funding Claim API

The Manage Your Education and Skills Funding (MYESF) Funding Claim API is used by the MYESF web application and funding claim processor azure function to allow the following:

- Provides both the funding claim and reconciliation administration services.

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is an ASP.NET Core 10 web application utilising Azure App Service for deployment.

The web application runs on an Azure App service on Azure.

**Note:** The project is currently being updated to be containerised via Docker where the deployment method and target will change, this document will be updated when these changes have been finalised.

# Local Configuration Guide

In order to run the application locally a valid `appsettings.json` file will need to be created in the `Pds.FundingClaim.Api` project. Below, and included in the repo, there is `appsettings.example.json` which can be used as a base and populated with the required values, which can be retrieved from the Azure Portal.

## Application Settings (`appsettings.json`)

```json
{
 "PdsApplicationInsights": {
    "InstrumentationKey": "",
    "Environment": "local"
  },
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Error"
      }
    },
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AzureAd": {
    "Instance": "",
    "ClientId": "",
    "TenantId": "",
    "Audience": ""
  },
  "ConnectionStrings": {
    "fundingclaims": ""
  }, 
  "AuditApiConfiguration": {
    "ApiBaseAddress": "",
    "Authority": "",
    "ClientId": "",
    "ClientSecret": "",
    "TenantId": "",
    "AppUri": ""
  },
  "AzureMessagingServiceBusOptions": {
    "ConnectionString": ""
  }
}
```

### Setting Details

- **`PdsApplicationInsights:InstrumentationKey`**  
  The key value for Application Insights resource for logging purposes.

- **`PdsApplicationInsights:Environment`**  
  The environment which the app is running on for Application Insights for logging purposes.

- **`Logging:ApplicationInsights:LogLevel:Default`**
  The default logging level for the service when logging to Application Insights; refer to the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=net-9.0-pp) for an explanation of the different levels.

- **`Logging:ApplicationInsights:LogLevel:Microsoft`**
  The default logging level for Microsoft specific information when logging to Application Insights; refer to the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=net-9.0-pp) for an explanation of the different levels.

- **`Logging:LogLevel:Default`**
  The default logging level for the service; refer to the [Microsoft Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=net-9.0-pp) for an explanation of the different levels.

- **`AzureAd:Instance`**  
  The URL of the azure ad service used to authenticate.

- **`AzureAd:ClientId`**  
  The application (client) ID registered in azure ad.

- **`AzureAd:TenantId`**  
  The unique identifier for your azure ad tenant.

- **`AzureAd:Audience`**  
  The intended recipient of the azure authentication token.

- **`ConnectionStrings:fundingclaims`**  
  The database connection configuration used by the application to access the funding claims database.

- **`AuditApiConfiguration:ApiBaseAddress`** 
  The base URL endpoint used by a client application to route network requests to the Audit API backend.

- **`AuditApiConfiguration:Authority`** 
  The base URL of the Identity Provider responsible for authenticating and issuing tokens for the Audit API client.

- **`AuditApiConfiguration:ClientId`** 
  The Audit API application (client) ID registered in azure ad.

- **`AuditApiConfiguration:ClientSecret`** 
  The confidential credential used by the Audit API application to securely prove its identity to the Identity Provider.

- **`AuditApiConfiguration:TenantId`** 
  The unique identifier for your azure ad tenant.

- **`AuditApiConfiguration:AppUri`** 
  The unique Application ID URI used as the identifier for the protected Audit API resource within the Identity Provider.

- **`AzureMessagingServiceBusOptions:ConnectionString`** 
  The key specifies the complete authentication string the application needs to connect to an Azure Service Bus namespace.
  
## Build and Test

To build and test locally, you can either use Visual Studio, Visual Studio Code or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Contribute

To contribute,

- If you are part of the team then create a branch for changes and then submit your changes for review by creating a pull request.
- If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.
  