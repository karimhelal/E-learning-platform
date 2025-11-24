// ========================================
// MY COURSES - JavaScript
// ========================================

//(function () {
//    "use strict";

//    // ========================================
//    // SEARCH FUNCTIONALITY
//    // ========================================

//    function initSearch() {
//        const searchInput = document.getElementById("courseSearch");
//        const courseCards = document.querySelectorAll(".my-course-card");

//        searchInput.addEventListener("input", (e) => {
//            const searchTerm = e.target.value.toLowerCase();

//            courseCards.forEach((card) => {
//                const title = card
//                    .querySelector(".course-title")
//                    .textContent.toLowerCase();
//                const description = card
//                    .querySelector(".course-description")
//                    .textContent.toLowerCase();

//                if (
//                    title.includes(searchTerm) ||
//                    description.includes(searchTerm)
//                ) {
//                    card.style.display = "";
//                } else {
//                    card.style.display = "none";
//                }
//            });

//            updateEmptyState();
//        });
//    }

//    // ========================================
//    // FILTER BY STATUS
//    // ========================================

//    function initStatusFilter() {
//        const statusFilter = document.getElementById("statusFilter");
//        const courseCards = document.querySelectorAll(".my-course-card");

//        statusFilter.addEventListener("change", (e) => {
//            const status = e.target.value;

//            courseCards.forEach((card) => {
//                if (status === "all" || card.dataset.status === status) {
//                    card.style.display = "";
//                } else {
//                    card.style.display = "none";
//                }
//            });

//            updateEmptyState();
//        });
//    }

//    // ========================================
//    // SORT FUNCTIONALITY
//    // ========================================

//    function initSort() {
//        const sortFilter = document.getElementById("sortFilter");
//        const coursesGrid = document.getElementById("coursesGrid");
//        const courseCards = Array.from(
//            document.querySelectorAll(".my-course-card")
//        );

//        sortFilter.addEventListener("change", (e) => {
//            const sortBy = e.target.value;
//            let sortedCards = [...courseCards];

//            // Simple sorting (in production, would use actual data)
//            if (sortBy === "newest") {
//                sortedCards.reverse();
//            } else if (sortBy === "popular") {
//                sortedCards.sort((a, b) => {
//                    const aStudents = parseInt(
//                        a.querySelector(".stat-item:first-child span")
//                            .textContent
//                    );
//                    const bStudents = parseInt(
//                        b.querySelector(".stat-item:first-child span")
//                            .textContent
//                    );
//                    return (bStudents || 0) - (aStudents || 0);
//                });
//            } else if (sortBy === "rating") {
//                sortedCards.sort((a, b) => {
//                    const aRating = parseFloat(
//                        a.querySelector(".course-rating span").textContent
//                    );
//                    const bRating = parseFloat(
//                        b.querySelector(".course-rating span").textContent
//                    );
//                    return (bRating || 0) - (aRating || 0);
//                });
//            }

//            // Re-append sorted cards
//            sortedCards.forEach((card) => coursesGrid.appendChild(card));
//        });
//    }

//    // ========================================
//    // VIEW TOGGLE (Grid/List)
//    // ========================================

//    function initViewToggle() {
//        const viewButtons = document.querySelectorAll(".view-btn");
//        const coursesGrid = document.getElementById("coursesGrid");

//        viewButtons.forEach((btn) => {
//            btn.addEventListener("click", () => {
//                viewButtons.forEach((b) => b.classList.remove("active"));
//                btn.classList.add("active");

//                const view = btn.dataset.view;

//                if (view === "list") {
//                    coursesGrid.classList.add("list-view");
//                } else {
//                    coursesGrid.classList.remove("list-view");
//                }
//            });
//        });
//    }

//    // ========================================
//    // EMPTY STATE
//    // ========================================

//    function updateEmptyState() {
//        const courseCards = document.querySelectorAll(".my-course-card");
//        const visibleCards = Array.from(courseCards).filter(
//            (card) => card.style.display !== "none"
//        );
//        const emptyState = document.getElementById("emptyState");
//        const coursesGrid = document.getElementById("coursesGrid");

//        if (visibleCards.length === 0) {
//            coursesGrid.style.display = "none";
//            emptyState.style.display = "flex";
//        } else {
//            coursesGrid.style.display = "grid";
//            emptyState.style.display = "none";
//        }
//    }

//    // ========================================
//    // INITIALIZE
//    // ========================================

//    function init() {
//        initSearch();
//        initStatusFilter();
//        initSort();
//        initViewToggle();

//        console.log("My Courses initialized");
//    }

//    if (document.readyState === "loading") {
//        document.addEventListener("DOMContentLoaded", init);
//    } else {
//        init();
//    }
//})();

// Instructor Courses Page JavaScript
document.addEventListener("DOMContentLoaded", function () {
    // Search functionality
    const searchInput = document.getElementById("courseSearch");
    const clearSearch = document.getElementById("clearSearch");

    if (searchInput) {
        searchInput.addEventListener("input", function (e) {
            const searchTerm = e.target.value.toLowerCase();
            filterCourses(searchTerm);

            if (e.target.value.length > 0) {
                clearSearch.style.display = "block";
            } else {
                clearSearch.style.display = "none";
            }
        });
    }

    if (clearSearch) {
        clearSearch.addEventListener("click", function () {
            searchInput.value = "";
            clearSearch.style.display = "none";
            searchInput.focus();
            filterCourses("");
        });
    }

    // Status Filter
    const statusFilter = document.getElementById("statusFilter");
    if (statusFilter) {
        statusFilter.addEventListener("change", function () {
            const selectedStatus = this.value;
            filterByStatus(selectedStatus);
        });
    }

    // Sort Filter
    const sortFilter = document.getElementById("sortFilter");
    const sortOrder = document.getElementById("sortOrder");

    if (sortOrder) {
        sortOrder.addEventListener("change", function () {
            const sortOrderValue = this.value;
            const sortBy = sortFilter ? sortFilter.value : "createdDate";
            sortCourses(sortBy, sortOrderValue);
        });
    }

    if (sortFilter) {
        sortFilter.addEventListener("change", function () {
            const sortBy = this.value;
            const sortOrderValue = sortOrder ? sortOrder.value : "desc";
            sortCourses(sortBy, sortOrderValue);
        });
    }

    // View Toggle - LIST/GRID
    const viewBtns = document.querySelectorAll(".view-btn");
    const coursesGrid = document.getElementById("coursesGrid");

    viewBtns.forEach((btn) => {
        btn.addEventListener("click", function () {
            // Remove active class from all buttons
            viewBtns.forEach((b) => b.classList.remove("active"));

            // Add active class to clicked button
            this.classList.add("active");

            // Get the view type
            const view = this.dataset.view;

            // Toggle classes on courses grid
            if (view === "grid") {
                coursesGrid.classList.remove("view-list");
                coursesGrid.classList.add("view-grid");
            } else if (view === "list") {
                coursesGrid.classList.remove("view-grid");
                coursesGrid.classList.add("view-list");
            }
        });
    });

    // Course dropdown menus
    const menuDropdowns = document.querySelectorAll(".course-menu-dropdown");

    menuDropdowns.forEach((dropdown) => {
        const trigger = dropdown.querySelector(".menu-trigger");

        trigger.addEventListener("click", function (e) {
            e.stopPropagation();

            // Close all other dropdowns
            menuDropdowns.forEach((d) => {
                if (d !== dropdown) {
                    d.classList.remove("active");
                }
            });

            // Toggle current dropdown
            dropdown.classList.toggle("active");
        });
    });

    // Close dropdowns when clicking outside
    document.addEventListener("click", function (e) {
        if (!e.target.closest(".course-menu-dropdown")) {
            menuDropdowns.forEach((dropdown) => {
                dropdown.classList.remove("active");
            });
        }
    });

    // Filter courses by search term
    function filterCourses(searchTerm) {
        const cards = document.querySelectorAll(".my-course-card");
        let visibleCount = 0;

        cards.forEach((card) => {
            const title = card
                .querySelector(".course-title")
                .textContent.toLowerCase();
            const description = card
                .querySelector(".course-description")
                .textContent.toLowerCase();
            const category = card
                .querySelector(".course-category")
                .textContent.toLowerCase();

            if (
                title.includes(searchTerm) ||
                description.includes(searchTerm) ||
                category.includes(searchTerm)
            ) {
                card.style.display = "flex";
                visibleCount++;
            } else {
                card.style.display = "none";
            }
        });

        // Show/hide empty state
        toggleEmptyState(visibleCount);
    }

    // Filter by status
    function filterByStatus(status) {
        const cards = document.querySelectorAll(".my-course-card");
        let visibleCount = 0;

        cards.forEach((card) => {
            const cardStatus = card.dataset.status;

            if (status === "all" || cardStatus === status) {
                card.style.display = "flex";
                visibleCount++;
            } else {
                card.style.display = "none";
            }
        });

        toggleEmptyState(visibleCount);
    }

    // Sort courses
    function sortCourses(sortBy, sortOrder) {
        const grid = document.getElementById("coursesGrid");
        const cards = Array.from(grid.querySelectorAll(".my-course-card"));

        cards.sort((a, b) => {
            switch (sortBy) {
                case "createdDate":
                    // Sort by date created
                    const dateA = new Date(a.dataset.creationDate);
                    const dateB = new Date(b.dataset.creationDate);

                    if (sortOrder == "asc")
                        return dateA - dateB;
                    else
                        return dateB - dateA;

                case "students":
                    // Sort by number of students
                    const studentsA = parseInt(
                        a
                            .querySelector(".stat-item.total-students .stat-value")
                            .textContent.replace(',', "")
                    );
                    const studentsB = parseInt(
                        b
                            .querySelector(".stat-item.total-students .stat-value")
                            .textContent.replace(',', "")
                    );
                    if (sortOrder == "asc")
                        return studentsA - studentsB;
                    else
                        return studentsB - studentsA;

                case "title":
                    // Sort by title
                    const titleA = a.querySelector(".course-title").textContent.toLowerCase();
                    const titleB = b.querySelector(".course-title").textContent.toLowerCase();
                    if (sortOrder == "asc")
                        return titleA.localeCompare(titleB);
                    else
                        return titleB.localeCompare(titleA);

                case "rate":
                    // Sort by rating
                    const ratingA = parseFloat(
                        a.querySelector(".rating-value").textContent
                    );
                    const ratingB = parseFloat(
                        b.querySelector(".rating-value").textContent
                    );
                    if (sortOrder == "asc")
                        return ratingA - ratingB;
                    else
                        return ratingB - ratingA;

                default:
                    return 0;
            }
        });

        // Re-append sorted cards
        cards.forEach((card) => grid.appendChild(card));
    }

    // Toggle empty state
    function toggleEmptyState(visibleCount) {
        const emptyState = document.querySelector(".empty-state");
        const coursesGrid = document.getElementById("coursesGrid");

        if (visibleCount === 0) {
            coursesGrid.style.display = "none";
            emptyState.style.display = "flex";
        } else {
            coursesGrid.style.display = "grid";
            emptyState.style.display = "none";
        }
    }

    //document.querySelectorAll(".pagintaion-btn").forEach(btn => {
    //    btn.addEventListener("click", fucntion(this) {
    //        this
    //    })
    //})
});

function updateCoursesView(coursesListContent) {
    $("#coursesGrid").html(coursesListContent);
}

function updatePaginationView(paginationContent) {
    $(".pagination-container").html(paginationContent);
}

function updateCourses(caller, dir) {
    const pageNumToFetch = parseInt((dir == 0)
        ? caller.textContent
        : (dir < 0)
            ? Math.max(1, parseInt($(".pagination-btn.active").text()) - 1)
            : Math.min(parseInt($(".pagination-btn.last-pagination-btn").text()), parseInt($(".pagination-btn.active").text()) + 1)
    );
    
    const pageSize = parseInt($(".pagination-options #pageSize").text());

    const filterOptions = {
        sortBy: $("#sortFilter").val(),
        sortOrder: $("#sortOrder").val()
    }

    const apiOptions = {
        url: `/instructor/my-courses`,
        data: {
            CurrentPage: pageNumToFetch,
            PageSize: pageSize,

            SortBy: filterOptions.sortBy,
            SortOrder: filterOptions.sortOrder
        }
    }

    fetchCourses(apiOptions)
}

function fetchCourses(apiOptions) {
    const xhr = new XMLHttpRequest();

    xhr.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            const jsonResponse = JSON.parse(xhr.responseText);
            console.log(jsonResponse.pagination);
            updateCoursesView(jsonResponse.coursesGrid);
            updatePaginationView(jsonResponse.pagination);
        }
    };

    xhr.open('POST', apiOptions.url, true);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(JSON.stringify(apiOptions.data));
}



//$(document).ready(function () {
//    const currentPage = $(".pagination-btn.active").text();
//    const pageSize = 1;

//    const sortBy = document.getElementById('sortFilter').value;
//    const sortOrder = document.getElementById('sortOrder').value;

//    const url = `/instructor/my-courses?CurrentPage=${currentPage}&PageSize=${pageSize}&SortBy=${sortBy}&SortOrder=${sortOrder}`;
//    const xhr = new XMLHttpRequest();
//    xhr.onreadystatechange = function () {
//        if (this.readyState == 4 && this.status == 200)
//            console.log(xhr.responseText);
//    };
//    xhr.open('POST', url, true);
//    xhr.setRequestHeader('Content-Type', 'x-www-form-urlencoded');
//    xhr.send()
//});
