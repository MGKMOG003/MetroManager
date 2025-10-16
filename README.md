# MetroManager

A demo municipal services web app built with **ASP.NET Core 8**, **Entity Framework Core (SQLite)**, and **Identity**.

It lets citizens submit service tickets, browse announcements and local events, and gives admins a dashboard to manage it all.

[![.NET CI](https://github.com/MGKMG003/MetroManager/actions/workflows/dotnet.yml/badge.svg)](https://github.com/MGKMG003/MetroManager/actions/workflows/dotnet.yml)

---

## Contents

- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Database & Migrations](#database--migrations)
- [Seed Data](#seed-data)
- [Running](#running)
- [Key URLs](#key-urls)
- [Admin Dashboard](#admin-dashboard)
- [Client Dashboard](#client-dashboard)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## Tech Stack

- **.NET 8** (ASP.NET Core MVC + Razor Pages)
- **Entity Framework Core** (SQLite provider)
- **Identity** with roles: `Admin`, `Client`
- Bootstrap 5 UI

---

## Project Structure

MetroManager/
├─ src/
│ ├─ MetroManager.Domain/ # Entities & enums
│ ├─ MetroManager.Application/ # Application services & abstractions
│ ├─ MetroManager.Infrastructure/ # EF Core DbContext, repositories, migrations
│ └─ MetroManager.Web/ # MVC app (controllers, views, Identity, seeding)
├─ docs/ # Optional docs
├─ dev/ # Dev scripts/snippets (optional)
├─ MetroManager.sln
├─ LICENSE
└─ README.md

yaml
Copy code

SQLite database file lives at:

src/MetroManager.Web/metro.db

yaml
Copy code

---

## Getting Started

Prereqs:

- [.NET SDK 8.x](https://dotnet.microsoft.com/en-us/download)
- (Optional) EF CLI tool for local migration commands:
  ```powershell
  dotnet tool update --global dotnet-ef
Restore & build:

powershell
Copy code
dotnet restore
dotnet build
Run the web app:

powershell
Copy code
dotnet run --project src/MetroManager.Web
The app applies pending EF Core migrations automatically at startup.

Database & Migrations
This project uses SQLite. Connection string is Default in appsettings.json. By default the DB file is created under src/MetroManager.Web/metro.db.

Common EF commands (optional—startup already migrates):

powershell
Copy code
# Add a migration in the Infrastructure project, using the Web app as startup
dotnet ef migrations add <MigrationName> -p src/MetroManager.Infrastructure -s src/MetroManager.Web

# Update the database
dotnet ef database update -p src/MetroManager.Infrastructure -s src/MetroManager.Web
If you ever change the DB file location, update the connection string accordingly.

Seed Data
On first run the app seeds:

Roles: Admin, Client

An admin user

Sample announcements and events for the homepage and /events

If you saw “Role CLIENT does not exist” before, it just means seeding hadn’t run. With the current Program startup it seeds on launch, so re-run the app and retry registration.

Running
powershell
Copy code
dotnet run --project src/MetroManager.Web
Migrations are applied at startup.

Seeders run (roles, admin, demo content) once when missing.

Key URLs
Home: /

Login / Register: /Identity/Account/Login and /Identity/Account/Register

Events listing: /events

Admin dashboard: /Admin
(Note: previously trying /Identity/Admin gave a 404; use /Admin.)

Admin Dashboard
The admin dashboard lets you manage:

Tickets (filter, bulk status update, bulk delete)

Announcements (list)

Events (list)

Tickets index view: /Admin/Tickets

Bulk actions are CSRF-protected and available directly from the grid.

Client Dashboard
After registering/signing in as a Client, you’ll see:

My Service Requests (close/retract, where enabled)

Latest Announcements

Clients can also submit new tickets: Dashboard → “New Report” or Home → Report Issue.

Troubleshooting
MSB3021 / MSB3027: file locked by another process
Stop running processes and clean output:

powershell
Copy code
taskkill /IM iisexpress.exe /F 2>$null
taskkill /IM dotnet.exe /F 2>$null
Remove-Item "src\MetroManager.Web\bin","src\MetroManager.Web\obj" -Recurse -Force -ErrorAction SilentlyContinue
dotnet build
SQLite “duplicate column” during migration
Your DB file already contains an older/partial schema. Remove the local DB and let the app recreate it:

powershell
Copy code
Remove-Item ".\src\MetroManager.Web\metro.db",".\app_data\metro.db",".\metro.db" -Force -ErrorAction SilentlyContinue
dotnet run --project src/MetroManager.Web
Stale temp files accidentally added
If odd files like View(new or w.Ignore(...) appear (copy/paste artifacts), remove them and commit again.

Contributing
Fork, create a feature branch

Make changes with tests where possible

Submit a PR

License
This project is licensed under the MIT License. See LICENSE for details.

yaml
Copy code

---

# GitHub Actions workflow (`.github/workflows/dotnet.yml`)

```yaml
name: .NET CI

on:
  push:
    branches: [ "master", "main" ]
  pull_request:
    branches: [ "master", "main" ]

jobs:
  build:
    runs-on: windows-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.x

      - name: Restore
        run: dotnet restore

      - name: Build (Release)
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --configuration Release --no-build --verbosity normal
This builds all projects and runs tests on every push/PR to master (or main if you switch).

