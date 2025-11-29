document.addEventListener("DOMContentLoaded", () => {
    // ==========================================
    // 1. MOBILE NAVBAR (Hamburger Menu)
    // ==========================================
    const navToggle = document.querySelector(".nav-toggle");
    const navMenu = document.querySelector(".nav-menu");
    const navActions = document.querySelector(".nav-actions");

    if (navToggle) {
        navToggle.addEventListener("click", () => {
            // Toggle active class on both menu and actions
            navMenu.classList.toggle("active");
            navActions.classList.toggle("active");

            // Change icon
            const icon = navToggle.querySelector("i");
            if (navMenu.classList.contains("active")) {
                icon.classList.remove("fa-bars");
                icon.classList.add("fa-times");
            } else {
                icon.classList.remove("fa-times");
                icon.classList.add("fa-bars");
            }
        });
    }

    // ==========================================
    // 2. FILTER SIDEBAR (Mobile & Desktop)
    // ==========================================
    const filterSidebar = document.getElementById("filterSidebar");
    const filterOverlay = document.getElementById("filterOverlay");
    const openFilterBtn = document.getElementById("openFilter");
    const closeFilterBtn = document.getElementById("closeFilter");

    // Desktop Toggle Elements
    const toggleSidebarBtn =
        document.getElementById("toggleSidebar");
    const toggleText = document.getElementById("toggleText");

    // --- Mobile Logic ---
    function toggleMobileFilter() {
        filterSidebar.classList.toggle("active");
        filterOverlay.classList.toggle("active");
        // Prevent scrolling background when mobile menu is open
        document.body.style.overflow =
            filterSidebar.classList.contains("active")
                ? "hidden"
                : "";
    }

    if (openFilterBtn)
        openFilterBtn.addEventListener("click", toggleMobileFilter);
    if (closeFilterBtn)
        closeFilterBtn.addEventListener("click", toggleMobileFilter);
    if (filterOverlay)
        filterOverlay.addEventListener("click", toggleMobileFilter);

    // --- Desktop Logic (Hide/Show) ---
    if (toggleSidebarBtn && filterSidebar) {
        toggleSidebarBtn.addEventListener("click", () => {
            // 1. Toggle the hidden class
            filterSidebar.classList.toggle("hidden");

            // 2. Update Button Text & Style
            if (filterSidebar.classList.contains("hidden")) {
                toggleText.textContent = "Show Filters";
                // Switch style to outline when filters are hidden
                toggleSidebarBtn.classList.add("btn-outline");
                toggleSidebarBtn.classList.remove("btn-secondary");
            } else {
                toggleText.textContent = "Hide Filters";
                // Revert to solid style when filters are visible
                toggleSidebarBtn.classList.remove("btn-outline");
                toggleSidebarBtn.classList.add("btn-secondary");
            }
        });
    }

    // --- Accordion Logic ---
    const filterTitles = document.querySelectorAll(".filter-title");
    filterTitles.forEach((title) => {
        title.addEventListener("click", () => {
            title.parentElement.classList.toggle("collapsed");
        });
    });


    // ==========================================
    // 4. VIEW TOGGLE (Grid vs List)
    // ==========================================
    const gridViewBtn = document.getElementById("gridViewBtn");
    const listViewBtn = document.getElementById("listViewBtn");
    const coursesCatalog =
        document.querySelector(".courses-catalog");

    if (gridViewBtn && listViewBtn && coursesCatalog) {
        listViewBtn.addEventListener("click", () => {
            coursesCatalog.classList.add("list-view");
            listViewBtn.classList.add("active");
            gridViewBtn.classList.remove("active");
        });

        gridViewBtn.addEventListener("click", () => {
            coursesCatalog.classList.remove("list-view");
            gridViewBtn.classList.add("active");
            listViewBtn.classList.remove("active");
        });
    }


    // ==========================================
    // 5. SORTING BUTTONS
    // ==========================================

    // 1. Toggle Dropdown Menu on Click
    $('.filter-dropdown-btn').on('click', function (e) {
        e.stopPropagation(); // Prevent clicking body from closing immediately

        // Get the parent container (.filter-dropdown)
        var $dropdown = $(this).closest('.filter-dropdown');

        // Close ALL other dropdowns except this one
        $('.filter-dropdown').not($dropdown).removeClass('active');

        // Toggle THIS dropdown
        $dropdown.toggleClass('active');
    });

    // 2. Handle Option Selection
    $('.filter-option').on('click', function () {
        var $parent = $(this).closest('.filter-dropdown');
        var $hiddenInput = $parent.find('input[type="hidden"]');
        var $displaySpan = $parent.find('.filter-dropdown-btn span');

        // Get Data
        var value = $(this).data('value');
        var text = $(this).text().trim();

        // Visual Updates: Update Button Text
        $displaySpan.text(text);

        // Visual Updates: Highlight Selected Option
        $parent.find('.filter-option').removeClass('active'); // Remove active from siblings
        $(this).addClass('active'); // Add active to clicked

        // Close the dropdown
        $parent.removeClass('active');

        // Data Update
        $hiddenInput.val(value); // Update the hidden input!

        // 3. Trigger AJAX Reload (Reset to page 1)
        filterCourses();
    });

    // 3. Close dropdowns when clicking outside
    $(document).on('click', function () {
        $('.filter-dropdown').removeClass('active');
    });


    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    document.querySelectorAll('.range-slider-container').forEach(container => {
        const minRange = container.querySelector('.min-range');
        const maxRange = container.querySelector('.max-range');
        const progress = container.querySelector('.range-track-progress');

        const minValueEl = container
            .closest('.filter-group')
            .querySelector('.range-values .min-value');
        const maxValueEl = container
            .closest('.filter-group')
            .querySelector('.range-values .max-value');

        const minHidden = container
            .closest('.filter-group')
            .querySelector('.min-input');
        const maxHidden = container
            .closest('.filter-group')
            .querySelector('.max-input');

        const minAttr = parseFloat(minRange.min);
        const maxAttr = parseFloat(minRange.max);
        const range = maxAttr - minAttr;

        function updateSlider() {
            let minVal = parseFloat(minRange.value);
            let maxVal = parseFloat(maxRange.value);

            // Prevent handles from crossing
            if (minVal > maxVal) {
                // Swap to keep logic simple
                [minVal, maxVal] = [maxVal, minVal];
            }

            // Convert values to percentages
            const left = ((minVal - minAttr) / range) * 100;
            const right = 100 - ((maxVal - minAttr) / range) * 100;

            progress.style.left = left + '%';
            progress.style.right = right + '%';

            // Update text displays (if they exist)
            if (minValueEl) minValueEl.textContent = minVal + ' ' + (minHidden?.dataset.unit ?? '');
            if (maxValueEl) maxValueEl.textContent = maxVal + ' ' + (maxHidden?.dataset.unit ?? '');

            // Update hidden inputs for form submission / filters
            if (minHidden) minHidden.value = minVal;
            if (maxHidden) maxHidden.value = maxVal;
        }

        // Attach events
        minRange.addEventListener('input', updateSlider);
        maxRange.addEventListener('input', updateSlider);

        // Initial render
        updateSlider();
    });
});

// 1. The State Object (Matches CourseFilterRequestDto structure)
const filterState = {
    // Arrays for checkboxes
    CategoryNames: [],
    LevelNames: [],
    LanguageNames: [],

    // Nullables for ranges
    MinDuration: null,
    MaxDuration: null,

    MinEnrollments: null,
    MaxEnrollments: null,

    MinReviews: null,
    MaxReviews: null,

    MinRating: null,
    MaxRating: null,

    MinCreationDate: null,
    MaxCreationDate: null
};

// 2. The Collector Function
function collectFilterState() {
    // A. Reset arrays to avoid duplicates
    filterState.CategoryNames = [];
    filterState.LevelNames = [];
    filterState.LanguageNames = [];

    // B. Loop through ALL checked checkboxes
    $('.filter-checkbox:checked').each(function () {
        const key = $(this).data('key'); // e.g., "CategoryIds"
        const value = $(this).val();     // e.g., "1" or "Beginner"

        // Dynamically find the array in our state object based on the key
        // Note: We lowercase the key first because JS objects are case-sensitive usually
        // But your DTO expects "CategoryIds".
        // Best practice: match your JS object keys to your HTML data-key exactly.

        //if (key === 'CategoryNames') {
        //    filterState.CategoryNames.push(value);
        //}
        //else if (key === 'LevelNames') {
        //    filterState.LevelNames.push(value);
        //} else if (key === 'LanguageNames') {
        //    filterState.LanguageNames.push(value);
        //}

        filterState[key].push(value);
    });

    // C. Collect Ranges (Numbers)
    $('.filter-range-number').each(function () {
        const key = $(this).data('key'); // "MinDuration"
        const val = $(this).val();

        filterState[key] = val ? parseFloat(val) : null;
    });

    $('.filter-range-date').each(function () {
        const key = $(this).data('key'); // "MinDate"
        const val = $(this).val();

        filterState[key] = val ? val : null;
    });

}

function updateCoursesView(coursesGridContent) {
    $(".courses-catalog").html(coursesGridContent);
}

function updatePaginationView(paginationContent) {
    $(".pagination-container").html(paginationContent);
}

//function updateFiltersView(filtersContent) {
//    $(".filter-groups-container").html(filtersContent);
//}


function fetchPage(caller, dir) {
    collectFilterState();

    const pageNumToFetch = parseInt((dir == 0)
        ? caller.textContent
        : (dir < 0)
            ? Math.max(1, parseInt($(".pagination-btn.active").text()) - 1)
            : Math.min(parseInt($(".pagination-btn.last-pagination-btn").text()), parseInt($(".pagination-btn.active").text()) + 1)
    );

    const pageSize = parseInt($(".pagination-options #pageSize").text());

    const filterOptions = {
        sortBy: $('#sortFilter').val() || "CreationDate",
        sortOrder: $('#sortOrder').val() || "DESC"
    }

    const apiOptions = {
        url: `/filter-courses`,
        data: {
            FilterGroups: {
                CategoryNames: filterState.CategoryNames,
                LanguageNames: filterState.LanguageNames,
                LevelNames: filterState.LevelNames,

                MinDuration: filterState.MinDuration,
                MaxDuration: filterState.MaxDuration,
                MinEnrollments: filterState.MinEnrollments,
                MaxEnrollments: filterState.MaxEnrollments,
                MinReviews: filterState.MinReviews,
                MaxReviews: filterState.MaxReviews,
                MinRating: filterState.MinRating,
                MaxRating: filterState.MaxRating,
                MinDate: filterState.MinCreationDate,
                MaxDate: filterState.MaxCreationDate
            },

            PagingRequest: {
                CurrentPage: pageNumToFetch,
                PageSize: pageSize,

                SortBy: filterOptions.sortBy,
                SortOrder: filterOptions.sortOrder
            }
        }
    }

    fetchCourses(apiOptions)
}

function filterCourses() {
    collectFilterState();

    const pageSize = parseInt($(".pagination-options #pageSize").text()) || 6;

    const filterOptions = {
        sortBy: $('#sortFilter').val() || "CreationDate",
        sortOrder: $('#sortOrder').val() || "DESC"
    }

    const apiOptions = {
        url: `/filter-courses`,
        data: {
            FilterGroups: {
                CategoryNames: filterState.CategoryNames,
                LanguageNames: filterState.LanguageNames,
                LevelNames: filterState.LevelNames,

                MinDuration: filterState.MinDuration,
                MaxDuration: filterState.MaxDuration,
                MinEnrollments: filterState.MinEnrollments,
                MaxEnrollments: filterState.MaxEnrollments,
                MinReviews: filterState.MinReviews,
                MaxReviews: filterState.MaxReviews,
                MinRating: filterState.MinRating,
                MaxRating: filterState.MaxRating,
                MinCreationDate: filterState.MinCreationDate,
                MaxCreationDate: filterState.MaxCreationDate
            },

            PagingRequest: {
                CurrentPage: 1,
                PageSize: pageSize,

                SortBy: filterOptions.sortBy,
                SortOrder: filterOptions.sortOrder
            }
        }
    }

    fetchCourses(apiOptions)
}

function fetchCourses(apiOptions) {
    const xhr = new XMLHttpRequest();

    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            const jsonResponse = JSON.parse(xhr.responseText);
            updateCoursesView(jsonResponse.coursesGrid);
            updatePaginationView(jsonResponse.pagination);
            $(".results-count strong").html(jsonResponse.totalCount);
            //updateFiltersView(jsonResponse.filterGroups);
        }
    };

    xhr.open('POST', apiOptions.url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');
    console.log(JSON.stringify(apiOptions.data));
    xhr.send(JSON.stringify(apiOptions.data));
}


// Triggers
$(document).on('change','.range-input, .filter-checkbox, .filter-range-number, .filter-range-date', function () {
    filterCourses();
});

// ==========================================
// 5. SORTING BUTTONS
// ==========================================

//$(document).ready(function () {
//});