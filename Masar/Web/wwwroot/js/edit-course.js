// Edit Course JavaScript - Updated for ManageCourse UI

document.addEventListener('DOMContentLoaded', function () {
    initTabs();
    initModals();
    initModuleActions();
    initLessonActions();
    initOutcomes();
    initThumbnailUpload();
    initLessonTypeSelector();
});

// ==================== TABS ====================
function initTabs() {
    const tabBtns = document.querySelectorAll('[data-action="switch-tab"]');
    const tabContents = document.querySelectorAll('.tab-content');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const tabId = this.getAttribute('data-tab-id');

            // Update active tab button
            tabBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            // Update active tab content
            tabContents.forEach(content => {
                if (content.getAttribute('data-tab-id') === tabId) {
                    content.classList.add('active');
                } else {
                    content.classList.remove('active');
                }
            });
        });
    });
}

// ==================== MODALS ====================
function initModals() {
    // Close modal on overlay click or close button
    document.querySelectorAll('[data-action="close-modal"]').forEach(el => {
        el.addEventListener('click', function () {
            const targetId = this.getAttribute('data-target');
            closeModal(targetId);
        });
    });

    // Close modal on Escape key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal').forEach(modal => {
                modal.style.display = 'none';
            });
        }
    });
}

function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'block';
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
}

// ==================== MODULE ACTIONS ====================
function initModuleActions() {
    // Add new module button
    document.getElementById('addNewModule')?.addEventListener('click', function () {
        document.getElementById('moduleModalTitle').textContent = 'Add Module';
        document.getElementById('moduleId').value = '0';
        document.getElementById('moduleTitle').value = '';
        document.getElementById('moduleDescription').value = '';
        document.getElementById('moduleOrder').value = document.querySelectorAll('.module-card').length + 1;
        openModal('moduleModal');
    });

    // Edit module buttons
    document.querySelectorAll('.btn-edit-module').forEach(btn => {
        btn.addEventListener('click', function () {
            const moduleCard = this.closest('.module-card');
            const moduleId = moduleCard.dataset.moduleId;
            const moduleTitle = moduleCard.querySelector('.module-title').textContent;
            const moduleOrder = moduleCard.dataset.moduleOrder;

            document.getElementById('moduleModalTitle').textContent = 'Edit Module';
            document.getElementById('moduleId').value = moduleId;
            document.getElementById('moduleTitle').value = moduleTitle;
            document.getElementById('moduleOrder').value = moduleOrder;
            openModal('moduleModal');
        });
    });

    // Delete module buttons
    document.querySelectorAll('.btn-delete-module').forEach(btn => {
        btn.addEventListener('click', function () {
            const moduleCard = this.closest('.module-card');
            const moduleId = moduleCard.dataset.moduleId;
            if (confirm('Are you sure you want to delete this module and all its lessons?')) {
                deleteModule(moduleId);
            }
        });
    });

    // Toggle module expand/collapse
    document.querySelectorAll('.btn-toggle').forEach(btn => {
        btn.addEventListener('click', function () {
            const moduleCard = this.closest('.module-card');
            moduleCard.classList.toggle('collapsed');
            const icon = this.querySelector('i');
            icon.classList.toggle('fa-chevron-down');
            icon.classList.toggle('fa-chevron-up');
        });
    });
}

// ==================== LESSON ACTIONS ====================
function initLessonActions() {
    // Add lesson buttons (in module header)
    document.querySelectorAll('.btn-add-lesson').forEach(btn => {
        btn.addEventListener('click', function () {
            const moduleCard = this.closest('.module-card');
            const moduleId = moduleCard.dataset.moduleId;
            openAddLessonModal(moduleId);
        });
    });

    // Add lesson buttons (inline)
    document.querySelectorAll('.btn-add-lesson-inline').forEach(btn => {
        btn.addEventListener('click', function () {
            const moduleId = this.dataset.moduleId;
            openAddLessonModal(moduleId);
        });
    });

    // Edit lesson buttons
    document.querySelectorAll('.btn-edit-lesson').forEach(btn => {
        btn.addEventListener('click', function () {
            const lessonItem = this.closest('.lesson-item');
            openEditLessonModal(lessonItem);
        });
    });

    // Delete lesson buttons
    document.querySelectorAll('.btn-delete-lesson').forEach(btn => {
        btn.addEventListener('click', function () {
            const lessonItem = this.closest('.lesson-item');
            const lessonId = lessonItem.dataset.lessonId;
            if (confirm('Are you sure you want to delete this lesson?')) {
                deleteLesson(lessonId);
            }
        });
    });
}

function openAddLessonModal(moduleId) {
    document.getElementById('lessonModalTitle').textContent = 'Add Lesson';
    document.getElementById('lessonId').value = '0';
    document.getElementById('lessonModuleId').value = moduleId;
    document.getElementById('lessonTitle').value = '';
    document.getElementById('lessonVideoUrl').value = '';
    document.getElementById('lessonPdfUrl').value = '';
    document.getElementById('lessonDuration').value = '';
    document.getElementById('lessonContentType').value = '1';
    document.getElementById('lessonResourcesList').innerHTML = '';

    // Reset lesson type selector
    document.querySelectorAll('.lesson-type-option').forEach(opt => {
        opt.classList.remove('selected');
        if (opt.dataset.type === '1') opt.classList.add('selected');
    });
    toggleContentFields();

    openModal('lessonModal');
}

function openEditLessonModal(lessonItem) {
    const lessonId = lessonItem.dataset.lessonId;
    const lessonTitle = lessonItem.querySelector('.lesson-title').textContent;
    const contentType = lessonItem.dataset.lessonContentType;
    const videoUrl = lessonItem.dataset.lessonVideoUrl || '';
    const pdfUrl = lessonItem.dataset.lessonPdfUrl || '';
    const duration = lessonItem.dataset.lessonDuration || 0;
    const resources = JSON.parse(lessonItem.dataset.lessonResources || '[]');
    const moduleId = lessonItem.closest('.module-card').dataset.moduleId;

    document.getElementById('lessonModalTitle').textContent = 'Edit Lesson';
    document.getElementById('lessonId').value = lessonId;
    document.getElementById('lessonModuleId').value = moduleId;
    document.getElementById('lessonTitle').value = lessonTitle;
    document.getElementById('lessonVideoUrl').value = videoUrl;
    document.getElementById('lessonPdfUrl').value = pdfUrl;
    document.getElementById('lessonDuration').value = Math.round(duration / 60);
    document.getElementById('lessonContentType').value = contentType;

    // Update lesson type selector
    document.querySelectorAll('.lesson-type-option').forEach(opt => {
        opt.classList.remove('selected');
        if (opt.dataset.type === contentType) opt.classList.add('selected');
    });
    toggleContentFields();

    // Load resources
    const resourcesList = document.getElementById('lessonResourcesList');
    resourcesList.innerHTML = '';
    resources.forEach(r => addResourceRow(r));

    openModal('lessonModal');
}

function initLessonTypeSelector() {
    document.querySelectorAll('[data-action="select-lesson-type"]').forEach(opt => {
        opt.addEventListener('click', function () {
            document.querySelectorAll('.lesson-type-option').forEach(o => o.classList.remove('selected'));
            this.classList.add('selected');
            document.getElementById('lessonContentType').value = this.dataset.type;
            toggleContentFields();
        });
    });
}

function toggleContentFields() {
    const contentType = document.getElementById('lessonContentType').value;
    const videoFields = document.getElementById('videoFields');
    const pdfFields = document.getElementById('pdfFields');

    if (contentType === '1') {
        videoFields.style.display = 'block';
        pdfFields.style.display = 'none';
    } else {
        videoFields.style.display = 'none';
        pdfFields.style.display = 'block';
    }
}

// ==================== SAVE FUNCTIONS ====================
async function saveModule() {
    const moduleId = document.getElementById('moduleId').value;
    const title = document.getElementById('moduleTitle').value;
    const description = document.getElementById('moduleDescription')?.value || '';
    const order = document.getElementById('moduleOrder').value;

    if (!title.trim()) {
        showToast('Please enter a module title', 'error');
        return;
    }

    try {
        const response = await fetch(`/instructor/edit-course/${window.courseData.courseId}/module`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                moduleId: parseInt(moduleId) || null,
                title: title,
                description: description,
                order: parseInt(order)
            })
        });

        const result = await response.json();
        if (result.success) {
            showToast('Module saved successfully!');
            closeModal('moduleModal');
            location.reload();
        } else {
            showToast(result.message || 'Failed to save module', 'error');
        }
    } catch (error) {
        console.error('Error saving module:', error);
        showToast('An error occurred while saving', 'error');
    }
}

async function deleteModule(moduleId) {
    try {
        const response = await fetch(`/instructor/edit-course/${window.courseData.courseId}/module/${moduleId}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        });

        const result = await response.json();
        if (result.success) {
            showToast('Module deleted successfully!');
            location.reload();
        } else {
            showToast(result.message || 'Failed to delete module', 'error');
        }
    } catch (error) {
        console.error('Error deleting module:', error);
        showToast('An error occurred while deleting', 'error');
    }
}

async function saveLesson() {
    const lessonId = document.getElementById('lessonId').value;
    const moduleId = document.getElementById('lessonModuleId').value;
    const title = document.getElementById('lessonTitle').value;
    const contentType = parseInt(document.getElementById('lessonContentType').value);
    const videoUrl = document.getElementById('lessonVideoUrl').value;
    const pdfUrl = document.getElementById('lessonPdfUrl').value;
    const durationMinutes = parseFloat(document.getElementById('lessonDuration').value) || 0;

    if (!title.trim()) {
        showToast('Please enter a lesson title', 'error');
        return;
    }

    // Collect resources
    const resources = [];
    document.querySelectorAll('#lessonResourcesList .resource-item').forEach(item => {
        resources.push({
            lessonResourceId: parseInt(item.dataset.resourceId) || 0,
            resourceType: parseInt(item.querySelector('.resource-type').value),
            url: item.querySelector('.resource-url').value,
            title: item.querySelector('.resource-title').value
        });
    });

    try {
        const response = await fetch(`/instructor/edit-course/${window.courseData.courseId}/lesson`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                lessonId: parseInt(lessonId) || null,
                moduleId: parseInt(moduleId),
                title: title,
                contentType: contentType,
                order: 1,
                videoUrl: contentType === 1 ? videoUrl : null,
                pdfUrl: contentType === 0 ? pdfUrl : null,
                durationInSeconds: Math.round(durationMinutes * 60),
                resources: resources
            })
        });

        const result = await response.json();
        if (result.success) {
            showToast('Lesson saved successfully!');
            closeModal('lessonModal');
            location.reload();
        } else {
            showToast(result.message || 'Failed to save lesson', 'error');
        }
    } catch (error) {
        console.error('Error saving lesson:', error);
        showToast('An error occurred while saving', 'error');
    }
}

async function deleteLesson(lessonId) {
    try {
        const response = await fetch(`/instructor/edit-course/${window.courseData.courseId}/lesson/${lessonId}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        });

        const result = await response.json();
        if (result.success) {
            showToast('Lesson deleted successfully!');
            location.reload();
        } else {
            showToast(result.message || 'Failed to delete lesson', 'error');
        }
    } catch (error) {
        console.error('Error deleting lesson:', error);
        showToast('An error occurred while deleting', 'error');
    }
}

// ==================== RESOURCES ====================
document.getElementById('addLessonResource')?.addEventListener('click', function () {
    addResourceRow();
});

function addResourceRow(resource = null) {
    const resourcesList = document.getElementById('lessonResourcesList');
    const resourceHtml = `
        <div class="resource-item" data-resource-id="${resource?.lessonResourceId || 0}">
            <select class="form-select resource-type">
                <option value="1" ${resource?.resourceType === 1 ? 'selected' : ''}>PDF</option>
                <option value="2" ${resource?.resourceType === 2 ? 'selected' : ''}>ZIP</option>
                <option value="3" ${resource?.resourceType === 3 ? 'selected' : ''}>URL</option>
            </select>
            <input type="text" class="form-input resource-title" placeholder="Resource title" value="${resource?.title || ''}" />
            <input type="url" class="form-input resource-url" placeholder="Resource URL" value="${resource?.url || ''}" />
            <button type="button" class="btn-icon-small btn-remove-resource">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    resourcesList.insertAdjacentHTML('beforeend', resourceHtml);

    // Add remove handler
    resourcesList.lastElementChild.querySelector('.btn-remove-resource').addEventListener('click', function () {
        this.closest('.resource-item').remove();
    });
}

// ==================== OUTCOMES ====================
function initOutcomes() {
    document.getElementById('addEditOutcome')?.addEventListener('click', function () {
        const input = document.getElementById('newOutcome');
        const value = input.value.trim();
        if (value) {
            addOutcomeRow(value);
            input.value = '';
            updateOutcomeCount();
        }
    });

    document.querySelectorAll('.btn-remove-outcome').forEach(btn => {
        btn.addEventListener('click', function () {
            this.closest('.outcome-item').remove();
            updateOutcomeCount();
            reindexOutcomes();
        });
    });
}

function addOutcomeRow(value) {
    const outcomesList = document.getElementById('editOutcomesList');
    const index = outcomesList.querySelectorAll('.outcome-item').length;
    const outcomeHtml = `
        <div class="outcome-item">
            <i class="fas fa-check-circle"></i>
            <input type="text" class="form-input" name="LearningOutcomes[${index}]" value="${value}" />
            <button type="button" class="btn-icon-small btn-remove-outcome">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    outcomesList.insertAdjacentHTML('beforeend', outcomeHtml);

    outcomesList.lastElementChild.querySelector('.btn-remove-outcome').addEventListener('click', function () {
        this.closest('.outcome-item').remove();
        updateOutcomeCount();
        reindexOutcomes();
    });
}

function addExampleOutcome(text) {
    addOutcomeRow(text);
    updateOutcomeCount();
}

function updateOutcomeCount() {
    const count = document.querySelectorAll('#editOutcomesList .outcome-item').length;
    document.getElementById('outcomeCount').textContent = `${count} outcome${count !== 1 ? 's' : ''}`;
}

function reindexOutcomes() {
    document.querySelectorAll('#editOutcomesList .outcome-item input').forEach((input, index) => {
        input.name = `LearningOutcomes[${index}]`;
    });
}

// ==================== THUMBNAIL ====================
function initThumbnailUpload() {
    document.getElementById('uploadFromDeviceBtn')?.addEventListener('click', function () {
        document.getElementById('thumbnailFileInput').click();
    });

    document.getElementById('thumbnailFileInput')?.addEventListener('change', async function () {
        const file = this.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('file', file);

        try {
            const response = await fetch('/instructor/upload-course-thumbnail', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                document.getElementById('thumbnailPreview').src = result.url;
                document.getElementById('thumbnailUrl').value = result.url;
                showToast('Thumbnail uploaded successfully!');
            } else {
                showToast(result.message || 'Upload failed', 'error');
            }
        } catch (error) {
            console.error('Upload error:', error);
            showToast('Upload failed', 'error');
        }
    });

    document.getElementById('useUrlBtn')?.addEventListener('click', function () {
        const url = prompt('Enter image URL:');
        if (url) {
            document.getElementById('thumbnailPreview').src = url;
            document.getElementById('thumbnailUrl').value = url;
        }
    });
}

// ==================== SUBMIT FOR REVIEW ====================
function submitForReview() {
    openModal('submitModal');
}

async function confirmSubmit() {
    try {
        // Add your submit for review API call here
        showToast('Course submitted for review!');
        closeModal('submitModal');
        location.reload();
    } catch (error) {
        showToast('Failed to submit course', 'error');
    }
}

// ==================== TOAST NOTIFICATION ====================
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toastMessage');
    const icon = toast.querySelector('i');

    toastMessage.textContent = message;
    icon.className = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle';
    toast.className = `toast ${type} show`;

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

// ==================== SETTINGS ====================
function saveSettings() {
    showToast('Settings saved successfully!');
}

function archiveCourse() {
    if (confirm('Are you sure you want to archive this course?')) {
        showToast('Course archived');
    }
}

function deleteCourse() {
    if (confirm('Are you sure you want to permanently delete this course? This action cannot be undone.')) {
        showToast('Course deleted');
    }
}