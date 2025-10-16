# Report Issues — UX + Form Design (Part 1, updated)

**Purpose:** Let citizens report municipal issues quickly, cleanly, and anonymously (no personal identifiers).

**Form fields (per your new scope):**

* **Location** (required): Google Places Autocomplete text box.

  * **Geo-tag (optional):** hidden fields for **Latitude** and **Longitude** (auto from place result or “Use my location”).
  * **Location Note (optional):** small textbox for extra clues (e.g., “Opposite the blue gate”).
* **Category** (required): primary dropdown (e.g., Pothole, Streetlight, Water, Sewer, Illegal Dumping, Noise, Vandalism, Other).
* **Subcategory** (required when applicable): dependent dropdown populated based on Category (e.g., Streetlight → “Light out”, “Exposed wires”, “Broken pole”).
* **Description** (required): 50–1000 chars; placeholder provides examples (severity, hazards, landmarks).
* **Media** (optional): images (JPG/PNG ≤ 5 MB) or video (MP4 ≤ 15 MB).

  * Show **filename + size** immediately.
  * **Upload animation**: progress bar during submission.
* **Consent** (required): “I confirm this report contains no personal info.” (checkbox gates submit).

**POPIA cues:**

* Banner: “Don’t include names, phone numbers, ID numbers, or faces.”
* Real-time PII quick-scan (emails/phones/SA ID) with red highlight + error.
* No contact fields.

**Accessibility & UI:**

* Proper labels, `aria-describedby`, visible focus, high contrast (≥4.5:1).
* Mobile-first; drag-and-drop + click-to-upload.
* Menu shows **Events** and **Request Tracking** disabled with “Coming in Part 2/3” tooltips.

---

# Workflow Logic (End-to-End, updated)

1. **Client validation:** required fields, description length, file type/size; PII quick-scan; ensure subcategory is valid for chosen category.
2. **Location logic:**

   * Places Autocomplete fills **Location**; we attempt to capture **lat/lng**.
   * Button “Use my location” (optional) requests browser geolocation → fills lat/lng and a rough text if Location empty.
3. **POST /Issue/Create** → **Server validation** (authoritative):

   * Sanitize/encode inputs; enforce lengths; re-scan for PII.
   * Verify **(Category, Subcategory)** pair is allowed.
   * Media whitelist + size caps; safe random filename; store under `/wwwroot/uploads/issues/YYYY/MM/`.
4. **Tracking code:** `MM-{yyyy}-{00001}` (per-year counter).
5. **Persist (SQLite, EF Core)**: location, **lat/lng** (nullable), **location note**, category/subcategory, description, media path, timestamps, status=Submitted.
6. **Confirmation**: show tracking code, category (and subcategory), timestamp; “Save as PDF/Print.”
7. **Thank-you toast** → back to Home; **Request Tracking** remains disabled (Part 3).
8. **Data minimisation:** Only store: location, location note, lat/lng (optional), category/subcategory, description, media path, metadata (ids/timestamps/status).

   * Retention text: “Reports retained for service delivery and audit; media stored only as needed.”

---

## Data Model (SQLite, EF Core) — updated

```csharp
// Models/Issue.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MunicipalManager.Models
{
    public enum IssueStatus { Submitted = 0, Acknowledged = 1, InProgress = 2, Resolved = 3, Rejected = 4 }

    public class Issue
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string TrackingCode { get; set; } = default!;

        // Google-formatted place text the user sees (or manual text)
        [Required, StringLength(160)]
        public string Location { get; set; } = default!;

        // Optional extra hint from user (e.g., "Opposite blue gate")
        [StringLength(120)]
        public string? LocationNote { get; set; }

        // Optional geo tags
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required, StringLength(64)]
        public string Category { get; set; } = default!;

        [StringLength(64)]
        public string? Subcategory { get; set; }

        [Required, StringLength(1000), MinLength(50)]
        public string Description { get; set; } = default!;

        [StringLength(512)]
        public string? MediaPath { get; set; }

        [StringLength(32)]
        public string? MediaType { get; set; } // "image/jpeg", "video/mp4"

        public IssueStatus Status { get; set; } = IssueStatus.Submitted;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(64)]
        public string? ContentHashSha256 { get; set; }
    }
}
```

```csharp
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using MunicipalManager.Models;

namespace MunicipalManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Issue> Issues => Set<Issue>();
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Issue>().HasIndex(i => i.TrackingCode).IsUnique();
            b.Entity<Issue>().Property(i => i.Category).HasDefaultValue("Other");
            b.Entity<Issue>().HasIndex(i => new { i.Category, i.Subcategory });
            b.Entity<Issue>().HasIndex(i => new { i.Latitude, i.Longitude });
        }
    }
}
```

```json
// appsettings.json (SQLite)
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=municipalmanager.db"
  }
}
```

```csharp
// Program.cs (DI)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## Controller (with Google/fallback, categories + subcategories, validation, progress-friendly)

```csharp
// Controllers/IssueController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MunicipalManager.Data;
using MunicipalManager.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class IssueController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedImage = { "image/jpeg", "image/png" };
    private static readonly string[] AllowedVideo = { "video/mp4" };

    // Server-side source of truth for cats/subcats
    private static readonly Dictionary<string, string[]> CatMap = new()
    {
        ["Pothole"]        = new[] { "Small (<30cm)", "Medium (30–60cm)", "Large (>60cm)" },
        ["Streetlight"]    = new[] { "Light out", "Flickering", "Exposed wires", "Broken pole" },
        ["Water Leak"]     = new[] { "Burst pipe", "Meter leak", "Road leak", "Property boundary" },
        ["Sewer"]          = new[] { "Overflow", "Blocked", "Manhole missing" },
        ["Illegal Dumping"]= new[] { "Household waste", "Construction rubble", "Hazardous (suspected)" },
        ["Noise"]          = new[] { "After-hours", "Industrial", "Vehicles" },
        ["Vandalism"]      = new[] { "Signage", "Park equipment", "Public building" },
        ["Other"]          = Array.Empty<string>()
    };

    public IssueController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db; _env = env;
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = CatMap.Keys.ToList();
        ViewBag.CatMapJson = JsonSerializer.Serialize(CatMap);
        return View();
    }

    [ValidateAntiForgeryToken]
    [HttpPost, RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Create(Issue vm, IFormFile? media)
    {
        ViewBag.Categories = CatMap.Keys.ToList();
        ViewBag.CatMapJson = JsonSerializer.Serialize(CatMap);

        // POPIA guardrails
        if (ContainsPII(vm.Description) || ContainsPII(vm.Location) || ContainsPII(vm.LocationNote))
            ModelState.AddModelError("", "Please remove phone numbers, emails or ID numbers.");

        // Category/Subcategory validation
        if (string.IsNullOrWhiteSpace(vm.Category) || !CatMap.ContainsKey(vm.Category))
            ModelState.AddModelError("Category", "Please choose a valid category.");

        var allowedSubs = CatMap.TryGetValue(vm.Category ?? "", out var subs) ? subs : Array.Empty<string>();
        if (allowedSubs.Length > 0 && string.IsNullOrWhiteSpace(vm.Subcategory))
            ModelState.AddModelError("Subcategory", "Please choose a subcategory.");
        if (!string.IsNullOrWhiteSpace(vm.Subcategory) && allowedSubs.Length > 0 && !allowedSubs.Contains(vm.Subcategory))
            ModelState.AddModelError("Subcategory", "Invalid subcategory for selected category.");

        // Media validation
        string? mediaPath = null; string? mediaType = null;
        if (media != null && media.Length > 0)
        {
            if (media.Length > 15_000_000) ModelState.AddModelError("", "Media exceeds size limit.");
            var ct = media.ContentType.ToLowerInvariant();
            var ok = AllowedImage.Contains(ct) || AllowedVideo.Contains(ct);
            if (!ok) ModelState.AddModelError("", "Unsupported media type.");
        }

        if (!ModelState.IsValid) return View(vm);

        // Save file
        if (media != null && media.Length > 0)
        {
            var y = DateTime.UtcNow.Year; var m = DateTime.UtcNow.Month.ToString("00");
            var dir = Path.Combine(_env.WebRootPath, "uploads", "issues", y.ToString(), m);
            Directory.CreateDirectory(dir);
            var safeName = $"{Guid.NewGuid():N}{Path.GetExtension(media.FileName)}";
            var fullPath = Path.Combine(dir, safeName);
            using var fs = System.IO.File.Create(fullPath);
            await media.CopyToAsync(fs);
            mediaPath = $"/uploads/issues/{y}/{m}/{safeName}";
            mediaType = media.ContentType.ToLowerInvariant();
        }

        // Dedup helper
        var hash = ComputeSha256($"{vm.Location}|{vm.Latitude}|{vm.Longitude}|{vm.Category}|{vm.Subcategory}|{vm.Description}");

        var issue = new Issue
        {
            Location = vm.Location.Trim(),
            LocationNote = string.IsNullOrWhiteSpace(vm.LocationNote) ? null : vm.LocationNote.Trim(),
            Latitude = vm.Latitude,
            Longitude = vm.Longitude,
            Category = vm.Category.Trim(),
            Subcategory = string.IsNullOrWhiteSpace(vm.Subcategory) ? null : vm.Subcategory.Trim(),
            Description = vm.Description.Trim(),
            MediaPath = mediaPath,
            MediaType = mediaType,
            ContentHashSha256 = hash,
            TrackingCode = await NextTrackingCodeAsync(),
            Status = IssueStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Confirmation), new { code = issue.TrackingCode });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string code)
    {
        var issue = await _db.Issues.AsNoTracking().FirstOrDefaultAsync(i => i.TrackingCode == code);
        if (issue == null) return NotFound();
        return View(issue);
    }

    private async Task<string> NextTrackingCodeAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _db.Issues.CountAsync(i => i.CreatedAtUtc.Year == year);
        return $"MM-{year}-{(count + 1):00000}";
    }

    private static bool ContainsPII(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var text = s.ToLowerInvariant();
        var email = System.Text.RegularExpressions.Regex.IsMatch(text, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var phone = System.Text.RegularExpressions.Regex.IsMatch(text, @"(\+?\d{2,3}[- ]?)?\d{3}[- ]?\d{3,4}[- ]?\d{3,4}");
        var idnum = System.Text.RegularExpressions.Regex.IsMatch(text, @"\b\d{13}\b");
        return email || phone || idnum;
    }

    private static string ComputeSha256(string s)
    {
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        return BitConverter.ToString(hashBytes).Replace("-", "");
    }
}
```

---

## Razor Views — with Google Places + geo, subcategories, file preview, and **upload progress**

```cshtml
@model MunicipalManager.Models.Issue
@{
    ViewData["Title"] = "Report an Issue";
    var categories = ViewBag.Categories as List<string>;
    var catMapJson = (string)ViewBag.CatMapJson!;
}
<section class="hero p-4 rounded-xl" style="background:#E6F3FF;color:#0B1D39;">
  <h1 class="h3 m-0">Report an Issue</h1>
  <p class="m-0">Please avoid personal details. Describe the problem and where it is.</p>
</section>

<form id="issueForm" asp-action="Create" method="post" enctype="multipart/form-data" class="mt-3">
  @Html.AntiForgeryToken()

  <!-- Location with Google Places -->
  <div class="mb-3">
    <label class="form-label">Location <span class="text-danger">*</span></label>
    <input asp-for="Location" id="locationInput" class="form-control"
           placeholder="Type an address or landmark…" aria-describedby="locHelp" />
    <div id="locHelp" class="form-text">
      Use the map search or <button class="btn btn-link p-0 align-baseline" id="btnUseMyLoc" type="button">use my location</button>.
    </div>
    <span asp-validation-for="Location" class="text-danger"></span>

    <!-- Optional note -->
    <label class="form-label mt-2">Location note (optional)</label>
    <input asp-for="LocationNote" class="form-control" placeholder="e.g., Opposite the blue gate" />

    <!-- Hidden geo tags -->
    <input asp-for="Latitude" type="hidden" id="lat" />
    <input asp-for="Longitude" type="hidden" id="lng" />
  </div>

  <!-- Category + Subcategory -->
  <div class="row g-3">
    <div class="col-md-6">
      <label class="form-label">Category <span class="text-danger">*</span></label>
      <select asp-for="Category" id="category" class="form-select">
        <option value="">Select a category</option>
        @foreach (var c in categories!) { <option value="@c">@c</option> }
      </select>
      <span asp-validation-for="Category" class="text-danger"></span>
    </div>
    <div class="col-md-6">
      <label class="form-label">Subcategory</label>
      <select asp-for="Subcategory" id="subcategory" class="form-select" disabled>
        <option value="">—</option>
      </select>
      <span asp-validation-for="Subcategory" class="text-danger"></span>
    </div>
  </div>

  <!-- Description -->
  <div class="mb-3 mt-3">
    <label class="form-label">Description <span class="text-danger">*</span></label>
    <textarea asp-for="Description" class="form-control" rows="5" maxlength="1000" minlength="50"
      placeholder="Describe what’s wrong, any hazards, and exactly where (e.g., outside No. 12, near clinic)…"></textarea>
    <div class="form-text">50–1000 characters.</div>
    <span asp-validation-for="Description" class="text-danger"></span>
  </div>

  <!-- Media -->
  <div class="mb-3">
    <label class="form-label">Media (optional)</label>
    <input type="file" name="media" id="mediaInput" class="form-control" accept=".jpg,.jpeg,.png,.mp4" />
    <div id="mediaInfo" class="form-text"></div>

    <!-- Upload progress (hidden until submit) -->
    <div class="progress mt-2" style="height: 10px; display:none;" id="uploadProgressWrap">
      <div class="progress-bar" role="progressbar" id="uploadProgress" style="width:0%"></div>
    </div>
  </div>

  <!-- Consent -->
  <div class="form-check mb-3">
    <input class="form-check-input" type="checkbox" id="consent" required>
    <label class="form-check-label" for="consent">
      I confirm this report contains no personal information (names, phone numbers, faces, IDs).
    </label>
  </div>

  <div class="alert alert-info" role="alert">
    POPIA: Don’t include personal information. We store only what’s needed to action your report.
  </div>

  <button type="submit" class="btn btn-primary">Submit Report</button>
</form>

@section Scripts {
  <partial name="_ValidationScriptsPartial" />
  <script>
    // Category/Subcategory linkage
    const CATMAP = @Html.Raw(catMapJson);
    const category = document.getElementById('category');
    const subcategory = document.getElementById('subcategory');

    category.addEventListener('change', () => {
      const subs = CATMAP[category.value] || [];
      subcategory.innerHTML = '<option value="">Select a subcategory</option>';
      if (subs.length === 0) {
        subcategory.value = '';
        subcategory.disabled = true;
      } else {
        subs.forEach(s => {
          const o = document.createElement('option');
          o.value = s; o.textContent = s;
          subcategory.appendChild(o);
        });
        subcategory.disabled = false;
      }
    });

    // File name + size preview
    const mediaInput = document.getElementById('mediaInput');
    const mediaInfo = document.getElementById('mediaInfo');
    mediaInput.addEventListener('change', () => {
      mediaInfo.textContent = '';
      if (mediaInput.files && mediaInput.files[0]) {
        const f = mediaInput.files[0];
        const sizeMB = (f.size / (1024*1024)).toFixed(2);
        mediaInfo.textContent = `Selected: ${f.name} — ${sizeMB} MB`;
      }
    });

    // Google Places Autocomplete + geo
    let autocomplete;
    function initPlaces() {
      const input = document.getElementById('locationInput');
      autocomplete = new google.maps.places.Autocomplete(input, { types: ['geocode'] });
      autocomplete.addListener('place_changed', () => {
        const place = autocomplete.getPlace();
        const latField = document.getElementById('lat');
        const lngField = document.getElementById('lng');
        if (place.geometry && place.geometry.location) {
          const loc = place.geometry.location;
          latField.value = loc.lat();
          lngField.value = loc.lng();
        }
      });
    }

    // Use my location (optional)
    document.getElementById('btnUseMyLoc').addEventListener('click', async () => {
      if (!navigator.geolocation) return alert('Geolocation not supported.');
      navigator.geolocation.getCurrentPosition(
        pos => {
          document.getElementById('lat').value = pos.coords.latitude;
          document.getElementById('lng').value = pos.coords.longitude;
          // If user didn't type a location, add a rough placeholder
          const locInput = document.getElementById('locationInput');
          if (!locInput.value) locInput.value = 'My current location (approx.)';
        },
        err => alert('Could not get your location.')
      );
    });

    // Submit with upload progress (XHR to preserve anti-forgery + files)
    const form = document.getElementById('issueForm');
    form.addEventListener('submit', function (ev) {
      ev.preventDefault();
      const formData = new FormData(form);
      const xhr = new XMLHttpRequest();
      const progressWrap = document.getElementById('uploadProgressWrap');
      const progressBar = document.getElementById('uploadProgress');
      progressWrap.style.display = 'block';
      xhr.upload.addEventListener('progress', e => {
        if (e.lengthComputable) {
          const pct = Math.round((e.loaded / e.total) * 100);
          progressBar.style.width = pct + '%';
          progressBar.setAttribute('aria-valuenow', pct);
        }
      });
      xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
          if (xhr.status >= 200 && xhr.status < 300) {
            window.location.href = xhr.responseURL || '@Url.Action("Confirmation","Issue")';
          } else {
            progressWrap.style.display = 'none';
            alert('Upload failed. Please check inputs and try again.');
          }
        }
      };
      xhr.open('POST', '@Url.Action("Create","Issue")', true);
      xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
      xhr.send(formData);
    });
  </script>
  <!-- Google Places API (replace YOUR_API_KEY) -->
  <script async defer src="https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&libraries=places&callback=initPlaces"></script>
}
```

**Notes:**

* Replace `YOUR_API_KEY` with your key (restrict it to your domain). The page still works if the script fails (manual location text + optional browser geo).
* The **progress bar** uses `XMLHttpRequest.upload.progress`.
* We keep using regular MVC action; the XHR posts the same form with anti-forgery token.

---

## Confirmation View (minor update: show subcategory + coords if present)

```cshtml
@model MunicipalManager.Models.Issue
@{
    ViewData["Title"] = "Report Submitted";
}
<div class="card p-4">
  <h2 class="h4">Thank you! Your report was submitted.</h2>
  <p><strong>Tracking Code:</strong> @Model.TrackingCode</p>
  <p>
    <strong>Category:</strong> @Model.Category
    @if(!string.IsNullOrWhiteSpace(Model.Subcategory)){ <span> → <em>@Model.Subcategory</em></span> }<br/>
    <strong>Location:</strong> @Model.Location
    @if(!string.IsNullOrWhiteSpace(Model.LocationNote)){ <span> — @Model.LocationNote</span> }<br/>
    @if(Model.Latitude.HasValue && Model.Longitude.HasValue){
      <span><strong>Geo:</strong> @Model.Latitude.Value.ToString("F6"), @Model.Longitude.Value.ToString("F6")</span><br/>
    }
    <strong>Submitted:</strong> @Model.CreatedAtUtc.ToLocalTime()
  </p>
  <p>Keep your tracking code to follow up when the Status page is enabled (Part 3).</p>
  <button class="btn btn-outline-secondary" onclick="window.print()">Save/Print</button>
  <a class="btn btn-primary" asp-controller="Home" asp-action="Index">Back to Home</a>
</div>
```

---

## How this maximises Part-1 marks

* **Implementation (80%)**

  * Full **Report Issues** flow with **Google location + geo-tags**, category/subcategory logic, upload preview, **progress animation**, POPIA guardrails, responsive UI, accessibility.
  * **SQLite persistence**; structured model ready for Part 2/3 (status pipeline already present).
  * **Disabled future features** visible (Events, Request Tracking) with tooltips.
* **Research (20%)**

  * Provide separately: 500-word IEEE-style write-up on citizen engagement strategies (we’ll add it next).

---

## Testing & Acceptance (quick)

* Required: Location, Category, (Subcategory when applicable), Description (50–1000).
* PII typed into Location/LocationNote/Description → error prompt.
* Media: `.jpg/.png` ≤ 5 MB or `.mp4` ≤ 15 MB accepted; others rejected.
* Progress bar visibly advances during upload.
* Tracking codes unique and year-scoped.
* If Google script blocked, user can still submit (manual text + optional browser geo).

---

## README notes (additions)

* **Google Places:** add your API key in the script tag. (Restrict by HTTP referrer.)
* **Privacy:** We don’t collect names, emails, phone numbers, or faces. Best-effort PII scanning blocks common mistakes.
* **Run:** `dotnet ef database update` → `dotnet run`. SQLite file `municipalmanager.db`. Uploads: `/wwwroot/uploads/issues/`.

---

If you want, I can also drop in a small **seed/migration** and a **Unit Test** for the `(Category, Subcategory)` validator so the marker sees quality gates.
