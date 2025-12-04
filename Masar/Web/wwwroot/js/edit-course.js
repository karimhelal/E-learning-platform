// ========================================
// EDIT COURSE - JavaScript
// ========================================

(function () {
    'use strict';

    let currentModuleData = null;
    let currentLessonData = null;

    // ========================================
    // TABS FUNCTIONALITY
    // ========================================

    function initTabs() {
        const tabLinks = document.querySelectorAll('.tab-link');
        const tabPanes = document.querySelectorAll('.tab-pane');

        tabLinks.forEach(link => {
            link.addEventListener('click', () => {
                const targetTab = link.dataset.tab;

                // Remove active class from all tabs
                tabLinks.forEach(l => l.classList.remove('active'));
                tabPanes.forEach(p => p.classList.remove('active'));

                // Add active class to clicked tab
                link.classList.add('active');
                document.querySelector(`.tab-pane[data-tab="${targetTab}"]`).classList.add('active');
            });
        });
    }

    // ========================================
    // MODULE COLLAPSE/EXPAND
    // ========================================

    function initModuleToggle() {
        const toggleButtons = document.querySelectorAll('.btn-toggle');

        toggleButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const moduleCard = btn.closest('.edit-module-card');
                const moduleBody = moduleCard.querySelector('.module-card-body');

                if (moduleBody.style.display === 'none' || !moduleBody.style.display) {
                    moduleBody.style.display = 'block';
                    btn.classList.remove('rotated');
                } else {
                    moduleBody.style.display = 'none';
                    btn.classList.add('rotated');
                }
            });
        });
    }

    // ========================================
    // ADD/REMOVE OUTCOMES
    // ========================================

    function initOutcomes() {
        const addBtn = document.getElementById('addEditOutcome');
        const container = document.getElementById('editOutcomesList');

        if (addBtn) {
            addBtn.addEventListener('click', () => {
                const currentCount = container.querySelectorAll('.outcome-item-edit').length;
                const outcomeHTML = `
                    <div class="outcome-item-edit">
                        <i class="fas fa-check-circle"></i>
                        <input type="text" name="LearningOutcomes[${currentCount}]" class="form-input" placeholder="Enter learning outcome">
                        <button type="button" class="btn-remove">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `;
                container.insertAdjacentHTML('beforeend', outcomeHTML);
                attachRemoveHandler();
            });
        }

        attachRemoveHandler();
    }

    function attachRemoveHandler() {
        const removeButtons = document.querySelectorAll('.outcome-item-edit .btn-remove');
        removeButtons.forEach(btn => {
            btn.onclick = () => {
                if (document.querySelectorAll('.outcome-item-edit').length > 1) {
                    btn.closest('.outcome-item-edit').remove();
                    // Re-index the remaining inputs
                    reindexOutcomes();
                } else {
                    alert('You must have at least one learning outcome');
                }
            };
        });
    }

    function reindexOutcomes() {
        const outcomes = document.querySelectorAll('.outcome-item-edit input');
        outcomes.forEach((input, index) => {
            input.name = `LearningOutcomes[${index}]`;
        });
    }

    // ========================================
    // ARCHIVE COURSE
    // ========================================

    function initArchive() {
        const archiveBtn = document.getElementById('archiveCourseBtn');

        if (archiveBtn) {
            archiveBtn.addEventListener('click', () => {
                if (confirm('Are you sure you want to archive this course? Students will no longer be able to enroll.')) {
                    console.log('Archiving course...');
                    alert('Course archived successfully');
                    // TODO: Implement actual archive functionality
                }
            });
        }
    }

    // ========================================
    // SAVE CHANGES
    // ========================================

    function initSaveChanges() {
        const saveBtn = document.getElementById('saveChangesBtn');

        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                // Submit the form
                const form = document.getElementById('editCourseForm');
                if (form) {
                    // ? ADD DEBUGGING - Log form data before submission
                    const formData = new FormData(form);
                    console.log('?? Form Data Being Submitted:');
                    for (let [key, value] of formData.entries()) {
                        console.log(`  ${key}: ${value}`);
                    }
                    
                    form.submit();
                }
            });
        }
    }

    // ========================================
    // CURRICULUM MANAGEMENT
    // ========================================

    function initCurriculumManagement() {
        const courseId = window.location.pathname.split('/').pop();

        // Add New Module
        const addModuleBtn = document.getElementById('addNewModule');
        if (addModuleBtn) {
            addModuleBtn.addEventListener('click', function() {
                openEditModuleModal(null);
            });
        }

        // Edit Module
        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-edit-module')) {
                const btn = e.target.closest('.btn-edit-module');
                const moduleCard = btn.closest('.edit-module-card');
                const moduleId = moduleCard.dataset.moduleId;
                const moduleOrder = moduleCard.dataset.moduleOrder;
                const moduleDescription = moduleCard.dataset.moduleDescription || ''; // ADD THIS LINE
                const moduleTitle = moduleCard.querySelector('.module-title-display').textContent.trim();
                
                openEditModuleModal({
                    moduleId: parseInt(moduleId),
                    title: moduleTitle,
                    description: moduleDescription, // UPDATE THIS LINE
                    order: parseInt(moduleOrder)
                });
            }
        });

        // Delete Module
        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-delete-module')) {
                if (confirm('Are you sure you want to delete this module? All lessons in this module will be deleted.')) {
                    const btn = e.target.closest('.btn-delete-module');
                    const moduleCard = btn.closest('.edit-module-card');
                    const moduleId = moduleCard.dataset.moduleId;
                    deleteModule(courseId, parseInt(moduleId));
                }
            }
        });

        // Add Lesson to Module
        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-add-lesson') || e.target.closest('.btn-add-lesson-edit')) {
                const btn = e.target.closest('.btn-add-lesson') || e.target.closest('.btn-add-lesson-edit');
                const moduleCard = btn.closest('.edit-module-card');
                const moduleId = moduleCard.dataset.moduleId;
                
                openEditLessonModal(null, parseInt(moduleId));
            }
        });

        // Edit Lesson - UPDATED TO READ DATA ATTRIBUTES
        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-edit-lesson')) {
                console.log('Edit lesson button clicked');
                
                const btn = e.target.closest('.btn-edit-lesson');
                const lessonCard = btn.closest('.lesson-card-edit');
                const lessonId = parseInt(lessonCard.dataset.lessonId);
                const lessonOrder = parseInt(lessonCard.dataset.lessonOrder);
                const contentType = parseInt(lessonCard.dataset.lessonContentType);
                const videoUrl = lessonCard.dataset.lessonVideoUrl;
                const pdfUrl = lessonCard.dataset.lessonPdfUrl;
                const durationInSeconds = parseInt(lessonCard.dataset.lessonDuration) || 0;
                const moduleCard = lessonCard.closest('.edit-module-card');
                const moduleId = parseInt(moduleCard.dataset.moduleId);
                const lessonTitle = lessonCard.querySelector('.lesson-title-edit').textContent.trim();
                
                // Parse resources from data attribute
                const resourcesJson = lessonCard.dataset.lessonResources || '[]';
                let resources = [];
                try {
                    resources = JSON.parse(resourcesJson);
                    console.log('Parsed resources:', resources);
                } catch (e) {
                    console.error('Failed to parse resources:', e);
                }
                
                console.log('Opening lesson modal with resources:', resources);
                
                openEditLessonModal({
                    lessonId: lessonId,
                    moduleId: moduleId,
                    title: lessonTitle,
                    contentType: contentType,
                    order: lessonOrder,
                    videoUrl: videoUrl,
                    pdfUrl: pdfUrl,
                    durationInSeconds: durationInSeconds,
                    resources: resources
                });
            }
        });

        // Delete Lesson
        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-delete-lesson')) {
                if (confirm('Are you sure you want to delete this lesson?')) {
                    const btn = e.target.closest('.btn-delete-lesson');
                    const lessonCard = btn.closest('.lesson-card-edit');
                    const lessonId = lessonCard.dataset.lessonId;
                    deleteLesson(courseId, parseInt(lessonId));
                }
            }
        });
    }

    // ========================================
    // MODAL FUNCTIONS
    // ========================================

    window.openEditModuleModal = function(moduleData) {
        currentModuleData = moduleData;
        const modal = document.getElementById('editModuleModal');
        const form = document.getElementById('editModuleForm');
        
        if (moduleData) {
            // Editing existing module
            document.getElementById('moduleId').value = moduleData.moduleId || '';
            document.getElementById('moduleTitle').value = moduleData.title || '';
            document.getElementById('moduleDescription').value = moduleData.description || '';
            document.getElementById('moduleOrder').value = moduleData.order || '';
            document.querySelector('#editModuleModal .modal-title').textContent = 'Edit Module';
        } else {
            // Adding new module
            form.reset();
            const modulesCount = document.querySelectorAll('.edit-module-card').length;
            document.getElementById('moduleOrder').value = modulesCount + 1;
            document.querySelector('#editModuleModal .modal-title').textContent = 'Add New Module';
        }
        
        modal.style.display = 'block';
    };

    window.closeEditModuleModal = function() {
        document.getElementById('editModuleModal').style.display = 'none';
        currentModuleData = null;
    };

    window.openEditLessonModal = function(lessonData, moduleId) {
        currentLessonData = lessonData;
        const modal = document.getElementById('editLessonModal');
        const form = document.getElementById('editLessonForm');
        
        console.log('openEditLessonModal called with:', lessonData, moduleId);
        
        if (lessonData) {
            // Editing existing lesson
            document.getElementById('lessonId').value = lessonData.lessonId || '';
            document.getElementById('lessonModuleId').value = lessonData.moduleId || '';
            document.getElementById('lessonTitle').value = lessonData.title || '';
            document.getElementById('lessonContentType').value = lessonData.contentType || 0;
            document.getElementById('lessonOrder').value = lessonData.order || '';
            
            // Pre-fill URL and duration fields
            document.getElementById('lessonVideoUrl').value = lessonData.videoUrl || '';
            document.getElementById('lessonPdfUrl').value = lessonData.pdfUrl || '';
            
            // Convert seconds to minutes for display
            const durationInMinutes = lessonData.durationInSeconds ? (lessonData.durationInSeconds / 60).toFixed(1) : '';
            document.getElementById('lessonDuration').value = durationInMinutes;
            
            // Load resources
            loadLessonResources(lessonData.resources || []);
            
            document.querySelector('#editLessonModal .modal-title').textContent = 'Edit Lesson';
        } else {
            // Adding new lesson
            form.reset();
            document.getElementById('lessonModuleId').value = moduleId;
            const moduleCard = document.querySelector(`.edit-module-card[data-module-id="${moduleId}"]`);
            const lessonsCount = moduleCard ? moduleCard.querySelectorAll('.lesson-card-edit').length : 0;
            document.getElementById('lessonOrder').value = lessonsCount + 1;
            document.getElementById('lessonContentType').value = 0;
            
            // Clear resources
            loadLessonResources([]);
            
            document.querySelector('#editLessonModal .modal-title').textContent = 'Add New Lesson';
        }
        
        toggleContentFields();
        modal.style.display = 'block';
    };

    window.closeEditLessonModal = function() {
        document.getElementById('editLessonModal').style.display = 'none';
        currentLessonData = null;
    };

    window.toggleContentFields = function() {
        const contentType = parseInt(document.getElementById('lessonContentType').value);
        const videoFields = document.getElementById('videoFields');
        const pdfFields = document.getElementById('pdfFields');
        
        if (contentType === 1) { // Video
            videoFields.style.display = 'block';
            pdfFields.style.display = 'none';
            document.getElementById('lessonVideoUrl').required = true;
            document.getElementById('lessonDuration').required = true;
            document.getElementById('lessonPdfUrl').required = false;
        } else { // Article (PDF) - contentType === 0
            videoFields.style.display = 'none';
            pdfFields.style.display = 'block';
            document.getElementById('lessonVideoUrl').required = false;
            document.getElementById('lessonDuration').required = false;
            document.getElementById('lessonPdfUrl').required = true;
        }
    };

    // ========================================
    // THUMBNAIL PREVIEW - ENHANCED
    // ========================================

    function initThumbnailPreview() {
        const thumbnailPreview = document.getElementById('thumbnailPreview');
        const thumbnailUrlHidden = document.getElementById('thumbnailUrl');
        const thumbnailFileInput = document.getElementById('thumbnailFileInput');
        const uploadFromDeviceBtn = document.getElementById('uploadFromDeviceBtn');
        const useUrlBtn = document.getElementById('useUrlBtn');
        const removeThumbnailBtn = document.getElementById('removeThumbnailBtn');
        const urlInputContainer = document.getElementById('urlInputContainer');
        const thumbnailUrlInput = document.getElementById('thumbnailUrlInput');
        const applyUrlBtn = document.getElementById('applyUrlBtn');
        const cancelUrlBtn = document.getElementById('cancelUrlBtn');
        const uploadLoadingIndicator = document.getElementById('uploadLoadingIndicator');

        // Upload from Device
        if (uploadFromDeviceBtn && thumbnailFileInput) {
            uploadFromDeviceBtn.addEventListener('click', function() {
                thumbnailFileInput.click();
            });

            thumbnailFileInput.addEventListener('change', async function(e) {
                const file = e.target.files[0];
                if (!file) return;

                // Validate file type
                const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
                if (!validTypes.includes(file.type)) {
                    alert('Please select a valid image file (JPG, PNG, or WebP)');
                    return;
                }

                // Validate file size (5MB)
                const maxSize = 5 * 1024 * 1024; // 5MB in bytes
                if (file.size > maxSize) {
                    alert('File size must be less than 5MB');
                    return;
                }

                // Show loading indicator
                uploadLoadingIndicator.style.display = 'flex';

                try {
                    // Create a local preview immediately
                    const reader = new FileReader();
                    reader.onload = function(event) {
                        thumbnailPreview.src = event.target.result;
                    };
                    reader.readAsDataURL(file);

                    // Here you would upload to a server/cloud storage
                    // For now, we'll simulate an upload and use a placeholder service
                    await uploadImageToServer(file);

                } catch (error) {
                    console.error('Error uploading image:', error);
                    alert('Failed to upload image. Please try again.');
                    thumbnailPreview.src = 'https://via.placeholder.com/1280x720/ef4444/ffffff?text=Upload+Failed';
                } finally {
                    uploadLoadingIndicator.style.display = 'none';
                }
            });
        }

        // Use URL
        if (useUrlBtn && urlInputContainer) {
            useUrlBtn.addEventListener('click', function() {
                urlInputContainer.style.display = 'block';
                thumbnailUrlInput.value = thumbnailUrlHidden.value || '';
                thumbnailUrlInput.focus();
            });
        }

        // Apply URL
        if (applyUrlBtn) {
            applyUrlBtn.addEventListener('click', function() {
                const url = thumbnailUrlInput.value.trim();
                if (!url) {
                    alert('Please enter a valid URL');
                    return;
                }

                // Validate URL format
                try {
                    new URL(url);
                } catch {
                    alert('Please enter a valid URL');
                    return;
                }

                // Show loading
                uploadLoadingIndicator.style.display = 'flex';

                // Test if image loads
                const testImg = new Image();
                testImg.onload = function() {
                    thumbnailPreview.src = url;
                    thumbnailUrlHidden.value = url;
                    urlInputContainer.style.display = 'none';
                    uploadLoadingIndicator.style.display = 'none';
                    
                    // Success feedback
                    thumbnailPreview.style.border = '3px solid #10b981';
                    setTimeout(() => {
                        thumbnailPreview.style.border = '';
                    }, 2000);
                };
                testImg.onerror = function() {
                    uploadLoadingIndicator.style.display = 'none';
                    alert('Unable to load image from this URL. Please check the URL and try again.');
                };
                testImg.src = url;
            });
        }

        // Cancel URL
        if (cancelUrlBtn) {
            cancelUrlBtn.addEventListener('click', function() {
                urlInputContainer.style.display = 'none';
                thumbnailUrlInput.value = '';
            });
        }

        // Remove Thumbnail
        if (removeThumbnailBtn) {
            removeThumbnailBtn.addEventListener('click', function() {
                if (confirm('Are you sure you want to remove the current thumbnail?')) {
                    thumbnailPreview.src = 'https://via.placeholder.com/1280x720/7c3aed/ffffff?text=Upload+Course+Thumbnail';
                    thumbnailUrlHidden.value = '';
                    thumbnailFileInput.value = '';
                    
                    // Remove the remove button
                    removeThumbnailBtn.style.display = 'none';
                }
            });
        }
    }

    // Replace the simulated upload function with real upload
    async function uploadImageToServer(file) {
        const formData = new FormData();
        formData.append('file', file);
        
        const antiForgeryToken = getAntiForgeryToken();
        
        try {
            const response = await fetch('/instructor/upload-course-thumbnail', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': antiForgeryToken
                },
                body: formData
            });
            
            if (!response.ok) {
                throw new Error('Upload failed');
            }
            
            const result = await response.json();
            
            if (result.success) {
                const thumbnailUrlHidden = document.getElementById('thumbnailUrl');
                thumbnailUrlHidden.value = result.url;
                
                // Show remove button
                const removeThumbnailBtn = document.getElementById('removeThumbnailBtn');
                if (removeThumbnailBtn) {
                    removeThumbnailBtn.style.display = 'flex';
                }
                
                return result.url;
            } else {
                throw new Error(result.message || 'Upload failed');
            }
        } catch (error) {
            console.error('Upload error:', error);
            throw error;
        }
    }

    // ========================================
    // THUMBNAIL PREVIEW
    // ========================================

    function initThumbnailPreviewLegacy() {
        const thumbnailUrlInput = document.getElementById('thumbnailUrl');
        const thumbnailPreview = document.getElementById('thumbnailPreview');
        const changeThumbnailBtn = document.getElementById('changeThumbnailBtn');
        const removeThumbnailBtn = document.getElementById('removeThumbnailBtn'); // New line

        if (thumbnailUrlInput && thumbnailPreview) {
            // Update preview when URL changes
            thumbnailUrlInput.addEventListener('input', function() {
                const url = this.value.trim();
                if (url) {
                    thumbnailPreview.src = url;
                } else {
                    thumbnailPreview.src = 'https://via.placeholder.com/400x225?text=No+Thumbnail';
                }
            });

            // Focus input when change button is clicked
            if (changeThumbnailBtn) {
                changeThumbnailBtn.addEventListener('click', function() {
                    thumbnailUrlInput.focus();
                    thumbnailUrlInput.select();
                });
            }

            // Validate image URL on blur
            thumbnailUrlInput.addEventListener('blur', function() {
                const url = this.value.trim();
                if (url) {
                    // Test if image loads
                    const testImg = new Image();
                    testImg.onload = function() {
                        thumbnailPreview.style.border = '3px solid #10b981';
                        setTimeout(() => {
                            thumbnailPreview.style.border = '';
                        }, 2000);
                    };
                    testImg.onerror = function() {
                        alert('Unable to load image from this URL. Please check the URL and try again.');
                        thumbnailPreview.src = 'https://via.placeholder.com/400x225?text=Invalid+URL';
                    };
                    testImg.src = url;
                }
            });
        }

        // Remove Thumbnail functionality
        if (removeThumbnailBtn) {
            removeThumbnailBtn.addEventListener('click', function() {
                thumbnailUrlInput.value = '';
                thumbnailPreview.src = 'https://via.placeholder.com/400x225?text=No+Thumbnail';
                thumbnailPreview.style.border = '';
                thumbnailUrlInput.focus();
            });
        }
    }

    // Helper function to get anti-forgery token
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    window.saveModule = async function() {
        const courseId = window.location.pathname.split('/').pop();
        const form = document.getElementById('editModuleForm');
        
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const moduleData = {
            moduleId: document.getElementById('moduleId').value ? parseInt(document.getElementById('moduleId').value) : null,
            title: document.getElementById('moduleTitle').value.trim(),
            description: document.getElementById('moduleDescription').value.trim(),
            order: parseInt(document.getElementById('moduleOrder').value) || 1
        };
        
        try {
            const antiForgeryToken = getAntiForgeryToken();
            
            const response = await fetch(`/instructor/edit-course/${courseId}/module`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: JSON.stringify(moduleData)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server response:', errorText);
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            if (result.success) {
                alert(moduleData.moduleId ? 'Module updated successfully!' : 'Module added successfully!');
                closeEditModuleModal();
                location.reload();
            } else {
                alert('Failed to save module: ' + result.message);
            }
        } catch (error) {
            console.error('Error saving module:', error);
            alert('An error occurred while saving the module: ' + error.message);
        }
    };

    window.saveLesson = async function() {
        const courseId = window.location.pathname.split('/').pop();
        const form = document.getElementById('editLessonForm');
        
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const contentType = parseInt(document.getElementById('lessonContentType').value);
        
        // Convert minutes to seconds before saving
        const durationInMinutes = parseFloat(document.getElementById('lessonDuration').value) || 0;
        const durationInSeconds = Math.round(durationInMinutes * 60);
        
        // Collect resources
        const resources = collectLessonResources();
        
        const lessonData = {
            lessonId: document.getElementById('lessonId').value ? parseInt(document.getElementById('lessonId').value) : null,
            moduleId: parseInt(document.getElementById('lessonModuleId').value),
            title: document.getElementById('lessonTitle').value.trim(),
            contentType: contentType,
            order: parseInt(document.getElementById('lessonOrder').value),
            videoUrl: contentType === 1 ? document.getElementById('lessonVideoUrl').value.trim() : null,
            pdfUrl: contentType === 0 ? document.getElementById('lessonPdfUrl').value.trim() : null,
            durationInSeconds: contentType === 1 ? durationInSeconds : null,
            resources: resources  // ADD THIS
        };
        
        try {
            const antiForgeryToken = getAntiForgeryToken();
            
            const response = await fetch(`/instructor/edit-course/${courseId}/lesson`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken
                },
                body: JSON.stringify(lessonData)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Server response:', errorText);
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            if (result.success) {
                alert(lessonData.lessonId ? 'Lesson updated successfully!' : 'Lesson added successfully!');
                window.closeEditLessonModal();
                location.reload();
            } else {
                alert('Failed to save lesson: ' + result.message);
            }
        } catch (error) {
            console.error('Error saving lesson:', error);
            alert('An error occurred while saving the lesson: ' + error.message);
        }
    };

    // Delete Module
    async function deleteModule(courseId, moduleId) {
        try {
            const response = await fetch(`/instructor/edit-course/${courseId}/module/${moduleId}`, {
                method: 'DELETE',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const result = await response.json();
            if (result.success) {
                alert('Module deleted successfully!');
                location.reload();
            } else {
                alert('Failed to delete module: ' + result.message);
            }
        } catch (error) {
            console.error('Error deleting module:', error);
            alert('An error occurred while deleting the module');
        }
    }

    // Delete Lesson
    async function deleteLesson(courseId, lessonId) {
        try {
            const response = await fetch(`/instructor/edit-course/${courseId}/lesson/${lessonId}`, {
                method: 'DELETE',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const result = await response.json();
            if (result.success) {
                alert('Lesson deleted successfully!');
                location.reload();
            } else {
                alert('Failed to delete lesson: ' + result.message);
            }
        } catch (error) {
            console.error('Error deleting lesson:', error);
            alert('An error occurred while deleting the lesson');
        }
    }

    // Close modals on overlay click
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('modal-overlay')) {
            closeEditModuleModal();
            closeEditLessonModal();
        }
    });

    // ========================================
    // LESSON RESOURCES MANAGEMENT
    // ========================================

    function initLessonResources() {
        const addResourceBtn = document.getElementById('addLessonResource');
        if (addResourceBtn) {
            addResourceBtn.addEventListener('click', () => addResourceItem());
        }
    }

    function addResourceItem(resourceData = null) {
        const resourcesList = document.getElementById('lessonResourcesList');
        
        // Remove empty state if exists
        const emptyState = resourcesList.querySelector('.empty-resources');
        if (emptyState) {
            emptyState.remove();
        }
        
        const index = resourcesList.children.length;
        
        const resourceHtml = `
            <div class="resource-item" data-resource-index="${index}">
                <div class="form-group">
                    <label class="form-label">
                        <i class="fas fa-tag"></i> Type
                    </label>
                    <select class="form-select resource-type" data-index="${index}">
                        <option value="3" ${resourceData?.ResourceType === 3 ? 'selected' : ''}>?? URL Link</option>
                        <option value="1" ${resourceData?.ResourceType === 1 ? 'selected' : ''}>?? PDF Document</option>
                        <option value="2" ${resourceData?.ResourceType === 2 ? 'selected' : ''}>?? ZIP File</option>
                    </select>
                </div>
                
                <div class="form-group">
                    <label class="form-label">
                        <i class="fas fa-link"></i> Resource URL *
                    </label>
                    <input type="url" 
                           class="form-input resource-url" 
                           data-index="${index}"
                           placeholder="https://example.com/resource.pdf" 
                           value="${resourceData?.Url || ''}"
                           required />
                </div>
                
                <div class="form-group">
                    <label class="form-label">
                        <i class="fas fa-font"></i> Display Title
                    </label>
                    <input type="text" 
                           class="form-input resource-title" 
                           data-index="${index}"
                           placeholder="e.g., Lecture Slides"
                           value="${resourceData?.Title || ''}" />
                </div>
                
                <button type="button" class="btn-remove-resource" data-index="${index}" title="Remove Resource">
                    <i class="fas fa-trash-alt"></i>
                </button>
                
                <input type="hidden" class="resource-id" data-index="${index}" value="${resourceData?.LessonResourceId || ''}" />
            </div>
        `;
        
        resourcesList.insertAdjacentHTML('beforeend', resourceHtml);
        
        // Attach event listener to the new remove button
        const newItem = resourcesList.lastElementChild;
        const removeBtn = newItem.querySelector('.btn-remove-resource');
        if (removeBtn) {
            removeBtn.addEventListener('click', function() {
                const item = this.closest('.resource-item');
                item.style.animation = 'slideOut 0.3s ease-out';
                setTimeout(() => {
                    item.remove();
                    checkEmptyState();
                }, 300);
            });
        }
    }

    function checkEmptyState() {
        const resourcesList = document.getElementById('lessonResourcesList');
        const resourceItems = resourcesList.querySelectorAll('.resource-item');
        
        // Remove existing empty state
        const existingEmpty = resourcesList.querySelector('.empty-resources');
        if (existingEmpty) {
            existingEmpty.remove();
        }
        
        // Add empty state if no resources
        if (resourceItems.length === 0) {
            resourcesList.innerHTML = `
                <div class="empty-resources">
                    <i class="fas fa-link"></i>
                    <p>No resources added yet. Click "Add Resource" to get started.</p>
                </div>
            `;
        }
    }

    function loadLessonResources(resources) {
        const resourcesList = document.getElementById('lessonResourcesList');
        resourcesList.innerHTML = '';
        
        if (resources && resources.length > 0) {
            resources.forEach(resource => {
                addResourceItem({
                    LessonResourceId: resource.LessonResourceId,
                    ResourceType: resource.ResourceType,
                    Url: resource.Url,
                    Title: resource.Title
                });
            });
        } else {
            checkEmptyState();
        }
    }

    function collectLessonResources() {
        const resources = [];
        const resourceItems = document.querySelectorAll('.resource-item');
        
        resourceItems.forEach(item => {
            const resourceId = item.querySelector('.resource-id').value;
            const resourceType = parseInt(item.querySelector('.resource-type').value);
            const url = item.querySelector('.resource-url').value.trim();
            const title = item.querySelector('.resource-title').value.trim();
            
            if (url) {
                resources.push({
                    lessonResourceId: resourceId ? parseInt(resourceId) : null,
                    resourceType: resourceType,
                    url: url,
                    title: title || null
                });
            }
        });
        
        return resources;
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
        initThumbnailPreview();
        initCurriculumManagement();
        initLessonResources(); // ADD THIS LINE

        console.log('Edit Course initialized');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();