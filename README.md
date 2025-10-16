Perfect — since your MetroManager app now builds and runs correctly (including seeding, admin dashboard, client registration, and events/announcements), here’s a full **updated `README.md`** suitable for your repo root.

---

```markdown
# MetroManager – Municipal Services Web Application

A secure ASP.NET Core 8 MVC web application for citizens and municipal administrators to report, track, and manage local service issues, community events, and announcements.

---

## 🚀 Overview

**MetroManager** enables citizens to submit municipal service requests and view updates, while administrators manage issues, announcements, and community events from a unified dashboard.

This project is part of the **INSY7314 / APDS7311 POE 2025** assignment, demonstrating secure, modern web app design with authentication, authorization, seeding, and data management features.

---

## 🧩 Features

### **Citizen (Client)**
- Register and log in securely via Identity.
- Create and manage service issue reports.
- View announcement updates and upcoming events.
- Access a personal dashboard to track open and closed issues.

### **Administrator**
- Log in via seeded admin credentials.
- Manage:
  - **Tickets / Issues**
  - **Announcements**
  - **Local Events**
- Perform bulk actions (update status, delete multiple).
- View metrics (total tickets, events, announcements).

### **System**
- Secure password hashing & role-based access.
- Entity Framework Core 8 (SQLite provider).
- Automatic database migrations on startup.
- Data seeding for:
  - Admin user & roles (`Admin`, `Client`)
  - Mock announcements (2)
  - Mock events (3)
- Responsive Bootstrap 5 front-end.

---

## 🧠 Architecture

**Solution Structure**

```

src/
├── MetroManager.Domain/           # Domain models & enums
├── MetroManager.Application/      # Application services, interfaces
├── MetroManager.Infrastructure/   # EF Core DbContext, repositories, seeding
├── MetroManager.Web/              # ASP.NET Core MVC + Razor UI
└── MetroManager.Tests/            # Unit tests

````

**Tech Stack**

| Layer | Technology |
|-------|-------------|
| Backend | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 (SQLite) |
| Authentication | ASP.NET Core Identity |
| Frontend | Bootstrap 5, Razor Pages |
| Logging | Microsoft.Extensions.Logging |
| Build | .NET 8 SDK |

---

## ⚙️ Setup Instructions

### 1. **Prerequisites**
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or VS Code
- SQLite (included in EF Core provider)

### 2. **Clone the Repository**
```bash
git clone https://github.com/<your-org-or-user>/MetroManager.git
cd MetroManager
````

### 3. **Clean & Restore**

```bash
dotnet clean
dotnet restore
```

### 4. **Run EF Migrations**

Rebuild the SQLite database and seed defaults:

```bash
taskkill /IM iisexpress.exe /F 2>$null
taskkill /IM dotnet.exe /F 2>$null
Remove-Item "src\MetroManager.Web\bin","src\MetroManager.Web\obj" -Recurse -Force -ErrorAction SilentlyContinue
dotnet ef database update -p src/MetroManager.Infrastructure -s src/MetroManager.Web
```

### 5. **Run the App**

```bash
dotnet run --project src/MetroManager.Web
```

Then open the browser at:

```
https://localhost:44355/
```

---

## 👥 Default Users

| Role              | Email                                           | Password     | Notes                  |
| ----------------- | ----------------------------------------------- | ------------ | ---------------------- |
| **Admin**         | [admin@metro.local](mailto:admin@metro.local)   | `Admin123!`  | Access Admin Dashboard |
| **Client (demo)** | [client@metro.local](mailto:client@metro.local) | `Client123!` | Create/view reports    |

---

## 📦 Seeded Data

| Type          | Count | Example                                                        |
| ------------- | ----- | -------------------------------------------------------------- |
| Announcements | 2     | “Scheduled Maintenance”, “Water Interruption Notice”           |
| Events        | 3     | “Community Clean-up”, “Safety Awareness Day”, “Public Meeting” |
| Issues        | 0     | Created dynamically by users                                   |

Seeding is handled in `IdentitySeedExtensions.SeedIdentityAsync()` and `DbInitializer.SeedDomainAsync()` during startup.

---

## 🖥️ Key Pages

| Area     | Path                         | Description                                    |
| -------- | ---------------------------- | ---------------------------------------------- |
| Public   | `/`                          | Home page with announcements & upcoming events |
| Identity | `/Identity/Account/Register` | Client registration                            |
| Client   | `/Dashboard`                 | Client issue tracking                          |
| Admin    | `/Admin`                     | Admin overview dashboard                       |
| Admin    | `/Admin/Tickets`             | Manage issue tickets                           |
| Admin    | `/Admin/Announcements`       | Manage announcements                           |
| Admin    | `/Admin/Events`              | Manage events                                  |

---

## 🔐 Security Features

* HTTPS enforced
* ASP.NET Core Identity (password hashing & salting)
* Role-based authorization (`[Authorize(Roles="Admin")]`)
* Anti-forgery tokens (`@Html.AntiForgeryToken()`)
* Input sanitization and model validation
* Separation of domain, infrastructure, and web layers

---

## 🧪 Testing

Run all unit tests:

```bash
dotnet test
```

---

## 📁 Sample Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=metro.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🏁 Current Build Status

* ✅ EF Core schema in sync
* ✅ Admin seeding & role setup
* ✅ Client registration & redirect
* ✅ Events & Announcements seeding and display
* ✅ Admin dashboard functional

---

## 📚 Authors

**Katiso Mogaki**
INSY7314 / APDS7311 – 2025
Faculty of Information Technology
Vaal University of Technology

---

## 📄 License

This project is released for educational purposes under the MIT License.

```

---

Would you like me to tailor the README for **academic submission** (with POE formatting and rubric mapping like “Task 2: Implementation Phase – Secure Portal Build”) or keep it as a **developer-oriented repo README** like above?
```
