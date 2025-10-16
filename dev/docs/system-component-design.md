# Municipal Services Application — Overall System Components & Design

> Project: **PROG7312 POE – Municipal Services ("MunicipalManager")**   
> Technology: **ASP.NET Core MVC (.NET 8)**, **EF Core**, **SQLite**, **Bootstrap 5**   
> Compliance: **POPIA-aligned**, minimal PII, consent banners, data minimisation    
> Statusing Phases: **Part 1 → Part 2 → Part 3** (standalone runnable at each phase)

---

## 1) Purpose & Scope

This document provides a single, high‑level view of the system’s components, responsibilities, data flow, and deployment considerations. It is designed for rubric alignment, onboarding, and portfolio visibility. It also maps the Parts (1–3) to concrete components so the app remains runnable at each stage.

---

## 2) System Context

**Actors**

* **Citizen User** – submits issues, views events/announcements, checks request status.
* **Municipal Admin** – reviews/triages issues, publishes events/notices, updates statuses.
* **System Services** – EF Core/SQLite database, file storage (local), logging.

**External Systems** (optional/replaceable in demos)

* **Email/Notifications** (local stub or SMTP sandbox)
* **Static Map Tile Provider** (optional, non‑personalised)

---

## 3) Architectural Overview

A layered architecture keeps concerns separated and enables progressive enablement across assessment parts.

```
Browser (Citizen / Admin)
        │
        ▼
┌───────────────────────────── ASP.NET Core MVC ─────────────────────────────┐
│  Presentation Layer (Controllers + Views + ViewModels)                     │
│   - IssueController, EventsController, StatusController                    │
│   - Razor Views, strongly typed view models                                │
│                                                                             │
│  Application Layer (Use-Case Services + Ports)                              │
│   - IIssueService, IEventService, IRequestStatusService                     │
│   - Orchestrates workflows; no EF/SQLite code here                          │
│                                                                             │
│  Domain Layer (Entities + Value Objects)                                    │
│   - Issue, Event, ServiceRequest, Status (enum/VO)                          │
│   - Invariants/validation, pure business rules                              │
│                                                                             │
│  Infrastructure Layer (Adapters)                                            │
│   - EF Core Repositories (SQLite): IssueRepository, EventRepository         │
│   - Data Migrations/Seeding, FileStorageProvider (uploads)                  │
│   - Logging, Configuration                                                  │
└─────────────────────────────────────────────────────────────────────────────┘
        │
        ▼
SQLite (local file)  +  Local file system (media uploads)
```

### 3.1 Cross-Cutting Concerns

* **Security & POPIA**: data minimisation, explicit consent for uploads, privacy notice, no sensitive identifiers; rate-limiting and model validation to prevent abuse.
* **Validation**: server-side via DataAnnotations/FluentValidation; client-side via unobtrusive validation.
* **Logging**: ASP.NET built-in logging providers, request/exception logs, minimal user data.
* **Configuration**: `appsettings.json` with environment overrides; secrets excluded from repo.

---

## 4) Major Components & Responsibilities

### 4.1 Presentation Layer

* **Controllers**: `IssueController`, `EventsController`, `StatusController`, `HomeController`.
* **Views**: Razor pages grouped by feature (`Views/Issue/*`, `Views/Events/*`, `Views/Status/*`).
* **ViewModels/DTOs**: shape data for the UI; prevent domain leakage.
* **Styling/UI**: Bootstrap 5, responsive; colour tokens (Navy `#0B1D39`, Baby Blue `#E6F3FF`, Accent Orange).

### 4.2 Application Layer

* **Services (Use-Case orchestration)**: `IssueService`, `EventService`, `RequestStatusService` implement business workflows and call repositories.
* **Mapping**: ViewModels ⇄ Domain entities (manual or mapper).

### 4.3 Domain Layer

* **Entities**: `Issue`, `Event`, `ServiceRequest` (with `RequestId`/tracking code).
* **Value Objects/Enums**: `Status`, `Category`, `Location` (string/geo-lite), `Priority`.
* **Policies**: invariants (e.g., description length, allowed media types, status transitions).

### 4.4 Infrastructure Layer

* **Repositories (EF Core)**: `IIssueRepository`, `IEventRepository`, `IRequestRepository`.
* **DbContext**: `MunicipalDbContext` (SQLite provider), migrations, seeders.
* **File Storage**: local `/wwwroot/uploads` with whitelist + max size; DB stores file metadata only.
* **Email/Notifications**: interface-first; can be stubbed for demo.

---

## 5) Data Model (High Level)

### 5.1 Core Tables (SQLite)

* **Issues** (`Id`, `TrackingCode`, `Category`, `LocationText`, `Description`, `MediaPath?`, `CreatedAt`, `Status`)
* **Events** (`Id`, `Title`, `Category`, `StartDate`, `EndDate`, `LocationText`, `Details`, `Priority`, `CreatedAt`)
* **ServiceRequests** (`Id`, `TrackingCode`, `IssueId?`, `CurrentStatus`, `HistoryJson`, `UpdatedAt`)

> **Note:** Media is stored on disk; the DB keeps a relative path + content-type. No sensitive personal data recorded.

### 5.2 Indices & Keys

* Primary keys on `Id`; unique index on `TrackingCode`.
* Indices on `Category`, `CreatedAt`, and `StartDate` for Events.

---

## 6) Assessment Parts → Components Mapping

### Part 1 (Report Issues + Research) — **Runnable**

* **Features**: Issue submission form (Location, Category, Description, optional Media), Menu with disabled future features, SQLite persistence, simple confirmation page with tracking code.
* **Data Structures** *(for write-up and in-code examples)*: Dictionaries/HashSets for category lookup and de-dup; basic queues for background stub processing.
* **Deliverables**: Research (500 words, IEEE refs), POPIA notice, README with run steps.

### Part 2 (Events & Announcements) — **Runnable**

* **Features**: Events listing, search by category/date, lightweight recommendations (based on recent search patterns stored in memory or DB).
* **Required Data Structures**: Stacks/Queues/PriorityQueue, Dictionaries/SortedDictionary/HashSet, Sets.

  * Example: `PriorityQueue<Event, int>` for featuring events; `SortedDictionary<DateOnly, List<Event>>` for calendars.

### Part 3 (Service Request Status & Algorithms) — **Runnable**

* **Features**: Status dashboard with tracking by unique IDs, status history, admin updates.
* **Required Data Structures/Algorithms**: Trees (BST/AVL/Red‑Black) for quick request lookup by code; Heaps for prioritisation; Graphs + traversal (BFS/DFS) to model dependency flows; **MST** for routing maintenance teams (theoretical demo or visual explanation).
* **Reports**: Implementation Report (with examples), Completion Report, Technology Recommendations, Feedback Updates.

---

## 7) Data Flow (Happy Paths)

### 7.1 Submit Issue (Citizen)

1. User opens **Report Issue** form → fills `Location`, `Category`, `Description`, optional `Media`.
2. Controller validates input → Application service creates `Issue` and `ServiceRequest` with tracking code.
3. Repository saves to SQLite; media (if any) stored under `/wwwroot/uploads`.
4. User sees confirmation with **Tracking Code**.

### 7.2 Browse Events & Search

1. User opens **Events** → controller pulls upcoming events (date ≥ today).
2. Optional search: filter by category/date; recent searches cached per session.
3. Recommended events shown (priority queue + simple scoring).

### 7.3 Check Request Status

1. User enters **Tracking Code**.
2. Service queries repository (index on `TrackingCode`).
3. Returns current status + timeline; shows next steps.

---

## 8) POPIA & Security Controls

* **Data Minimisation**: only `Location` (free text), `Category`, `Description`, and optional media. No ID numbers, emails, phone numbers.
* **Consent**: checkbox for media upload + privacy notice link.
* **Retention**: demo policy (e.g., purge test data on rebuild/semester end).
* **Access Control**: Admin-only actions guarded by role; anti-forgery tokens on forms; strict model binding.
* **Validation**: whitelist file types (JPEG/PNG/PDF), max size, server-side verification.
* **Observability**: request/exception logs with redaction; no PII in logs.

---

## 9) Solution Structure (Repo Layout)

```
MunicipalManager/
├─ src/
│  ├─ MunicipalManager.Web/            # ASP.NET Core MVC app
│  │  ├─ Controllers/
│  │  ├─ Views/
│  │  ├─ ViewModels/
│  │  ├─ Domain/
│  │  ├─ Application/
│  │  ├─ Infrastructure/
│  │  │  ├─ Data/ (DbContext, Migrations, Seed)
│  │  │  └─ Storage/ (FileStorageProvider)
│  │  ├─ wwwroot/
│  │  ├─ appsettings*.json
│  │  └─ Program.cs, Startup.cs (or minimal hosting)
│  └─ MunicipalManager.Tests/          # Unit tests (optional)
├─ docs/
│  ├─ overall-system-componentsdesign.md
│  ├─ research-part1.md
│  ├─ implementation-report-part3.md
│  └─ completion-report-part3.md
├─ scripts/
│  └─ seed-dev-data.ps1
├─ README.md
└─ LICENSE
```

---

## 10) Build, Run & Environments

* **Local Dev**: `dotnet restore` → `dotnet ef database update` → `dotnet run`.
* **SQLite**: file-based DB created under `App_Data/municipal.db` or project root; migrations included.
* **Static Files**: uploads under `wwwroot/uploads`; ensure folder exists and is writeable.
* **App Settings**: `appsettings.Development.json` for dev toggles; sample file provided.

---

## 11) UI Conventions

* **Navigation**: top navbar – *Report Issues*, *Events & Notices*, *Request Tracking*.
* **States**: In Part 1, non-implemented pages appear but are disabled with tooltips.
* **Colour Tokens**: `--navy: #0B1D39; --navy-900: #08142A; --baby: #E6F3FF; --accent: #FF7A00;`.
* **Accessibility**: semantic HTML, labels, sufficient contrast, keyboard focus.

---

## 12) Testing Strategy (Lightweight)

* **Unit**: services and validators (domain rules, status transitions).
* **Integration**: repository tests against in-memory SQLite.
* **UI/Manual**: feature checklists per part; form validation & file upload tests.

---

## 13) Risks & Mitigations

* **File Upload Abuse** → MIME/type checks, size limits, quarantine folder.
* **Data Leakage** → minimise fields, redact logs, no PII in bug reports.
* **Non‑runnable phases** → keep stubs and feature toggles; seed data for demos.

---

## 14) Rubric Alignment Quick Map

* **Research (20)**: `docs/research-part1.md` with IEEE refs, ≤10% quotes.
* **Implementation (80)**: MVC form, SQLite, navigation with disabled features, responsive UI.
* **Part 2**: Events page + required data structures (doc + code examples).
* **Part 3**: Status tracking + tree/heap/graph/MST examples and explanations in report.

---

## 15) Future Enhancements (Optional)

* Swap SQLite for SQL Server (same EF Core model).
* Background job runner (Hangfire/Quartz) for notifications.
* Map visual on Events (static tiles, no tracking).
* Admin moderation queue with role-based UI.

---

### Appendix A: Minimal EF Entity Sketches (illustrative)

```csharp
public class Issue
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = default!; // unique
    public string Category { get; set; } = default!;
    public string LocationText { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? MediaPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Status Status { get; set; } = Status.Submitted;
}

public enum Status { Submitted, InReview, InProgress, Resolved, Closed }

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Category { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? LocationText { get; set; }
    public string? Details { get; set; }
    public int Priority { get; set; } = 0; // for PriorityQueue
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

**End of Document**
