// ========================================
// EDIT COURSE - JavaScript
// ========================================

(function () {
    "use strict";

    // ========================================
    // TABS FUNCTIONALITY
    // ========================================

    function initTabs() {
        const tabLinks = document.querySelectorAll(".tab-link");
        const tabPanes = document.querySelectorAll(".tab-pane");

        tabLinks.forEach((link) => {
            link.addEventListener("click", () => {
                const targetTab = link.dataset.tab;

                // Remove active class from all tabs
                tabLinks.forEach((l) => l.classList.remove("active"));
                tabPanes.forEach((p) => p.classList.remove("active"));

                // Add active class to clicked tab
                link.classList.add("active");
                document
                    .querySelector(`.tab-pane[data-tab="${targetTab}"]`)
                    .classList.add("active");
            });
        });
    }

    // ========================================
    // MODULE COLLAPSE/EXPAND
    // ========================================

    function initModuleToggle() {
        const toggleButtons = document.querySelectorAll(".btn-toggle");

        toggleButtons.forEach((btn) => {
            btn.addEventListener("click", () => {
                const moduleCard = btn.closest(".edit-module-card");
                const moduleBody =
                    moduleCard.querySelector(".module-card-body");

                if (moduleBody.style.display === "none") {
                    moduleBody.style.display = "block";
                    btn.classList.remove("rotated");
                } else {
                    moduleBody.style.display = "none";
                    btn.classList.add("rotated");
                }
            });
        });
    }

    // ========================================
    // ADD/REMOVE OUTCOMES
    // ========================================

    function initOutcomes() {
        const addBtn = document.getElementById("addEditOutcome");
        const container = document.getElementById("editOutcomesList");

        if (addBtn) {
            addBtn.addEventListener("click", () => {
                const outcomeHTML = `
                    <div class="outcome-item-edit">
                        <i class="fas fa-check-circle"></i>
                        <input type="text" class="form-input" placeholder="Enter learning outcome">
                        <button type="button" class="btn-remove">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `;
                container.insertAdjacentHTML("beforeend", outcomeHTML);
                attachRemoveHandler();
            });
        }

        attachRemoveHandler();
    }

    function attachRemoveHandler() {
        const removeButtons = document.querySelectorAll(
            ".outcome-item-edit .btn-remove"
        );
        removeButtons.forEach((btn) => {
            btn.onclick = () => {
                if (
                    document.querySelectorAll(".outcome-item-edit").length > 1
                ) {
                    btn.closest(".outcome-item-edit").remove();
                } else {
                    alert("You must have at least one learning outcome");
                }
            };
        });
    }

    // ========================================
    // ARCHIVE COURSE
    // ========================================

    function initArchive() {
        const archiveBtn = document.getElementById("archiveCourseBtn");

        if (archiveBtn) {
            archiveBtn.addEventListener("click", () => {
                if (
                    confirm(
                        "Are you sure you want to archive this course? Students will no longer be able to enroll."
                    )
                ) {
                    console.log("Archiving course...");
                    alert("Course archived successfully");
                    // Redirect or update UI
                }
            });
        }
    }

    // ========================================
    // SAVE CHANGES
    // ========================================

    function initSaveChanges() {
        const saveBtn = document.getElementById("saveChangesBtn");

        if (saveBtn) {
            saveBtn.addEventListener("click", () => {
                console.log("Saving course changes...");
                alert("Changes saved successfully!");
            });
        }
    }

    // ========================================
    // CHANGE THUMBNAIL
    // ========================================

    function initThumbnailChange() {
        const changeBtn = document.getElementById("changeThumbnailBtn");

        if (changeBtn) {
            changeBtn.addEventListener("click", () => {
                const input = document.createElement("input");
                input.type = "file";
                input.accept = "image/*";
                input.onchange = (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        const reader = new FileReader();
                        reader.onload = (event) => {
                            document.querySelector(
                                ".current-thumbnail img"
                            ).src = event.target.result;
                        };
                        reader.readAsDataURL(file);
                    }
                };
                input.click();
            });
        }
    }

    // ========================================
    // INITIALIZE
    // ========================================

    function init() {
        initTabs();
        initModuleToggle();
        initOutcomes();
        initArchive();
        initSaveChanges();
        initThumbnailChange();

        console.log("Edit Course initialized");
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();

// NEW FUNCTIONALITIES BELOW
// Curriculum Manager JavaScript
let currentModuleForLesson = null;

// Toggle Module
function toggleModule(button) {
    const moduleCard = button.closest(".edit-module-card");
    const moduleBody = moduleCard.querySelector(".module-card-body");
    const icon = button.querySelector("i");

    if (moduleBody.style.display === "none") {
        moduleBody.style.display = "block";
        icon.classList.remove("fa-chevron-down");
        icon.classList.add("fa-chevron-up");
        button.classList.add("active");
    } else {
        moduleBody.style.display = "none";
        icon.classList.remove("fa-chevron-up");
        icon.classList.add("fa-chevron-down");
        button.classList.remove("active");
    }
}

// Add New Module
document.getElementById("addNewModule")?.addEventListener("click", function () {
    openModal("addModuleModal");
    document.getElementById("newModuleTitle").value = "";
});

function saveModuleFromModal() {
    const title = document.getElementById("newModuleTitle").value.trim();

    if (!title) {
        alert("Please enter a module title");
        return;
    }

    const modulesList = document.getElementById("editModulesList");
    const moduleCount =
        modulesList.querySelectorAll(".edit-module-card").length + 1;

    const newModule = createModuleElement(moduleCount, title);
    modulesList.appendChild(newModule);

    closeModal("addModuleModal");
    updateModuleNumbers();
}

function createModuleElement(number, title) {
    const div = document.createElement("div");
    div.className = "edit-module-card";
    div.draggable = true;
    div.innerHTML = `
        <div class="module-card-header">
            <div class="module-info">
                <button class="drag-handle" title="Drag to reorder">
                    <i class="fas fa-grip-vertical"></i>
                </button>
                <div class="module-number">${number}</div>
                <div class="module-details">
                    <h4 class="module-title-display">${title}</h4>
                    <div class="module-meta">
                        <span class="meta-item">
                            <i class="fas fa-book"></i>
                            <span class="lesson-count">0</span> lessons
                        </span>
                        <span class="meta-item">
                            <i class="fas fa-clock"></i>
                            <span class="module-duration">0m</span>
                        </span>
                    </div>
                </div>
            </div>
            <div class="module-card-actions">
                <button type="button" class="btn-icon" title="Edit Module" onclick="editModule(this)">
                    <i class="fas fa-edit"></i>
                </button>
                <button type="button" class="btn-icon btn-danger-ghost" title="Delete Module" onclick="deleteModule(this)">
                    <i class="fas fa-trash"></i>
                </button>
                <button type="button" class="btn-icon btn-toggle" onclick="toggleModule(this)">
                    <i class="fas fa-chevron-down"></i>
                </button>
            </div>
        </div>
        <div class="module-card-body">
            <div class="lessons-list-edit"></div>
            <button type="button" class="btn-add-lesson" onclick="addLesson(this)">
                <i class="fas fa-plus"></i>
                <span>Add Lesson to This Module</span>
            </button>
        </div>
    `;

    setupModuleDragDrop(div);
    return div;
}

// Add Lesson
function addLesson(button) {
    currentModuleForLesson = button.closest(".edit-module-card");
    openModal("addLessonModal");
    document.getElementById("newLessonTitle").value = "";
    document.getElementById("newLessonDuration").value = "";
    document.querySelector(
        'input[name="lessonType"][value="video"]'
    ).checked = true;
}

function saveLessonFromModal() {
    const title = document.getElementById("newLessonTitle").value.trim();
    const duration = document.getElementById("newLessonDuration").value.trim();
    const type = document.querySelector(
        'input[name="lessonType"]:checked'
    ).value;

    if (!title) {
        alert("Please enter a lesson title");
        return;
    }

    const lessonsList =
        currentModuleForLesson.querySelector(".lessons-list-edit");
    const newLesson = createLessonElement(title, type, duration);
    lessonsList.appendChild(newLesson);

    updateModuleStats(currentModuleForLesson);
    closeModal("addLessonModal");
}

function createLessonElement(title, type, duration) {
    const typeConfig = {
        video: {
            icon: "fa-play-circle",
            iconClass: "video",
            badgeIcon: "fa-video",
        },
        article: {
            icon: "fa-file-alt",
            iconClass: "article",
            badgeIcon: "fa-file-alt",
        },
        quiz: {
            icon: "fa-question-circle",
            iconClass: "quiz",
            badgeIcon: "fa-question-circle",
        },
    };

    const config = typeConfig[type];
    const div = document.createElement("div");
    div.className = "lesson-card-edit";
    div.draggable = true;
    div.innerHTML = `
        <button class="drag-handle-small" title="Drag to reorder">
            <i class="fas fa-grip-vertical"></i>
        </button>
        <div class="lesson-type-icon ${config.iconClass}">
            <i class="fas ${config.icon}"></i>
        </div>
        <div class="lesson-info-edit">
            <div class="lesson-title-edit">${title}</div>
            <div class="lesson-meta-edit">
                <span class="lesson-type-badge ${config.iconClass}">
                    <i class="fas ${config.badgeIcon}"></i>
                    ${type.charAt(0).toUpperCase() + type.slice(1)}
                </span>
                <span class="lesson-duration">${duration || "N/A"}</span>
            </div>
        </div>
        <div class="lesson-actions-edit">
            <button type="button" class="btn-icon-small" title="Edit" onclick="editLesson(this)">
                <i class="fas fa-edit"></i>
            </button>
            <button type="button" class="btn-icon-small btn-danger-ghost" title="Delete" onclick="deleteLesson(this)">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    `;

    setupLessonDragDrop(div);
    return div;
}

// Edit Module
function editModule(button) {
    const moduleCard = button.closest(".edit-module-card");
    const titleElement = moduleCard.querySelector(".module-title-display");
    const currentTitle = titleElement.textContent.trim();

    const newTitle = prompt("Edit Module Title:", currentTitle);
    if (newTitle && newTitle.trim() !== "") {
        titleElement.textContent = newTitle.trim();
    }
}

// Delete Module
function deleteModule(button) {
    if (
        confirm(
            "Are you sure you want to delete this module? All lessons will be removed."
        )
    ) {
        const moduleCard = button.closest(".edit-module-card");
        moduleCard.remove();
        updateModuleNumbers();
    }
}

// Edit Lesson
function editLesson(button) {
    const lessonCard = button.closest(".lesson-card-edit");
    const titleElement = lessonCard.querySelector(".lesson-title-edit");
    const currentTitle = titleElement.textContent.trim();

    const newTitle = prompt("Edit Lesson Title:", currentTitle);
    if (newTitle && newTitle.trim() !== "") {
        titleElement.textContent = newTitle.trim();
    }
}

// Delete Lesson
function deleteLesson(button) {
    if (confirm("Are you sure you want to delete this lesson?")) {
        const lessonCard = button.closest(".lesson-card-edit");
        const moduleCard = lessonCard.closest(".edit-module-card");
        lessonCard.remove();
        updateModuleStats(moduleCard);
    }
}

// Update Module Numbers
function updateModuleNumbers() {
    const modules = document.querySelectorAll(".edit-module-card");
    modules.forEach((module, index) => {
        module.querySelector(".module-number").textContent = index + 1;
    });
}

// Update Module Stats
function updateModuleStats(moduleCard) {
    const lessons = moduleCard.querySelectorAll(".lesson-card-edit");
    const lessonCount = lessons.length;

    moduleCard.querySelector(".lesson-count").textContent = lessonCount;

    // You can add duration calculation here if needed
}

// Drag and Drop for Modules
function setupModuleDragDrop(module) {
    module.addEventListener("dragstart", handleModuleDragStart);
    module.addEventListener("dragend", handleModuleDragEnd);
    module.addEventListener("dragover", handleModuleDragOver);
    module.addEventListener("drop", handleModuleDrop);
}

let draggedModule = null;

function handleModuleDragStart(e) {
    draggedModule = this;
    this.classList.add("dragging");
    e.dataTransfer.effectAllowed = "move";
}

function handleModuleDragEnd(e) {
    this.classList.remove("dragging");
    document.querySelectorAll(".edit-module-card").forEach((module) => {
        module.classList.remove("drag-over");
    });
}

function handleModuleDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    e.dataTransfer.dropEffect = "move";

    const afterElement = getDragAfterElement(this.parentElement, e.clientY);
    if (afterElement == null) {
        this.parentElement.appendChild(draggedModule);
    } else {
        this.parentElement.insertBefore(draggedModule, afterElement);
    }

    return false;
}

function handleModuleDrop(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    updateModuleNumbers();
    return false;
}

// Drag and Drop for Lessons
function setupLessonDragDrop(lesson) {
    lesson.addEventListener("dragstart", handleLessonDragStart);
    lesson.addEventListener("dragend", handleLessonDragEnd);
    lesson.addEventListener("dragover", handleLessonDragOver);
    lesson.addEventListener("drop", handleLessonDrop);
}

let draggedLesson = null;

function handleLessonDragStart(e) {
    draggedLesson = this;
    this.classList.add("dragging");
    e.dataTransfer.effectAllowed = "move";
}

function handleLessonDragEnd(e) {
    this.classList.remove("dragging");
    document.querySelectorAll(".lesson-card-edit").forEach((lesson) => {
        lesson.classList.remove("drag-over");
    });
}

function handleLessonDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    e.dataTransfer.dropEffect = "move";

    const container = this.parentElement;
    const afterElement = getDragAfterElement(container, e.clientY);
    if (afterElement == null) {
        container.appendChild(draggedLesson);
    } else {
        container.insertBefore(draggedLesson, afterElement);
    }

    return false;
}

function handleLessonDrop(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    return false;
}

function getDragAfterElement(container, y) {
    const draggableElements = [
        ...container.querySelectorAll(
            ".edit-module-card:not(.dragging), .lesson-card-edit:not(.dragging)"
        ),
    ];

    return draggableElements.reduce(
        (closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;

            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child };
            } else {
                return closest;
            }
        },
        { offset: Number.NEGATIVE_INFINITY }
    ).element;
}

// Modal Functions
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.add("active");
    document.body.style.overflow = "hidden";
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove("active");
    document.body.style.overflow = "";
}

// Initialize Drag and Drop on Page Load
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".edit-module-card").forEach(setupModuleDragDrop);
    document.querySelectorAll(".lesson-card-edit").forEach(setupLessonDragDrop);
});
