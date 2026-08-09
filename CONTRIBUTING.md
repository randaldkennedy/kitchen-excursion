# Contributing

This repository is part of the **La Última Excursión** project.

## Branch Naming Convention

Development work should be performed on a branch created from `main`.

Do not make development changes directly on `main`.

Branch names use this format:

`<type>/<short-description>`

Use lowercase letters and hyphens in the description.

### Features

Use `feature/` for new capabilities or functionality.

Examples:

- `feature/cook-log`
- `feature/backend-api`
- `feature/sqlite`
- `feature/recipe-editor`
- `feature/favorites`

### Bug Fixes

Use `bugfix/` for correcting broken or incorrect behavior.

Examples:

- `bugfix/cook-log-save`
- `bugfix/mobile-layout`
- `bugfix/search-filter`

### UI / UX

Use `ui/` for visual, navigation, layout, or usability improvements.

Examples:

- `ui/recipe-dialog`
- `ui/mobile-polish`
- `ui/dark-mode`
- `ui/navigation`

### Documentation

Use `docs/` for documentation-only changes.

Examples:

- `docs/readme`
- `docs/changelog`
- `docs/backlog`
- `docs/branch-naming-conventions`

### Infrastructure / DevOps

Use `infra/` for deployment, hosting, CI/CD, project structure,
database migration, and other infrastructure work.

Examples:

- `infra/azure-deployment`
- `infra/github-actions`
- `infra/project-cleanup`
- `infra/sqlite-migration`

## Standard Git Workflow

Start from an up-to-date `main` branch:

```powershell
git switch main
git pull origin main