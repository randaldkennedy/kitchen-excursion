# Kitchen Excursion

Part of **La Última Excursión**.

A personal recipe journal that combines recipes, cooking notes, shopping lists, and cook logs into a single Progressive Web App.

---

## Features

- Recipe search
- Category filters
- Recipe detail dialogs
- Cooking mode
- Shopping lists
- Cook Log (persistent)
- Mobile-friendly design
- Progressive Web App (installable)

---

## Project Structure

```
KitchenExcursion.Api/
    ASP.NET Core Web API
    Serves recipes
    Saves cook log entries

assets/
    Recipe photos

data/
    recipes.json

index.html
styles.css
app.js
```

---

## Technology

Frontend
- HTML
- CSS
- Vanilla JavaScript

Backend
- ASP.NET Core Minimal API (.NET 10)

Storage
- JSON file (temporary)
- SQLite planned

---

## Running Locally

### Frontend

Serve the site from IIS, GitHub Pages, or another web server.

Example:

http://localhost/kitchen-excursion/

### Backend

```
cd KitchenExcursion.Api
dotnet run
```

API:

```
http://localhost:5066/api/recipes
```

---

## Roadmap

Current work:
- ✅ Backend API
- ✅ Persistent Cook Log

Next:
- SQLite database
- Recipe editing
- Add recipes from the browser
- Image uploads
- Authentication
- Cloud deployment

See BACKLOG.md for the full roadmap.

---

## License

Personal project.