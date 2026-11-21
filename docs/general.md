# CARBON-KICKERS — AI Agent Context & Project Guide

## 1. Project Overview
**CARBON-KICKERS** is a gamified native Windows desktop application that transforms personal carbon footprint reduction into a competitive experience ("sustainability as a sport"). Users log daily activities—primarily transportation and destination choices—to calculate real-time carbon emissions, track CO2 savings, and compete against others.

- **Repository:** `Group-4-Junior-Project-IUP-2024`
- **Academic Context:** Computer Science Laboratory — Junior Project (Databases & Object-Based Programming)

---

## 2. Core Features & Domain Scope
When generating code, database schemas, API endpoints, or UI views, align with these six core modules:

1. **The Arena (PvP Battles):** Head-to-head friend challenges and weekly community leagues competing for the lowest carbon footprint.
2. **Smart Logging:** External API integrations (Google Maps for transport distance, Climatiq for emission factors) to automate and verify CO2 savings calculations.
3. **Progression System:** Gamification layer featuring "Green XP," unlockable badges, league tiers (Bronze to Diamond), and virtual rewards for consistent eco-friendly habits.
4. **Trip Tracking:** Start/end trip recording paired with distance calculation.
5. **Carbon Emission Calculation:** Core calculation engine estimating CO2 emissions per logged trip.
6. **Impact Dashboard:** Historical data visualization showing cumulative CO2 "kicked" out of the atmosphere over time.

---

## 3. Tech Stack & 3-Tier Architecture
This project uses a strict **3-Tier Architecture** (Presentation → Application/API → Data):

1. **Presentation Tier (Desktop Client):**
   - **Framework:** WPF (Windows Presentation Foundation) on .NET / C#.
   - **Pattern:** **MVVM (Model-View-ViewModel)**. Keep `.xaml.cs` code-behind minimal; place UI state and commands in ViewModels.
   - **Communication:** Communicates exclusively with the Backend Web API via `HttpClient` (JSON over HTTPS).
2. **Application Tier (Backend API):**
   - **Framework:** ASP.NET Core Web API (C# / .NET).
   - **Responsibilities:** Authentication, business logic, PvP/league rules, CO2 calculations, and proxying calls to external APIs (**Google Maps API**, **Climatiq API**).
3. **Data Tier (Database):**
   - **Database:** PostgreSQL (administered locally via pgAdmin 4).
   - **ORM / Data Access:** Entity Framework Core with `Npgsql.EntityFrameworkCore.PostgreSQL` (Code-First migrations).
4. **Infrastructure & Tooling:**
   - **Cloud & Hosting:** Microsoft Azure (API hosting & managed secrets).
   - **Version Control:** Git & GitHub.
   - **IDE:** Visual Studio Code (with C# and `.NET Install Tool` extensions).

---

## 4. Recommended Solution Structure
Keep concerns cleanly separated across projects in a single `.NET` solution:

```text
CarbonKickers.slnx
├── CarbonKickers.Client/        # WPF Desktop App (Views, ViewModels, Services/ApiClients)
├── CarbonKickers.Api/           # ASP.NET Core Web API (Controllers)
├── CarbonKickers.Application/   # Application use cases and interfaces
├── CarbonKickers.Domain/        # Entities, enums, and domain interfaces
├── CarbonKickers.Infrastructure/# EF Core, PostgreSQL, and external API clients
├── CarbonKickers.Shared/        # Shared DTOs and request/response contracts
└── tests/
    └── CarbonKickers.Tests/    # Unit & Integration Tests
```

---

## 5. Strict Agent Guardrails
> **CRITICAL RULES FOR AI CODING AGENTS:**
> 1. **No Direct DB Access from WPF:** `CarbonKickers.Client` must NEVER reference Entity Framework Core, `Npgsql`, or `DbContext`. All data operations must go through `CarbonKickers.Api`.
> 2. **No External API Keys in WPF:** Google Maps and Climatiq integrations live strictly in `CarbonKickers.Api`. The desktop client must never hold third-party API keys.
> 3. **No Hardcoded Secrets:** Never commit API keys, database passwords, or connection strings to source files or `appsettings.json`.
> 4. **Use Shared DTOs:** Never expose EF Core database entity models directly from API controllers; always map to DTOs defined in `CarbonKickers.Shared`.

### Secrets & Configuration Handling
- **Local Development:** Store connection strings and API keys in `CarbonKickers.Api` using `.NET Secret Manager` (`dotnet user-secrets set ...`).
- **Production (Azure):** Load credentials via Azure Key Vault or Azure App Service Environment Variables.

---

## 6. Team & Domain Ownership
- **Josh Jovancio** (`24/536680/TK/59554`) — Back-End (`CarbonKickers.Api`, PostgreSQL, External APIs)
- **Aurellia Safa Madrim** (`24/546878/TK/60781`) — Front-End (`CarbonKickers.Client`, WPF / MVVM)
- **Musa Hanif Moeljawan** (`24/533080/TK/59061`) — UI/UX (Design system, layouts, user flows)

---

## 7. Build, Run & Database Commands
Run commands from the repository root:

- **Build entire solution:** `dotnet build`
- **Run Backend API:** `dotnet run --project CarbonKickers.Api`
- **Run WPF Desktop Client:** `dotnet run --project CarbonKickers.Client`
- **Run Tests:** `dotnet test`
- **Add EF Core Migration:** `dotnet ef migrations add <MigrationName> --project CarbonKickers.Infrastructure --startup-project CarbonKickers.Api`
- **Apply Database Migrations:** `dotnet ef database update --project CarbonKickers.Infrastructure --startup-project CarbonKickers.Api`
