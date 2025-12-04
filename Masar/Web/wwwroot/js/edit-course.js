// ========================================
// EDIT COURSE - JavaScript
// ========================================

(function () {
    'use strict';

    let currentModuleData = null;
    let currentLessonData = null;
    
    // Track pending changes (not yet saved to server)
    let pendingLessons = []; // { action: 'add'|'update'|'delete', data: {...}, tempId: '...' }
    let pendingModules = []; // { action: 'add'|'update'|'delete', data: {...}, tempId: '...' }
    let tempIdCounter = 0;
    
    // Track if there are unsaved changes to course details form
    let hasFormChanges = false;

    // ========================================
    // UNSAVED CHANGES WARNING
    // ========================================

    function hasUnsavedChanges() {
        return pendingLessons.length > 0 || pendingModules.length > 0 || hasFormChanges;
    }

    function initUnsavedChangesWarning() {
        // Warn user when trying to leave the page with unsaved changes
        window.addEventListener('beforeunload', function(e) {
            if (hasUnsavedChanges()) {
                // Standard way to show browser's native confirmation dialog
                e.preventDefault();
                // For older browsers
                e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
                return e.returnValue;
            }
        });

        // Track changes in the course details form
        const form = document.getElementById('editCourseForm');
        if (form) {
            // Track input changes
            form.addEventListener('input', function() {
                hasFormChanges = true;
                updatePendingIndicator();
            });
            
            // Track select changes
            form.addEventListener('change', function() {
                hasFormChanges = true;
                updatePendingIndicator();
            });
        }

        // Intercept navigation links to show custom warning
        document.addEventListener('click', function(e) {
            const link = e.target.closest('a[href]');
            if (link && hasUnsavedChanges()) {
                const href = link.getAttribute('href');
                // Skip if it's a hash link, javascript link, or same page
                if (href && !href.startsWith('#') && !href.startsWith('javascript:') && href !== window.location.href) {
                    e.preventDefault();
                    showUnsavedChangesModal(href);
                }
            }
        });
    }

    function showUnsavedChangesModal(targetUrl) {
        // Check if modal already exists
        let modal = document.getElementById('unsavedChangesModal');
        
        if (!modal) {
            // Create the modal
            const modalHtml = `
                <div id="unsavedChangesModal" class="modal" style="display: block;">
                    <div class="modal-overlay" id="unsavedChangesOverlay"></div>
                    <div class="modal-content modal-sm">
                        <div class="modal-header warning-header">
                            <h3 class="modal-title">
                                <i class="fas fa-exclamation-triangle warning-icon"></i>
                                Unsaved Changes
                            </h3>
                            <button type="button" class="modal-close" id="closeUnsavedModal">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <div class="modal-body">
                            <p class="warning-message">You have unsaved changes that will be lost if you leave this page.</p>
                            <p class="warning-submessage">Do you want to save your changes before leaving?</p>
                        </div>
                        <div class="modal-footer warning-footer">
                            <button type="button" class="btn btn-secondary" id="discardChangesBtn">
                                <i class="fas fa-trash-alt"></i>
                                Discard Changes
                            </button>
                            <button type="button" class="btn btn-primary" id="saveBeforeLeaveBtn">
                                <i class="fas fa-save"></i>
                                Save Changes
                            </button>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.insertAdjacentHTML('beforeend', modalHtml);
            modal = document.getElementById('unsavedChangesModal');
            
            // Add modal styles
            addUnsavedChangesModalStyles();
        } else {
            modal.style.display = 'block';
        }

        // Event handlers
        const closeBtn = document.getElementById('closeUnsavedModal');
        const overlay = document.getElementById('unsavedChangesOverlay');
        const discardBtn = document.getElementById('discardChangesBtn');
        const saveBtn = document.getElementById('saveBeforeLeaveBtn');

        function closeModal() {
            modal.style.display = 'none';
        }

        closeBtn.onclick = closeModal;
        overlay.onclick = closeModal;

        discardBtn.onclick = function() {
            // Clear pending changes so beforeunload doesn't trigger
            pendingLessons = [];
            pendingModules = [];
            hasFormChanges = false;
            // Navigate to target
            window.location.href = targetUrl;
        };

        saveBtn.onclick = async function() {
            // Show loading state
            saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Saving...';
            saveBtn.disabled = true;
            discardBtn.disabled = true;

            try {
                await saveAllChanges();
                // Navigate after successful save
                window.location.href = targetUrl;
            } catch (error) {
                console.error('Error saving changes:', error);
                showToast('Failed to save changes. Please try again.', 'error');
                saveBtn.innerHTML = '<i class="fas fa-save"></i> Save Changes';
                saveBtn.disabled = false;
                discardBtn.disabled = false;
            }
        };
    }

    function addUnsavedChangesModalStyles() {
        const styleId = 'unsaved-changes-modal-styles';
        if (document.getElementById(styleId)) return;

        const styles = document.createElement('style');
        styles.id = styleId;
        styles.textContent = `
            #unsavedChangesModal .modal-sm {
                max-width: 450px;
            }
            
            #unsavedChangesModal .warning-header {
                background: linear-gradient(135deg, #f59e0b, #d97706);
                color: white;
                border-radius: 12px 12px 0 0;
            }
            
            #unsavedChangesModal .warning-header .modal-close {
                color: white;
            }
            
            #unsavedChangesModal .warning-header .modal-close:hover {
                background: rgba(255,255,255,0.2);
            }
            
            #unsavedChangesModal .warning-icon {
                margin-right: 10px;
                animation: shake 0.5s ease-in-out;
            }
            
            #unsavedChangesModal .warning-message {
                font-size: 1.1rem;
                color: #1f2937;
                margin-bottom: 0.5rem;
                font-weight: 500;
            }
            
            #unsavedChangesModal .warning-submessage {
                color: #6b7280;
                font-size: 0.95rem;
            }
            
            #unsavedChangesModal .warning-footer {
                display: flex;
                gap: 12px;
                justify-content: flex-end;
            }
            
            #unsavedChangesModal .warning-footer .btn {
                min-width: 140px;
            }
            
            @keyframes shake {
                0%, 100% { transform: translateX(0); }
                25% { transform: translateX(-5px); }
                75% { transform: translateX(5px); }
            }
        `;
        document.head.appendChild(styles);
    }

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
                hasFormChanges = true;
                updatePendingIndicator();
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
                    reindexOutcomes();
                    hasFormChanges = true;
                    updatePendingIndicator();
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
                }
            });
        }
    }

    // ========================================
    // SAVE ALL CHANGES - BATCH SAVE
    // ========================================

    function initSaveChanges() {
        const saveBtn = document.getElementById('saveChangesBtn');

        if (saveBtn) {
            saveBtn.addEventListener('click', async () => {
                await saveAllChanges();
            });
        }
    }

    async function saveAllChanges() {
        const saveBtn = document.getElementById('saveChangesBtn');
        const originalText = saveBtn ? saveBtn.innerHTML : '';
        
        if (saveBtn) {
            saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>Saving...</span>';
            saveBtn.disabled = true;
        }

        try {
            const courseId = window.location.pathname.split('/').pop();
            const antiForgeryToken = getAntiForgeryToken();
            let hasErrors = false;

            // Save pending modules first
            for (const pendingModule of pendingModules) {
                if (pendingModule.action === 'add' || pendingModule.action === 'update') {
                    const response = await fetch(`/instructor/edit-course/${courseId}/module`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': antiForgeryToken
                        },
                        body: JSON.stringify(pendingModule.data)
                    });
                    
                    const result = await response.json();
                    if (!result.success) {
                        hasErrors = true;
                        console.error('Failed to save module:', result.message);
                    } else if (pendingModule.action === 'add' && result.moduleId) {
                        pendingLessons.forEach(pl => {
                            if (pl.data.moduleId === pendingModule.tempId) {
                                pl.data.moduleId = result.moduleId;
                            }
                        });
                        const tempModuleCard = document.querySelector(`.edit-module-card[data-module-id="${pendingModule.tempId}"]`);
                        if (tempModuleCard) {
                            tempModuleCard.dataset.moduleId = result.moduleId;
                        }
                    }
                } else if (pendingModule.action === 'delete') {
                    const response = await fetch(`/instructor/edit-course/${courseId}/module/${pendingModule.data.moduleId}`, {
                        method: 'DELETE',
                        headers: {
                            'RequestVerificationToken': antiForgeryToken
                        }
                    });
                    
                    const result = await response.json();
                    if (!result.success) {
                        hasErrors = true;
                        console.error('Failed to delete module:', result.message);
                    }
                }
            }

            // Save pending lessons
            for (const pendingLesson of pendingLessons) {
                if (pendingLesson.action === 'add' || pendingLesson.action === 'update') {
                    const response = await fetch(`/instructor/edit-course/${courseId}/lesson`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': antiForgeryToken
                        },
                        body: JSON.stringify(pendingLesson.data)
                    });
                    
                    const result = await response.json();
                    if (!result.success) {
                        hasErrors = true;
                        console.error('Failed to save lesson:', result.message);
                    } else if (pendingLesson.action === 'add' && result.lessonId) {
                        const tempLessonCard = document.querySelector(`.lesson-card-edit[data-lesson-id="${pendingLesson.tempId}"]`);
                        if (tempLessonCard) {
                            tempLessonCard.dataset.lessonId = result.lessonId;
                        }
                    }
                } else if (pendingLesson.action === 'delete' && !pendingLesson.tempId?.toString().startsWith('temp-')) {
                    const response = await fetch(`/instructor/edit-course/${courseId}/lesson/${pendingLesson.data.lessonId}`, {
                        method: 'DELETE',
                        headers: {
                            'RequestVerificationToken': antiForgeryToken
                        }
                    });
                    
                    const result = await response.json();
                    if (!result.success) {
                        hasErrors = true;
                        console.error('Failed to delete lesson:', result.message);
                    }
                }
            }

            // Clear pending changes
            pendingModules = [];
            pendingLessons = [];

            // Also save the course details form if on details tab
            const form = document.getElementById('editCourseForm');
            if (form) {
                const formData = new FormData(form);
                const response = await fetch(`/instructor/edit-course/${courseId}`, {
                    method: 'POST',
                    body: formData
                });
                
                if (!response.ok && !response.redirected) {
                    hasErrors = true;
                }
            }

            // Reset form changes flag after successful save
            hasFormChanges = false;

            if (hasErrors) {
                showToast('Some changes could not be saved. Please try again.', 'error');
            } else {
                showToast('All changes saved successfully!', 'success');
                updatePendingIndicator();
            }
        } catch (error) {
            console.error('Error saving changes:', error);
            showToast('An error occurred while saving', 'error');
            throw error; // Re-throw for the modal handler
        } finally {
            if (saveBtn) {
                saveBtn.innerHTML = originalText;
                saveBtn.disabled = false;
            }
        }
    }

    function updatePendingIndicator() {
        const saveBtn = document.getElementById('saveChangesBtn');
        const pendingChangesCount = pendingLessons.length + pendingModules.length;
        const hasPendingChanges = hasUnsavedChanges();
        
        if (saveBtn) {
            if (hasPendingChanges) {
                saveBtn.classList.add('has-changes');
                if (pendingChangesCount > 0) {
                    saveBtn.innerHTML = '<i class="fas fa-save"></i> <span>Save Changes (' + pendingChangesCount + ')</span>';
                } else {
                    saveBtn.innerHTML = '<i class="fas fa-save"></i> <span>Save Changes *</span>';
                }
            } else {
                saveBtn.classList.remove('has-changes');
                saveBtn.innerHTML = '<i class="fas fa-save"></i> <span>Save Changes</span>';
            }
        }
    }

    // ========================================
    // DELETE MODULE LOCAL (IN-MEMORY)
    // ========================================

    function deleteModuleLocal(moduleId, moduleCard) {
        console.log('deleteModuleLocal called with moduleId:', moduleId);
        
        if (!moduleId.toString().startsWith('temp-')) {
            pendingModules.push({
                action: 'delete',
                data: { moduleId: parseInt(moduleId) }
            });
        } else {
            pendingModules = pendingModules.filter(pm => pm.tempId !== moduleId);
        }
        
        pendingLessons = pendingLessons.filter(pl => pl.data.moduleId.toString() !== moduleId.toString());
        
        if (moduleCard) {
            moduleCard.style.animation = 'fadeOut 0.3s ease-out';
            setTimeout(() => {
                moduleCard.remove();
                
                const modulesContainer = document.querySelector('.modules-list') || document.getElementById('editModulesList');
                if (modulesContainer && modulesContainer.querySelectorAll('.edit-module-card').length === 0) {
                    modulesContainer.innerHTML = `
                        <div class="empty-state">
                            <i class="fas fa-book-open"></i>
                            <p>No modules yet. Click "Add Module" to get started!</p>
                        </div>
                    `;
                }
            }, 300);
        }
        
        showToast('Module marked for deletion. Click "Save Changes" to apply.', 'success');
        updatePendingIndicator();
    }

    // ========================================
    // DELETE LESSON LOCAL (IN-MEMORY)
    // ========================================

    function deleteLessonLocal(lessonId, lessonCard) {
        console.log('deleteLessonLocal called with lessonId:', lessonId);
        
        if (!lessonId.toString().startsWith('temp-')) {
            pendingLessons.push({
                action: 'delete',
                data: { lessonId: parseInt(lessonId) }
            });
        } else {
            pendingLessons = pendingLessons.filter(pl => pl.tempId !== lessonId);
        }
        
        if (lessonCard) {
            const moduleCard = lessonCard.closest('.edit-module-card');
            lessonCard.style.animation = 'fadeOut 0.3s ease-out';
            
            setTimeout(() => {
                lessonCard.remove();
                
                if (moduleCard) {
                    const lessonsList = moduleCard.querySelector('.lessons-list-edit, .lessons-list');
                    const lessonsCount = lessonsList ? lessonsList.querySelectorAll('.lesson-card-edit').length : 0;
                    
                    const metaElement = moduleCard.querySelector('.module-meta');
                    if (metaElement) {
                        metaElement.textContent = `${lessonsCount} lesson${lessonsCount !== 1 ? 's' : ''}`;
                    }
                    
                    if (lessonsCount === 0 && lessonsList) {
                        lessonsList.innerHTML = `
                            <div class="empty-lessons">
                                <i class="fas fa-file-alt"></i>
                                <p>No lessons yet. Add your first lesson!</p>
                            </div>
                        `;
                    }
                }
            }, 300);
        }
        
        showToast('Lesson marked for deletion. Click "Save Changes" to apply.', 'success');
        updatePendingIndicator();
    }

    // ========================================
    // CURRICULUM MANAGEMENT
    // ========================================

    function initCurriculumManagement() {
        const courseId = window.location.pathname.split('/').pop();

        const addModuleBtn = document.getElementById('addNewModule');
        if (addModuleBtn) {
            addModuleBtn.addEventListener('click', function() {
                openEditModuleModal(null);
            });
        }

        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-edit-module')) {
                const btn = e.target.closest('.btn-edit-module');
                const moduleCard = btn.closest('.edit-module-card');
                const moduleId = moduleCard.dataset.moduleId;
                const moduleOrder = moduleCard.dataset.moduleOrder;
                const moduleDescription = moduleCard.dataset.moduleDescription || '';
                const moduleTitle = moduleCard.querySelector('.module-title-display').textContent.trim();
                
                openEditModuleModal({
                    moduleId: moduleId,
                    title: moduleTitle,
                    description: moduleDescription,
                    order: parseInt(moduleOrder)
                });
            }
        });

        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-delete-module')) {
                e.preventDefault();
                e.stopPropagation();
                
                if (confirm('Are you sure you want to delete this module? All lessons in this module will be deleted.')) {
                    const btn = e.target.closest('.btn-delete-module');
                    const moduleCard = btn.closest('.edit-module-card');
                    const moduleId = moduleCard.dataset.moduleId;
                    console.log('Delete module clicked, moduleId:', moduleId);
                    deleteModuleLocal(moduleId, moduleCard);
                }
            }
        });

        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-add-lesson') || e.target.closest('.btn-add-lesson-edit')) {
                const btn = e.target.closest('.btn-add-lesson') || e.target.closest('.btn-add-lesson-edit');
                const moduleCard = btn.closest('.edit-module-card');
                const moduleId = moduleCard.dataset.moduleId;
                
                openEditLessonModal(null, moduleId);
            }
        });

        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-edit-lesson')) {
                console.log('Edit lesson button clicked');
                
                const btn = e.target.closest('.btn-edit-lesson');
                const lessonCard = btn.closest('.lesson-card-edit');
                const lessonId = lessonCard.dataset.lessonId;
                const lessonOrder = parseInt(lessonCard.dataset.lessonOrder);
                const contentType = parseInt(lessonCard.dataset.lessonContentType);
                const videoUrl = lessonCard.dataset.lessonVideoUrl;
                const pdfUrl = lessonCard.dataset.lessonPdfUrl;
                const durationInSeconds = parseInt(lessonCard.dataset.lessonDuration) || 0;
                const moduleCard = lessonCard.closest('.edit-module-card');
                const moduleId = moduleCard.dataset.moduleId;
                const lessonTitle = lessonCard.querySelector('.lesson-title-edit').textContent.trim();
                
                const resourcesJson = lessonCard.dataset.lessonResources || '[]';
                let resources = [];
                try {
                    resources = JSON.parse(resourcesJson);
                    console.log('Parsed resources:', resources);
                } catch (e) {
                    console.error('Failed to parse resources:', e);
                }
                
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

        document.addEventListener('click', function(e) {
            if (e.target.closest('.btn-delete-lesson')) {
                e.preventDefault();
                e.stopPropagation();
                
                if (confirm('Are you sure you want to delete this lesson?')) {
                    const btn = e.target.closest('.btn-delete-lesson');
                    const lessonCard = btn.closest('.lesson-card-edit');
                    const lessonId = lessonCard.dataset.lessonId;
                    console.log('Delete lesson clicked, lessonId:', lessonId);
                    deleteLessonLocal(lessonId, lessonCard);
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
            document.getElementById('moduleId').value = moduleData.moduleId || '';
            document.getElementById('moduleTitle').value = moduleData.title || '';
            document.getElementById('moduleDescription').value = moduleData.description || '';
            document.getElementById('moduleOrder').value = moduleData.order || '';
            document.querySelector('#editModuleModal .modal-title').textContent = 'Edit Module';
        } else {
            form.reset();
            document.getElementById('moduleId').value = '';
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
            document.getElementById('lessonId').value = lessonData.lessonId || '';
            document.getElementById('lessonModuleId').value = lessonData.moduleId || '';
            document.getElementById('lessonTitle').value = lessonData.title || '';
            document.getElementById('lessonContentType').value = lessonData.contentType || 0;
            document.getElementById('lessonOrder').value = lessonData.order || '';
            
            document.getElementById('lessonVideoUrl').value = lessonData.videoUrl || '';
            document.getElementById('lessonPdfUrl').value = lessonData.pdfUrl || '';
            
            const durationInMinutes = lessonData.durationInSeconds ? (lessonData.durationInSeconds / 60).toFixed(1) : '';
            document.getElementById('lessonDuration').value = durationInMinutes;
            
            loadLessonResources(lessonData.resources || []);
            
            document.querySelector('#editLessonModal .modal-title').textContent = 'Edit Lesson';
        } else {
            form.reset();
            document.getElementById('lessonId').value = '';
            document.getElementById('lessonModuleId').value = moduleId;
            const moduleCard = document.querySelector(`.edit-module-card[data-module-id="${moduleId}"]`);
            const lessonsCount = moduleCard ? moduleCard.querySelectorAll('.lesson-card-edit').length : 0;
            document.getElementById('lessonOrder').value = lessonsCount + 1;
            document.getElementById('lessonContentType').value = 0;
            
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
        
        if (contentType === 1) {
            videoFields.style.display = 'block';
            pdfFields.style.display = 'none';
            document.getElementById('lessonVideoUrl').required = true;
            document.getElementById('lessonDuration').required = true;
            document.getElementById('lessonPdfUrl').required = false;
        } else {
            videoFields.style.display = 'none';
            pdfFields.style.display = 'block';
            document.getElementById('lessonVideoUrl').required = false;
            document.getElementById('lessonDuration').required = false;
            document.getElementById('lessonPdfUrl').required = true;
        }
    };

    // ========================================
    // TOAST NOTIFICATION HELPER
    // ========================================

    function showToast(message, type = 'success') {
        const existingToast = document.querySelector('.toast-notification');
        if (existingToast) {
            existingToast.remove();
        }
        
        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.innerHTML = `
            <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'}"></i>
            <span>${message}</span>
        `;
        
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 16px 24px;
            border-radius: 8px;
            background: ${type === 'success' ? '#10b981' : '#ef4444'};
            color: white;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 10px;
            z-index: 10000;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            animation: slideInRight 0.3s ease-out;
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.style.animation = 'slideOutRight 0.3s ease-out';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

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

        if (uploadFromDeviceBtn && thumbnailFileInput) {
            uploadFromDeviceBtn.addEventListener('click', function() {
                thumbnailFileInput.click();
            });

            thumbnailFileInput.addEventListener('change', async function(e) {
                const file = e.target.files[0];
                if (!file) return;

                const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
                if (!validTypes.includes(file.type)) {
                    alert('Please select a valid image file (JPG, PNG, or WebP)');
                    return;
                }

                const maxSize = 5 * 1024 * 1024;
                if (file.size > maxSize) {
                    alert('File size must be less than 5MB');
                    return;
                }

                uploadLoadingIndicator.style.display = 'flex';

                try {
                    const reader = new FileReader();
                    reader.onload = function(event) {
                        thumbnailPreview.src = event.target.result;
                    };
                    reader.readAsDataURL(file);

                    await uploadImageToServer(file);
                    hasFormChanges = true;
                    updatePendingIndicator();

                } catch (error) {
                    console.error('Error uploading image:', error);
                    alert('Failed to upload image. Please try again.');
                    thumbnailPreview.src = 'https://via.placeholder.com/1280x720/ef4444/ffffff?text=Upload+Failed';
                } finally {
                    uploadLoadingIndicator.style.display = 'none';
                }
            });
        }

        if (useUrlBtn && urlInputContainer) {
            useUrlBtn.addEventListener('click', function() {
                urlInputContainer.style.display = 'block';
                thumbnailUrlInput.value = thumbnailUrlHidden.value || '';
                thumbnailUrlInput.focus();
            });
        }

        if (applyUrlBtn) {
            applyUrlBtn.addEventListener('click', function() {
                const url = thumbnailUrlInput.value.trim();
                if (!url) {
                    alert('Please enter a valid URL');
                    return;
                }

                try {
                    new URL(url);
                } catch {
                    alert('Please enter a valid URL');
                    return;
                }

                uploadLoadingIndicator.style.display = 'flex';

                const testImg = new Image();
                testImg.onload = function() {
                    thumbnailPreview.src = url;
                    thumbnailUrlHidden.value = url;
                    urlInputContainer.style.display = 'none';
                    uploadLoadingIndicator.style.display = 'none';
                    hasFormChanges = true;
                    updatePendingIndicator();
                    
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

        if (cancelUrlBtn) {
            cancelUrlBtn.addEventListener('click', function() {
                urlInputContainer.style.display = 'none';
                thumbnailUrlInput.value = '';
            });
        }

        if (removeThumbnailBtn) {
            removeThumbnailBtn.addEventListener('click', function() {
                if (confirm('Are you sure you want to remove the current thumbnail?')) {
                    thumbnailPreview.src = 'https://via.placeholder.com/1280x720/7c3aed/ffffff?text=Upload+Course+Thumbnail';
                    thumbnailUrlHidden.value = '';
                    thumbnailFileInput.value = '';
                    removeThumbnailBtn.style.display = 'none';
                    hasFormChanges = true;
                    updatePendingIndicator();
                }
            });
        }
    }

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

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // ========================================
    // SAVE MODULE (LOCAL ONLY - NO SERVER CALL)
    // ========================================

    window.saveModule = function() {
        const form = document.getElementById('editModuleForm');
        
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const moduleIdValue = document.getElementById('moduleId').value;
        const isNewModule = !moduleIdValue;
        const tempId = isNewModule ? `temp-module-${++tempIdCounter}` : moduleIdValue;
        
        const moduleData = {
            moduleId: isNewModule ? null : (moduleIdValue.startsWith('temp-') ? null : parseInt(moduleIdValue)),
            title: document.getElementById('moduleTitle').value.trim(),
            description: document.getElementById('moduleDescription').value.trim(),
            order: parseInt(document.getElementById('moduleOrder').value) || 1
        };
        
        console.log('Saving module locally:', moduleData);
        
        if (isNewModule) {
            pendingModules.push({
                action: 'add',
                tempId: tempId,
                data: moduleData
            });
            
            addNewModuleToUI(moduleData, tempId);
            showToast('Module added. Click "Save Changes" to save to server.', 'success');
        } else {
            const existingPending = pendingModules.find(pm => pm.tempId === moduleIdValue);
            if (existingPending) {
                existingPending.data = moduleData;
            } else if (!moduleIdValue.startsWith('temp-')) {
                const existingUpdate = pendingModules.find(pm => pm.data.moduleId === parseInt(moduleIdValue) && pm.action === 'update');
                if (existingUpdate) {
                    existingUpdate.data = { ...moduleData, moduleId: parseInt(moduleIdValue) };
                } else {
                    pendingModules.push({
                        action: 'update',
                        data: { ...moduleData, moduleId: parseInt(moduleIdValue) }
                    });
                }
            }
            
            updateModuleInUI({ ...moduleData, moduleId: moduleIdValue });
            showToast('Module updated. Click "Save Changes" to save to server.', 'success');
        }
        
        closeEditModuleModal();
        updatePendingIndicator();
    };

    function updateModuleInUI(moduleData) {
        const moduleCard = document.querySelector(`.edit-module-card[data-module-id="${moduleData.moduleId}"]`);
        if (!moduleCard) return;
        
        const titleElement = moduleCard.querySelector('.module-title-display');
        if (titleElement) {
            titleElement.textContent = moduleData.title;
        }
        
        moduleCard.dataset.moduleOrder = moduleData.order;
        moduleCard.dataset.moduleDescription = moduleData.description || '';
    }

    // ========================================
    // SAVE LESSON (LOCAL ONLY - NO SERVER CALL)
    // ========================================

    window.saveLesson = function() {
        const form = document.getElementById('editLessonForm');
        
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const contentType = parseInt(document.getElementById('lessonContentType').value);
        const lessonIdValue = document.getElementById('lessonId').value;
        const isNewLesson = !lessonIdValue;
        const tempId = isNewLesson ? `temp-lesson-${++tempIdCounter}` : lessonIdValue;
        
        const durationInMinutes = parseFloat(document.getElementById('lessonDuration').value) || 0;
        const durationInSeconds = Math.round(durationInMinutes * 60);
        
        const resources = collectLessonResources();
        const moduleId = document.getElementById('lessonModuleId').value;
        
        const lessonData = {
            lessonId: isNewLesson ? null : (lessonIdValue.startsWith('temp-') ? null : parseInt(lessonIdValue)),
            moduleId: moduleId.startsWith('temp-') ? moduleId : parseInt(moduleId),
            title: document.getElementById('lessonTitle').value.trim(),
            contentType: contentType,
            order: parseInt(document.getElementById('lessonOrder').value),
            videoUrl: contentType === 1 ? document.getElementById('lessonVideoUrl').value.trim() : null,
            pdfUrl: contentType === 0 ? document.getElementById('lessonPdfUrl').value.trim() : null,
            durationInSeconds: contentType === 1 ? durationInSeconds : null,
            resources: resources
        };
        
        console.log('Saving lesson locally:', lessonData);
        
        if (isNewLesson) {
            pendingLessons.push({
                action: 'add',
                tempId: tempId,
                data: lessonData
            });
            
            addNewLessonToUI({ ...lessonData, moduleId: moduleId }, tempId);
            showToast('Lesson added. Click "Save Changes" to save to server.', 'success');
        } else {
            const existingPending = pendingLessons.find(pl => pl.tempId === lessonIdValue);
            if (existingPending) {
                existingPending.data = lessonData;
            } else if (!lessonIdValue.startsWith('temp-')) {
                const existingUpdate = pendingLessons.find(pl => pl.data.lessonId === parseInt(lessonIdValue) && pl.action === 'update');
                if (existingUpdate) {
                    existingUpdate.data = { ...lessonData, lessonId: parseInt(lessonIdValue) };
                } else {
                    pendingLessons.push({
                        action: 'update',
                        data: { ...lessonData, lessonId: parseInt(lessonIdValue) }
                    });
                }
            }
            
            updateLessonInUI({ ...lessonData, lessonId: lessonIdValue });
            showToast('Lesson updated. Click "Save Changes" to save to server.', 'success');
        }
        
        window.closeEditLessonModal();
        updatePendingIndicator();
    };

    function updateLessonInUI(lessonData) {
        const lessonCard = document.querySelector(`.lesson-card-edit[data-lesson-id="${lessonData.lessonId}"]`);
        if (!lessonCard) {
            console.log('Lesson card not found for ID:', lessonData.lessonId);
            return;
        }
        
        const titleElement = lessonCard.querySelector('.lesson-title-edit');
        if (titleElement) {
            titleElement.textContent = lessonData.title;
        }
        
        lessonCard.dataset.lessonOrder = lessonData.order;
        lessonCard.dataset.lessonContentType = lessonData.contentType;
        lessonCard.dataset.lessonVideoUrl = lessonData.videoUrl || '';
        lessonCard.dataset.lessonPdfUrl = lessonData.pdfUrl || '';
        lessonCard.dataset.lessonDuration = lessonData.durationInSeconds || 0;
        lessonCard.dataset.lessonResources = JSON.stringify(lessonData.resources || []);
        
        const typeBadge = lessonCard.querySelector('.lesson-type, .lesson-type-icon');
        if (typeBadge) {
            if (lessonData.contentType === 1) {
                typeBadge.innerHTML = '<i class="fas fa-play-circle"></i>';
                typeBadge.className = 'lesson-type-icon video';
            } else {
                typeBadge.innerHTML = '<i class="fas fa-file-alt"></i>';
                typeBadge.className = 'lesson-type-icon article';
            }
        }
        
        const metaElement = lessonCard.querySelector('.lesson-meta-edit');
        if (metaElement && lessonData.contentType === 1 && lessonData.durationInSeconds) {
            const minutes = Math.floor(lessonData.durationInSeconds / 60);
            const durationSpan = metaElement.querySelector('span:last-child');
            if (durationSpan) {
                durationSpan.innerHTML = `<i class="fas fa-clock"></i> ${minutes}m`;
            }
        }
        
        console.log('Lesson UI updated successfully for ID:', lessonData.lessonId);
    }

    function addNewModuleToUI(moduleData, newModuleId) {
        const modulesContainer = document.querySelector('.modules-list') || document.querySelector('[data-modules-container]') || document.getElementById('editModulesList');
        if (!modulesContainer) {
            console.error('Modules container not found');
            return;
        }

        const emptyState = modulesContainer.querySelector('.empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        const moduleHtml = `
            <div class="edit-module-card" 
                 data-module-id="${newModuleId || 'new'}" 
                 data-module-order="${moduleData.order}"
                 data-module-description="${moduleData.description || ''}">
                <div class="module-card-header">
                    <div class="module-info">
                        <i class="fas fa-grip-vertical drag-handle"></i>
                        <div class="module-details">
                            <h4 class="module-title-display">${moduleData.title}</h4>
                            <span class="module-meta">0 lessons</span>
                        </div>
                    </div>
                    <div class="module-card-actions">
                        <button type="button" class="btn-icon btn-edit-module" title="Edit Module">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn-icon btn-add-lesson" title="Add Lesson">
                            <i class="fas fa-plus"></i>
                        </button>
                        <button type="button" class="btn-icon btn-delete-module" title="Delete Module">
                            <i class="fas fa-trash"></i>
                        </button>
                        <button type="button" class="btn-icon btn-toggle">
                            <i class="fas fa-chevron-down"></i>
                        </button>
                    </div>
                </div>
                <div class="module-card-body">
                    <div class="lessons-list-edit">
                        <div class="empty-lessons">
                            <i class="fas fa-file-alt"></i>
                            <p>No lessons yet. Add your first lesson!</p>
                        </div>
                    </div>
                    <button type="button" class="btn-add-lesson-edit">
                        <i class="fas fa-plus"></i>
                        <span>Add Lesson to This Module</span>
                    </button>
                </div>
            </div>
        `;

        modulesContainer.insertAdjacentHTML('beforeend', moduleHtml);
        
        const newModuleCard = modulesContainer.lastElementChild;
        const toggleBtn = newModuleCard.querySelector('.btn-toggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', () => {
                const moduleBody = newModuleCard.querySelector('.module-card-body');
                if (moduleBody.style.display === 'none') {
                    moduleBody.style.display = 'block';
                    toggleBtn.classList.remove('rotated');
                } else {
                    moduleBody.style.display = 'none';
                    toggleBtn.classList.add('rotated');
                }
            });
        }
    }

    function addNewLessonToUI(lessonData, newLessonId) {
        const moduleCard = document.querySelector(`.edit-module-card[data-module-id="${lessonData.moduleId}"]`);
        if (!moduleCard) {
            console.error('Module card not found for ID:', lessonData.moduleId);
            return;
        }

        const lessonsList = moduleCard.querySelector('.lessons-list-edit, .lessons-list');
        if (!lessonsList) {
            console.error('Lessons list not found in module');
            return;
        }

        const emptyState = lessonsList.querySelector('.empty-lessons');
        if (emptyState) {
            emptyState.remove();
        }

        const typeIcon = lessonData.contentType === 1 ? 'fa-play-circle' : 'fa-file-alt';
        const typeClass = lessonData.contentType === 1 ? 'video' : 'article';
        
        let durationText = 'N/A';
        if (lessonData.contentType === 1 && lessonData.durationInSeconds) {
            const minutes = Math.floor(lessonData.durationInSeconds / 60);
            durationText = `${minutes}m`;
        }

        const lessonHtml = `
            <div class="lesson-card-edit" 
                 data-lesson-id="${newLessonId || 'new'}"
                 data-lesson-order="${lessonData.order}"
                 data-lesson-content-type="${lessonData.contentType}"
                 data-lesson-video-url="${lessonData.videoUrl || ''}"
                 data-lesson-pdf-url="${lessonData.pdfUrl || ''}"
                 data-lesson-duration="${lessonData.durationInSeconds || 0}"
                 data-lesson-resources='${JSON.stringify(lessonData.resources || []).replace(/'/g, "&#39;")}'>
                
                <div class="drag-handle">
                    <i class="fas fa-grip-vertical"></i>
                </div>
                
                <div class="lesson-type-icon ${typeClass}">
                    <i class="fas ${typeIcon}"></i>
                </div>
                
                <div class="lesson-info-edit">
                    <div class="lesson-title-edit">${lessonData.title}</div>
                    <div class="lesson-meta-edit">
                        <span><i class="fas fa-sort-numeric-up"></i> Order: ${lessonData.order}</span>
                        <span><i class="fas fa-clock"></i> ${durationText}</span>
                    </div>
                </div>
                
                <div class="lesson-actions-edit">
                    <button type="button" class="btn-icon-small btn-edit-lesson" title="Edit Lesson">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button type="button" class="btn-icon-small btn-delete-lesson" title="Delete Lesson">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;

        lessonsList.insertAdjacentHTML('beforeend', lessonHtml);

        const lessonsCount = lessonsList.querySelectorAll('.lesson-card-edit').length;
        const metaElement = moduleCard.querySelector('.module-meta');
        if (metaElement) {
            metaElement.textContent = `${lessonsCount} lesson${lessonsCount !== 1 ? 's' : ''}`;
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
        
        const existingEmpty = resourcesList.querySelector('.empty-resources');
        if (existingEmpty) {
            existingEmpty.remove();
        }
        
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
        initLessonResources();
        initUnsavedChangesWarning(); // Initialize unsaved changes warning

        // Add CSS for animations
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideInRight {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes slideOutRight {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(100%); opacity: 0; }
            }
            @keyframes fadeOut {
                from { opacity: 1; transform: scale(1); }
                to { opacity: 0; transform: scale(0.95); }
            }
            @keyframes slideOut {
                from { opacity: 1; transform: translateX(0); }
                to { opacity: 0; transform: translateX(-20px); }
            }
            #saveChangesBtn.has-changes {
                background: linear-gradient(135deg, #f59e0b, #d97706) !important;
                animation: pulse 2s infinite;
            }
            @keyframes pulse {
                0%, 100% { box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.4); }
                50% { box-shadow: 0 0 0 10px rgba(245, 158, 11, 0); }
            }
        `;
        document.head.appendChild(style);

        console.log('Edit Course initialized with batch save support and unsaved changes warning');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();