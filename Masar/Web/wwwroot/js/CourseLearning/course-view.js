const CoursePlayer = {

    // ============================================================
    // 1. STATE MANAGEMENT
    // ============================================================
    state: {
        // Fallback to 0 if window.courseState isn't loaded yet
        courseId: window.courseState ? window.courseState.courseId : 0,
        currentLessonId: null,
        playerInstance: null, // To track Plyr so we can destroy it before reloading
        currentLessonStarted: false
    },

    // ============================================================
    // 2. INITIALIZATION
    // ============================================================
    init: function () {
        console.log("CoursePlayer Initializing...");

        // A. Load saved notes from LocalStorage
        this.functions.loadNotes();

        // B. Initialize Video Player (if present on load)
        this.functions.initPlyr();

        // C. Bind the Global Event Listener
        this.bindEvents();
    },

    // ============================================================
    // 3. ACTIONS (The "Command Center")
    // ============================================================
    // Every interaction in your UI maps to one of these keys.
    actions: {

        // --- NAVIGATION ---
        'navigate-lesson': function (element, event) {
            // 1. Visual Feedback
            document.querySelectorAll(".lesson-item").forEach(i => i.classList.remove("active"));
            element.classList.add("active");

            // 2. Update Accordion State
            document.querySelectorAll(".module-indicator").forEach(i => i.classList.remove("active"));
            const parentModule = element.closest(".module-item");
            if (parentModule) parentModule.querySelector(".module-indicator").classList.add("active");

            // 3. Trigger Logic
            const lessonId = element.dataset.id;
            CoursePlayer.functions.loadLessonAjax(lessonId);

            // 4. Auto-Close Sidebar on Mobile
            //if (window.innerWidth <= 1024) {
            //    CoursePlayer.actions['toggle-sidebar'](); // Call internal action
            //}
        },

        'navigate-back': function (element, event) {
            const prevItem = CoursePlayer.functions.getPreviousLessonItem();

            if (prevItem) {
                // 1. Make sure the module is open so the user sees where they went
                CoursePlayer.functions.ensureModuleOpen(prevItem);

                // 2. Trigger the navigation action directly
                // We pass 'prevItem' as the element, mimicking a click on the sidebar
                CoursePlayer.actions['navigate-lesson'](prevItem);
            }
        },

        'navigate-forward': function (element, event) {
            const nextItem = CoursePlayer.functions.getNextLessonItem();

            if (nextItem) {
                // 1. Make sure the module is open
                CoursePlayer.functions.ensureModuleOpen(nextItem);

                // 2. Trigger the navigation action
                CoursePlayer.actions['navigate-lesson'](nextItem);
            } else {
                // Optional: Handle "Course Completed" scenario
                alert("Congratulations! You've reached the end of the course.");
            }
        },

        // --- SIDEBAR & LAYOUT ---
        'toggle-sidebar': function () {
            const sidebar = document.getElementById("sidebar");
            const overlay = document.getElementById("sidebarOverlay");

            // Logic: If open, close it. If closed, open it.
            // We handle both Mobile (.open) and Desktop (.hidden) classes here

            if (window.innerWidth <= 1024) {
                // Mobile Logic
                sidebar.classList.toggle("open");
                if (overlay) overlay.classList.toggle("active");
            }
        //    else {
        //        // Desktop Logic
        //        sidebar.classList.toggle("hidden");
        //        mainContent.classList.toggle("expanded");

        //        // Toggle the "Show" button visibility
        //        if (sidebar.classList.contains("hidden")) {
        //            showBtn.classList.add("visible");
        //        } else {
        //            showBtn.classList.remove("visible");
        //        }
        //    }
        },

        'toggle-module': function (element) {
            // Toggle the arrow rotation
            element.classList.toggle("open");
            // Toggle the list visibility
            const list = element.nextElementSibling;
            if (list) list.classList.toggle("open");
        },

        // --- CONTENT TABS ---
        'switch-tab': function (element) {
            const targetId = element.dataset.tab;

            // 1. Reset all buttons
            document.querySelectorAll(".tab-btn").forEach(b => b.classList.remove("active"));
            // 2. Reset all content divs
            document.querySelectorAll(".tab-content").forEach(c => c.classList.remove("active"));

            // 3. Activate clicked
            element.classList.add("active");
            document.getElementById(targetId + "Tab").classList.add("active");
        },

        // --- ACTIONS ---
        'toggle-complete': function (element, event) {
            event.stopPropagation(); // CRITICAL: Don't trigger 'navigate-lesson'

            element.classList.toggle("done");

            // get true or false
            const isComplete = element.classList.contains("done");

            // Call API (Optional)
            const lessonId = element.dataset.id;

            CoursePlayer.functions.updateCompletionState(lessonId, isComplete);
        },

        'save-notes': function () {
            const notes = document.getElementById("notesArea").value;
            const lessonId = CoursePlayer.state.currentLessonId || "global";
            // Saving by LessonID allows different notes per lesson!
            localStorage.setItem(`notes_${lessonId}`, notes);
            alert("Notes saved locally!");
        }
    },

    // ============================================================
    // 4. HELPER FUNCTIONS (The Logic)
    // ============================================================
    functions: {

        loadLessonAjax: function (lessonId) {
            const courseId = CoursePlayer.state.courseId;
            CoursePlayer.state.currentLessonId = lessonId;

            // Show loading state (Optional)
            $('#lessonResourcesContainer').css('opacity', 0.5);

            $.ajax({
                url: `/Classroom/Course/GetLessonContent/${courseId}/${lessonId}`,
                type: 'GET',
                success: function (response) {
                    // 1. Inject HTML
                    $('#lessonMetaDataContainer').html(response.playerHtml);
                    $('#lessonResourcesContainer').html(response.resourcesHtml);

                    // 2. Update Header/Breadcrumbs
                    $('#breadcrumbModule').text(response.moduleTitle);

                    // 3. Load Notes for this specific lesson
                    CoursePlayer.functions.loadNotes();

                    // 4. Re-Initialize Video Player
                    CoursePlayer.functions.initPlyr();

                    // Restore Opacity
                    $('#lessonResourcesContainer').css('opacity', 1);
                },
                error: function (xhr) {
                    if (xhr.status === 403) {
                        window.location.href = `/Course/Details/${courseId}`;
                    } else {
                        console.error("AJAX Error:", xhr);
                    }
                }
            });
        },

        updateCompletionState: function (lessonId, completionState) {
            const courseId = CoursePlayer.state.courseId;
            //CoursePlayer.state.currentLessonId = lessonId;

            $.ajax({
                url: `/Classroom/Course/UpdateLessonCompletionState/${courseId}/${lessonId}/${completionState}`,
                type: 'POST',
                success: function (response) {
                    // success 
                    // 1. Inject HTML
                    $('#progressSubtitle').text(response.formattedProgressSubtitle);
                    //$('#progressRing').attr('stroke-dasharray', `${response.progressPercentage}, 100`);
                    $('.progress-circle circle.progress').css('stroke-dasharray', `${response.progressPercentage}, 100`);
                    $('#progressValue').text(response.progressPercentage + '%');
                },
                error: function (xhr) {
                    if (xhr.status === 403) {
                        window.location.href = `/Course/Details/${courseId}`;
                    } else {
                        console.error("AJAX Error:", xhr);
                    }
                }
            });
        },

        initPlyr: function () {
            // Check if Plyr library exists
            if (typeof Plyr === 'undefined') {
                console.log('error finding the Plyr library');
                return;
            }

            // 1. Cleanup old instance
            if (CoursePlayer.state.playerInstance) {
                CoursePlayer.state.playerInstance.destroy();
                CoursePlayer.state.playerInstance = null;
            }

            CoursePlayer.state.currentLessonStarted = false;

            // 2. Init new instance
            const playerElement = document.getElementById('player');
            if (playerElement) {
                const player = new Plyr('#player', {
                    // Define EXACTLY which controls you want
                    //controls: [],
                    controls: [
                        'play-large',      // Big play button in center
                        'rewind',          // Rewind by seekTime
                        'play',            // Play/pause
                        'fast-forward',    // Fast forward by seekTime
                        'progress',        // Progress bar
                        'current-time',    // Current time display
                        'duration',        // Duration display
                        'mute',            // Mute toggle
                        'volume',          // Volume slider
                        'captions',        // Captions toggle
                        'settings',        // Settings menu (includes Quality!)
                        'pip',             // Picture-in-picture
                        'fullscreen'       // Fullscreen toggle
                    ],
                    settings: ['captions', 'quality', 'speed', 'loop'],

                    // Quality configuration
                    quality: {
                        default: 576,
                        options: [4320, 2880, 2160, 1440, 1080, 720, 576, 480, 360, 240],
                        forced: true,
                        onChange: (quality) => {
                            console.log('Quality changed to:', quality);
                            updateInfo();
                        }
                    },

                    // Speed configuration
                    speed: {
                        selected: 1,
                        options: [0.5, 0.75, 1, 1.25, 1.5, 1.75, 2]
                    },

                    // Seek time for rewind/fast-forward buttons
                    seekTime: 10,

                    // FORCE the ratio via JS configuration
                    ratio: '16:9',

                    // YouTube specific options
                    youtube: {
                        noCookie: true,
                        rel: 0,
                        //showinfo: 0,
                        iv_load_policy: 3,
                        //modestbranding: 1,
                        //controls = 0,
                        showinfo: 0,
                        modestbranding: 1
                    },

                    // Enable tooltips
                    tooltips: {
                        controls: true,
                        seek: true
                    },

                    // Keyboard shortcuts
                    keyboard: {
                        focused: true,
                        global: false
                    },

                    // i18n for quality labels
                    i18n: {
                        qualityLabel: {
                            0: 'Auto'
                        }
                    },

                    theme: '#7c3aed',

                    // 3. Interaction settings
                    //clickToPlay: true,
                    //autoplay: true,
                    //muted: false
                    //resetOnEnd: true,  // Reset to start when finished
                });

                CoursePlayer.state.playerInstance = player;

                player.on('playing', event => {
                    event.stopPropagation();

                    if (!CoursePlayer.state.currentLessonStarted) {
                        console.log("Video started playing! Marking as started...");

                        // 1. Get the ID
                        let lessonId = CoursePlayer.state.currentLessonId;

                        if (!lessonId) {
                            const currentLesson = document.querySelector(".lesson-item.active");
                            if (!currentLesson)
                                return;

                            lessonId = currentLesson.dataset.id;
                            CoursePlayer.state.currentLessonId = lessonId;
                        }

                        // 2. Send the signal
                        if (lessonId && CoursePlayer.state.currentLessonStarted == false) {
                            $.post(`/Classroom/Course/MarkLessonStarted/${lessonId}`, (response) => {
                                if (response.success == true)
                                    // 3. Lock it so seeking/pausing doesn't re-trigger it
                                    CoursePlayer.state.currentLessonStarted = true;
                            });
                        }
                    }
                });
            }
        },

        loadNotes: function () {
            const lessonId = CoursePlayer.state.currentLessonId || "global";
            const savedNotes = localStorage.getItem(`notes_${lessonId}`);
            const textArea = document.getElementById("notesArea");

            if (textArea) {
                textArea.value = savedNotes || ""; // Load text or clear if empty
            }
        },

        getPreviousLessonItem: function () {
            const current = document.querySelector(".lesson-item.active");
            if (!current) return null;

            const allLessons = Array.from(document.querySelectorAll(".lesson-item"));
            const index = allLessons.indexOf(current);

            // Return previous if it exists
            return index > 0 ? allLessons[index - 1] : null;
        },

        getNextLessonItem: function () {
            const current = document.querySelector(".lesson-item.active");
            if (!current) return null;

            const allLessons = Array.from(document.querySelectorAll(".lesson-item"));
            const index = allLessons.indexOf(current);

            // Return next if it exists
            return index < allLessons.length - 1 ? allLessons[index + 1] : null;
        },

        // Helper to ensure the parent module is visible
        ensureModuleOpen: function (lessonItem) {
            const parentModuleHeader = lessonItem.closest('.module-item').querySelector('.module-header');
            // If it's closed, run the toggle action to open it
            if (!parentModuleHeader.classList.contains('open')) {
                CoursePlayer.actions['toggle-module'](parentModuleHeader);
            }
        }
    },

    // ============================================================
    // 5. THE EVENT BINDER (The Router)
    // ============================================================
    bindEvents: function () {
        document.addEventListener("click", (e) => {
            // 1. Find the closest element with [data-action]
            const trigger = e.target.closest("[data-action]");

            // 2. If found, executes the matching action from the dictionary
            if (trigger) {
                const actionName = trigger.dataset.action;
                if (this.actions[actionName]) {
                    this.actions[actionName](trigger, e);
                }
            }
        });

        // Overlay Click (Close sidebar on mobile)
        const overlay = document.getElementById("sidebarOverlay");
        if (overlay) {
            overlay.addEventListener("click", () => {
                CoursePlayer.actions['toggle-sidebar']();
            });
        }
    }
};

// Start the Engine
document.addEventListener("DOMContentLoaded", () => {
    CoursePlayer.init();
});



//// Confetti Effect
//function triggerConfetti() {
//    const container = document.getElementById('confettiContainer');
//    const colors = ['#7c3aed', '#06b6d4', '#ec4899', '#10b981', '#f59e0b'];

//    for (let i = 0; i < 50; i++) {
//        const confetti = document.createElement('div');
//        confetti.className = 'confetti';
//        confetti.style.left = Math.random() * 100 + '%';
//        confetti.style.background = colors[Math.floor(Math.random() * colors.length)];
//        confetti.style.transform = `rotate(${Math.random() * 360}deg)`;
//        confetti.style.animation = `fall ${1.5 + Math.random()}s ease-out forwards`;
//        container.appendChild(confetti);

//        setTimeout(() => confetti.remove(), 2000);
//    }
//}

//// Add confetti animation
//const style = document.createElement('style');
//style.textContent = `
//            @keyframes fall {
//                0% { opacity: 1; top: -10%; transform: translateX(0) rotate(0deg); }
//                100% { opacity: 0; top: 100%; transform: translateX(${Math.random() > 0.5 ? '' : '-'}100px) rotate(720deg); }
//            }
//        `;
//document.head.appendChild(style);




//// Navigation
//document.getElementById('prevBtn').addEventListener('click', () => {
//    const allLessons = getAllLessons();
//    const currentIndex = allLessons.findIndex(l => l.id === currentLessonId);
//    if (currentIndex > 0) {
//        selectLesson(allLessons[currentIndex - 1].id);
//    }
//});

//document.getElementById('nextBtn').addEventListener('click', () => {
//    const allLessons = getAllLessons();
//    const currentIndex = allLessons.findIndex(l => l.id === currentLessonId);
//    if (currentIndex < allLessons.length - 1) {
//        selectLesson(allLessons[currentIndex + 1].id);
//    }
//});