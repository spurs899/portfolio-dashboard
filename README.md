# PortfolioDashboard

PortfolioDashboard is a .NET solution for managing and visualizing investment portfolios. It is organized into multiple projects for API, core logic, contracts, web interface, and testing.

## Projects

- **PortfolioManager.Api**: ASP.NET Core Web API for portfolio management operations.
- **PortfolioManager.Web**: Web frontend for interacting with the portfolio dashboard.
- **PortfolioManager.Core**: Core business logic and domain models.
- **PortfolioManager.Contracts**: Shared contracts and DTOs for API and core communication.
- **PortfolioManager.Api.Tests**: Unit and integration tests for the API.
- **PortfolioManager.Core.Tests**: Unit tests for the core logic.

## Getting Started

1. **Clone the repository**
   ```sh
   git clone <repo-url>
   cd PortfolioDashboard
   ```
2. **Restore dependencies**
   ```sh
   dotnet restore
   ```
3. **Build the solution**
   ```sh
   dotnet build
   ```
4. **Run tests**
   ```sh
   dotnet test
   ```
5. **Run the API**
   ```sh
   dotnet run --project PortfolioManager.Api
   ```
6. **Run the Web App**
   ```sh
   dotnet run --project PortfolioManager.Web
   ```

## Requirements
- [.NET 6.0 SDK or later](https://dotnet.microsoft.com/download)

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
This project is licensed under the MIT License.
