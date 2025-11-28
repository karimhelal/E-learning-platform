// ========================================
// COURSE DETAILS PAGE - JavaScript
// Optimized for .NET MVC
// ========================================

(function() {
    'use strict';

    // ========================================
    // TABS FUNCTIONALITY
    // ========================================
    
    function initTabs() {
        const tabBtns = document.querySelectorAll('.tab-btn');
        const tabContents = document.querySelectorAll('.tab-content');

        tabBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const targetTab = this.getAttribute('data-tab');
                
                tabBtns.forEach(b => b.classList.remove('active'));
                tabContents.forEach(c => c.classList.remove('active'));
                
                this.classList.add('active');
                const targetContent = document.getElementById(targetTab);
                if (targetContent) {
                    targetContent.classList.add('active');
                }
            });
        });
    }

    // ========================================
    // MODULE ACCORDION
    // ========================================
    
    function initModuleAccordion() {
        const moduleHeaders = document.querySelectorAll('.module-header');
        
        moduleHeaders.forEach(header => {
            header.addEventListener('click', function(e) {
                if (e.target.closest('.lesson-status')) {
                    return;
                }

                const moduleItem = this.closest('.module-item');
                const wasOpen = moduleItem.classList.contains('open');
                
                document.querySelectorAll('.module-item').forEach(item => {
                    item.classList.remove('open');
                });
                
                if (!wasOpen) {
                    moduleItem.classList.add('open');
                }
            });
        });
    }

    // ========================================
    // LESSON COMPLETION TOGGLE
    // ========================================
    
    function initLessonCompletion() {
        document.addEventListener('click', function(e) {
            const statusBtn = e.target.closest('.lesson-status');
            if (!statusBtn) return;

            e.stopPropagation();
            
            const lessonItem = statusBtn.closest('.lesson-item');
            const lessonId = lessonItem.dataset.lessonId;
            const wasCompleted = statusBtn.classList.contains('completed');
            
            // Toggle UI
            if (wasCompleted) {
                statusBtn.classList.remove('completed');
                statusBtn.innerHTML = '';
            } else {
                statusBtn.classList.add('completed');
                statusBtn.innerHTML = '<i class="fas fa-check"></i>';
            }
            
            // Update module progress
            const moduleItem = lessonItem.closest('.module-item');
            updateModuleProgress(moduleItem);
            
            // Update overall course progress
            updateCourseProgress();
            
            // Submit to server using form (MVC style)
            submitLessonProgress(lessonId, !wasCompleted);
        });
    }

    // ========================================
    // UPDATE MODULE PROGRESS
    // ========================================
    
    function updateModuleProgress(moduleItem) {
        if (!moduleItem) return;

        const allLessons = moduleItem.querySelectorAll('.lesson-status');
        const completedLessons = moduleItem.querySelectorAll('.lesson-status.completed');
        
        const total = allLessons.length;
        const completed = completedLessons.length;
        const percentage = total > 0 ? (completed / total) * 100 : 0;
        
        const progressBar = moduleItem.querySelector('.module-progress-fill');
        if (progressBar) {
            progressBar.style.width = percentage + '%';
            
            if (percentage === 100) {
                progressBar.classList.add('completed');
            } else {
                progressBar.classList.remove('completed');
            }
        }
        
        const progressText = moduleItem.querySelector('.module-progress-text');
        if (progressText) {
            progressText.textContent = `${completed}/${total} Completed`;
        }
        
        updateModuleActions(moduleItem, percentage);
    }

    // ========================================
    // UPDATE MODULE ACTIONS
    // ========================================
    
    function updateModuleActions(moduleItem, percentage) {
        const moduleActions = moduleItem.querySelector('.module-actions');
        if (!moduleActions) return;

        const moduleNumber = moduleItem.dataset.moduleId || 
                           moduleItem.querySelector('.module-number')?.textContent || '1';
        
        if (percentage === 100) {
            moduleActions.innerHTML = `
                <div style="padding: 1.5rem; text-align: center; background: rgba(16, 185, 129, 0.1); border-radius: 0 0 16px 16px;">
                    <i class="fas fa-check-circle" style="color: var(--accent-green); font-size: 2rem; margin-bottom: 0.5rem;"></i>
                    <p style="color: var(--accent-green); font-weight: 600; font-size: 1.1rem; margin-bottom: 0.3rem;">Module Completed!</p>
                    <p style="color: var(--text-secondary); font-size: 0.9rem;">Great job! You've finished all lessons in this module.</p>
                </div>
            `;
        } else if (percentage > 0) {
            moduleActions.innerHTML = `
                <a href="/Student/Lesson/Continue?moduleId=${moduleNumber}" class="btn btn-primary btn-small">
                    <span>Continue Module</span>
                    <i class="fas fa-arrow-right"></i>
                </a>
            `;
        } else {
            moduleActions.innerHTML = `
                <a href="/Student/Lesson/Start?moduleId=${moduleNumber}" class="btn btn-primary btn-small">
                    <span>Start Module</span>
                    <i class="fas fa-play"></i>
                </a>
            `;
        }
    }

    // ========================================
    // UPDATE OVERALL COURSE PROGRESS
    // ========================================
    
    function updateCourseProgress() {
        const allLessons = document.querySelectorAll('.lesson-status');
        const completedLessons = document.querySelectorAll('.lesson-status.completed');
        
        const total = allLessons.length;
        const completed = completedLessons.length;
        const percentage = total > 0 ? Math.round((completed / total) * 100) : 0;
        
        const courseProgressBar = document.getElementById('courseProgressBar');
        if (courseProgressBar) {
            courseProgressBar.style.width = percentage + '%';
        }
        
        const courseProgressPercent = document.getElementById('courseProgressPercent');
        if (courseProgressPercent) {
            courseProgressPercent.textContent = percentage + '%';
        }
    }

    // ========================================
    // SUBMIT LESSON PROGRESS TO MVC
    // ========================================
    
    function submitLessonProgress(lessonId, isCompleted) {
        // Create hidden form and submit (MVC style)
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Student/Lesson/ToggleComplete';
        form.style.display = 'none';

        // Add anti-forgery token if available
        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
        if (antiForgeryToken) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = antiForgeryToken.value;
            form.appendChild(tokenInput);
        }

        // Add lesson data
        const lessonInput = document.createElement('input');
        lessonInput.type = 'hidden';
        lessonInput.name = 'lessonId';
        lessonInput.value = lessonId;
        form.appendChild(lessonInput);

        const completedInput = document.createElement('input');
        completedInput.type = 'hidden';
        completedInput.name = 'isCompleted';
        completedInput.value = isCompleted;
        form.appendChild(completedInput);

        // Submit form via AJAX to avoid page reload
        document.body.appendChild(form);
        
        const formData = new FormData(form);
        
        fetch(form.action, {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (response.ok) {
                console.log('Progress saved successfully');
            } else {
                console.error('Failed to save progress');
                Masar.showNotification('Failed to save progress', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
        })
        .finally(() => {
            form.remove();
        });
    }

    // ========================================
    // INITIALIZE ON PAGE LOAD
    // ========================================
    
    function init() {
        initTabs();
        initModuleAccordion();
        initLessonCompletion();
        
        document.querySelectorAll('.module-item').forEach(module => {
            updateModuleProgress(module);
        });
        
        updateCourseProgress();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();