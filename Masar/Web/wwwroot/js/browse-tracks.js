// ========================================
// BROWSE TRACKS PAGE - JavaScript
// ========================================

(function () {
  "use strict";

  // ========================================
  // VIEW TOGGLE (Grid/List)
  // ========================================

  function initViewToggle() {
    const viewBtns = document.querySelectorAll(".view-btn");
    const tracksCatalog = document.getElementById("tracksCatalog");

    viewBtns.forEach((btn) => {
      btn.addEventListener("click", function () {
        const view = this.getAttribute("data-view");

        viewBtns.forEach((b) => b.classList.remove("active"));
        this.classList.add("active");

        if (view === "list") {
          tracksCatalog.classList.add("list-view");
        } else {
          tracksCatalog.classList.remove("list-view");
        }

        Masar.saveToStorage("preferredTrackView", view);
      });
    });

    // Load saved preference
    const savedView = Masar.getFromStorage("preferredTrackView");
    if (savedView === "list") {
      document.querySelector('[data-view="list"]')?.click();
    }
  }

  // ========================================
  // DROPDOWN FILTERS
  // ========================================

  function initDropdowns() {
    const dropdowns = document.querySelectorAll(".filter-dropdown");

    dropdowns.forEach((dropdown) => {
      const btn = dropdown.querySelector(".filter-dropdown-btn");
      const options = dropdown.querySelectorAll(".filter-option");

      btn.addEventListener("click", function (e) {
        e.stopPropagation();

        // Close other dropdowns
        dropdowns.forEach((d) => {
          if (d !== dropdown) {
            d.classList.remove("active");
          }
        });

        dropdown.classList.toggle("active");
      });

      options.forEach((option) => {
        option.addEventListener("click", function () {
          const text = this.textContent.trim();
          const icon = btn.querySelector("i:first-child").outerHTML;
          const chevron = btn.querySelector("i:last-child").outerHTML;

          // Update button text
          const span = btn.querySelector("span");
          if (span) {
            span.textContent = text;
          }

          // Update active state
          options.forEach((opt) => opt.classList.remove("active"));
          this.classList.add("active");

          dropdown.classList.remove("active");

          // Trigger filter
          filterTracks();
        });
      });
    });

    // Close dropdowns when clicking outside
    document.addEventListener("click", function () {
      dropdowns.forEach((dropdown) => {
        dropdown.classList.remove("active");
      });
    });
  }

  // ========================================
  // SEARCH FUNCTIONALITY
  // ========================================

  function initSearch() {
    const searchInput = document.getElementById("trackSearch");
    let searchTimeout;

    searchInput.addEventListener("input", function () {
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => {
        filterTracks();
      }, 300);
    });
  }

  // ========================================
  // FILTER TRACKS
  // ========================================

  function filterTracks() {
    const searchTerm = document
      .getElementById("trackSearch")
      .value.toLowerCase();
    const categoryFilter =
      document
        .querySelector("#categoryMenu .filter-option.active")
        ?.getAttribute("data-category") || "all";
    const levelFilter =
      document
        .querySelector("#levelMenu .filter-option.active")
        ?.getAttribute("data-level") || "all";
    const durationFilter =
      document
        .querySelector("#durationMenu .filter-option.active")
        ?.getAttribute("data-duration") || "all";
    const sortFilter =
      document
        .querySelector("#sortMenu .filter-option.active")
        ?.getAttribute("data-sort") || "popular";

    const trackCards = document.querySelectorAll(".catalog-track-card");
    let visibleCount = 0;
    let tracks = [];

    trackCards.forEach((card) => {
      const title = card
        .querySelector(".track-card-title")
        .textContent.toLowerCase();
      const description = card
        .querySelector(".track-card-description")
        .textContent.toLowerCase();
      const category = card.getAttribute("data-category");
      const level = card.getAttribute("data-level");
      const duration = card.getAttribute("data-duration");

      // Check if card matches all filters
      const matchesSearch =
        !searchTerm ||
        title.includes(searchTerm) ||
        description.includes(searchTerm);
      const matchesCategory =
        categoryFilter === "all" || category === categoryFilter;
      const matchesLevel = levelFilter === "all" || level === levelFilter;
      const matchesDuration =
        durationFilter === "all" || duration === durationFilter;

      if (matchesSearch && matchesCategory && matchesLevel && matchesDuration) {
        card.style.display = "";
        visibleCount++;

        // Store for sorting
        tracks.push({
          element: card,
          title: card.querySelector(".track-card-title").textContent,
          rating:
            parseFloat(
              card.querySelector(".track-stats-grid .stat-value").textContent
            ) || 0,
          students:
            parseInt(
              card
                .querySelector(
                  ".track-stats-grid .track-stat:nth-child(4) .stat-value"
                )
                .textContent.replace("K", "000")
            ) || 0,
        });
      } else {
        card.style.display = "none";
      }
    });

    // Sort tracks
    sortTracks(tracks, sortFilter);

    // Update results count
    updateResultsCount(visibleCount);

    // Show/hide clear filters button
    updateClearFiltersButton(
      searchTerm,
      categoryFilter,
      levelFilter,
      durationFilter
    );

    // Show/hide empty state
    showEmptyState(visibleCount === 0);
  }

  // ========================================
  // SORT TRACKS
  // ========================================

  function sortTracks(tracks, sortBy) {
    const container = document.getElementById("tracksCatalog");

    switch (sortBy) {
      case "newest":
        // Reverse current order (assuming newer tracks are added last)
        tracks.reverse();
        break;
      case "rating":
        tracks.sort((a, b) => b.rating - a.rating);
        break;
      case "title":
        tracks.sort((a, b) => a.title.localeCompare(b.title));
        break;
      case "popular":
      default:
        tracks.sort((a, b) => b.students - a.students);
        break;
    }

    // Reorder DOM elements
    tracks.forEach((track) => {
      container.appendChild(track.element);
    });
  }

  // ========================================
  // UPDATE RESULTS COUNT
  // ========================================

  function updateResultsCount(count) {
    const resultsCount = document.getElementById("resultsCount");
    if (resultsCount) {
      resultsCount.textContent = count;
    }
  }

  // ========================================
  // UPDATE CLEAR FILTERS BUTTON
  // ========================================

  function updateClearFiltersButton(search, category, level, duration) {
    const clearBtn = document.getElementById("clearFilters");
    const hasFilters =
      search || category !== "all" || level !== "all" || duration !== "all";

    if (clearBtn) {
      clearBtn.style.display = hasFilters ? "flex" : "none";
    }
  }

  // ========================================
  // CLEAR ALL FILTERS
  // ========================================

  function initClearFilters() {
    const clearBtn = document.getElementById("clearFilters");
    const resetBtn = document.getElementById("resetFilters");

    function clearAllFilters() {
      // Reset search
      document.getElementById("trackSearch").value = "";

      // Reset all dropdowns
      document.querySelectorAll(".filter-option").forEach((opt) => {
        opt.classList.remove("active");
      });

      // Activate "All" options
      document
        .querySelectorAll('.filter-option[data-category="all"]')
        .forEach((opt) => {
          opt.classList.add("active");
        });
      document
        .querySelectorAll('.filter-option[data-level="all"]')
        .forEach((opt) => {
          opt.classList.add("active");
        });
      document
        .querySelectorAll('.filter-option[data-duration="all"]')
        .forEach((opt) => {
          opt.classList.add("active");
        });

      // Reset dropdown button texts
      const categoryBtn = document.querySelector("#categoryDropdown span");
      if (categoryBtn) categoryBtn.textContent = "All Categories";

      const levelBtn = document.querySelector("#levelDropdown span");
      if (levelBtn) levelBtn.textContent = "All Levels";

      const durationBtn = document.querySelector("#durationDropdown span");
      if (durationBtn) durationBtn.textContent = "Any Duration";

      // Re-filter
      filterTracks();
    }

    if (clearBtn) {
      clearBtn.addEventListener("click", clearAllFilters);
    }

    if (resetBtn) {
      resetBtn.addEventListener("click", clearAllFilters);
    }
  }

  // ========================================
  // SHOW/HIDE EMPTY STATE
  // ========================================

  function showEmptyState(show) {
    const emptyState = document.getElementById("emptyState");
    const tracksCatalog = document.getElementById("tracksCatalog");

    if (emptyState && tracksCatalog) {
      if (show) {
        emptyState.style.display = "block";
        tracksCatalog.style.display = "none";
      } else {
        emptyState.style.display = "none";
        tracksCatalog.style.display = "grid";
      }
    }
  }

  // ========================================
  // ENROLL IN TRACK
  // ========================================

  function initEnrollButtons() {
    const enrollBtns = document.querySelectorAll(".enroll-btn");

    enrollBtns.forEach((btn) => {
      btn.addEventListener("click", function () {
        const trackId = this.getAttribute("data-track-id");
        const trackTitle = this.closest(".catalog-track-card").querySelector(
          ".track-card-title"
        ).textContent;

        // Show confirmation message
        if (window.Masar && Masar.showToast) {
          Masar.showToast(`Enrolling in "${trackTitle}"...`, "success");

          // Simulate enrollment process
          setTimeout(() => {
            Masar.showToast(
              `Successfully enrolled in "${trackTitle}"!`,
              "success"
            );

            // Update button state
            this.innerHTML =
              '<i class="fas fa-check"></i><span>Enrolled</span>';
            this.classList.add("btn-success");
            this.disabled = true;
          }, 1500);
        } else {
          alert(`Enrolling in: ${trackTitle}`);
        }

        // Store enrollment in local storage
        const enrolledTracks = JSON.parse(
          localStorage.getItem("enrolledTracks") || "[]"
        );
        if (!enrolledTracks.includes(trackId)) {
          enrolledTracks.push(trackId);
          localStorage.setItem(
            "enrolledTracks",
            JSON.stringify(enrolledTracks)
          );
        }
      });
    });
  }

  // ========================================
  // CHECK ENROLLED TRACKS
  // ========================================

  function checkEnrolledTracks() {
    const enrolledTracks = JSON.parse(
      localStorage.getItem("enrolledTracks") || "[]"
    );

    enrolledTracks.forEach((trackId) => {
      const btn = document.querySelector(
        `.enroll-btn[data-track-id="${trackId}"]`
      );
      if (btn) {
        btn.innerHTML = '<i class="fas fa-check"></i><span>Enrolled</span>';
        btn.classList.add("btn-success");
        btn.disabled = true;
      }
    });
  }

  // ========================================
  // KEYBOARD SHORTCUTS
  // ========================================

  function initKeyboardShortcuts() {
    document.addEventListener("keydown", function (e) {
      // Ctrl/Cmd + K to focus search
      if ((e.ctrlKey || e.metaKey) && e.key === "k") {
        e.preventDefault();
        document.getElementById("trackSearch")?.focus();
      }

      // Escape to clear search
      if (e.key === "Escape") {
        const searchInput = document.getElementById("trackSearch");
        if (searchInput && searchInput === document.activeElement) {
          searchInput.value = "";
          searchInput.blur();
          filterTracks();
        }
      }
    });
  }

  // ========================================
  // INITIALIZE ALL
  // ========================================

  function init() {
    initViewToggle();
    initDropdowns();
    initSearch();
    initClearFilters();
    initEnrollButtons();
    checkEnrolledTracks();
    initKeyboardShortcuts();

    // Initial count update
    const trackCards = document.querySelectorAll(".catalog-track-card");
    updateResultsCount(trackCards.length);

    console.log("Browse Tracks page initialized");
  }

  // Run when DOM is ready
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
