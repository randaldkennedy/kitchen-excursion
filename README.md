# Kitchen Excursion

Part of **La Última Excursión**.

Kitchen Excursion is a personal cooking journal and recipe-management application for preserving recipes, recording cook notes, building shopping lists, and eventually managing pantry inventory and grocery history.

---

## Features

- Recipe search and structured filters
- Recipe detail dialogs with preserved browsing position across desktop and mobile
- Cooking mode
- Shopping-list view
- Persistent Cook Log
- Mobile-friendly, installable Progressive Web App
- Public Azure-hosted deployment
- Automated deployments from GitHub Actions
- Deployment version and build metadata endpoint

---

## Architecture

```text
Browser / Phone
    |
    v
ASP.NET Core Minimal API (.NET 10)
    |-- Serves the frontend from wwwroot
    |-- Exposes /api endpoints
    |-- Uses Entity Framework Core
    |
    +--> Azure SQL Database

GitHub main branch
    |
    v
GitHub Actions
    |
    v
Azure App Service
```

The application currently continues to serve complete recipe data from `SeedData/recipes.json` while the normalized Azure SQL schema is built out. The initial `Recipes` table and EF Core migration are in place, and the existing recipes have been seeded into Azure SQL.

---

## Project Structure

```text
KitchenExcursion.Api/
    Data/
        KitchenExcursionContext.cs

    Migrations/
        Entity Framework Core migrations

    Models/
        Database entity models

    SeedData/
        recipes.json
        DatabaseSeeder.cs
        SeedRecipe.cs

    wwwroot/
        assets/
        app.js
        index.html
        styles.css

    Program.cs
    appsettings.json
```

---

## Technology

### Frontend

- HTML
- CSS
- Vanilla JavaScript
- Progressive Web App

### Backend

- ASP.NET Core Minimal API
- .NET 10
- Entity Framework Core

### Data

- Azure SQL Database
- EF Core code-first migrations
- JSON seed/import file during the database migration

### Hosting and Deployment

- Azure App Service
- GitHub Actions continuous deployment from `main`
- Azure App Service connection strings
- .NET User Secrets for local development

---

## Running Locally

From the repository root:

```powershell
dotnet run --project KitchenExcursion.Api
```

Then open:

```text
http://localhost:5066/
```

Useful API endpoints:

```text
http://localhost:5066/api/recipes
http://localhost:5066/api/version
```

The local Azure SQL connection string is stored with .NET User Secrets and must not be committed to the repository.

---

## Current Milestone

**v0.4.0 — Azure SQL foundation**

Completed foundation work:

- Azure-hosted ASP.NET application
- GitHub Actions deployment pipeline
- Azure SQL Database
- Entity Framework Core integration
- Initial database migration
- Recipe seed import
- Local and Azure connection-string configuration
- Cross-browser recipe-dialog scrolling and close-button behavior
- Recipe-list position preservation when opening and closing a recipe

Next:

- Normalize ingredients, recipe steps, categories, tags, cook logs, and media
- Move all recipe reads and writes from JSON to Azure SQL
- Address small UI and API bugs before expanding the feature set
- Build recipe creation and editing inside the application

See `BACKLOG.md` for the full roadmap.

---

## License

Personal project.
