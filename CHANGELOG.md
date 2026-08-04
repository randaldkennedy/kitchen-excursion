# Kitchen Excursion Changelog
## v0.3.0 — In Development

### Added
- Added an ASP.NET Core Minimal API using .NET 10.
- Added `GET /api/recipes` to serve recipe data.
- Added `POST /api/recipes/{id}/cook-log` to save Cook Log entries.
- Added persistent Cook Log entries backed by `data/recipes.json`.
- Added a Cook Log entry form with autofocus.
- Added a top-right close button to recipe and shopping dialogs.
- Added support for opening recipes by clicking their images.
- Added `.gitignore` rules for .NET build output and editor files.

### Changed
- Updated the frontend to load recipes through the API instead of reading `recipes.json` directly.
- Renamed “Randy’s notes” to “Recipe notes.”
- Improved Cook Log formatting and date display.
- Improved recipe dialog and Cook Log editor styling.

### Fixed
- Fixed Cook Log expand and collapse behavior.
- Fixed duplicate JavaScript that prevented recipes from rendering.
- Added CORS support so the IIS-hosted frontend can call the local API.

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
