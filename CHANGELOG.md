# Kitchen Excursion Changelog

## v0.4.0 — In Development

### Added

- Azure App Service hosting for the complete ASP.NET Core application.
- GitHub Actions workflow for automatic deployment from `main`.
- Azure SQL Database using the ongoing free offer.
- Entity Framework Core SQL Server and design packages.
- `KitchenExcursionContext` database context.
- Initial `Recipe` entity and `Recipes` table.
- Initial EF Core migration and migration history tracking.
- One-time recipe seeding from `SeedData/recipes.json`.
- Azure SQL transient-failure retry configuration.
- `/api/version` deployment-information endpoint.
- Build metadata containing application version, short commit hash, and deployment time.
- .NET User Secrets for the local database connection string.
- Azure App Service connection-string configuration.

### Changed

- Moved the complete frontend into `KitchenExcursion.Api/wwwroot` so the API and website deploy as one application.
- Changed frontend API calls to use relative `/api` URLs.
- Moved `recipes.json` from `data` to `SeedData` to clarify its temporary role as seed/import data.
- Limited Azure deployment workflow triggers to application and workflow changes.
- Shortened the deployment commit hash to seven characters.
- Increased the Azure SQL connection timeout to tolerate serverless database wake-up time.
- Updated the recipe dialog so its content scrolls independently from the recipe list behind it.

### Fixed

- Fixed recipe dialogs opening at an inherited scroll position on iPhone.
- Preserved the user's recipe-list position while opening and closing a recipe.
- Removed the visible jump caused by smooth scrolling during page-position restoration.
- Fixed the recipe-dialog close button in iPhone Safari and Chrome by replacing the inline handler with an explicit event listener.
- Prevented the mobile close button from being clipped by the browser viewport.
- Fixed clipped recipe status labels and headings caused by horizontal dialog-content drift.
- Removed duplicate `dialogContent` markup.
- Corrected the Shopping List dialog close button so it closes the correct dialog.

### Current Transition

- The normalized Azure SQL foundation is live.
- The application still serves the complete recipe payload from `SeedData/recipes.json` while the remaining relational tables are designed and populated.
- The seeded `Recipes` table currently contains the core recipe-card fields.

---

## v0.3.0 — August 2026

### Added

- ASP.NET Core backend (`KitchenExcursion.Api`).
- Recipe API (`/api/recipes`).
- Persistent Cook Log API.
- Frontend recipe loading through the API.
- Cook Log entries saved to `recipes.json`.
- CORS support for local development.
- Project reorganization for full-stack hosting.

### Changed

- Prepared the frontend to be served directly from ASP.NET `wwwroot`.
- Replaced hardcoded localhost API URLs with relative `/api` URLs.
- Moved frontend assets into `KitchenExcursion.Api/wwwroot`.

### Notes

This release marked the transition from a static website to a full-stack web application.

---

## v0.2.6

- Split recipe filters into Meal, Protein, Method, and Status groups.
- Added structured recipe fields for meal, protein, cooking method, and status.
- Added Oven and Stovetop method filters.
- Added La Jefa Approved as a status filter.
- Kept visible recipe-card tags separate from internal filter values.

## v0.2.5

- Added Dinner to Smothered Pork Chops and Mississippi Pot Roast.
- Added the Dinner filter.
- Added this changelog.
