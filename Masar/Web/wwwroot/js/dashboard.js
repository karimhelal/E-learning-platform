// ========================================
// DASHBOARD PAGE - JavaScript
// Optimized for .NET MVC
// ========================================

(function() {
    'use strict';

    // ========================================
    // INITIALIZE PROGRESS CIRCLES
    // ========================================
    
    function initProgressCircles() {
        const circles = document.querySelectorAll('.progress-circle');
        
        circles.forEach(circle => {
            const progress = parseInt(circle.getAttribute('data-progress')) || 0;
            const svg = circle.querySelector('circle:last-of-type');
            
            if (svg) {
                const radius = 35;
                const circumference = 2 * Math.PI * radius;
                const offset = circumference - (progress / 100) * circumference;
                
                svg.style.strokeDasharray = `${circumference} ${circumference}`;
                svg.style.strokeDashoffset = circumference;
                
                // Animate on load
                setTimeout(() => {
                    svg.style.transition = 'stroke-dashoffset 1s ease-in-out';
                    svg.style.strokeDashoffset = offset;
                }, 100);
            }
        });
    }

    // ========================================
    // ANIMATE STATS ON SCROLL
    // ========================================
    
    function animateStats() {
        const statCards = document.querySelectorAll('.stat-card');
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    animateStatValue(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });

        statCards.forEach(card => observer.observe(card));
    }

    function animateStatValue(card) {
        const valueElement = card.querySelector('.stat-value');
        if (!valueElement) return;

        const targetValue = parseInt(valueElement.textContent) || 0;
        const duration = 1000;
        const startTime = performance.now();

        function update(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            const currentValue = Math.floor(progress * targetValue);
            valueElement.textContent = currentValue;

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                valueElement.textContent = targetValue;
            }
        }

        requestAnimationFrame(update);
    }

    // ========================================
    // COURSE CARD INTERACTIONS
    // ========================================
    
    function initCourseCards() {
        const courseCards = document.querySelectorAll('.course-card');
        
        courseCards.forEach(card => {
            card.addEventListener('mouseenter', function() {
                this.style.transform = 'translateY(-8px)';
            });
            
            card.addEventListener('mouseleave', function() {
                this.style.transform = '';
            });
        });
    }

    // ========================================
    // TRACK CARD INTERACTIONS
    // ========================================
    
    function initTrackCards() {
        const trackCards = document.querySelectorAll('.track-card');
        
        trackCards.forEach(card => {
            card.addEventListener('mouseenter', function() {
                const progressCircle = this.querySelector('.progress-circle svg circle:last-of-type');
                if (progressCircle) {
                    progressCircle.style.filter = 'drop-shadow(0 0 8px currentColor)';
                }
            });
            
            card.addEventListener('mouseleave', function() {
                const progressCircle = this.querySelector('.progress-circle svg circle:last-of-type');
                if (progressCircle) {
                    progressCircle.style.filter = '';
                }
            });
        });
    }

    // ========================================
    // ACTIVITY FEED AUTO-REFRESH (Optional)
    // ========================================
    
    function initActivityFeed() {
        // This would be used to periodically fetch new activities from the server
        // For now, it's just a placeholder
        
        // Example: Refresh every 5 minutes
        // setInterval(fetchLatestActivity, 5 * 60 * 1000);
    }

    function fetchLatestActivity() {
        // In MVC, you would make a request to a controller action
        // For now, this is just a placeholder
        console.log('Fetching latest activity...');
        
        // Example implementation:
        /*
        fetch('/Student/GetLatestActivity')
            .then(response => response.json())
            .then(data => {
                updateActivityFeed(data);
            })
            .catch(error => {
                console.error('Error fetching activity:', error);
            });
        */
    }

    // ========================================
    // WELCOME MESSAGE BASED ON TIME
    // ========================================
    
    function updateWelcomeMessage() {
        const titleElement = document.querySelector('.page-title');
        if (!titleElement) return;

        const hour = new Date().getHours();
        let greeting = 'Welcome back';
        let emoji = '👋';

        if (hour < 12) {
            greeting = 'Good morning';
            emoji = '🌅';
        } else if (hour < 18) {
            greeting = 'Good afternoon';
            emoji = '☀️';
        } else {
            greeting = 'Good evening';
            emoji = '🌙';
        }

        // Get user name from the element (or you can pass it from MVC)
        const userName = document.querySelector('.user-name')?.textContent || 'Student';
        const firstName = userName.split(' ')[0];

        titleElement.textContent = `${greeting}, ${firstName}! ${emoji}`;
    }

    // ========================================
    // MOTIVATIONAL QUOTES (Optional)
    // ========================================
    
    function displayMotivationalQuote() {
        const quotes = [
            "Every expert was once a beginner.",
            "The only way to learn a new programming language is by writing programs in it.",
            "Code is like humor. When you have to explain it, it's bad.",
            "Programming isn't about what you know; it's about what you can figure out.",
            "The best error message is the one that never shows up."
        ];

        const randomQuote = quotes[Math.floor(Math.random() * quotes.length)];
        
        // You can display this in a dedicated element
        // For now, just log it
        console.log('💡 Quote of the day:', randomQuote);
    }

    // ========================================
    // LEARNING STREAK TRACKER
    // ========================================
    
    function trackLearningStreak() {
        const today = new Date().toDateString();
        const lastVisit = Masar.getFromStorage('lastVisit');
        
        if (lastVisit !== today) {
            // New day visit
            Masar.saveToStorage('lastVisit', today);
            
            const streak = Masar.getFromStorage('learningStreak') || 0;
            const newStreak = streak + 1;
            
            Masar.saveToStorage('learningStreak', newStreak);
            
            if (newStreak > 1 && newStreak % 7 === 0) {
                Masar.showNotification(
                    `🔥 Amazing! You're on a ${newStreak}-day learning streak!`, 
                    'success', 
                    5000
                );
            }
        }
    }

    // ========================================
    // KEYBOARD SHORTCUTS
    // ========================================
    
    function initKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + K: Search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                // Open search modal or focus search input
                console.log('Search shortcut triggered');
            }
            
            // Ctrl/Cmd + /: Show shortcuts help
            if ((e.ctrlKey || e.metaKey) && e.key === '/') {
                e.preventDefault();
                showKeyboardShortcuts();
            }
        });
    }

    function showKeyboardShortcuts() {
        const shortcuts = `
            <div style="text-align: left;">
                <h4 style="margin-bottom: 1rem; color: var(--text-primary);">Keyboard Shortcuts</h4>
                <table style="width: 100%; color: var(--text-secondary);">
                    <tr>
                        <td style="padding: 0.5rem;"><kbd>Ctrl + K</kbd></td>
                        <td style="padding: 0.5rem;">Search</td>
                    </tr>
                    <tr>
                        <td style="padding: 0.5rem;"><kbd>Ctrl + /</kbd></td>
                        <td style="padding: 0.5rem;">Show shortcuts</td>
                    </tr>
                    <tr>
                        <td style="padding: 0.5rem;"><kbd>Esc</kbd></td>
                        <td style="padding: 0.5rem;">Close modals</td>
                    </tr>
                </table>
            </div>
        `;
        
        Masar.showModal('Keyboard Shortcuts', shortcuts);
    }

    // ========================================
    // LOAD SAVED PREFERENCES
    // ========================================
    
    function loadUserPreferences() {
        // Load any saved user preferences from localStorage
        const preferences = Masar.getFromStorage('userPreferences') || {};
        
        // Apply preferences
        if (preferences.theme) {
            // Apply theme if you have theme switching
            console.log('Applying theme:', preferences.theme);
        }
        
        if (preferences.compactMode) {
            // Apply compact mode if available
            document.body.classList.toggle('compact-mode', preferences.compactMode);
        }
    }

    // ========================================
    // INITIALIZE ON PAGE LOAD
    // ========================================
    
    function init() {
        // Initialize all dashboard features
        initProgressCircles();
        animateStats();
        initCourseCards();
        initTrackCards();
        initActivityFeed();
        updateWelcomeMessage();
        displayMotivationalQuote();
        trackLearningStreak();
        initKeyboardShortcuts();
        loadUserPreferences();

        // Log initialization
        console.log('Dashboard initialized successfully');
    }

    // Run initialization when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();