
# MetroManager: Scope Document

## Document Control

* **Title:** MetroManager Scope Document
* **Owner:** Mogaki Mogaki
* **Date:** 2025-09-05
* **Status:** Planning & Design — Draft
* **Version:** 1.1
* **Related to:** (PROG7312 POE - Confidential, not for online publishing); Replaced with Project Brief Summary Document

---

## Purpose

MetroManager is a C# **ASP.NET Core MVC web application** designed to streamline municipal services in South Africa. It aims to improve citizen engagement and service delivery by enabling residents to report issues, access local events, and track service requests. This document defines the project’s scope.

---

## Project Overview

### 1. Context and Background

South African municipalities face challenges in responsiveness, citizen engagement, and service request tracking. MetroManager is conceptualised as a digital platform that enhances communication between citizens and municipalities while improving efficiency in handling requests.

### 2. Problem Statement

Citizens currently struggle with fragmented, manual, or inefficient methods for reporting municipal issues and staying updated on local services/events. A unified, user-friendly application is required to centralise these interactions.

### 3. Phased development:

* **Part 1:** Report Issues (core functionality).
* **Part 2:** Local Events & Announcements (advanced data structures).
* **Part 3:** Service Request Status (tracking using trees, heaps, and graphs).

---

## Objectives and Goals

### Business Objectives

* Provide a digital channel for citizens to engage with municipalities.
* Reduce delays in issue reporting and feedback loops.
* Improve transparency in municipal service management.

### Core Goals

* Enable structured reporting of municipal issues (location, category, description, media).
* Establish a foundation for citizen engagement strategies.
* Implement scalable design for future expansion.

### Secondary Goals

* Promote inclusivity by ensuring ease of use for diverse age and literacy groups.
* Encourage adoption through clear feedback and engagement features.
* Provide an administrative panel for municipal staff in later phases.

### Success Criteria

* Application meets academic rubric requirements across all phases.
* Standalone execution at each development phase.
* User interface is consistent, clear, and responsive.

### KPIs

* 100% coverage of functional requirements per phase.
* ≤10% similarity score for documentation (academic integrity).
* Positive lecturer evaluation aligned with rubric.

---

## Scope

### In-Scope

* Design of Report Issues module (MVC forms, models, controllers, SQLite integration).
* Planning of future modules (Events, Service Requests) without implementation.
* UI/UX design specifications (menus, navigation, layout consistency).
* Documentation deliverables (scope, design, reports, readme).

### Out-of-Scope

* Backend municipal system integrations beyond local demo database.
* Mobile versions (focus is web-based ASP.NET Core app).
* Real-time analytics or GIS mapping features.

---

## Planned Phases

1. **Discovery:** Requirements gathering from POE brief.
2. **Design:** UI mockups, data structure planning, navigation flow.
3. **Development (Future):** Incremental implementation per Part (1–3).
4. **Testing (Future):** Validation of forms, interactions, and data handling.
5. **Deployment (Future):** Hosting as an ASP.NET Core MVC web app with SQLite demo DB.
6. **Maintenance & Support (Future):** Iterative improvements, bug fixes, and updates based on lecturer feedback.

---

## Stakeholders

* **Student Developer:** Responsible for planning, design, coding, documentation.
* **Lecturer/Assessor:** Evaluates compliance with rubric and academic standards.
* **End-Users (Simulated Citizens):** Represented in design considerations.

---

## Requirements

### High-Level (Non-Technical)

* Must be intuitive for non-technical citizens.
* Should support multiple issue categories.
* Must allow media attachments for richer reporting.

### Functional

* **Part 1:** Report Issues form with inputs (location, category, description, media).
* **Part 2:** Events display + search + recommendations (planned).
* **Part 3:** Service status tracking using unique IDs and advanced structures (planned).

### Non-Functional

* Standalone runnable at each phase (demo-ready).
* Consistent MVC design patterns across modules.
* Efficient data structure usage (lists, dictionaries, trees, graphs per part).
* Local storage using SQLite for demo purposes.

---

## Constraints

* Must use **ASP.NET Core MVC**.
* SQLite will be the database for demo/testing (lightweight, file-based).
* Only *Report Issues* will be implemented in Part 1; other features must be disabled but visible.
* Documentation must not exceed 10% direct quotes (per academic policy).

---

## Compliance

* Academic compliance with PROG7312 POE requirements.
* Referencing style: **IEEE (for ICT projects)**.
* Integrity policies (no plagiarism, limited AI assistance as per rules).
* **POPIA Compliance:**

  * Limit collection of unnecessary personal data.
  * Data stored will be restricted to issue-related details (location, category, description, optional media).
  * No sensitive identifiers (ID numbers, addresses, contact details) will be captured.
  * Any demo/test data will be anonymised.
  * SQLite database will be secured with basic access controls to prevent unauthorised use.

---

## Security

* SQLite database for demo purposes with file-level protection.
* No external municipal integrations.
* No sensitive personal information collected (ensuring POPIA compliance).
* Input validation and sanitisation to mitigate basic risks.

---

## Assumptions

* Citizens/users simulated during evaluation.
* Application will run in local ASP.NET Core MVC environment.

---

## Risks

* **Technical Risk:** Incorrect implementation may prevent code from compiling (zero marks).
* **Documentation Risk:** Improper referencing may result in academic penalties.
* **POPIA Risk:** Any accidental inclusion of personal data could breach compliance; this will be mitigated by strict data collection limits.
* **Timeline Risk:** Delays in planning may impact phased submissions.

---

## Reviews & Validation

### Review Process

* **Peer Review:** Conducted by classmates or mentors (if applicable) to ensure clarity and alignment with POE brief.
* **Lecturer Review:** Assessor validation against rubric to confirm completeness and academic integrity.
* **Self-Validation:** Cross-checking against requirements in the POE brief and marking rubric to confirm all deliverables are addressed.

### Validation Checklist



### Approval

* **Prepared by:** Mogaki Mogaki (Student Developer)
* **Reviewed by:** \[Insert Reviewer/Lecturer Name]
* **Approval Date:** \[Insert Date after review]


