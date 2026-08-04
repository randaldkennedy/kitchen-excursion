# Kitchen Excursion Backlog

Future work for the Kitchen Excursion project.

---

# High Priority

## Backend

- [ ] Move recipe loading completely to the API
- [ ] Move Cook Log saving completely to the API
- [ ] Validate API requests
- [ ] Return proper HTTP error responses
- [ ] Add logging

## Data

- [ ] Replace recipes.json with SQLite
- [ ] Create Recipe table
- [ ] Create CookLog table
- [ ] Add database migrations

---

# Medium Priority

## Recipe Management

- [ ] Add "Create Recipe" page
- [ ] Edit existing recipes
- [ ] Delete recipes
- [ ] Upload recipe images
- [ ] Drag-and-drop image support

## Cook Log

- [ ] Edit cook log entries
- [ ] Delete cook log entries
- [ ] Search cook log
- [ ] Sort by newest/oldest

## Shopping Lists

- [ ] Save shopping lists
- [ ] Multiple shopping lists
- [ ] Check off items across sessions

---

# Nice to Have

## UI

- [ ] Dark mode
- [ ] Better animations
- [ ] Keyboard shortcuts
- [ ] Better mobile layout

## Search

- [ ] Ingredient search
- [ ] Full-text recipe search
- [ ] Search recipe notes

## Cooking

- [ ] Built-in timers
- [ ] Ingredient scaling
- [ ] Unit conversion

---

# Long-Term Ideas

- [ ] User accounts
- [ ] Cloud database
- [ ] Share recipes
- [ ] Public/private recipes
- [ ] Recipe ratings
- [ ] Meal planning
- [ ] Grocery history
- [ ] AI recipe suggestions

---

# Technical Debt

- [ ] Split app.js into modules
- [ ] Move API URL into configuration
- [ ] Improve error handling
- [ ] Add automated tests
- [ ] Improve API documentation
- [ ] Add Swagger/OpenAPI UI

---

# Completed

## v0.3.0

- [x] ASP.NET Core backend
- [x] Recipe API endpoint
- [x] Persistent Cook Log
- [x] CORS support
- [x] Frontend loads recipes through the API