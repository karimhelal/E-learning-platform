// ========================================
// CREATE COURSE - JavaScript
// ========================================

(function () {
    "use strict";

    let currentStep = 1;
    let moduleCount = 1;
    let currentLessonElement = null;

    // ========================================
    // STEP NAVIGATION
    // ========================================

    function initStepNavigation() {
        const nextButtons = document.querySelectorAll("[data-next-step]");
        const prevButtons = document.querySelectorAll("[data-prev-step]");

        nextButtons.forEach((btn) => {
            btn.addEventListener("click", () => {
                const nextStep = parseInt(btn.dataset.nextStep);
                if (validateCurrentStep()) {
                    goToStep(nextStep);
                }
            });
        });

        prevButtons.forEach((btn) => {
            btn.addEventListener("click", () => {
                const prevStep = parseInt(btn.dataset.prevStep);
                goToStep(prevStep);
            });
        });
    }

    function goToStep(stepNumber) {
        document
            .querySelector(`.form-step[data-step="${currentStep}"]`)
            .classList.remove("active");
        document
            .querySelector(`.progress-step[data-step="${currentStep}"]`)
            .classList.remove("active");

        currentStep = stepNumber;
        document
            .querySelector(`.form-step[data-step="${currentStep}"]`)
            .classList.add("active");
        document
            .querySelector(`.progress-step[data-step="${currentStep}"]`)
            .classList.add("active");

        for (let i = 1; i < currentStep; i++) {
            document
                .querySelector(`.progress-step[data-step="${i}"]`)
                .classList.add("completed");
        }

        window.scrollTo({ top: 0, behavior: "smooth" });
    }

    function validateCurrentStep() {
        const currentStepElement = document.querySelector(
            `.form-step[data-step="${currentStep}"]`
        );
        const requiredInputs = currentStepElement.querySelectorAll("[required]");

        let isValid = true;
        requiredInputs.forEach((input) => {
            if (!input.value.trim()) {
                input.style.borderColor = "var(--accent-red)";
                isValid = false;
            } else {
                input.style.borderColor = "var(--border-color)";
            }
        });

        if (!isValid) {
            alert("Please fill in all required fields");
        }

        return isValid;
    }

    // ========================================
    // THUMBNAIL UPLOAD
    // ========================================

    function initThumbnailUpload() {
        const uploadArea = document.getElementById("thumbnailUpload");
        const fileInput = document.getElementById("courseThumbnail");
        const preview = document.getElementById("thumbnailPreview");
        const previewImage = document.getElementById("thumbnailImage");
        const removeBtn = document.getElementById("removeThumbnail");

        if (!uploadArea || !fileInput) return;

        uploadArea.addEventListener("click", () => fileInput.click());

        fileInput.addEventListener("change", (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    previewImage.src = e.target.result;
                    document.getElementById("thumbnailUrl").value = e.target.result;
                    uploadArea.style.display = "none";
                    preview.style.display = "block";
                };
                reader.readAsDataURL(file);
            }
        });

        if (removeBtn) {
            removeBtn.addEventListener("click", (e) => {
                e.stopPropagation();
                fileInput.value = "";
                document.getElementById("thumbnailUrl").value = "";
                uploadArea.style.display = "flex";
                preview.style.display = "none";
            });
        }

        // Drag and drop
        uploadArea.addEventListener("dragover", (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = "var(--accent-purple)";
        });

        uploadArea.addEventListener("dragleave", () => {
            uploadArea.style.borderColor = "var(--border-color)";
        });

        uploadArea.addEventListener("drop", (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = "var(--border-color)";
            const file = e.dataTransfer.files[0];
            if (file && file.type.startsWith("image/")) {
                fileInput.files = e.dataTransfer.files;
                fileInput.dispatchEvent(new Event("change"));
            }
        });
    }

    // ========================================
    // LEARNING OUTCOMES
    // ========================================

    function initLearningOutcomes() {
        const container = document.getElementById("learningOutcomes");
        const addBtn = document.getElementById("addOutcome");

        if (!addBtn) return;

        addBtn.addEventListener("click", () => {
            const outcomeItem = document.createElement("div");
            outcomeItem.className = "outcome-item";
            outcomeItem.innerHTML = `
                <input 
                    type="text" 
                    class="form-input" 
                    placeholder="Enter a learning outcome"
                    name="outcomes[]"
                >
                <button type="button" class="btn-remove-outcome">
                    <i class="fas fa-times"></i>
                </button>
            `;
            container.appendChild(outcomeItem);
            attachRemoveHandler(
                outcomeItem.querySelector(".btn-remove-outcome"),
                outcomeItem
            );
        });

        // Attach remove handler to existing items
        container.querySelectorAll(".btn-remove-outcome").forEach((btn) => {
            attachRemoveHandler(btn, btn.closest(".outcome-item"));
        });
    }

    function attachRemoveHandler(btn, item) {
        btn.addEventListener("click", () => {
            if (document.querySelectorAll(".outcome-item").length > 1) {
                item.remove();
            } else {
                alert("You must have at least one learning outcome");
            }
        });
    }

    // ========================================
    // CURRICULUM BUILDER
    // ========================================

    function initCurriculumBuilder() {
        const addModuleBtn = document.getElementById("addModule");
        const curriculumBuilder = document.getElementById("curriculumBuilder");

        if (!addModuleBtn) return;

        addModuleBtn.addEventListener("click", () => {
            addModule();
        });

        // Initialize ALL existing modules
        curriculumBuilder.querySelectorAll(".module-container").forEach(module => {
            initModuleHandlers(module);
        });
    }

    function addModule() {
        const curriculumBuilder = document.getElementById("curriculumBuilder");
        const moduleHTML = `
            <div class="module-container">
                <div class="module-header">
                    <div class="module-title-section">
                        <i class="fas fa-grip-vertical module-drag-handle"></i>
                        <input 
                            type="text" 
                            class="module-title-input" 
                            placeholder="Module Title"
                            name="modules[${moduleCount}][title]"
                        >
                    </div>
                    <div class="module-actions">
                        <button type="button" class="btn-icon btn-add-lesson-module" title="Add Lesson">
                            <i class="fas fa-plus"></i>
                        </button>
                        <button type="button" class="btn-icon btn-collapse" title="Collapse">
                            <i class="fas fa-chevron-up"></i>
                        </button>
                        <button type="button" class="btn-icon btn-delete" title="Delete Module">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>

                <div class="module-body">
                    <div class="lessons-container"></div>
                    <button type="button" class="btn-add-lesson">
                        <i class="fas fa-plus"></i>
                        <span>Add Lesson</span>
                    </button>
                </div>
            </div>
        `;

        curriculumBuilder.insertAdjacentHTML("beforeend", moduleHTML);
        const newModule = curriculumBuilder.lastElementChild;
        initModuleHandlers(newModule);
        moduleCount++;
    }

    function initModuleHandlers(moduleContainer) {
        const deleteBtn = moduleContainer.querySelector(".btn-delete");
        const collapseBtn = moduleContainer.querySelector(".btn-collapse");
        const addLessonBtn = moduleContainer.querySelector(".btn-add-lesson");
        const addLessonModuleBtn = moduleContainer.querySelector(".btn-add-lesson-module");
        const moduleBody = moduleContainer.querySelector(".module-body");

        // Delete module
        if (deleteBtn) {
            deleteBtn.addEventListener("click", () => {
                if (confirm("Are you sure you want to delete this module?")) {
                    moduleContainer.remove();
                }
            });
        }

        // Collapse/Expand
        if (collapseBtn) {
            collapseBtn.addEventListener("click", () => {
                moduleBody.classList.toggle("collapsed");
                const icon = collapseBtn.querySelector("i");
                icon.classList.toggle("fa-chevron-up");
                icon.classList.toggle("fa-chevron-down");
            });
        }

        // Add lesson (bottom button)
        if (addLessonBtn) {
            addLessonBtn.addEventListener("click", () => {
                addLesson(moduleContainer);
            });
        }

        // Add lesson (header button)
        if (addLessonModuleBtn) {
            addLessonModuleBtn.addEventListener("click", () => {
                addLesson(moduleContainer);
            });
        }

        // Initialize ALL existing lessons in this module
        moduleContainer.querySelectorAll(".lesson-item").forEach(lesson => {
            initLessonHandlers(lesson);
        });
    }

    function addLesson(moduleContainer) {
        const lessonsContainer = moduleContainer.querySelector(".lessons-container");
        const moduleIndex = Array.from(
            document.querySelectorAll(".module-container")
        ).indexOf(moduleContainer);
        const lessonIndex = lessonsContainer.children.length;

        const lessonHTML = `
            <div class="lesson-item" data-lesson-index="${lessonIndex}">
                <i class="fas fa-grip-vertical lesson-drag-handle"></i>
                <div class="lesson-icon">
                    <i class="fas fa-play-circle"></i>
                </div>
                <input 
                    type="text" 
                    class="lesson-title-input" 
                    placeholder="Lesson title"
                    name="modules[${moduleIndex}][lessons][${lessonIndex}][title]"
                >
                <select class="lesson-type-select" name="modules[${moduleIndex}][lessons][${lessonIndex}][type]">
                    <option value="Video">Video</option>
                    <option value="Article">Article</option>
                </select>
                <button type="button" class="btn-icon btn-edit-lesson-details" title="Edit Lesson Details">
                    <i class="fas fa-edit"></i>
                </button>
                <button type="button" class="btn-icon btn-delete-lesson" title="Delete Lesson">
                    <i class="fas fa-trash"></i>
                </button>
                <!-- Hidden fields for lesson details -->
                <input type="hidden" class="lesson-video-url" value="" />
                <input type="hidden" class="lesson-pdf-url" value="" />
                <input type="hidden" class="lesson-duration" value="" />
                <input type="hidden" class="lesson-resources" value="[]" />
            </div>
        `;

        lessonsContainer.insertAdjacentHTML("beforeend", lessonHTML);
        const newLesson = lessonsContainer.lastElementChild;
        initLessonHandlers(newLesson);
    }

    function initLessonHandlers(lessonItem) {
        console.log("Initializing lesson handlers for:", lessonItem);
        
        // Delete handler
        const deleteBtn = lessonItem.querySelector(".btn-delete-lesson");
        if (deleteBtn) {
            // Remove any existing listeners by cloning
            const newDeleteBtn = deleteBtn.cloneNode(true);
            deleteBtn.parentNode.replaceChild(newDeleteBtn, deleteBtn);
            
            newDeleteBtn.addEventListener("click", (e) => {
                e.preventDefault();
                e.stopPropagation();
                if (confirm("Delete this lesson?")) {
                    lessonItem.remove();
                }
            });
        }

        // Edit lesson details handler
        const editBtn = lessonItem.querySelector(".btn-edit-lesson-details");
        if (editBtn) {
            console.log("Found edit button, attaching handler");
            
            // Remove any existing listeners by cloning
            const newEditBtn = editBtn.cloneNode(true);
            editBtn.parentNode.replaceChild(newEditBtn, editBtn);
            
            newEditBtn.addEventListener("click", (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log("Edit button clicked!");
                openCreateLessonModal(lessonItem);
            });
        }

        // Sync content type with icon
        const typeSelect = lessonItem.querySelector(".lesson-type-select");
        const lessonIcon = lessonItem.querySelector(".lesson-icon i");
        if (typeSelect && lessonIcon) {
            typeSelect.addEventListener("change", () => {
                if (typeSelect.value === "Video") {
                    lessonIcon.className = "fas fa-play-circle";
                } else {
                    lessonIcon.className = "fas fa-file-alt";
                }
            });
        }
    }

    // ========================================
    // LESSON DETAILS MODAL
    // ========================================

    function openCreateLessonModal(lessonElement) {
        console.log("Opening modal for lesson:", lessonElement);
        currentLessonElement = lessonElement;
        const modal = document.getElementById("createLessonModal");
        
        if (!modal) {
            console.error("Modal not found!");
            alert("Modal not found! Please check that the modal HTML exists on the page.");
            return;
        }
        
        // Get current values
        const title = lessonElement.querySelector(".lesson-title-input")?.value || "";
        const typeSelect = lessonElement.querySelector(".lesson-type-select");
        const contentType = typeSelect?.value === "Video" ? 1 : 0;
        const videoUrl = lessonElement.querySelector(".lesson-video-url")?.value || "";
        const pdfUrl = lessonElement.querySelector(".lesson-pdf-url")?.value || "";
        const duration = lessonElement.querySelector(".lesson-duration")?.value || "";
        const resourcesJson = lessonElement.querySelector(".lesson-resources")?.value || "[]";
        
        // Populate modal fields
        const titleInput = document.getElementById("createLessonTitle");
        const contentTypeSelect = document.getElementById("createLessonContentType");
        const videoUrlInput = document.getElementById("createLessonVideoUrl");
        const pdfUrlInput = document.getElementById("createLessonPdfUrl");
        const durationInput = document.getElementById("createLessonDuration");

        if (titleInput) titleInput.value = title;
        if (contentTypeSelect) contentTypeSelect.value = contentType;
        if (videoUrlInput) videoUrlInput.value = videoUrl;
        if (pdfUrlInput) pdfUrlInput.value = pdfUrl;
        if (durationInput) durationInput.value = duration ? (parseFloat(duration) / 60).toFixed(1) : "";
        
        // Load resources
        let resources = [];
        try {
            resources = JSON.parse(resourcesJson || "[]");
        } catch (e) {
            resources = [];
        }
        loadCreateLessonResources(resources);
        
        // Toggle fields based on content type
        toggleCreateContentFields();
        
        modal.style.display = "block";
        console.log("Modal displayed successfully");
    }
    
    // Make functions globally accessible
    window.openCreateLessonModal = openCreateLessonModal;

    window.closeCreateLessonModal = function() {
        const modal = document.getElementById("createLessonModal");
        if (modal) {
            modal.style.display = "none";
        }
        currentLessonElement = null;
    };

    window.toggleCreateContentFields = function() {
        const contentTypeSelect = document.getElementById("createLessonContentType");
        const videoFields = document.getElementById("createVideoFields");
        const pdfFields = document.getElementById("createPdfFields");
        
        if (!contentTypeSelect || !videoFields || !pdfFields) return;
        
        const contentType = parseInt(contentTypeSelect.value);
        
        if (contentType === 1) {
            videoFields.style.display = "block";
            pdfFields.style.display = "none";
        } else {
            videoFields.style.display = "none";
            pdfFields.style.display = "block";
        }
    };

    window.saveCreateLessonDetails = function() {
        if (!currentLessonElement) {
            console.error("No lesson element selected");
            alert("No lesson selected. Please try again.");
            return;
        }
        
        const title = document.getElementById("createLessonTitle")?.value?.trim() || "";
        const contentType = parseInt(document.getElementById("createLessonContentType")?.value || 1);
        const videoUrl = document.getElementById("createLessonVideoUrl")?.value?.trim() || "";
        const pdfUrl = document.getElementById("createLessonPdfUrl")?.value?.trim() || "";
        const durationMinutes = parseFloat(document.getElementById("createLessonDuration")?.value) || 0;
        const durationSeconds = Math.round(durationMinutes * 60);
        
        // Validate
        if (!title) {
            alert("Please enter a lesson title");
            return;
        }
        
        if (contentType === 1 && !videoUrl) {
            alert("Please enter a video URL");
            return;
        }
        
        if (contentType === 0 && !pdfUrl) {
            alert("Please enter a PDF URL");
            return;
        }
        
        // Collect resources
        const resources = collectCreateLessonResources();
        
        // Update the lesson element
        const titleInput = currentLessonElement.querySelector(".lesson-title-input");
        const typeSelect = currentLessonElement.querySelector(".lesson-type-select");
        const videoUrlInput = currentLessonElement.querySelector(".lesson-video-url");
        const pdfUrlInput = currentLessonElement.querySelector(".lesson-pdf-url");
        const durationInput = currentLessonElement.querySelector(".lesson-duration");
        const resourcesInput = currentLessonElement.querySelector(".lesson-resources");
        const lessonIcon = currentLessonElement.querySelector(".lesson-icon i");

        if (titleInput) titleInput.value = title;
        if (typeSelect) typeSelect.value = contentType === 1 ? "Video" : "Article";
        if (videoUrlInput) videoUrlInput.value = videoUrl;
        if (pdfUrlInput) pdfUrlInput.value = pdfUrl;
        if (durationInput) durationInput.value = durationSeconds;
        if (resourcesInput) resourcesInput.value = JSON.stringify(resources);
        if (lessonIcon) lessonIcon.className = contentType === 1 ? "fas fa-play-circle" : "fas fa-file-alt";
        
        // Mark as having details
        currentLessonElement.classList.add("has-details");
        
        window.closeCreateLessonModal();
        showNotification("Lesson details saved!", "success");
    };

    // ========================================
    // LESSON RESOURCES
    // ========================================

    function initLessonResources() {
        const addResourceBtn = document.getElementById("addCreateLessonResource");
        if (addResourceBtn) {
            addResourceBtn.addEventListener("click", () => addCreateResourceItem());
        }
    }

    function addCreateResourceItem(resourceData = null) {
        const resourcesList = document.getElementById("createLessonResourcesList");
        if (!resourcesList) return;
        
        const emptyState = resourcesList.querySelector(".empty-resources");
        if (emptyState) {
            emptyState.remove();
        }
        
        const index = resourcesList.querySelectorAll(".resource-item").length;
        
        const resourceHtml = `
            <div class="resource-item" data-resource-index="${index}">
                <div class="form-group">
                    <label class="form-label">Type</label>
                    <select class="form-select resource-type">
                        <option value="3" ${resourceData?.resourceType === 3 ? 'selected' : ''}>?? URL Link</option>
                        <option value="1" ${resourceData?.resourceType === 1 ? 'selected' : ''}>?? PDF Document</option>
                        <option value="2" ${resourceData?.resourceType === 2 ? 'selected' : ''}>?? ZIP File</option>
                    </select>
                </div>
                
                <div class="form-group">
                    <label class="form-label">Resource URL *</label>
                    <input type="url" 
                           class="form-input resource-url" 
                           placeholder="https://example.com/resource" 
                           value="${resourceData?.url || ''}" />
                </div>
                
                <div class="form-group">
                    <label class="form-label">Title</label>
                    <input type="text" 
                           class="form-input resource-title" 
                           placeholder="e.g., Lecture Slides"
                           value="${resourceData?.title || ''}" />
                </div>
                
                <button type="button" class="btn-remove-resource" title="Remove Resource">
                    <i class="fas fa-trash-alt"></i>
                </button>
            </div>
        `;
        
        resourcesList.insertAdjacentHTML("beforeend", resourceHtml);
        
        const newItem = resourcesList.lastElementChild;
        const removeBtn = newItem.querySelector(".btn-remove-resource");
        if (removeBtn) {
            removeBtn.addEventListener("click", function() {
                newItem.remove();
                checkCreateEmptyState();
            });
        }
    }

    function checkCreateEmptyState() {
        const resourcesList = document.getElementById("createLessonResourcesList");
        if (!resourcesList) return;
        
        const resourceItems = resourcesList.querySelectorAll(".resource-item");
        
        if (resourceItems.length === 0) {
            resourcesList.innerHTML = `
                <div class="empty-resources">
                    <i class="fas fa-link"></i>
                    <p>No resources added yet. Click "Add Resource" to get started.</p>
                </div>
            `;
        }
    }

    function loadCreateLessonResources(resources) {
        const resourcesList = document.getElementById("createLessonResourcesList");
        if (!resourcesList) return;
        
        resourcesList.innerHTML = "";
        
        if (resources && resources.length > 0) {
            resources.forEach(resource => {
                addCreateResourceItem(resource);
            });
        } else {
            checkCreateEmptyState();
        }
    }

    function collectCreateLessonResources() {
        const resources = [];
        const resourceItems = document.querySelectorAll("#createLessonResourcesList .resource-item");
        
        resourceItems.forEach(item => {
            const resourceType = parseInt(item.querySelector(".resource-type")?.value || 3);
            const url = item.querySelector(".resource-url")?.value?.trim() || "";
            const title = item.querySelector(".resource-title")?.value?.trim() || "";
            
            if (url) {
                resources.push({
                    resourceType: resourceType,
                    url: url,
                    title: title || null
                });
            }
        });
        
        return resources;
    }

    // Close modal on overlay click
    document.addEventListener("click", function(e) {
        if (e.target.classList.contains("modal-overlay")) {
            window.closeCreateLessonModal();
        }
    });

    // ========================================
    // FORM SUBMISSION
    // ========================================

    function validateAllSteps() {
        const title = document.getElementById("courseTitle")?.value?.trim();
        const description = document.getElementById("courseDescription")?.value?.trim();
        const category = document.getElementById("courseCategory")?.value;
        const level = document.getElementById("courseLevel")?.value;
        const language = document.getElementById("courseLanguage")?.value;

        if (!title || !description || !category || !level || !language) {
            alert("Please complete the basic information in Step 1");
            goToStep(1);
            return false;
        }

        const modules = document.querySelectorAll(".module-container");
        if (modules.length === 0) {
            alert("Please add at least one module in Step 2");
            goToStep(2);
            return false;
        }

        let hasLesson = false;
        modules.forEach((module) => {
            const lessons = module.querySelectorAll(".lesson-item");
            if (lessons.length > 0) hasLesson = true;
        });

        if (!hasLesson) {
            alert("Please add at least one lesson to your modules");
            goToStep(2);
            return false;
        }

        return true;
    }

    function getLearningOutcomes() {
        const outcomes = [];
        document.querySelectorAll('.outcome-item input').forEach(input => {
            if (input.value.trim()) {
                outcomes.push(input.value.trim());
            }
        });
        return outcomes;
    }

    function getModulesDataWithDetails() {
        const modules = [];
        const moduleContainers = document.querySelectorAll(".module-container");

        moduleContainers.forEach((container, moduleIndex) => {
            const moduleTitle = container.querySelector(".module-title-input");
            const lessons = [];

            const lessonItems = container.querySelectorAll(".lesson-item");
            lessonItems.forEach((item, lessonIndex) => {
                const titleInput = item.querySelector(".lesson-title-input");
                const typeSelect = item.querySelector(".lesson-type-select");
                const contentType = typeSelect?.value === 'Video' ? 1 : 0;
                
                const videoUrl = item.querySelector(".lesson-video-url")?.value || null;
                const pdfUrl = item.querySelector(".lesson-pdf-url")?.value || null;
                const durationSeconds = parseInt(item.querySelector(".lesson-duration")?.value) || null;
                const resourcesJson = item.querySelector(".lesson-resources")?.value || "[]";
                
                let resources = [];
                try {
                    resources = JSON.parse(resourcesJson || "[]");
                } catch (e) {
                    resources = [];
                }

                lessons.push({
                    title: titleInput?.value?.trim() || "Untitled Lesson",
                    contentType: contentType,
                    order: lessonIndex + 1,
                    videoUrl: contentType === 1 ? (videoUrl || "https://www.youtube.com/watch?v=dQw4w9WgXcQ") : null,
                    pdfUrl: contentType === 0 ? (pdfUrl || "https://example.com/sample.pdf") : null,
                    durationInSeconds: contentType === 1 ? (durationSeconds || 600) : null,
                    resources: resources
                });
            });

            modules.push({
                title: moduleTitle?.value?.trim() || "Untitled Module",
                description: "",
                order: moduleIndex + 1,
                lessons: lessons
            });
        });

        return modules;
    }

    function getThumbnailUrl() {
        const thumbnailUrl = document.getElementById("thumbnailUrl");
        return thumbnailUrl?.value || "https://images.unsplash.com/photo-1516397281156-ca07cf9746fc?w=800";
    }

    async function publishCourse() {
        console.log("?? Publish course button clicked!");

        if (!validateAllSteps()) {
            console.log("? Validation failed");
            return;
        }

        const courseData = {
            instructorId: 0,
            title: document.getElementById("courseTitle")?.value?.trim(),
            description: document.getElementById("courseDescription")?.value?.trim(),
            thumbnailImageUrl: getThumbnailUrl(),
            level: parseInt(document.getElementById("courseLevel")?.value),
            categoryId: parseInt(document.getElementById("courseCategory")?.value),
            languageId: parseInt(document.getElementById("courseLanguage")?.value),
            learningOutcomes: getLearningOutcomes(),
            modules: getModulesDataWithDetails()
        };

        console.log("?? Course data to send:", courseData);

        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenElement) {
            console.error("? Anti-forgery token not found!");
            showNotification("Security token missing. Please refresh the page.", "error");
            return;
        }

        try {
            showNotification("Publishing course...", "info");

            const response = await fetch('/api/courses/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': tokenElement.value
                },
                body: JSON.stringify(courseData)
            });

            console.log("?? Response status:", response.status);

            let result;
            try {
                const text = await response.text();
                console.log("?? Raw response:", text);
                result = text ? JSON.parse(text) : {};
            } catch (parseError) {
                console.error("? Failed to parse response:", parseError);
                result = { success: false, message: "Invalid server response" };
            }

            if (response.ok && result.success) {
                showNotification(result.message || "Course published successfully!", "success");
                console.log("? Course published! ID:", result.courseId);
                setTimeout(() => window.location.href = '/instructor/my-courses', 2000);
            } else {
                let errorMsg = result.message || result.title || `Server error: ${response.status}`;

                if (result.errors) {
                    console.error("? Validation errors:", result.errors);
                    errorMsg += "\n\nValidation errors:\n";
                    for (const [field, messages] of Object.entries(result.errors)) {
                        errorMsg += `- ${field}: ${messages.join(', ')}\n`;
                    }
                }

                console.error("? Error response:", result);
                showNotification(errorMsg, "error");
            }
        } catch (error) {
            console.error('? Network/Fetch error:', error);
            showNotification("Failed to publish course: " + error.message, "error");
        }
    }

    function showNotification(message, type = "info") {
        const notification = document.createElement("div");
        notification.className = `notification notification-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 1rem 1.5rem;
            background: ${type === "success" ? "#10b981" : type === "error" ? "#ef4444" : "#7c3aed"};
            color: white;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            z-index: 10000;
            animation: slideIn 0.3s ease;
            font-weight: 600;
            max-width: 400px;
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = "slideOut 0.3s ease";
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    function initFormSubmission() {
        const form = document.getElementById("createCourseForm");
        const publishBtn = document.getElementById("publishBtn");
        const submitReviewBtn = document.getElementById("submitReviewBtn");

        if (form) {
            form.addEventListener("submit", (e) => {
                e.preventDefault();
            });
        }

        if (publishBtn) {
            publishBtn.addEventListener("click", (e) => {
                e.preventDefault();
                publishCourse();
            });
        }

        if (submitReviewBtn) {
            submitReviewBtn.addEventListener("click", (e) => {
                e.preventDefault();
                publishCourse();
            });
        }
    }

    // ========================================
    // INITIALIZE
    // ========================================

    function init() {
        console.log("Initializing create course page...");

        initStepNavigation();
        initThumbnailUpload();
        initLearningOutcomes();
        initCurriculumBuilder();
        initFormSubmission();
        initLessonResources();

        const style = document.createElement("style");
        style.textContent = `
            @keyframes slideIn {
                from { transform: translateX(400px); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes slideOut {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(400px); opacity: 0; }
            }
        `;
        document.head.appendChild(style);

        console.log("Create Course page initialized successfully");
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
