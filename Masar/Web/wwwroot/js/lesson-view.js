// ========================================
// LESSON VIEW PAGE - JavaScript
// Optimized for .NET MVC
// ========================================

(function() {
    'use strict';

    // ========================================
    // NOTES PANEL TOGGLE
    // ========================================
    
    const toggleNotesBtn = document.getElementById('toggleNotesBtn');
    const closeNotesBtn = document.getElementById('closeNotesBtn');
    const notesPanel = document.getElementById('notesPanel');

    if (toggleNotesBtn) {
        toggleNotesBtn.addEventListener('click', () => {
            notesPanel.classList.add('active');
        });
    }

    if (closeNotesBtn) {
        closeNotesBtn.addEventListener('click', () => {
            notesPanel.classList.remove('active');
        });
    }

    // Close notes panel on ESC
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && notesPanel.classList.contains('active')) {
            notesPanel.classList.remove('active');
        }
    });

    // ========================================
    // LESSON COMPLETE TOGGLE
    // ========================================
    
    const lessonCompleteBtn = document.getElementById('lessonCompleteBtn');
    
    if (lessonCompleteBtn) {
        lessonCompleteBtn.addEventListener('click', function() {
            const isCompleted = this.classList.contains('completed');
            
            if (isCompleted) {
                this.classList.remove('completed');
                this.innerHTML = '<i class="fas fa-check"></i><span>Mark Complete</span>';
            } else {
                this.classList.add('completed');
                this.innerHTML = '<i class="fas fa-undo"></i><span>Mark Incomplete</span>';
                Masar.showNotification('Lesson marked as complete!', 'success');
            }
            
            // Submit to server
            submitLessonCompletion(!isCompleted);
        });
    }

    function submitLessonCompletion(isCompleted) {
        // Get lesson ID from URL or data attribute
        const urlParams = new URLSearchParams(window.location.search);
        const lessonId = urlParams.get('lessonId') || '201';
        
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Student/Lesson/ToggleComplete';
        form.style.display = 'none';

        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
        if (antiForgeryToken) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = antiForgeryToken.value;
            form.appendChild(tokenInput);
        }

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

        document.body.appendChild(form);
        
        const formData = new FormData(form);
        
        fetch(form.action, {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (response.ok) {
                console.log('Progress saved successfully');
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
    // VIDEO PLAYER CONTROLS (Basic)
    // ========================================
    
    const playPauseBtn = document.getElementById('playPauseBtn');
    
    if (playPauseBtn) {
        let isPlaying = false;
        
        playPauseBtn.addEventListener('click', function() {
            isPlaying = !isPlaying;
            const icon = this.querySelector('i');
            
            if (isPlaying) {
                icon.classList.remove('fa-play');
                icon.classList.add('fa-pause');
            } else {
                icon.classList.remove('fa-pause');
                icon.classList.add('fa-play');
            }
        });
    }

    // ========================================
    // AUTO-SAVE NOTES
    // ========================================
    
    const notesTextarea = document.querySelector('.notes-textarea');
    
    if (notesTextarea) {
        let saveTimeout;
        
        notesTextarea.addEventListener('input', function() {
            clearTimeout(saveTimeout);
            
            saveTimeout = setTimeout(() => {
                const notes = this.value;
                Masar.saveToStorage('lesson-notes', notes);
                console.log('Notes auto-saved');
            }, 1000);
        });

        // Load saved notes
        const savedNotes = Masar.getFromStorage('lesson-notes');
        if (savedNotes) {
            notesTextarea.value = savedNotes;
        }
    }

})();