// ========================================
// PROFILE PAGE - JavaScript
// Works for both Student and Instructor profiles
// ========================================

(function() {
    'use strict';

    // ========================================
    // AVATAR & COVER UPLOAD
    // ========================================
    
    function initAvatarUpload() {
        const editAvatarBtn = document.getElementById('editAvatarBtn');
        const editCoverBtn = document.getElementById('editCoverBtn');

        if (editAvatarBtn) {
            editAvatarBtn.addEventListener('click', () => {
                const input = document.createElement('input');
                input.type = 'file';
                input.accept = 'image/*';
                input.onchange = (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        const reader = new FileReader();
                        reader.onload = (event) => {
                            // In production, upload to server
                            console.log('Avatar file selected:', file.name);
                            alert('Avatar uploaded successfully!');
                            // Update avatar preview
                            // document.querySelector('.avatar-xl').style.backgroundImage = `url(${event.target.result})`;
                        };
                        reader.readAsDataURL(file);
                    }
                };
                input.click();
            });
        }

        if (editCoverBtn) {
            editCoverBtn.addEventListener('click', () => {
                const input = document.createElement('input');
                input.type = 'file';
                input.accept = 'image/*';
                input.onchange = (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        const reader = new FileReader();
                        reader.onload = (event) => {
                            console.log('Cover photo selected:', file.name);
                            alert('Cover photo uploaded successfully!');
                            // Update cover preview
                            // document.querySelector('.profile-cover').style.backgroundImage = `url(${event.target.result})`;
                        };
                        reader.readAsDataURL(file);
                    }
                };
                input.click();
            });
        }
    }

    // ========================================
    // SECTION EDIT BUTTONS
    // ========================================
    
    function initSectionEditing() {
        const editButtons = document.querySelectorAll('.section-edit-btn');

        editButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const section = btn.closest('.profile-section');
                const sectionTitle = section.querySelector('.section-title').textContent.trim();
                
                console.log(`Editing section: ${sectionTitle}`);
                
                // Create simple edit modal or inline editing
                // For now, show alert (in production, open modal with form)
                alert(`Edit mode for "${sectionTitle}" section.\n\nIn production, this would open an edit form.`);
            });
        });
    }

    // ========================================
    // PROFILE ACTIONS
    // ========================================
    
    function initProfileActions() {
        const editProfileBtn = document.getElementById('editProfileBtn');
        const shareProfileBtn = document.getElementById('shareProfileBtn');

        if (editProfileBtn) {
            editProfileBtn.addEventListener('click', () => {
                console.log('Opening profile editor');
                alert('Full profile editor would open here.\n\nThis would allow editing all profile information in one place.');
            });
        }

        if (shareProfileBtn) {
            shareProfileBtn.addEventListener('click', () => {
                const profileUrl = window.location.href;
                
                // Check if Web Share API is available
                if (navigator.share) {
                    navigator.share({
                        title: 'My Masar Profile',
                        text: 'Check out my profile on Masar!',
                        url: profileUrl
                    }).then(() => {
                        console.log('Profile shared successfully');
                    }).catch(err => {
                        console.log('Share cancelled or failed:', err);
                    });
                } else {
                    // Fallback: copy to clipboard
                    navigator.clipboard.writeText(profileUrl).then(() => {
                        alert('✓ Profile link copied to clipboard!');
                    }).catch(() => {
                        // Fallback for older browsers
                        prompt('Copy this link:', profileUrl);
                    });
                }
            });
        }
    }

    // ========================================
    // ANIMATE SKILLS ON SCROLL
    // ========================================
    
    function animateSkills() {
        const skillBars = document.querySelectorAll('.skill-progress-bar');
        
        if (skillBars.length === 0) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const targetWidth = entry.target.style.width;
                    entry.target.style.width = '0%';
                    
                    // Animate to target width
                    setTimeout(() => {
                        entry.target.style.width = targetWidth;
                    }, 100);
                    
                    // Stop observing this element
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });

        skillBars.forEach(bar => observer.observe(bar));
    }

    // ========================================
    // ANIMATE TIMELINE ON SCROLL
    // ========================================
    
    function animateTimeline() {
        const timelineItems = document.querySelectorAll('.timeline-item');
        
        if (timelineItems.length === 0) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.2 });

        timelineItems.forEach(item => observer.observe(item));
    }

    // ========================================
    // SOCIAL LINKS ANALYTICS
    // ========================================
    
    function initSocialLinks() {
        const socialLinks = document.querySelectorAll('.social-link');

        socialLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                const platform = link.classList[1]; // github, linkedin, etc.
                console.log(`Social link clicked: ${platform}`);
                // In production, track with analytics
            });
        });
    }

    // ========================================
    // ACHIEVEMENT TOOLTIPS
    // ========================================
    
    function initAchievementTooltips() {
        const achievements = document.querySelectorAll('.achievement-badge');

        achievements.forEach(badge => {
            badge.addEventListener('mouseenter', () => {
                const name = badge.querySelector('.achievement-name').textContent;
                console.log(`Viewing achievement: ${name}`);
                // In production, show detailed tooltip
            });
        });
    }

    // ========================================
    // STATS COUNTER ANIMATION
    // ========================================
    
    function animateStats() {
        const stats = document.querySelectorAll('.stat-value, .activity-stat-value');
        
        if (stats.length === 0) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const target = entry.target;
                    const value = target.textContent;
                    
                    // Extract number from text
                    const numberMatch = value.match(/[\d,.]+/);
                    if (numberMatch) {
                        const number = parseFloat(numberMatch[0].replace(/,/g, ''));
                        animateValue(target, 0, number, 1000, value);
                    }
                    
                    observer.unobserve(target);
                }
            });
        }, { threshold: 0.5 });

        stats.forEach(stat => observer.observe(stat));
    }

    function animateValue(element, start, end, duration, originalText) {
        const range = end - start;
        const increment = range / (duration / 16);
        let current = start;
        
        const timer = setInterval(() => {
            current += increment;
            if (current >= end) {
                current = end;
                clearInterval(timer);
            }
            
            // Format number and preserve original text format
            const formatted = Math.floor(current).toLocaleString();
            element.textContent = originalText.replace(/[\d,.]+/, formatted);
        }, 16);
    }

    // ========================================
    // RESPONSIVE ACTIONS
    // ========================================
    
    function handleResponsive() {
        const profileStats = document.querySelector('.profile-stats');
        
        if (!profileStats) return;

        function checkWidth() {
            if (window.innerWidth <= 768) {
                // Mobile: ensure stats are visible
                profileStats.style.display = 'flex';
            }
        }

        checkWidth();
        window.addEventListener('resize', checkWidth);
    }

    // ========================================
    // INITIALIZE
    // ========================================
    
    function init() {
        console.log('Initializing Profile Page...');

        initAvatarUpload();
        initSectionEditing();
        initProfileActions();
        animateSkills();
        animateTimeline();
        initSocialLinks();
        initAchievementTooltips();
        animateStats();
        handleResponsive();

        console.log('Profile page initialized successfully ✓');
    }

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();