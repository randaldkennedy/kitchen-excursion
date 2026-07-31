# Randy's Recipe Book

Part of **La Última Excursión Kitchen**.

## Structure

- `index.html` — website shell
- `styles.css` — visual design
- `app.js` — search, filters, recipe dialogs, cooking mode, and shopping lists
- `data/recipes.json` — recipe content
- `assets/` — recipe photos

## Add a recipe

Add one recipe object to `data/recipes.json`, then add its photo to `assets/` and set the `image` property.

## Local testing

Because recipes load from JSON, serve the folder through IIS, GitHub Pages, VS Code Live Server, or another web server. Opening `index.html` directly from Windows may block the JSON request.
