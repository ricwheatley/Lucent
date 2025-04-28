# Lucent

Lucent is a modular, full-stack .NET solution designed to streamline and automate critical business processes through a robust and scalable architecture. The system integrates API services, user authentication, data loading, task scheduling, resilience patterns, and user onboarding, making it suitable for complex enterprise environments.

## 📂 Project Structure

- **Lucent.Api**: RESTful API services.
- **Lucent.Auth**: Authentication and authorization services.
- **Lucent.Client**: Front-end client interface.
- **Lucent.Core**: Core domain logic and business models.
- **Lucent.Loader**: Data ingestion and ETL processes.
- **Lucent.Onboard**: User onboarding and initial setup.
- **Lucent.Resilience**: Fault tolerance mechanisms.
- **Lucent.Runner**: Background tasks execution.
- **Lucent.Scheduler**: Task scheduling and orchestration.

## 🚀 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- SQL Server

### Installation

Clone the repository:

```bash
git clone https://github.com/ricwheatley/Lucent.git
cd Lucent
```

Restore dependencies:

```bash
dotnet restore
```

Set up your database using the scripts provided in the `database` directory.

### Running the Application

Launch the API and client applications:

```bash
dotnet run --project Lucent.Api
```

Open your browser and navigate to the client interface URL provided upon launch.

## 🧪 Testing

(To be implemented: Unit and integration tests will be added in future development phases.)

## 📚 Roadmap

- [ ] Implement structured testing with xUnit
- [ ] Add comprehensive API documentation using Swagger/OpenAPI
- [ ] Set up CI/CD pipelines with GitHub Actions
- [ ] Enhance documentation and architecture diagrams

## 🤝 Contributing

Contributions are welcome! Please fork the repository, create a feature branch, and submit a pull request.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

