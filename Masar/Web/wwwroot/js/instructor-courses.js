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
            const sortBy = sortFilter ? sortFilter.value : "CreationDate";
            sortCourses(sortBy, sortOrderValue);
        });
    }

    if (sortFilter) {
        sortFilter.addEventListener("change", function () {
            const sortBy = this.value;
            const sortOrderValue = sortOrder ? sortOrder.value : "Descending";
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
                case "CreationDate":
                    // Sort by date created
                    const dateA = new Date(a.dataset.creationDate);
                    const dateB = new Date(b.dataset.creationDate);

                    if (sortOrder == "Ascending")
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
                    if (sortOrder == "Ascending")
                        return studentsA - studentsB;
                    else
                        return studentsB - studentsA;

                case "Title":
                    // Sort by title
                    const titleA = a.querySelector(".course-title").textContent.toLowerCase();
                    const titleB = b.querySelector(".course-title").textContent.toLowerCase();
                    if (sortOrder == "Ascending")
                        return titleA.localeCompare(titleB);
                    else
                        return titleB.localeCompare(titleA);

                case "Rating":
                    // Sort by rating
                    const ratingA = parseFloat(
                        a.querySelector(".rating-value").textContent
                    );
                    const ratingB = parseFloat(
                        b.querySelector(".rating-value").textContent
                    );
                    if (sortOrder == "Ascending")
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
});

function updateCoursesView(coursesListContent) {
    $("#coursesGrid").html(coursesListContent);
}

function updatePaginationView(paginationContent) {
    $(".pagination-container").html(paginationContent);
}

function fetchPage(caller, dir) {
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
