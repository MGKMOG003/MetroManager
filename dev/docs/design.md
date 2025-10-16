# MetroManager — System Design Document
## Document Control

* **Title:** MetroManager System Design (Architecture & Patterns)
* **Owner:** Mogaki Mogaki
* **Date:** 2025-09-06
* **Status:** Planning & Design Part 1— Draft
* **Version:** 1.0
* **Related:** Scope.md, ProjectBriefSummary.md
---

## 1) Purpose & Fit

This document defines the **architecture, boundaries, design patterns, security posture, and data design** for *MetroManager* part 1, a **C# ASP.NET Core MVC** web application with **SQLite** storage for demo purposes. It ensures the solution is **SOLID-compliant**, secure by design, and **traceable** to the POE brief’s phased deliverables:

* **Part 1:** Report Issues
* **Part 2:** Local Events & Announcements (with required data structures)
* **Part 3:** Service Request Status (trees, heaps, graphs)&#x20;

No implementation is included here.

---

## 2) Guiding Principles

### 2.1 Architecture Principles

* **SOLID**: Single Responsibility (thin controllers), Open/Closed (strategy & DI), Liskov (substitutable interfaces), Interface Segregation (narrow, intent-specific ports), Dependency Inversion (domain/application depend on abstractions).
* **Clean Architecture layering**: UI (MVC) → Application (use-cases) → Domain (entities/VOs) ← Infrastructure (SQLite, file storage) depends inward.
* **Separation of Concerns**: presentation, application logic, domain logic, persistence, cross-cutting concerns (logging, validation, caching) are isolated.
* **12-Factor Config** (environment-based): no secrets in code, configuration via environment variables.

### 2.2 Secure-by-Design & POPIA

* **Data minimisation**: only issue/event/request details; **no personal IDs/contact data**.
* **Privacy by design**: default to anonymised demo data, strict file-upload rules, retention & deletion controls.
* **OWASP alignment**: input validation, output encoding, CSRF protection, authz policies, sensible security headers.
* **POE constraints** and phased data structure use are met (lists in Part 1; stacks/queues/dictionaries/sets in Part 2; trees/heaps/graphs in Part 3).&#x20;

---

## 3) High-Level Architecture

```
Browser (Citizen / Admin)
        │
        ▼
┌───────────────────────────── ASP.NET Core MVC ─────────────────────────────┐
│  Presentation Layer (Controllers + Views + ViewModels)                     │
│   - IssueController, EventsController, StatusController                    │
│   - Razor Views, view models (no domain leakage)                           │
│                                                                             │
│  Application Layer (Use-Case Services + Ports)                              │
│   - IIssueService, IEventService, IRequestStatusService (interfaces)        │
│   - Coordinators/orchestrators; no EF/SQLite code here                      │
│                                                                             │
│  Domain Layer (Entities + Value Objects + Domain Events)                    │
│   - Issue, Event, ServiceRequest; Status (enum/VO);                         │
│   - Policies: validation/invariants; side-effect-free core                  │
│                                                                             │
│  Infrastructure Layer (Adapters)                                            │
│   - Repositories (EF Core/SQLite): IssueRepository, EventRepository         │
│   - Data Migrations, FileStorageProvider (uploads)                          │
│   - Logging (Serilog), Caching, Email stub (future)                         │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Dependency Rule:** Presentation/Application/Domain never depend on Infrastructure concretions; use **DI containers** to bind interfaces to implementations at composition root.

---

## 4) Module Decomposition & Responsibilities

### 4.1 Presentation (MVC)

* **Controllers:** Orchestrate use-cases; zero business rules.
* **Filters/Middleware:** global exception mapping → problem details; anti-forgery, security headers, rate limiting.
* **View Models & Mappers:** map between DTOs and views; prevent domain leakage.

### 4.2 Application (Use-Cases / Services)

* **Service Interfaces:** `IIssueService`, `IEventService`, `IRequestStatusService`.
* **Orchestration:** input validation, call domain policies, coordinate repositories, return DTOs.
* **No persistence code** here; persistence is accessed via **ports** (repository interfaces).

### 4.3 Domain (Enterprise Rules)

* **Entities:** `Issue`, `Event`, `ServiceRequest`.
* **Value Objects:** `Category`, `Location`, `RequestStatus`.
* **Invariants:** e.g., `Issue.Description` required; `RequestStatus` transitions valid (Pending→InProgress→Resolved).
* **Domain Events (optional later):** `IssueReported`, `StatusChanged`.

### 4.4 Infrastructure (Adapters)

* **EF Core / SQLite Repositories:** concrete `IssueRepository`, `EventRepository`, `ServiceRequestRepository`.
* **Migrations Plan:** schema created via tooling; migration files versioned; idempotent local setup.
* **File Storage Provider:** validates type/size, stores outside web root, generates random file names, persists metadata only.
* **Logging:** structured logs (Serilog) to rolling file; correlation IDs.

---

## 5) Key Design Patterns (Where / Why)

| Concern          | Pattern                                    | Why (SOLID/Security)                                       |
| ---------------- | ------------------------------------------ | ---------------------------------------------------------- |
| Layer boundaries | **Ports & Adapters** (Hexagonal)           | Dependency Inversion; testability; swap SQLite later.      |
| Use-cases        | **Application Services**                   | Single Responsibility; orchestration isolated from UI/DB.  |
| Persistence      | **Repository + UoW (EF Core context)**     | Encapsulate queries; transaction boundaries; test doubles. |
| Validation       | **Decorator / Pipeline** around services   | Centralise validation; reusable; open for extension.       |
| Mapping          | **Assembler/Mapper**                       | Prevent fat controllers; DTO↔VM↔Domain isolation.          |
| Policies         | **Strategy** (e.g., event recommendations) | Open/Closed for different strategies in Part 2.            |
| Status evolution | **State/Policy** for RequestStatus         | Valid transitions; simpler rule checking.                  |
| Caching          | **Cache Aside**                            | Keep services pure; improve read perf w/out coupling.      |
| Files            | **Gateway** (FileStorageProvider)          | Centralised controls for type/size/path; easy to harden.   |

---

## 6) Core Use-Cases (Phased)

### 6.1 Part 1 — Report Issues (Minimum Viable)

* **Actors:** Citizen (anonymous), Admin (future).
* **Flow:** Create Issue → Validate → Store (SQLite) → Acknowledge.
* **Data Structures:** **List\<Issue>** at service boundary (and persisted via repository) to align with Part 1 expectations.&#x20;
* **Non-Goals:** No personal data; no external integrations.

### 6.2 Part 2 — Local Events & Announcements

* **Search & Display:**

  * **Queues**: upcoming events ordered by start time for “next up”.
  * **PriorityQueue**: highlight urgent/featured events.
  * **Dictionaries/SortedDictionary**: fast category/date indices.
  * **Sets**: unique categories/tags.&#x20;
* **Recommendation Strategy (pluggable):** based on recent searches (**Stack** for last N queries) + weighted categories (Dictionary counters).

### 6.3 Part 3 — Service Request Status

* **Tracking:**

  * **Tree / Balanced BST**: index requests by updated date / priority range queries.
  * **Heap**: prioritise unresolved requests by severity/SLA.
  * **Graph**: model dependencies (e.g., water main fix before road repair); simple traversals for reachable next steps; optional **MST** demo on grouped tasks (academic illustration).&#x20;

---

## 7) Data Design (Conceptual)

### 7.1 ER Overview (Conceptual)

* **Issue**(IssueId, Location, Category, Description, MediaRef?, CreatedAt)
* **ServiceRequest**(RequestId, IssueId↦Issue, Status, UpdatedAt)
* **Event**(EventId, Title, Category, EventDate, Description)

**Rules:**

* `ServiceRequest.IssueId` required; status ∈ {Pending, InProgress, Resolved}.
* Media files stored via `MediaRef` metadata only; blob outside web root.

### 7.2 Data Dictionary (selected)

| Entity         | Field             | Notes                                               |
| -------------- | ----------------- | --------------------------------------------------- |
| Issue          | IssueId (GUID)    | Primary key; generated in app layer.                |
| Issue          | Category (string) | Enum/lookup later; validated.                       |
| ServiceRequest | Status            | Value object; state transitions enforced by policy. |
| Event          | EventDate         | Local date/time; indexed for range queries.         |

### 7.3 SQLite Usage

* **Why SQLite:** portable demo DB; zero infra.
* **Concurrency:** short transactions; single-writer awareness; backoff/retry.
* **Migrations:** versioned schema; automated create/update in dev only.
* **Resilience:** integrity checks; foreign keys ON; indices on `EventDate`, `UpdatedAt`.

---

## 8) Security Architecture

### 8.1 Threat Model (scope-appropriate)

* **T1:** Malicious uploads → **M1:** allowlist MIME/extension; max size; scan filenames; randomise keys; store outside web root; strict download endpoint with authZ (admin only).
* **T2:** Input injection (XSS/SQLi) → **M2:** model binding + server validation; output encoding; parameterised queries (EF Core); CSP.
* **T3:** CSRF → **M3:** Anti-forgery tokens on mutating requests.
* **T4:** Enumeration/info leaks → **M4:** generic error pages; no stack traces to client; correlation IDs in logs.
* **T5:** Excessive requests → **M5:** rate limiting middleware; request body limits on uploads.
* **T6:** Sensitive data → **M6:** **do not collect** IDs/contacts; minimise fields; timed retention; purge scripts.

### 8.2 POPIA Alignment (privacy by design)

* **Minimisation:** Only categories, locations (non-exact where possible), descriptions, optional media.
* **Transparency:** simple privacy notice in app footer.
* **Retention:** demo data purged at project end or on demand.
* **Rights:** export/delete demo items on request (admin view).
* **Security:** DB file access protected; no public download of raw files; secure headers (HSTS, X-Content-Type-Options, X-Frame-Options via `frame-ancestors`, Referrer-Policy).

---

## 9) Validation & Data Quality

* **Synchronous validation** (DataAnnotations or Fluent rules) at controller boundary.
* **Cross-field policies** in application/domain (e.g., description length, category validity).
* **Anti-tamper**: server-side re-validation; never trust client.

---

## 10) Cross-Cutting Concerns

### 10.1 Logging & Observability

* **Structured logging** (request ID, user agent, route, outcome).
* **Audit trail** (admin actions only).
* **PII policy:** logs contain no personal data.

### 10.2 Error Handling

* Global exception handler → **ProblemDetails** responses; error codes mapped; friendly UI messages.

### 10.3 Caching

* **Read-through** on events list; short TTL to keep demo fresh; cache invalidated on admin updates.

### 10.4 Configuration & Secrets

* All secrets via environment variables; **no secrets in source**; local dev uses dotfiles ignored by VCS.

### 10.5 Accessibility & UX

* Keyboard navigable forms; ARIA labels; color contrast; clear error summaries; simple language.

---

## 11) Testing Strategy (No Code)

* **Unit tests** (domain rules, use-case services via in-memory repos).
* **Integration tests** (controllers + EF Core SQLite in-memory/file).
* **Security tests** (CSRF present, headers present, upload guardrails).
* **Data-structure tests** (Part 2/3): verify queue/heap/tree/graph behaviors with seeded data.
* **Traceability:** test cases mapped to POE features & parts.&#x20;

---

## 12) Deployment & Ops (Demo Scope)

* **Runtime:** .NET ASP.NET Core MVC, local Kestrel; optional IIS/Reverse proxy if needed.
* **Database:** SQLite file in protected app data path.
* **Build:** release configuration produces self-contained artifact; **no background services** required.
* **Backups:** copy DB file as needed in dev (demo only).

---

## 13) Risks & Mitigations

| Risk            | Impact         | Mitigation                                           |
| --------------- | -------------- | ---------------------------------------------------- |
| Feature creep   | Delays         | Lock scope per part; feature flags.                  |
| Data misuse     | POPIA breach   | Strict minimisation; validation; retention purge.    |
| Upload abuse    | Storage/attack | Size caps; type allowlist; outside web root storage. |
| Tight deadlines | Quality drop   | Phased delivery; tests first; linting & checklists.  |

---

## 14) Review & Acceptance

* **Design Reviews:** peer + lecturer walkthrough against this document.
* **Checklists:** SOLID compliance, security controls present, data minimisation, DS per part aligned to brief.&#x20;
* **Sign-off:** document status remains *Draft* until lecturer approval.

---

## 15) Glossary (selected)

* **Domain Entity:** Business object with identity and rules.
* **Value Object:** Immutable object defined by its values.
* **Port/Adapter:** Interface (port) with environment-specific implementation (adapter).
* **Repository:** Abstraction over persistence.
* **POPIA:** South Africa’s data protection law.

---

### Traceability to POE

This design enables the exact **phased features** and **data-structure usage** required by the brief for Parts 1–3 (lists; stacks/queues/dictionaries/sets; trees/heaps/graphs), while remaining SOLID and secure.&#x20;

---

If you want, I can also draft concise **sequence diagrams** (text/PlantUML) for the three core flows—Report Issue, Search Events, Update Status—to embed here.
