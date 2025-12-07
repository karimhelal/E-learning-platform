// ========================================
// PROFILE PAGE - JavaScript
// Works for both Student and Instructor profiles
// ========================================

(function() {
    'use strict';

    // Detect if we're on student or instructor profile
    const isStudentProfile = window.location.pathname.includes('/student/');
    const profileEditEndpoint = isStudentProfile ? '/student/profile/edit' : '/instructor/profile/edit';
    const uploadEndpoint = isStudentProfile ? '/student/upload-image' : '/instructor/upload-image';

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
                input.onchange = async (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        await uploadProfileImage(file, 'profile');
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
                input.onchange = async (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        await uploadProfileImage(file, 'cover');
                    }
                };
                input.click();
            });
        }
    }

    async function uploadProfileImage(file, type) {
        // Validate file
        const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
        if (!allowedTypes.includes(file.type)) {
            showNotification('Invalid file type. Please use JPG, PNG, WebP, or GIF.', 'error');
            return;
        }

        if (file.size > 5 * 1024 * 1024) {
            showNotification('File size must be less than 5MB.', 'error');
            return;
        }

        const formData = new FormData();
        formData.append('file', file);
        formData.append('type', type);

        // Get antiforgery token from page (check multiple locations)
        let token = '';
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            token = tokenInput.value;
        }

        try {
            showNotification('Uploading image...', 'info');

            const headers = {};
            if (token) {
                headers['RequestVerificationToken'] = token;
            }

            const response = await fetch(uploadEndpoint, {
                method: 'POST',
                headers: headers,
                body: formData
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Upload response error:', response.status, errorText);
                throw new Error(`Upload failed with status ${response.status}`);
            }

            const result = await response.json();
            console.log('Upload result:', result);

            if (result.success) {
                showNotification('Image uploaded successfully!', 'success');
                
                // Update the avatar/cover image immediately
                if (type === 'profile') {
                    const avatarContainer = document.querySelector('.avatar.avatar-xl');
                    if (avatarContainer) {
                        // Check if img already exists
                        let img = avatarContainer.querySelector('img');
                        if (img) {
                            img.src = result.url + '?t=' + Date.now(); // Cache bust
                        } else {
                            // Create new img element and replace initials
                            img = document.createElement('img');
                            img.src = result.url + '?t=' + Date.now();
                            img.alt = 'Profile Picture';
                            avatarContainer.innerHTML = '';
                            avatarContainer.appendChild(img);
                        }
                    }
                } else if (type === 'cover') {
                    const coverContainer = document.querySelector('.profile-cover');
                    if (coverContainer) {
                        coverContainer.style.backgroundImage = `url('${result.url}?t=${Date.now()}')`;
                        coverContainer.style.backgroundSize = 'cover';
                        coverContainer.style.backgroundPosition = 'center';
                    }
                }
            } else {
                showNotification(result.message || 'Upload failed', 'error');
            }
        } catch (error) {
            console.error('Upload error:', error);
            showNotification('Failed to upload image. Please try again.', 'error');
        }
    }

    // ========================================
    // EDIT PROFILE MODAL
    // ========================================

    function initEditProfileModal() {
        const editProfileBtn = document.getElementById('editProfileBtn');

        if (editProfileBtn) {
            editProfileBtn.addEventListener('click', loadEditProfileForm);
        }

        // Also handle section edit buttons
        document.querySelectorAll('.section-edit-btn').forEach(btn => {
            btn.addEventListener('click', loadEditProfileForm);
        });
    }

    async function loadEditProfileForm() {
        try {
            console.log('Loading edit profile form from:', profileEditEndpoint);
            
            const response = await fetch(profileEditEndpoint, {
                method: 'GET',
                headers: {
                    'Accept': 'text/html'
                }
            });

            if (!response.ok) {
                console.error('Failed to load edit form. Status:', response.status);
                throw new Error('Failed to load edit form');
            }

            const html = await response.text();
            
            // Create container if it doesn't exist
            let container = document.getElementById('editProfileModalContainer');
            if (!container) {
                container = document.createElement('div');
                container.id = 'editProfileModalContainer';
                document.body.appendChild(container);
            }

            container.innerHTML = html;

            // Initialize the modal
            initModalFunctionality();

            // Show the modal
            const modal = document.getElementById('editProfileModal');
            if (modal) {
                requestAnimationFrame(() => {
                    modal.classList.add('active');
                });
            }

        } catch (error) {
            console.error('Error loading edit profile form:', error);
            showNotification('Failed to load edit form. Please try again.', 'error');
        }
    }

    function initModalFunctionality() {
        const modal = document.getElementById('editProfileModal');
        const closeBtn = document.getElementById('closeEditProfileModal');
        const cancelBtn = document.getElementById('cancelEditProfile');
        const form = document.getElementById('editProfileForm');

        // Close modal handlers
        if (closeBtn) {
            closeBtn.addEventListener('click', closeEditProfileModal);
        }

        if (cancelBtn) {
            cancelBtn.addEventListener('click', closeEditProfileModal);
        }

        // Close on overlay click
        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    closeEditProfileModal();
                }
            });
        }

        // Close on Escape key
        document.addEventListener('keydown', handleEscapeKey);

        // Initialize tabs
        initProfileTabs();

        // Initialize skills tags
        initSkillsTags();

        // Initialize character counter
        initCharCounter();

        // Form submission
        if (form) {
            form.addEventListener('submit', handleProfileSubmit);
        }
    }

    function handleEscapeKey(e) {
        const modal = document.getElementById('editProfileModal');
        if (e.key === 'Escape' && modal?.classList.contains('active')) {
            closeEditProfileModal();
        }
    }

    function closeEditProfileModal() {
        const modal = document.getElementById('editProfileModal');
        if (modal) {
            modal.classList.remove('active');
            setTimeout(() => {
                const container = document.getElementById('editProfileModalContainer');
                if (container) {
                    container.innerHTML = '';
                }
                document.removeEventListener('keydown', handleEscapeKey);
            }, 300);
        }
    }

    function initProfileTabs() {
        const tabs = document.querySelectorAll('.profile-tab');
        const contents = document.querySelectorAll('.profile-tab-content');

        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const targetTab = tab.dataset.tab;

                // Update active tab
                tabs.forEach(t => t.classList.remove('active'));
                tab.classList.add('active');

                // Update active content
                contents.forEach(content => {
                    content.classList.remove('active');
                    if (content.id === `tab-${targetTab}`) {
                        content.classList.add('active');
                    }
                });
            });
        });
    }

    function initSkillsTags() {
        // Both student and instructor now use skillInput/skillTags
        const input = document.getElementById('skillInput');
        const addBtn = document.getElementById('addSkillBtn');
        const container = document.getElementById('skillTags');
        
        // Field name is 'Skills' for both profiles
        const fieldName = 'Skills';

        if (!input || !addBtn || !container) return;

        function addTag() {
            const value = input.value.trim();
            if (!value) return;

            // Check for duplicates
            const existing = container.querySelectorAll(`input[name="${fieldName}"]`);
            for (const inp of existing) {
                if (inp.value.toLowerCase() === value.toLowerCase()) {
                    input.value = '';
                    showNotification('Skill already exists', 'info');
                    return;
                }
            }

            // Create tag element
            const tag = document.createElement('span');
            tag.className = 'expertise-tag';
            tag.innerHTML = `
                ${escapeHtml(value)}
                <button type="button" class="remove-tag-btn" data-skill="${escapeHtml(value)}">
                    <i class="fas fa-times"></i>
                </button>
                <input type="hidden" name="${fieldName}" value="${escapeHtml(value)}" />
            `;

            container.appendChild(tag);
            input.value = '';

            // Add remove handler
            tag.querySelector('.remove-tag-btn').addEventListener('click', () => {
                tag.remove();
            });
        }

        addBtn.addEventListener('click', addTag);

        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                addTag();
            }
        });

        // Initialize existing remove buttons
        container.querySelectorAll('.remove-tag-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                btn.closest('.expertise-tag').remove();
            });
        });
    }

    function initCharCounter() {
        const bio = document.getElementById('bio');
        const counter = document.getElementById('bioCharCount');

        if (bio && counter) {
            bio.addEventListener('input', () => {
                counter.textContent = bio.value.length;
            });
        }
    }

    async function handleProfileSubmit(e) {
        e.preventDefault();

        const form = e.target;
        const saveBtn = document.getElementById('saveProfileBtn');
        const formData = new FormData(form);

        // Build JSON object - common fields
        const data = {
            firstName: formData.get('FirstName'),
            lastName: formData.get('LastName'),
            phone: formData.get('Phone') || null,
            bio: formData.get('Bio') || null,
            skills: formData.getAll('Skills'),
            githubUrl: formData.get('GithubUrl') || null,
            linkedInUrl: formData.get('LinkedInUrl') || null,
            facebookUrl: formData.get('FacebookUrl') || null,
            websiteUrl: formData.get('WebsiteUrl') || null
        };

        // Add profile-specific fields
        if (isStudentProfile) {
            // Student profile fields
            data.location = formData.get('Location') || null;
        } else {
            // Instructor profile fields
            data.yearsOfExperience = formData.get('YearsOfExperience') ? parseInt(formData.get('YearsOfExperience')) : null;
        }

        // Set loading state
        saveBtn.classList.add('loading');
        saveBtn.disabled = true;

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            const response = await fetch(profileEditEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                showNotification(result.message, 'success');
                closeEditProfileModal();
                
                // Reload page to show updated data
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                showNotification(result.message || 'Failed to update profile', 'error');
                
                if (result.errors) {
                    result.errors.forEach(error => {
                        console.error('Validation error:', error);
                    });
                }
            }

        } catch (error) {
            console.error('Error updating profile:', error);
            showNotification('An error occurred. Please try again.', 'error');
        } finally {
            saveBtn.classList.remove('loading');
            saveBtn.disabled = false;
        }
    }

    function showNotification(message, type = 'info') {
        // Check if there's an existing notification system
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
            return;
        }

        // Fallback notification
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas ${type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-exclamation-circle' : 'fa-info-circle'}"></i>
                <span>${escapeHtml(message)}</span>
            </div>
        `;

        const bgColor = type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6';
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 1rem 1.5rem;
            background: ${bgColor};
            color: white;
            border-radius: 8px;
            z-index: 2000;
            display: flex;
            align-items: center;
            gap: 0.75rem;
            animation: slideIn 0.3s ease;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease forwards';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // ========================================
    // PROFILE ACTIONS
    // ========================================
    
    function initProfileActions() {
        const shareProfileBtn = document.getElementById('shareProfileBtn');

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
                        showNotification('Profile link copied to clipboard!', 'success');
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
                    
                    setTimeout(() => {
                        entry.target.style.width = targetWidth;
                    }, 100);
                    
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
                const platform = link.classList[1];
                console.log(`Social link clicked: ${platform}`);
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
                profileStats.style.display = 'flex';
            }
        }

        checkWidth();
        window.addEventListener('resize', checkWidth);
    }

    // ========================================
    // ADD CSS ANIMATIONS
    // ========================================

    function addCSSAnimations() {
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes slideOut {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(100%); opacity: 0; }
            }
        `;
        document.head.appendChild(style);
    }

    // ========================================
    // INITIALIZE
    // ========================================
    
    function init() {
        console.log('Initializing Profile Page...');
        console.log('Profile type:', isStudentProfile ? 'Student' : 'Instructor');
        console.log('Edit endpoint:', profileEditEndpoint);

        addCSSAnimations();
        initAvatarUpload();
        initEditProfileModal();
        initProfileActions();
        animateSkills();
        animateTimeline();
        initSocialLinks();
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