// ========================================
// TRACK DETAILS - JavaScript
// ========================================

(function() {
    'use strict';

    // ========================================
    // ANIMATE PROGRESS CIRCLE
    // ========================================
    
    function animateProgressCircle() {
        const circle = document.querySelector('.progress-circle circle:last-child');
        
        if (circle) {
            const percentage = 67; // Get from data attribute in production
            const radius = 54;
            const circumference = 2 * Math.PI * radius;
            const offset = circumference - (percentage / 100) * circumference;

            // Animate on load
            setTimeout(() => {
                circle.style.transition = 'stroke-dashoffset 1.5s ease';
                circle.style.strokeDashoffset = offset;
            }, 300);
        }
    }

    // ========================================
    // SMOOTH SCROLL TO CURRENT COURSE
    // ========================================
    
    function initContinueLearning() {
        const continueBtn = document.querySelector('.track-progress-widget .btn-primary');
        
        if (continueBtn) {
            continueBtn.addEventListener('click', (e) => {
                const inProgressCourse = document.querySelector('.path-item.in-progress');
                
                if (inProgressCourse) {
                    e.preventDefault();
                    
                    inProgressCourse.scrollIntoView({ 
                        behavior: 'smooth', 
                        block: 'center' 
                    });
                    
                    // Highlight effect
                    const card = inProgressCourse.querySelector('.course-card');
                    card.style.boxShadow = '0 0 30px rgba(124, 58, 237, 0.5)';
                    
                    setTimeout(() => {
                        card.style.boxShadow = '';
                    }, 2000);
                }
            });
        }
    }

    // ========================================
    // ANIMATE PROGRESS BARS
    // ========================================
    
    function animateProgressBars() {
        const progressBars = document.querySelectorAll('.progress-fill');
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const bar = entry.target;
                    const width = bar.style.width;
                    
                    bar.style.width = '0%';
                    setTimeout(() => {
                        bar.style.width = width;
                    }, 100);
                    
                    observer.unobserve(bar);
                }
            });
        }, { threshold: 0.5 });

        progressBars.forEach(bar => observer.observe(bar));
    }

    // ========================================
    // COURSE CARD INTERACTIONS
    // ========================================
    
    function initCourseCards() {
        const courseCards = document.querySelectorAll('.course-card');

        courseCards.forEach(card => {
            card.addEventListener('click', (e) => {
                // Don't trigger if clicking a button
                if (e.target.closest('.btn')) return;
                
                const courseName = card.querySelector('.course-name').textContent;
                console.log('Course clicked:', courseName);
            });
        });
    }

    // ========================================
    // SKILL PILLS
    // ========================================
    
    function initSkillPills() {
        const skills = document.querySelectorAll('.skill-pill');

        skills.forEach(skill => {
            skill.addEventListener('click', () => {
                const skillName = skill.textContent.trim();
                console.log('Skill clicked:', skillName);
                // Could filter courses by skill or show info
            });
        });
    }

    // ========================================
    // TRACK PROGRESS DATA
    // ========================================
    
    function loadTrackProgress() {
        // In production, fetch from API
        const trackData = {
            trackId: 1,
            title: 'Web Development Track',
            totalCourses: 8,
            completedCourses: 5,
            progress: 67,
            timeSpent: 82,
            certificates: 5,
            streak: 45
        };

        console.log('Track Progress:', trackData);
        return trackData;
    }

    // ========================================
    // COURSE COMPLETION CHECK
    // ========================================
    
    function checkCompletionStatus() {
        const completed = document.querySelectorAll('.path-item.completed').length;
        const total = document.querySelectorAll('.path-item').length;
        
        console.log(`Progress: ${completed}/${total} courses completed`);

        // Check if all courses are completed
        if (completed === total) {
            console.log('🎉 Track completed! Certificate available.');
        }
    }

    // ========================================
    // FADE IN ANIMATION
    // ========================================
    
    function animateOnScroll() {
        const items = document.querySelectorAll('.path-item');
        
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '0';
                    entry.target.style.transform = 'translateY(20px)';
                    
                    setTimeout(() => {
                        entry.target.style.transition = 'all 0.6s ease';
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }, 100);
                    
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.2 });

        items.forEach(item => observer.observe(item));
    }

    // ========================================
    // INITIALIZE
    // ========================================
    
    function init() {
        animateProgressCircle();
        initContinueLearning();
        animateProgressBars();
        initCourseCards();
        initSkillPills();
        loadTrackProgress();
        checkCompletionStatus();
        animateOnScroll();

        console.log('Track Details page initialized ✓');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();