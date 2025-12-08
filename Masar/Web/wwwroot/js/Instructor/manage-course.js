const CourseManager = {

    // 1. STATE
    state: {
        activeTab: 'curriculum',
        courseId: null,
        courseStatus: null,
        resourceCounter: 0,
        // Track selected categories and languages
        selectedCategories: [],
        selectedLanguages: [],
        allCategories: [],
        allLanguages: []
    },

    // 2. ACTIONS
    actions: {
        'toggle-module': function (element, event) {
            event.stopPropagation();
            const parentModuleItem = element.closest('.module-item');
            if (parentModuleItem)
                parentModuleItem.classList.toggle('expanded');
        },

        'open-add-module': function () {
            document.getElementById("moduleForm").reset();
            document.getElementById("moduleIdInput").value = "0";
            document.getElementById("moduleModalTitle").innerText = "Add New Module";
            CourseManager.functions.showModal("moduleModal");
        },

        'open-edit-module': function (element, event) {
            event.stopPropagation();
            const moduleId = element.dataset.moduleId;
            const moduleTitle = element.dataset.moduleTitle;

            document.getElementById('moduleIdInput').value = moduleId;
            document.getElementById('moduleTitleInput').value = moduleTitle;
            document.getElementById('moduleModalTitle').innerText = "Edit Module";
            CourseManager.functions.showModal("moduleModal");
        },

        'save-module': function () {
            const moduleId = parseInt(document.getElementById('moduleIdInput').value) || 0;
            const moduleTitle = document.getElementById('moduleTitleInput').value.trim();
            const courseId = parseInt(document.getElementById('courseIdInput').value);

            if (!moduleTitle) {
                CourseManager.functions.showToast("Please enter a module title", "error");
                return;
            }

            const payload = {
                moduleId: moduleId,
                courseId: courseId,
                moduleTitle: moduleTitle
            };

            CourseManager.functions.saveModuleAjax(courseId, payload);
        },

        'open-add-lesson': function (element, event) {
            event.stopPropagation();
            const moduleId = element.dataset.moduleId;
            const courseId = document.getElementById('courseIdInput').value;

            CourseManager.functions.fetchLessonModal(courseId, moduleId, 0);
        },

        'open-edit-lesson': function (element, event) {
            event.stopPropagation();
            const lessonId = element.dataset.lessonId;
            const moduleId = element.dataset.moduleId;
            const courseId = document.getElementById('courseIdInput').value;

            CourseManager.functions.fetchLessonModal(courseId, moduleId, lessonId);
        },

        'select-lesson-type': function (element, event) {
            event.stopPropagation();
            const lessonType = element.dataset.type;
            document.getElementById("contentTypeInput").value = lessonType;

            document.querySelectorAll('.lesson-type-option').forEach(el => el.classList.remove('selected'));
            element.classList.add('selected');

            const videoFields = document.getElementById("videoFields");
            const articleFields = document.getElementById("articleFields");

            if (lessonType === "Video") {
                videoFields.classList.remove("hidden");
                articleFields.classList.add("hidden");
            } else {
                articleFields.classList.remove("hidden");
                videoFields.classList.add("hidden");
            }
        },

        'save-lesson': function () {
            const lessonId = parseInt(document.getElementById('lessonIdInput').value) || 0;
            const moduleId = parseInt(document.getElementById('lessonModuleIdInput').value);
            const courseId = parseInt(document.getElementById('lessonCourseIdInput').value);
            const title = document.getElementById('lessonTitleInput').value.trim();
            const contentType = document.getElementById('contentTypeInput').value;

            if (!title) {
                CourseManager.functions.showToast("Please enter a lesson title", "error");
                return;
            }

            let videoUrl = null;
            let durationMinutes = null;
            let articleContent = null;

            if (contentType === 'Video') {
                videoUrl = document.getElementById('videoUrlInput')?.value || null;
                durationMinutes = parseInt(document.getElementById('durationMinutesInput')?.value) || null;
            } else {
                const editorContent = document.getElementById('articleContentEditor')?.innerHTML;
                const hiddenTextarea = document.getElementById('hiddenArticleContent');
                if (hiddenTextarea && editorContent) {
                    hiddenTextarea.value = editorContent;
                }
                articleContent = editorContent || null;
            }

            const resources = CourseManager.functions.collectResources();

            const payload = {
                lessonId: lessonId,
                moduleId: moduleId,
                courseId: courseId,
                title: title,
                contentType: contentType,
                videoUrl: videoUrl,
                durationMinutes: durationMinutes,
                articleContent: articleContent,
                resources: resources
            };

            CourseManager.functions.saveLessonAjax(courseId, payload);
        },

        'close-modal': function (element) {
            const targetId = element.dataset.target;
            CourseManager.functions.hideModal(targetId);
        },

        'switch-tab': function (element, event) {
            const tabId = element.dataset.tabId;
            CourseManager.functions.activateTab(element, tabId);
        },

        'remove-resource': function (element, event) {
            event.stopPropagation();
            const resourceItem = element.closest('.resource-item');
            if (resourceItem) {
                resourceItem.style.animation = 'slideOut 0.3s ease';
                setTimeout(() => {
                    resourceItem.remove();
                    CourseManager.functions.checkEmptyResources();
                }, 300);
            }
        },

        // Submit for Review actions
        'open-submit-modal': function () {
            CourseManager.functions.showModal("submitModal");
        },

        'confirm-submit': function () {
            CourseManager.functions.submitForReview();
        },

        // Basic Info - Add/Remove Category
        'add-category': function (element, event) {
            const select = element;
            const categoryId = parseInt(select.value);
            if (!categoryId) return;

            const categoryName = select.options[select.selectedIndex].text;

            // Add to state
            if (!CourseManager.state.selectedCategories.find(c => c.id === categoryId)) {
                CourseManager.state.selectedCategories.push({ id: categoryId, name: categoryName });
            }

            // Add tag to UI
            CourseManager.functions.addCategoryTag(categoryId, categoryName);

            // Remove from dropdown
            select.querySelector(`option[value="${categoryId}"]`).remove();

            // Reset dropdown
            select.value = "";

            // Update summary
            CourseManager.functions.updateCategoriesSummary();
        },

        'remove-category': function (element, event) {
            event.stopPropagation();
            const categoryId = parseInt(element.dataset.id);
            const tag = element.closest('.tag');
            const categoryName = tag.textContent.trim();

            // Remove from state
            CourseManager.state.selectedCategories = CourseManager.state.selectedCategories.filter(c => c.id !== categoryId);

            // Remove tag from UI
            tag.remove();

            // Add back to dropdown
            const select = document.getElementById('addCategorySelect');
            const option = document.createElement('option');
            option.value = categoryId;
            option.textContent = categoryName;
            select.appendChild(option);

            // Update summary
            CourseManager.functions.updateCategoriesSummary();
        },

        // Basic Info - Add/Remove Language
        'add-language': function (element, event) {
            const select = element;
            const languageId = parseInt(select.value);
            if (!languageId) return;

            const languageName = select.options[select.selectedIndex].text;

            // Add to state
            if (!CourseManager.state.selectedLanguages.find(l => l.id === languageId)) {
                CourseManager.state.selectedLanguages.push({ id: languageId, name: languageName });
            }

            // Add tag to UI
            CourseManager.functions.addLanguageTag(languageId, languageName);

            // Remove from dropdown
            select.querySelector(`option[value="${languageId}"]`).remove();

            // Reset dropdown
            select.value = "";

            // Update summary
            CourseManager.functions.updateLanguagesSummary();
        },

        'remove-language': function (element, event) {
            event.stopPropagation();
            const languageId = parseInt(element.dataset.id);
            const tag = element.closest('.tag');
            const languageName = tag.textContent.trim();

            // Remove from state
            CourseManager.state.selectedLanguages = CourseManager.state.selectedLanguages.filter(l => l.id !== languageId);

            // Remove tag from UI
            tag.remove();

            // Add back to dropdown
            const select = document.getElementById('addLanguageSelect');
            const option = document.createElement('option');
            option.value = languageId;
            option.textContent = languageName;
            select.appendChild(option);

            // Update summary
            CourseManager.functions.updateLanguagesSummary();
        },

        // Save Basic Info
        'save-basic-info': function () {
            CourseManager.functions.saveBasicInfo();
        },

        // Learning Outcomes - Add
        'add-outcome': function () {
            CourseManager.functions.addLearningOutcome();
        }
    },

    // 3. FUNCTIONS
    functions: {
        showModal: function (modalId) {
            const modal = document.getElementById(modalId);
            if (modal) modal.classList.add("show");
        },

        hideModal: function (modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.classList.remove("show");
                if (modal.dataset.dynamic === "true") {
                    setTimeout(() => modal.remove(), 300);
                }
            }
        },

        activateTab: function (clickedBtn, tabId) {
            document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
            document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));

            clickedBtn.classList.add('active');
            const targetContent = document.getElementById(`tab-${tabId}`);
            if (targetContent) {
                targetContent.classList.add('active');
            }
            CourseManager.state.activeTab = tabId;
        },

        showToast: function (message, type = 'success') {
            const toast = document.getElementById('toast');
            const toastMessage = document.getElementById('toastMessage');
            if (!toast || !toastMessage) return;

            const icon = toast.querySelector('i');
            toastMessage.textContent = message;

            if (icon) {
                icon.className = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle';
            }

            toast.className = `toast ${type} show`;

            setTimeout(() => {
                toast.classList.remove('show');
            }, 3000);
        },

        fetchLessonModal: function (courseId, moduleId, lessonId) {
            const url = `/instructor/manage-course/${courseId}/module/${moduleId}/lesson/${lessonId}/modal`;

            $.ajax({
                url: url,
                type: 'GET',
                success: function (html) {
                    const existingModal = document.getElementById('lessonModal');
                    if (existingModal) existingModal.remove();

                    document.body.insertAdjacentHTML('beforeend', html);

                    const newModal = document.getElementById('lessonModal');
                    if (newModal) {
                        newModal.dataset.dynamic = "true";
                        CourseManager.functions.bindResourceButton();
                    }
                },
                error: function (xhr) {
                    console.error("Failed to load lesson modal:", xhr);
                    CourseManager.functions.showToast("Failed to load lesson details", "error");
                }
            });
        },

        bindResourceButton: function () {
            const addResourceBtn = document.getElementById('addLessonResource');
            if (addResourceBtn) {
                addResourceBtn.addEventListener('click', () => {
                    CourseManager.functions.addResourceRow();
                });
            }
        },

        // ========================================
        // RESOURCE FUNCTIONS
        // ========================================

        addResourceRow: function (resourceData = null) {
            const container = document.getElementById('lessonResourcesList');
            if (!container) return;

            const emptyState = container.querySelector('.empty-resources');
            if (emptyState) emptyState.remove();

            const index = CourseManager.state.resourceCounter++;
            const resId = resourceData?.lessonResourceId || 0;
            const resType = resourceData?.resourceType || 'URL';
            const resTitle = resourceData?.title || '';
            const resUrl = resourceData?.url || '';

            const html = `
                <div class="resource-item" data-resource-id="${resId}" data-index="${index}">
                    <div class="form-group">
                        <label class="form-label">Type</label>
                        <select class="form-select resource-type">
                            <option value="URL" ${resType === 'URL' ? 'selected' : ''}>🔗 URL Link</option>
                            <option value="PDF" ${resType === 'PDF' ? 'selected' : ''}>📄 PDF Document</option>
                            <option value="ZIP" ${resType === 'ZIP' ? 'selected' : ''}>📦 ZIP File</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Title</label>
                        <input type="text" class="form-input resource-title"
                               placeholder="Resource title" value="${resTitle}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">URL</label>
                        <input type="url" class="form-input resource-url"
                               placeholder="https://..." value="${resUrl}" />
                    </div>
                    <button type="button" class="btn-remove-resource" data-action="remove-resource" title="Remove">
                        <i class="fas fa-trash-alt"></i>
                    </button>
                    <input type="hidden" class="resource-id" value="${resId}" />
                </div>
            `;

            container.insertAdjacentHTML('beforeend', html);
        },

        checkEmptyResources: function () {
            const container = document.getElementById('lessonResourcesList');
            if (!container) return;

            const items = container.querySelectorAll('.resource-item');
            if (items.length === 0) {
                container.innerHTML = `
                    <div class="empty-resources">
                        <i class="fas fa-link"></i>
                        <p>No resources added. Click "Add Resource" to add one.</p>
                    </div>
                `;
            }
        },

        collectResources: function () {
            const resources = [];
            const items = document.querySelectorAll('#lessonResourcesList .resource-item');

            items.forEach(item => {
                const resId = parseInt(item.querySelector('.resource-id')?.value) || 0;
                const resType = item.querySelector('.resource-type')?.value || 'URL';
                const resTitle = item.querySelector('.resource-title')?.value?.trim() || '';
                const resUrl = item.querySelector('.resource-url')?.value?.trim() || '';

                if (resUrl) {
                    resources.push({
                        lessonResourceId: resId,
                        resourceType: resType,
                        title: resTitle || 'Resource',
                        url: resUrl
                    });
                }
            });

            return resources;
        },

        // ========================================
        // MODULE/LESSON AJAX FUNCTIONS
        // ========================================

        saveModuleAjax: function (courseId, payload) {
            const isNewModule = payload.moduleId === 0;
            
            $.ajax({
                url: `/instructor/manage-course/${courseId}/module`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Module saved!");
                        CourseManager.functions.hideModal("moduleModal");
                        
                        if (isNewModule) {
                            CourseManager.functions.addModuleToUI(response.moduleId, payload.moduleTitle);
                        } else {
                            CourseManager.functions.updateModuleInUI(payload.moduleId, payload.moduleTitle);
                        }
                        
                        CourseManager.functions.updateCurriculumStats();
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to save module", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred", "error");
                }
            });
        },

        saveLessonAjax: function (courseId, payload) {
            const isNewLesson = payload.lessonId === 0;
            
            $.ajax({
                url: `/instructor/manage-course/${courseId}/lesson`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Lesson saved!");
                        CourseManager.functions.hideModal("lessonModal");
                        
                        if (isNewLesson) {
                            CourseManager.functions.addLessonToUI(
                                response.lessonId, 
                                payload.moduleId, 
                                payload.title, 
                                payload.contentType,
                                payload.durationMinutes
                            );
                        } else {
                            CourseManager.functions.updateLessonInUI(
                                payload.lessonId, 
                                payload.moduleId, 
                                payload.title, 
                                payload.contentType,
                                payload.durationMinutes
                            );
                        }
                        
                        CourseManager.functions.updateCurriculumStats();
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to save lesson", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred", "error");
                }
            });
        },

        submitForReview: function () {
            const courseId = CourseManager.state.courseId || window.courseData?.courseId;

            if (!courseId) {
                CourseManager.functions.showToast("Course ID not found", "error");
                return;
            }

            $.ajax({
                url: `/instructor/manage-course/${courseId}/submit-for-review`,
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Course submitted for review!");
                        CourseManager.functions.hideModal("submitModal");
                        setTimeout(() => location.reload(), 1500);
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to submit course", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred while submitting", "error");
                }
            });
        },

        // ========================================
        // UI UPDATE FUNCTIONS (No Page Refresh)
        // ========================================

        addModuleToUI: function (moduleId, moduleTitle) {
            const modulesList = document.getElementById('modulesList');
            if (!modulesList) return;

            const emptyState = modulesList.querySelector('.empty-state');
            if (emptyState) emptyState.remove();

            const existingModules = modulesList.querySelectorAll('.module-item');
            const moduleOrder = existingModules.length + 1;

            const html = `
                <div class="module-item" data-module-id="${moduleId}" draggable="true">
                    <div class="module-header" data-action="toggle-module">
                        <div class="module-header-left">
                            <div class="module-drag" draggable="true" onmousedown="event.stopPropagation()">
                                <i class="fas fa-grip-vertical"></i>
                            </div>
                            <div class="module-expand">
                                <i class="fas fa-chevron-right"></i>
                            </div>
                            <div class="module-info">
                                <div class="module-title">Module ${moduleOrder}: <span class="module-title-text">${CourseManager.functions.escapeHtml(moduleTitle)}</span></div>
                                <div class="module-meta">0 lessons • ---</div>
                            </div>
                        </div>
                        <div class="module-actions">
                            <button title="Edit"
                                    data-action="open-edit-module"
                                    data-module-id="${moduleId}"
                                    data-module-title="${CourseManager.functions.escapeHtml(moduleTitle)}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button title="Add Lesson"
                                    data-action="open-add-lesson"
                                    data-module-id="${moduleId}">
                                <i class="fas fa-plus"></i>
                            </button>
                            <button class="delete" title="Delete" onclick="deleteModule(${moduleId})">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                    <div class="lessons-list" data-module-id="${moduleId}">
                        <button class="add-lesson-btn"
                                data-action="open-add-lesson"
                                data-module-id="${moduleId}">
                            <i class="fas fa-plus"></i> Add Lesson
                        </button>
                    </div>
                </div>
            `;

            const addModuleBtn = modulesList.querySelector('.add-module-btn');
            if (addModuleBtn) {
                addModuleBtn.insertAdjacentHTML('beforebegin', html);
            } else {
                modulesList.insertAdjacentHTML('beforeend', html);
            }

            const newModule = modulesList.querySelector(`.module-item[data-module-id="${moduleId}"]`);
            if (newModule) {
                newModule.style.animation = 'slideIn 0.3s ease';
            }
        },

        updateModuleInUI: function (moduleId, moduleTitle) {
            const moduleItem = document.querySelector(`.module-item[data-module-id="${moduleId}"]`);
            if (!moduleItem) return;

            const titleSpan = moduleItem.querySelector('.module-title-text') || moduleItem.querySelector('.module-info .module-title span');
            if (titleSpan) {
                titleSpan.textContent = moduleTitle;
            }

            const editBtn = moduleItem.querySelector('[data-action="open-edit-module"]');
            if (editBtn) {
                editBtn.dataset.moduleTitle = moduleTitle;
            }
        },

        addLessonToUI: function (lessonId, moduleId, lessonTitle, contentType, durationMinutes) {
            const lessonsList = document.querySelector(`.lessons-list[data-module-id="${moduleId}"]`);
            if (!lessonsList) return;

            const iconClass = contentType === 'Video' ? 'play-circle' : 'file-alt';
            const formattedDuration = CourseManager.functions.formatDuration(durationMinutes);

            const html = `
                <div class="lesson-item" data-lesson-id="${lessonId}" data-module-id="${moduleId}" draggable="true">
                    <div class="lesson-drag"><i class="fas fa-grip-vertical"></i></div>
                    <div class="lesson-icon ${contentType.toLowerCase()}">
                        <i class="fas fa-${iconClass}"></i>
                    </div>
                    <div class="lesson-info">
                        <div class="lesson-title">${CourseManager.functions.escapeHtml(lessonTitle)}</div>
                        <div class="lesson-meta">${contentType} • ${formattedDuration}</div>
                    </div>
                    <div class="lesson-actions">
                        <button title="Edit"
                                data-action="open-edit-lesson"
                                data-module-id="${moduleId}"
                                data-lesson-id="${lessonId}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="delete" title="Delete" onclick="deleteLesson(${moduleId}, ${lessonId})">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            `;

            const addLessonBtn = lessonsList.querySelector('.add-lesson-btn');
            if (addLessonBtn) {
                addLessonBtn.insertAdjacentHTML('beforebegin', html);
            } else {
                lessonsList.insertAdjacentHTML('beforeend', html);
            }

            const newLesson = lessonsList.querySelector(`.lesson-item[data-lesson-id="${lessonId}"]`);
            if (newLesson) {
                newLesson.style.animation = 'slideIn 0.3s ease';
            }

            CourseManager.functions.updateModuleMeta(moduleId);
        },

        updateLessonInUI: function (lessonId, moduleId, lessonTitle, contentType, durationMinutes) {
            const lessonItem = document.querySelector(`.lesson-item[data-lesson-id="${lessonId}"]`);
            if (!lessonItem) return;

            const titleEl = lessonItem.querySelector('.lesson-title');
            if (titleEl) titleEl.textContent = lessonTitle;

            const iconClass = contentType === 'Video' ? 'play-circle' : 'file-alt';
            const iconContainer = lessonItem.querySelector('.lesson-icon');
            if (iconContainer) {
                iconContainer.className = `lesson-icon ${contentType.toLowerCase()}`;
                iconContainer.innerHTML = `<i class="fas fa-${iconClass}"></i>`;
            }

            const formattedDuration = CourseManager.functions.formatDuration(durationMinutes);
            const metaEl = lessonItem.querySelector('.lesson-meta');
            if (metaEl) metaEl.textContent = `${contentType} • ${formattedDuration}`;
        },

        updateModuleMeta: function (moduleId) {
            const moduleItem = document.querySelector(`.module-item[data-module-id="${moduleId}"]`);
            if (!moduleItem) return;

            const lessonsList = moduleItem.querySelector('.lessons-list');
            const lessonCount = lessonsList ? lessonsList.querySelectorAll('.lesson-item').length : 0;

            const metaEl = moduleItem.querySelector('.module-meta');
            if (metaEl) {
                metaEl.textContent = `${lessonCount} lesson${lessonCount !== 1 ? 's' : ''} • ---`;
            }
        },

        updateCurriculumStats: function () {
            const modulesList = document.getElementById('modulesList');
            if (!modulesList) return;

            const modules = modulesList.querySelectorAll('.module-item');
            const lessons = modulesList.querySelectorAll('.lesson-item');
            const videoLessons = modulesList.querySelectorAll('.lesson-icon.video');
            const articleLessons = modulesList.querySelectorAll('.lesson-icon.article');

            const statModules = document.getElementById('statModules');
            const statVideos = document.getElementById('statVideos');
            const statArticles = document.getElementById('statArticles');
            const lessonCount = document.getElementById('lessonCount');

            if (statModules) statModules.textContent = modules.length;
            if (statVideos) statVideos.textContent = videoLessons.length;
            if (statArticles) statArticles.textContent = articleLessons.length;
            if (lessonCount) lessonCount.textContent = `${lessons.length} lessons`;
        },

        removeModuleFromUI: function (moduleId) {
            const moduleItem = document.querySelector(`.module-item[data-module-id="${moduleId}"]`);
            if (!moduleItem) return;

            moduleItem.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => {
                moduleItem.remove();
                CourseManager.functions.updateCurriculumStats();
                CourseManager.functions.checkEmptyModules();
            }, 300);
        },

        removeLessonFromUI: function (lessonId, moduleId) {
            const lessonItem = document.querySelector(`.lesson-item[data-lesson-id="${lessonId}"]`);
            if (!lessonItem) return;

            lessonItem.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => {
                lessonItem.remove();
                CourseManager.functions.updateModuleMeta(moduleId);
                CourseManager.functions.updateCurriculumStats();
            }, 300);
        },

        checkEmptyModules: function () {
            const modulesList = document.getElementById('modulesList');
            if (!modulesList) return;

            const modules = modulesList.querySelectorAll('.module-item');
            if (modules.length === 0) {
                const addBtn = modulesList.querySelector('.add-module-btn');
                const emptyHtml = `
                    <div class="empty-state">
                        <i class="fas fa-layer-group"></i>
                        <p>No modules yet. Add your first module!</p>
                    </div>
                `;
                if (addBtn) {
                    addBtn.insertAdjacentHTML('beforebegin', emptyHtml);
                } else {
                    modulesList.insertAdjacentHTML('afterbegin', emptyHtml);
                }
            }
        },

        formatDuration: function (minutes) {
            if (!minutes || minutes <= 0) return '---';
            
            const h = Math.floor(minutes / 60);
            const m = minutes % 60;
            const parts = [];

            if (h > 0) parts.push(`${h} ${h === 1 ? 'hr' : 'hrs'}`);
            if (m > 0) parts.push(`${m} ${m === 1 ? 'min' : 'mins'}`);

            return parts.length > 0 ? parts.join(' ') : '---';
        },

        escapeHtml: function (text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        },

        // ========================================
        // CATEGORY/LANGUAGE TAG FUNCTIONS
        // ========================================

        addCategoryTag: function (categoryId, categoryName) {
            const container = document.getElementById('selectedCategories');
            if (!container) return;

            const html = `
                <span class="tag" data-id="${categoryId}">
                    ${categoryName}
                    <button type="button" class="tag-remove" data-action="remove-category" data-id="${categoryId}">
                        <i class="fas fa-times"></i>
                    </button>
                </span>
            `;
            container.insertAdjacentHTML('beforeend', html);
        },

        addLanguageTag: function (languageId, languageName) {
            const container = document.getElementById('selectedLanguages');
            if (!container) return;

            const html = `
                <span class="tag" data-id="${languageId}">
                    ${languageName}
                    <button type="button" class="tag-remove" data-action="remove-language" data-id="${languageId}">
                        <i class="fas fa-times"></i>
                    </button>
                </span>
            `;
            container.insertAdjacentHTML('beforeend', html);
        },

        updateCategoriesSummary: function () {
            const summaryEl = document.getElementById('infoCategories');
            if (!summaryEl) return;

            if (CourseManager.state.selectedCategories.length > 0) {
                summaryEl.textContent = CourseManager.state.selectedCategories.map(c => c.name).join(', ');
            } else {
                summaryEl.textContent = 'Uncategorized';
            }
        },

        updateLanguagesSummary: function () {
            const summaryEl = document.getElementById('infoLanguages');
            if (!summaryEl) return;

            if (CourseManager.state.selectedLanguages.length > 0) {
                summaryEl.textContent = CourseManager.state.selectedLanguages.map(l => l.name).join(', ');
            } else {
                summaryEl.textContent = 'N/A';
            }
        },

        // ========================================
        // BASIC INFO FUNCTIONS
        // ========================================

        saveBasicInfo: function () {
            const courseId = CourseManager.state.courseId || window.courseData?.courseId;

            if (!courseId) {
                CourseManager.functions.showToast("Course ID not found", "error");
                return;
            }

            const courseTitle = document.getElementById('courseTitleInput')?.value?.trim() || '';
            const courseDescription = document.getElementById('courseDescriptionInput')?.value?.trim() || '';
            const level = document.getElementById('courseLevelSelect')?.value || 'Beginner';
            const thumbnailImageUrl = document.getElementById('thumbnailUrlInput')?.value || null;

            const categoryIds = [];
            document.querySelectorAll('#selectedCategories .tag').forEach(tag => {
                const id = parseInt(tag.dataset.id);
                if (id) categoryIds.push(id);
            });

            const languageIds = [];
            document.querySelectorAll('#selectedLanguages .tag').forEach(tag => {
                const id = parseInt(tag.dataset.id);
                if (id) languageIds.push(id);
            });

            if (!courseTitle) {
                CourseManager.functions.showToast("Course title is required", "error");
                return;
            }

            if (categoryIds.length === 0) {
                CourseManager.functions.showToast("Please select at least one category", "error");
                return;
            }

            if (languageIds.length === 0) {
                CourseManager.functions.showToast("Please select at least one language", "error");
                return;
            }

            const payload = {
                courseId: courseId,
                courseTitle: courseTitle,
                courseDescription: courseDescription,
                thumbnailImageUrl: thumbnailImageUrl,
                level: level,
                categoryIds: categoryIds,
                languageIds: languageIds
            };

            $.ajax({
                url: `/instructor/manage-course/${courseId}/basic-info`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Basic info saved!");

                        // Update page header with new title
                        const headerTitle = document.getElementById('courseTitleHeader');
                        const breadcrumbTitle = document.getElementById('courseTitleBreadcrumb');
                        if (headerTitle) headerTitle.textContent = courseTitle;
                        if (breadcrumbTitle) breadcrumbTitle.textContent = courseTitle;

                        // Update level summary
                        const levelSummary = document.getElementById('infoLevel');
                        if (levelSummary) levelSummary.textContent = level;

                        // Update thumbnail summary
                        const thumbnailSummary = document.getElementById('infoThumbnail');
                        if (thumbnailSummary) {
                            thumbnailSummary.textContent = thumbnailImageUrl ? '✓ Uploaded' : 'Not set';
                        }
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to save basic info", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred while saving", "error");
                }
            });
        },

        // ========================================
        // THUMBNAIL UPLOAD FUNCTIONS
        // ========================================

        triggerThumbnailUpload: function () {
            const fileInput = document.getElementById('thumbnailFileInput');
            if (fileInput) {
                fileInput.click();
            }
        },

        initThumbnailUpload: function () {
            const fileInput = document.getElementById('thumbnailFileInput');
            const previewContainer = document.getElementById('thumbnailPreview');
            const placeholder = document.getElementById('thumbnailPlaceholder');

            if (!fileInput) return;

            // Click on preview to trigger upload
            if (previewContainer) {
                previewContainer.addEventListener('click', (e) => {
                    // Don't trigger if clicking on action buttons
                    if (e.target.closest('.btn-thumbnail-action')) return;
                    
                    // Only trigger if there's no image yet
                    if (placeholder || !previewContainer.querySelector('img')) {
                        CourseManager.functions.triggerThumbnailUpload();
                    }
                });
            }

            // Handle file selection
            fileInput.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    CourseManager.functions.uploadThumbnail(file);
                }
            });
        },

        uploadThumbnail: function (file) {
            const previewContainer = document.getElementById('thumbnailPreview');
            const thumbnailUrlInput = document.getElementById('thumbnailUrlInput');

            // Validate file type
            const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
            if (!allowedTypes.includes(file.type)) {
                CourseManager.functions.showToast("Invalid file type. Please use JPG, PNG, or WebP.", "error");
                return;
            }

            // Validate file size (5MB max)
            if (file.size > 5 * 1024 * 1024) {
                CourseManager.functions.showToast("File size exceeds 5MB limit.", "error");
                return;
            }

            // Show uploading state
            previewContainer.classList.add('uploading');

            // Create FormData
            const formData = new FormData();
            formData.append('file', file);

            $.ajax({
                url: '/instructor/upload-course-thumbnail',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    previewContainer.classList.remove('uploading');

                    if (response.success) {
                        // Update hidden input
                        if (thumbnailUrlInput) {
                            thumbnailUrlInput.value = response.url;
                        }

                        // Update preview
                        CourseManager.functions.updateThumbnailPreview(response.url);

                        CourseManager.functions.showToast("Thumbnail uploaded successfully!");
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to upload thumbnail", "error");
                    }
                },
                error: function (xhr, status, error) {
                    previewContainer.classList.remove('uploading');
                    console.error("Upload error:", status, error);
                    CourseManager.functions.showToast("An error occurred while uploading: " + error, "error");
                }
            });
        },

        updateThumbnailPreview: function (imageUrl) {
            const previewContainer = document.getElementById('thumbnailPreview');
            if (!previewContainer) return;

            previewContainer.innerHTML = `
                <img src="${imageUrl}" alt="Course Thumbnail" id="thumbnailImage" />
                <div class="thumbnail-overlay">
                    <button type="button" class="btn-thumbnail-action" onclick="CourseManager.functions.triggerThumbnailUpload()">
                        <i class="fas fa-camera"></i> Change
                    </button>
                    <button type="button" class="btn-thumbnail-action btn-remove" onclick="CourseManager.functions.removeThumbnail()">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            `;
            previewContainer.classList.add('has-image');

            // Update summary
            const thumbnailSummary = document.getElementById('infoThumbnail');
            if (thumbnailSummary) {
                thumbnailSummary.textContent = '✓ Uploaded';
            }
        },

        removeThumbnail: function () {
            const previewContainer = document.getElementById('thumbnailPreview');
            const thumbnailUrlInput = document.getElementById('thumbnailUrlInput');
            const fileInput = document.getElementById('thumbnailFileInput');

            if (thumbnailUrlInput) {
                thumbnailUrlInput.value = '';
            }

            if (fileInput) {
                fileInput.value = '';
            }

            if (previewContainer) {
                previewContainer.innerHTML = `
                    <div class="thumbnail-placeholder" id="thumbnailPlaceholder">
                        <i class="fas fa-image"></i>
                        <span>Click to upload thumbnail</span>
                        <span class="thumbnail-hint">Recommended: 1280x720px (16:9)</span>
                    </div>
                `;
                previewContainer.classList.remove('has-image');
            }

            // Update summary
            const thumbnailSummary = document.getElementById('infoThumbnail');
            if (thumbnailSummary) {
                thumbnailSummary.textContent = 'Not set';
            }

            CourseManager.functions.showToast("Thumbnail removed");
        },

        initBasicInfoState: function () {
            document.querySelectorAll('#selectedCategories .tag').forEach(tag => {
                const id = parseInt(tag.dataset.id);
                const name = tag.textContent.trim();
                if (id && name) {
                    CourseManager.state.selectedCategories.push({ id: id, name: name });
                }
            });

            document.querySelectorAll('#selectedLanguages .tag').forEach(tag => {
                const id = parseInt(tag.dataset.id);
                const name = tag.textContent.trim();
                if (id && name) {
                    CourseManager.state.selectedLanguages.push({ id: id, name: name });
                }
            });

            // Initialize thumbnail upload
            CourseManager.functions.initThumbnailUpload();
        },

        // ========================================
        // LEARNING OUTCOMES FUNCTIONS
        // ========================================

        addLearningOutcome: function () {
            const input = document.getElementById('newOutcome');
            if (!input) return;

            const value = input.value.trim();
            if (!value) {
                CourseManager.functions.showToast("Please enter a learning outcome", "error");
                return;
            }

            const courseId = CourseManager.state.courseId || window.courseData?.courseId;
            if (!courseId) {
                CourseManager.functions.showToast("Course ID not found", "error");
                return;
            }

            const payload = {
                learningOutcomeId: 0,
                courseId: courseId,
                outcomeName: value
            };

            $.ajax({
                url: `/instructor/manage-course/${courseId}/learning-outcome`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Learning outcome added!");
                        CourseManager.functions.addOutcomeToUI(response.outcomeId, value);
                        input.value = '';
                        CourseManager.functions.updateOutcomeCount();
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to add outcome", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred", "error");
                }
            });
        },

        addOutcomeToUI: function (outcomeId, outcomeName) {
            const outcomesList = document.getElementById('outcomesList');
            if (!outcomesList) return;

            const html = `
                <div class="outcome-item" draggable="true" data-outcome-id="${outcomeId}">
                    <div class="outcome-icon"><i class="fas fa-check"></i></div>
                    <span class="outcome-text">${outcomeName}</span>
                    <div class="outcome-actions">
                        <button onclick="editOutcome(${outcomeId})" title="Edit"><i class="fas fa-edit"></i></button>
                        <button class="delete" onclick="deleteOutcome(${outcomeId})" title="Delete"><i class="fas fa-trash"></i></button>
                    </div>
                </div>
            `;
            outcomesList.insertAdjacentHTML('beforeend', html);
        },

        updateOutcomeCount: function () {
            const countEl = document.getElementById('outcomeCount');
            if (!countEl) return;

            const count = document.querySelectorAll('#outcomesList .outcome-item').length;
            countEl.textContent = `${count} outcome${count !== 1 ? 's' : ''}`;
        },

        saveLearningOutcome: function (outcomeId, outcomeName) {
            const courseId = CourseManager.state.courseId || window.courseData?.courseId;
            if (!courseId) {
                CourseManager.functions.showToast("Course ID not found", "error");
                return;
            }

            const payload = {
                learningOutcomeId: outcomeId,
                courseId: courseId,
                outcomeName: outcomeName
            };

            $.ajax({
                url: `/instructor/manage-course/${courseId}/learning-outcome`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Learning outcome saved!");
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to save outcome", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred", "error");
                }
            });
        },

        deleteLearningOutcome: function (outcomeId) {
            const courseId = CourseManager.state.courseId || window.courseData?.courseId;
            if (!courseId) {
                CourseManager.functions.showToast("Course ID not found", "error");
                return;
            }

            $.ajax({
                url: `/instructor/manage-course/${courseId}/learning-outcome/${outcomeId}`,
                type: 'DELETE',
                success: function (response) {
                    if (response.success) {
                        CourseManager.functions.showToast(response.message || "Learning outcome deleted!");
                        const item = document.querySelector(`.outcome-item[data-outcome-id="${outcomeId}"]`);
                        if (item) {
                            item.style.animation = 'slideOut 0.3s ease';
                            setTimeout(() => {
                                item.remove();
                                CourseManager.functions.updateOutcomeCount();
                            }, 300);
                        }
                    } else {
                        CourseManager.functions.showToast(response.message || "Failed to delete outcome", "error");
                    }
                },
                error: function () {
                    CourseManager.functions.showToast("An error occurred", "error");
                }
            });
        }
    },

    // 4. ROUTER
    bindEvents: function () {
        document.addEventListener("click", (e) => {
            const trigger = e.target.closest("[data-action]");
            if (trigger) {
                const actionName = trigger.dataset.action;
                if (this.actions[actionName]) {
                    this.actions[actionName](trigger, e);
                } else {
                    console.warn(`Action '${actionName}' not defined.`);
                }
            }
        });

        document.addEventListener("change", (e) => {
            const trigger = e.target.closest("[data-action]");
            if (trigger) {
                const actionName = trigger.dataset.action;
                if (this.actions[actionName]) {
                    this.actions[actionName](trigger, e);
                }
            }
        });
    },

    // 5. INIT
    init: function () {
        console.log("CourseManager Initializing...");
        this.bindEvents();

        if (window.courseData) {
            this.state.courseId = window.courseData.courseId;
            this.state.courseStatus = window.courseData.courseStatus;
        }

        const courseIdInput = document.getElementById('courseIdInput');
        if (courseIdInput && !this.state.courseId) {
            this.state.courseId = parseInt(courseIdInput.value) || 0;
        }

        this.functions.initBasicInfoState();
    }
};

// ========================================
// GLOBAL LEARNING OUTCOMES FUNCTIONS
// ========================================

function addOutcome() {
    CourseManager.functions.addLearningOutcome();
}

function editOutcome(outcomeId) {
    const outcomeItem = document.querySelector(`.outcome-item[data-outcome-id="${outcomeId}"]`);
    if (!outcomeItem) return;

    const outcomeText = outcomeItem.querySelector('.outcome-text');
    if (!outcomeText) return;

    const currentValue = outcomeText.textContent.trim();
    const newValue = prompt("Edit learning outcome:", currentValue);

    if (newValue !== null && newValue.trim() !== '' && newValue.trim() !== currentValue) {
        CourseManager.functions.saveLearningOutcome(outcomeId, newValue.trim());
        outcomeText.textContent = newValue.trim();
    }
}

function deleteOutcome(outcomeId) {
    if (!confirm("Are you sure you want to delete this learning outcome?")) return;
    CourseManager.functions.deleteLearningOutcome(outcomeId);
}

function addExampleOutcome(text) {
    const input = document.getElementById('newOutcome');
    if (input) {
        input.value = text;
        CourseManager.functions.addLearningOutcome();
    }
}

// Global delete functions
function deleteModule(moduleId) {
    if (!confirm("Are you sure you want to delete this module and all its lessons?")) return;

    const courseId = CourseManager.state.courseId || document.getElementById('courseIdInput')?.value;

    $.ajax({
        url: `/instructor/manage-course/${courseId}/module/${moduleId}`,
        type: 'DELETE',
        success: function (response) {
            if (response.success) {
                CourseManager.functions.showToast("Module deleted!");
                CourseManager.functions.removeModuleFromUI(moduleId);
            } else {
                CourseManager.functions.showToast(response.message || "Failed to delete", "error");
            }
        },
        error: function () {
            CourseManager.functions.showToast("An error occurred", "error");
        }
    });
}

function deleteLesson(moduleId, lessonId) {
    if (!confirm("Are you sure you want to delete this lesson?")) return;

    const courseId = CourseManager.state.courseId || document.getElementById('courseIdInput')?.value;

    $.ajax({
        url: `/instructor/manage-course/${courseId}/lesson/${lessonId}`,
        type: 'DELETE',
        success: function (response) {
            if (response.success) {
                CourseManager.functions.showToast("Lesson deleted!");
                CourseManager.functions.removeLessonFromUI(lessonId, moduleId);
            } else {
                CourseManager.functions.showToast(response.message || "Failed to delete", "error");
            }
        },
        error: function () {
            CourseManager.functions.showToast("An error occurred", "error");
        }
    });
}

// Initialize
document.addEventListener("DOMContentLoaded", () => {
    CourseManager.init();
});