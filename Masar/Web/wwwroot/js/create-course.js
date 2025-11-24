// ========================================
// CREATE COURSE - JavaScript
// ========================================

(function() {
    'use strict';

    let currentStep = 1;
    let moduleCount = 1;

    // ========================================
    // STEP NAVIGATION
    // ========================================
    
    function initStepNavigation() {
        const nextButtons = document.querySelectorAll('[data-next-step]');
        const prevButtons = document.querySelectorAll('[data-prev-step]');

        nextButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const nextStep = parseInt(btn.dataset.nextStep);
                if (validateCurrentStep()) {
                    goToStep(nextStep);
                }
            });
        });

        prevButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const prevStep = parseInt(btn.dataset.prevStep);
                goToStep(prevStep);
            });
        });
    }

    function goToStep(stepNumber) {
        // Hide current step
        document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.remove('active');
        document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.remove('active');

        // Show new step
        currentStep = stepNumber;
        document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.add('active');
        document.querySelector(`.progress-step[data-step="${currentStep}"]`).classList.add('active');

        // Mark previous steps as completed
        for (let i = 1; i < currentStep; i++) {
            document.querySelector(`.progress-step[data-step="${i}"]`).classList.add('completed');
        }

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    function validateCurrentStep() {
        const currentStepElement = document.querySelector(`.form-step[data-step="${currentStep}"]`);
        const requiredInputs = currentStepElement.querySelectorAll('[required]');
        
        let isValid = true;
        requiredInputs.forEach(input => {
            if (!input.value.trim()) {
                input.style.borderColor = 'var(--accent-red)';
                isValid = false;
            } else {
                input.style.borderColor = 'var(--border-color)';
            }
        });

        if (!isValid) {
            alert('Please fill in all required fields');
        }

        return isValid;
    }

    // ========================================
    // THUMBNAIL UPLOAD
    // ========================================
    
    function initThumbnailUpload() {
        const uploadArea = document.getElementById('thumbnailUpload');
        const fileInput = document.getElementById('courseThumbnail');
        const preview = document.getElementById('thumbnailPreview');
        const previewImage = document.getElementById('thumbnailImage');
        const removeBtn = document.getElementById('removeThumbnail');

        uploadArea.addEventListener('click', () => fileInput.click());

        fileInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    previewImage.src = e.target.result;
                    uploadArea.style.display = 'none';
                    preview.style.display = 'block';
                };
                reader.readAsDataURL(file);
            }
        });

        removeBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            fileInput.value = '';
            uploadArea.style.display = 'flex';
            preview.style.display = 'none';
        });

        // Drag and drop
        uploadArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = 'var(--accent-purple)';
        });

        uploadArea.addEventListener('dragleave', () => {
            uploadArea.style.borderColor = 'var(--border-color)';
        });

        uploadArea.addEventListener('drop', (e) => {
            e.preventDefault();
            uploadArea.style.borderColor = 'var(--border-color)';
            const file = e.dataTransfer.files[0];
            if (file && file.type.startsWith('image/')) {
                fileInput.files = e.dataTransfer.files;
                fileInput.dispatchEvent(new Event('change'));
            }
        });
    }

    // ========================================
    // LEARNING OUTCOMES
    // ========================================
    
    function initLearningOutcomes() {
        const container = document.getElementById('learningOutcomes');
        const addBtn = document.getElementById('addOutcome');

        addBtn.addEventListener('click', () => {
            const outcomeItem = document.createElement('div');
            outcomeItem.className = 'outcome-item';
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
            attachRemoveHandler(outcomeItem.querySelector('.btn-remove-outcome'), outcomeItem);
        });

        // Attach remove handler to existing items
        container.querySelectorAll('.btn-remove-outcome').forEach(btn => {
            attachRemoveHandler(btn, btn.closest('.outcome-item'));
        });
    }

    function attachRemoveHandler(btn, item) {
        btn.addEventListener('click', () => {
            if (document.querySelectorAll('.outcome-item').length > 1) {
                item.remove();
            } else {
                alert('You must have at least one learning outcome');
            }
        });
    }

    // ========================================
    // CURRICULUM BUILDER
    // ========================================
    
    function initCurriculumBuilder() {
        const addModuleBtn = document.getElementById('addModule');
        const curriculumBuilder = document.getElementById('curriculumBuilder');

        addModuleBtn.addEventListener('click', () => {
            addModule();
        });

        // Initialize first module
        initModuleHandlers(curriculumBuilder.querySelector('.module-container'));
    }

    function addModule() {
        const curriculumBuilder = document.getElementById('curriculumBuilder');
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

        curriculumBuilder.insertAdjacentHTML('beforeend', moduleHTML);
        const newModule = curriculumBuilder.lastElementChild;
        initModuleHandlers(newModule);
        moduleCount++;
    }

    function initModuleHandlers(moduleContainer) {
        const deleteBtn = moduleContainer.querySelector('.btn-delete');
        const collapseBtn = moduleContainer.querySelector('.btn-collapse');
        const addLessonBtn = moduleContainer.querySelector('.btn-add-lesson');
        const moduleBody = moduleContainer.querySelector('.module-body');

        // Delete module
        deleteBtn.addEventListener('click', () => {
            if (confirm('Are you sure you want to delete this module?')) {
                moduleContainer.remove();
            }
        });

        // Collapse/Expand
        collapseBtn.addEventListener('click', () => {
            moduleBody.classList.toggle('collapsed');
            const icon = collapseBtn.querySelector('i');
            icon.classList.toggle('fa-chevron-up');
            icon.classList.toggle('fa-chevron-down');
        });

        // Add lesson
        addLessonBtn.addEventListener('click', () => {
            addLesson(moduleContainer);
        });
    }

    function addLesson(moduleContainer) {
        const lessonsContainer = moduleContainer.querySelector('.lessons-container');
        const moduleIndex = Array.from(document.querySelectorAll('.module-container')).indexOf(moduleContainer);
        const lessonIndex = lessonsContainer.children.length;

        const lessonHTML = `
            <div class="lesson-item">
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
                    <option value="video">Video</option>
                    <option value="article">Article</option>
                    <option value="quiz">Quiz</option>
                    <option value="assignment">Assignment</option>
                </select>
                <button type="button" class="btn-delete-lesson">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        `;

        lessonsContainer.insertAdjacentHTML('beforeend', lessonHTML);
        const newLesson = lessonsContainer.lastElementChild;

        // Attach delete handler
        newLesson.querySelector('.btn-delete-lesson').addEventListener('click', () => {
            if (confirm('Delete this lesson?')) {
                newLesson.remove();
            }
        });
    }

    // ========================================
    // PREREQUISITES
    // ========================================
    
    function initPrerequisites() {
        const container = document.getElementById('prerequisitesList');
        const addBtn = document.getElementById('addPrerequisite');

        addBtn.addEventListener('click', () => {
            const prerequisiteItem = document.createElement('div');
            prerequisiteItem.className = 'prerequisite-item';
            prerequisiteItem.innerHTML = `
                <input 
                    type="text" 
                    class="form-input" 
                    placeholder="Enter a prerequisite"
                    name="prerequisites[]"
                >
                <button type="button" class="btn-remove-prerequisite">
                    <i class="fas fa-times"></i>
                </button>
            `;
            container.appendChild(prerequisiteItem);
            
            prerequisiteItem.querySelector('.btn-remove-prerequisite').addEventListener('click', () => {
                prerequisiteItem.remove();
            });
        });

        // Attach remove handler to existing items
        container.querySelectorAll('.btn-remove-prerequisite').forEach(btn => {
            btn.addEventListener('click', () => btn.closest('.prerequisite-item').remove());
        });
    }

    // ========================================
    // FORM SUBMISSION
    // ========================================
    
    function initFormSubmission() {
        const form = document.getElementById('createCourseForm');
        const saveDraftBtn = document.getElementById('saveDraftBtn');
        const publishBtn = document.getElementById('publishBtn');

        form.addEventListener('submit', (e) => {
            e.preventDefault();
            if (validateCurrentStep()) {
                submitCourse('published');
            }
        });

        saveDraftBtn.addEventListener('click', () => {
            submitCourse('draft');
        });

        publishBtn.addEventListener('click', () => {
            if (validateAllSteps()) {
                submitCourse('published');
            }
        });
    }

    function validateAllSteps() {
        // Validate all required fields across all steps
        const requiredFields = document.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                isValid = false;
            }
        });

        if (!isValid) {
            alert('Please complete all required fields before publishing');
        }

        return isValid;
    }

    function submitCourse(status) {
        const formData = new FormData(document.getElementById('createCourseForm'));
        formData.append('status', status);

        // In production, send to backend API
        console.log('Submitting course with status:', status);
        console.log('Form Data:', Object.fromEntries(formData));

        // Show success message
        alert(status === 'draft' ? 'Course saved as draft!' : 'Course published successfully!');
        
        // Redirect to my courses
        // window.location.href = 'my-courses.html';
    }

    // ========================================
    // INITIALIZE
    // ========================================
    
    function init() {
        initStepNavigation();
        initThumbnailUpload();
        initLearningOutcomes();
        initCurriculumBuilder();
        initPrerequisites();
        initFormSubmission();

        console.log('Create Course initialized');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();