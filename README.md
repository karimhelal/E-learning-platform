# 🎓 Masar (مسار)

**An E-Learning Platform Built for Developers.**

## 📖 Overview

**The Problem:** Aspiring developers often get stuck in "Tutorial Hell," struggling with fragmented resources and a lack of clear direction. Generic video platforms lack structure and focus.

**The Solution:** Masar focuses on **Structured Learning Tracks** (Roadmaps) instead of isolated courses. It features a distraction-free, IDE-style "Focus Mode" classroom and enforces learning through rigorous progress tracking and verifiable certificates.

---

## ✨ Features by Role

### 👨‍🎓 Student Experience
* **Smart Discovery:** AJAX-based advanced filtering (by Category, Level, Duration) and pagination.
* **The Classroom (Focus Mode):** Distraction-free, dark-mode IDE-style interface with a dynamic sidebar. Remembers progress and allows you to "Resume Learning" exactly where you left off.
* **Multi-Format Learning:** Supports Video lessons (with duration tracking) and Article lessons (Rich Text).
* **Certificates:** Automated PDF certificate generation upon 100% completion, featuring a unique verification URL and QR code.

### 👨‍🏫 Instructor Tools
* **Advanced Curriculum Builder:** A drag-and-drop style dynamic interface. Instructors can add modules, create video/article lessons, and update metadata using modals and AJAX—all without leaving the page.
* **Performance Dashboard:** Real-time analytics aggregating student enrollments, completion rates, and average course ratings.
* **Course Lifecycle Management:** Courses start as Drafts, are submitted for Review, and wait for Admin publication.

### 👑 Admin (Quality Control)
* **Pending Courses Workflow:** Admins act as gatekeepers, reviewing submitted courses to approve or reject them (with feedback), transforming the platform into a curated premium marketplace.
* **Track Builder:** Create high-level "Learning Tracks" (e.g., "Full Stack .NET Developer") and assign specific courses to them.
* **User Management:** Full control over role permissions and user status.

### 🌍 Public / Unauthenticated
* **Landing Page:** Platform statistics, featured tracks, and value proposition.
* **Public Instructor Profiles:** Portfolio pages showing instructor bios, social links, and published courses.
* **Certificate Verification:** A public route to validate the authenticity of a student's certificate.

---

## 🛠️ Tech Stack

* **Backend:** ASP.NET Core MVC (.NET 9, C#)
* **Database:** SQL Server with Entity Framework Core (EF Core)
* **Frontend:** HTML5, CSS3 (Custom Dark-Mode IDE-style UI), JavaScript (Vanilla & jQuery)
* **Authentication/Security:** ASP.NET Core Identity
* **Key Libraries:**
  * **Plyr.js:** Custom video player integration (supports YouTube securely).
  * **QuestPDF:** Dynamic, tamper-proof PDF certificate generation.
  * **SignalR:** Real-time notifications.
  * **HtmlSanitizer:** Cross-Site Scripting (XSS) protection for rich-text article lessons.

---

## 🏗️ System Architecture & Design Patterns

* **Clean Architecture (N-Tier/Onion):** Strict separation of concerns into Core (Entities/Enums), Infrastructure/DAL (Data access), BLL (Services/DTOs), and Web (MVC/Controllers).
* **Data Access:** Repository Pattern and Unit of Work (`UnitOfWork.cs`).
* **Database Design (TPH):** Uses Table-Per-Hierarchy to efficiently store polymorphic lesson types (e.g., VideoContent vs. ArticleContent) in a single database table. Handles complex Many-to-Many relationships.
* **Frontend Architecture:** * **Action Dispatcher Pattern:** A centralized JavaScript architecture to handle complex UI events (like the Curriculum Builder) without spaghetti code.
  * **Event Delegation:** Efficiently handles DOM events for elements injected dynamically via AJAX.

---

## 🚀 Technical Highlights & Under the Hood

* **Real-Time Notifications (SignalR):** Live updates pushed directly to users. For example, when an Admin approves a course, the Instructor receives an instant notification without refreshing the page.
* **Smart Data Seeding:** A custom `DbSeeder.cs` utility that creates a highly realistic testing environment with developer courses, modules, lessons, and simulated student enrollments.
* **Eliminating N+1 Queries:** Utilized EF Core's `.AsSplitQuery()` to prevent Cartesian Explosion when loading massive nested data structures (Course -> Modules -> Lessons -> Resources).
* **SPA-like Experience:** Heavy use of AJAX and Razor Partial Views (`RenderViewToString`). Filtering, pagination, and navigating between lessons happen instantly without full page reloads.
* **Memory Optimization:** Used `HashSet` in memory for ultra-fast lookups when calculating user progress across thousands of lessons.
* **Granular RBAC & Security:** Strict server-side authorization checks on AJAX calls to prevent IDOR attacks. Clear separation of Admin, Instructor, and Student roles using ASP.NET Core Identity.
* **Developer-Centric User Profiles:** Students and Instructors can upload avatars and link GitHub/LinkedIn portfolios, acting as an extension of their developer resume.
* **Media & File Management:** Secure handling of image uploads for Course Thumbnails and User Avatars to the `wwwroot/uploads` directory.
* **Responsive "Anywhere" Design:** The complex Curriculum Builder and Focus Mode Classroom collapse gracefully on mobile devices and tablets.

---

## 💼 Business Logic & Workflow

**Curated Marketplace Model:** Operates on a commission/curation model. Instructors create content ➡️ Admins verify quality ➡️ Students consume content.
**Retention Strategy:** By selling Tracks instead of just Courses, the platform increases student retention and lifetime value by giving them a long-term roadmap.

---

## 🏁 Getting Started

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
* SQL Server (LocalDB or standard instance)
* Visual Studio 2022, JetBrains Rider, or VS Code

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/YourUsername/Masar.git
   cd Masar
   ```
2. **Update Connection String:** Configure `DefaultConnection` in `Masar/Web/appsettings.json`.
3. **Apply Migrations:**
   ```bash
   dotnet ef database update --project ../DAL
   ```
4. **Seed the Database:** Run the application. The integrated Data Seeder will populate the initial categories, users, and realistic test courses.
5. **Run:**
   ```bash
   dotnet run --project Web
   ```

## 🤝 Contributing
Contributions are welcome! Please feel free to submit a Pull Request or open an issue to discuss new features or architectural improvements.
