document.addEventListener("DOMContentLoaded", () => {
    // ==========================================
    // 1. FILTER SIDEBAR (Mobile & Desktop)
    // ==========================================
    const filterSidebar = document.getElementById("filterSidebar");
    const filterOverlay = document.getElementById("filterOverlay");
    const openFilterBtn = document.getElementById("openFilter");
    const closeFilterBtn = document.getElementById("closeFilter");

    // Desktop Toggle Elements
    const toggleSidebarBtn = document.getElementById("toggleSidebar");
    const toggleText = document.getElementById("toggleText");

    // --- Mobile Logic ---
    function toggleMobileFilter() {
        filterSidebar.classList.toggle("active");
        filterOverlay.classList.toggle("active");
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
            filterSidebar.classList.toggle("hidden");

            if (filterSidebar.classList.contains("hidden")) {
                toggleText.textContent = "Show Filters";
                toggleSidebarBtn.classList.add("btn-outline");
                toggleSidebarBtn.classList.remove("btn-secondary");
            } else {
                toggleText.textContent = "Hide Filters";
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
    // 2. VIEW TOGGLE (Grid vs List)
    // ==========================================
    const gridViewBtn = document.getElementById("gridViewBtn");
    const listViewBtn = document.getElementById("listViewBtn");
    const coursesCatalog = document.querySelector(".courses-catalog");

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
    // 3. SORTING BUTTONS
    // ==========================================

    // Toggle Dropdown Menu on Click
    $('.filter-dropdown-btn').on('click', function (e) {
        e.stopPropagation();

        var $dropdown = $(this).closest('.filter-dropdown');
        $('.filter-dropdown').not($dropdown).removeClass('active');
        $dropdown.toggleClass('active');
    });

    // Handle Option Selection
    $('.filter-option').on('click', function () {
        var $parent = $(this).closest('.filter-dropdown');
        var $hiddenInput = $parent.find('input[type="hidden"]');
        var $displaySpan = $parent.find('.filter-dropdown-btn span');

        var value = $(this).data('value');
        var text = $(this).text().trim();

        $displaySpan.text(text);
        $parent.find('.filter-option').removeClass('active');
        $(this).addClass('active');
        $parent.removeClass('active');
        $hiddenInput.val(value);

        filterCourses();
    });

    // Close dropdowns when clicking outside
    $(document).on('click', function () {
        $('.filter-dropdown').removeClass('active');
    });


    // ==========================================
    // 4. RANGE SLIDERS
    // ==========================================
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

            if (minVal > maxVal) {
                [minVal, maxVal] = [maxVal, minVal];
            }

            const left = ((minVal - minAttr) / range) * 100;
            const right = 100 - ((maxVal - minAttr) / range) * 100;

            progress.style.left = left + '%';
            progress.style.right = right + '%';

            if (minValueEl) minValueEl.textContent = minVal + ' ' + (minHidden?.dataset.unit ?? '');
            if (maxValueEl) maxValueEl.textContent = maxVal + ' ' + (maxHidden?.dataset.unit ?? '');

            if (minHidden) minHidden.value = minVal;
            if (maxHidden) maxHidden.value = maxVal;
        }

        minRange.addEventListener('input', updateSlider);
        maxRange.addEventListener('input', updateSlider);

        updateSlider();
    });
});

// ==========================================
// FILTER STATE & API
// ==========================================

const filterState = {
    CategoryNames: [],
    LevelNames: [],
    LanguageNames: [],
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

function collectFilterState() {
    filterState.CategoryNames = [];
    filterState.LevelNames = [];
    filterState.LanguageNames = [];

    $('.filter-checkbox:checked').each(function () {
        const key = $(this).data('key');
        const value = $(this).val();
        filterState[key].push(value);
    });

    $('.filter-range-number').each(function () {
        const key = $(this).data('key');
        const val = $(this).val();
        filterState[key] = val ? parseFloat(val) : null;
    });

    $('.filter-range-date').each(function () {
        const key = $(this).data('key');
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

function fetchPage(caller, dir) {
    collectFilterState();

    const pageNumToFetch = parseInt((dir == 0)
        ? caller.textContent
        : (dir < 0)
            ? Math.max(1, parseInt($(".pagination-btn.active").text()) - 1)
            : Math.min(parseInt($(".pagination-btn.last-pagination-btn").text()), parseInt($(".pagination-btn.active").text()) + 1)
    );

    const pageSize = parseInt($(".pagination-options #pageSize").text()) || 6;

    const filterOptions = {
        sortBy: $('#sortFilter').val() || "CreationDate",
        sortOrder: $('#sortOrder').val() || "Descending"
    };

    const apiOptions = {
        url: `/student/filter-courses`,
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
    };

    fetchCourses(apiOptions);
}

function filterCourses() {
    collectFilterState();

    const pageSize = parseInt($(".pagination-options #pageSize").text()) || 6;

    const filterOptions = {
        sortBy: $('#sortFilter').val() || "CreationDate",
        sortOrder: $('#sortOrder').val() || "Descending"
    };

    const apiOptions = {
        url: `/student/filter-courses`,
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
    };

    fetchCourses(apiOptions);
}

function fetchCourses(apiOptions) {
    const xhr = new XMLHttpRequest();

    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            const jsonResponse = JSON.parse(xhr.responseText);
            updateCoursesView(jsonResponse.coursesGrid);
            updatePaginationView(jsonResponse.pagination);
            $(".results-count strong").html(jsonResponse.totalCount);
        }
    };

    xhr.open('POST', apiOptions.url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(JSON.stringify(apiOptions.data));
}

// Triggers
$(document).on('change', '.range-input, .filter-checkbox, .filter-range-number, .filter-range-date', function () {
    filterCourses();
});
