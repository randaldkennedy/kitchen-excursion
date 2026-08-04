# Kitchen Excursion Changelog
# v0.3.0

Released: August 2026

## Added

- ASP.NET Core backend (KitchenExcursion.Api)
- Recipe API (`/api/recipes`)
- Persistent cook log API
- Frontend now loads recipes through the API
- Cook log entries are saved to `recipes.json`
- CORS support for local development
- Project reorganized for future hosting

## Changed

- Frontend prepared to be served directly from ASP.NET (`wwwroot`)
- API URLs are now relative (`/api`) instead of hardcoded localhost URLs
- Frontend assets moved into `KitchenExcursion.Api/wwwroot`

## Notes

This release marks the transition from a static website to a full-stack web application, enabling cloud hosting and persistent data.

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
