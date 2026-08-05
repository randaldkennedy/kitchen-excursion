# Kitchen Excursion Backlog

Future work for the Kitchen Excursion project.

---



---

# High Priority

## Relational Data Model

- [x] Create Azure SQL Database.
- [x] Add Entity Framework Core.
- [x] Create the initial `Recipes` table.
- [x] Add and apply the initial database migration.
- [x] Seed core recipe records from `SeedData/recipes.json`.
- [ ] Create `Ingredients` table.
- [ ] Create `Units` table.
- [ ] Create `RecipeIngredients` table with quantity, unit, preparation, optional flag, and display order.
- [ ] Create `RecipeSteps` table.
- [ ] Create `Categories` and `RecipeCategories` tables.
- [ ] Create `Tags` and `RecipeTags` tables.
- [ ] Create `CookLogEntries` table.
- [ ] Create `RecipeMedia` table for recipe images, step images, videos, thumbnails, captions, and source links.
- [ ] Add indexes and uniqueness constraints for slugs, ingredient names, categories, tags, and lookup fields.

## Backend

- [ ] Move recipe loading completely to Azure SQL.
- [ ] Move Cook Log saving completely to Azure SQL.
- [ ] Preserve the current API response shape during the SQL migration.
- [ ] Validate API requests.
- [ ] Return consistent HTTP error responses.
- [ ] Add structured logging.
- [ ] Add database health and readiness checks.
- [ ] Avoid blocking application startup when the serverless database is waking.

## Ingredient Knowledge

- [ ] Add ingredient aliases and canonical ingredient matching.
- [ ] Add ingredient substitutions and substitution recipes.
- [ ] Support compound substitutions, such as cocktail sauce made from ketchup, horseradish, Worcestershire sauce, and hot sauce.
- [ ] Track ingredient storage location, shelf life, package sizes, and default shopping section.

---

# Medium Priority

## Recipe Management

- [ ] Add a Create Recipe page.
- [ ] Edit existing recipes.
- [ ] Delete or retire recipes.
- [ ] Add public, private, and do-not-use recipe-version statuses.
- [ ] Add recipe version history and aging/retention rules.
- [ ] Import a recipe from a URL.
- [ ] Show an import-review screen before saving.
- [ ] Upload recipe images.
- [ ] Drag-and-drop image support.
- [ ] Add step-specific images.
- [ ] Add recipe and step videos.
- [ ] Add related external videos and source attribution.

## Cook Log

- [ ] Edit Cook Log entries.
- [ ] Delete Cook Log entries.
- [ ] Search Cook Log entries.
- [ ] Sort by newest or oldest.
- [ ] Add ratings and outcome tags.
- [ ] Link Cook Log entries to the recipe version that was cooked.

## Shopping Lists

- [ ] Save shopping lists.
- [ ] Support multiple shopping lists.
- [ ] Accumulate ingredients from multiple selected recipes.
- [ ] Combine duplicate ingredients and quantities.
- [ ] Check off items across sessions.
- [ ] Organize items by store section.
- [ ] Subtract pantry inventory from generated shopping lists.
- [ ] Suggest substitutions when an ingredient is unavailable.

## Pantry

- [ ] Create pantry inventory tables.
- [ ] Track quantity, unit, storage location, opened date, expiration date, and minimum quantity.
- [ ] Record inventory transactions instead of only storing the current quantity.
- [ ] Add and remove pantry items by UPC barcode scan.
- [ ] Identify packaged products and map them to canonical ingredients.
- [ ] Answer “What can I cook with what I have?”
- [ ] Warn about ingredients approaching expiration.

## Grocery Trips and Receipts

- [ ] Create `Stores` table.
- [ ] Create `ShoppingTrips` table.
- [ ] Create `PurchaseItems` table with raw receipt description, quantity, unit price, total price, UPC, and ingredient/product mapping.
- [ ] Store receipt image URLs and metadata.
- [ ] Scan receipts to add an entire grocery trip to pantry inventory.
- [ ] Maintain grocery purchase and price history.
- [ ] Compare common item prices by store.
- [ ] Learn preferred stores and shopping patterns.

---

# Nice to Have

## UI

- [ ] Dark mode.
- [ ] Better animations.
- [ ] Keyboard shortcuts.
- [ ] Better mobile layout.
- [ ] Pantry and shopping dashboards.
- [ ] Camera-first barcode and receipt workflow.

## Search

- [ ] Ingredient search.
- [ ] Full-text recipe search.
- [ ] Search recipe notes.
- [ ] Search by available pantry ingredients.
- [ ] Search by substitutions and ingredient aliases.

## Cooking

- [ ] Built-in timers.
- [ ] Ingredient scaling.
- [ ] Unit conversion.
- [ ] Guided cooking mode with optional step media.
- [ ] Automatically deduct used pantry items after cooking.

---

# Long-Term Ideas

- [ ] User accounts.
- [ ] Share recipes.
- [ ] Public/private recipes.
- [ ] Recipe ratings.
- [ ] Meal planning.
- [ ] Grocery history and analytics.
- [ ] AI-assisted recipe import and normalization.
- [ ] Import recipes from videos or transcripts.
- [ ] Import handwritten recipe cards from photos.
- [ ] Receipt recognition and automatic purchase matching.
- [ ] Pantry-aware recipe recommendations.
- [ ] Store-section and optional aisle mapping.
- [ ] Product and brand preferences.
- [ ] Nutrition information.

---

# Technical Debt

- [ ] Split `app.js` into modules.
- [x] Use relative API URLs instead of a hardcoded localhost URL.
- [ ] Improve frontend and API error handling.
- [ ] Add automated tests.
- [ ] Improve API documentation.
- [ ] Add Swagger/OpenAPI UI after resolving the OpenAPI package warning.
- [ ] Replace startup-time seeding with an explicit import or administration workflow.
- [ ] Add separate development and production database configuration when a second environment is justified.
- [ ] Replace publish-profile deployment credentials with OpenID Connect.
- [ ] Remove archived project snapshots from the repository after confirming they are no longer needed.

---

# Completed

## v0.4.0 Foundation

- [x] Azure App Service hosting.
- [x] GitHub Actions automatic deployment.
- [x] Deployment path filters.
- [x] Version and build metadata endpoint.
- [x] Azure SQL Database.
- [x] Entity Framework Core SQL Server provider.
- [x] Initial migration and `__EFMigrationsHistory`.
- [x] Initial `Recipes` table.
- [x] Seed import from `SeedData/recipes.json`.
- [x] Local User Secrets connection string.
- [x] Azure App Service connection string.
- [x] Azure SQL transient retry configuration.

## v0.3.0

- [x] ASP.NET Core backend.
- [x] Recipe API endpoint.
- [x] Persistent Cook Log.
- [x] CORS support.
- [x] Frontend loads recipes through the API.
- [x] Frontend and API hosted as one ASP.NET application.
