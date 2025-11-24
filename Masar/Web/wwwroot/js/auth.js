// ========================================
// AUTHENTICATION - JavaScript (Updated)
// ========================================

(function() {
    'use strict';

    // ========================================
    // SIMULATE USER DATA
    // In production, this comes from API
    // ========================================
    
    const mockUserData = {
        email: 'user@example.com',
        password: 'password123',
        roles: ['student', 'instructor'], // User has both roles
        studentData: {
            activeCourses: 12,
            certificates: 5
        },
        instructorData: {
            courses: 8,
            students: 2547
        }
    };

    // ========================================
    // LOGIN FORM SUBMISSION
    // ========================================
    
    function initLoginForm() {
        const loginForm = document.getElementById('loginForm');

        if (!loginForm) return;

        loginForm.addEventListener('submit', (e) => {
            e.preventDefault();

            const formData = new FormData(loginForm);
            const email = formData.get('email');
            const password = formData.get('password');
            const remember = formData.get('remember');

            console.log('Login attempt:', { email, remember });

            // Simulate API call
            setTimeout(() => {
                // In production, validate credentials with backend
                const user = authenticateUser(email, password);
                
                if (user) {
                    handleSuccessfulLogin(user);
                } else {
                    alert('Invalid email or password');
                }
            }, 500);
        });
    }

    function authenticateUser(email, password) {
        // Simulate API authentication
        // In production, this would be an API call
        if (email === mockUserData.email && password === mockUserData.password) {
            return mockUserData;
        }
        return null;
    }

    function handleSuccessfulLogin(user) {
        // Check if user has multiple roles
        if (user.roles && user.roles.length > 1) {
            // Show role selection modal
            showRoleSelectionModal(user);
        } else {
            // Redirect directly to their dashboard
            const role = user.roles[0];
            redirectToDashboard(role);
        }
    }

    // ========================================
    // ROLE SELECTION MODAL
    // ========================================
    
    function showRoleSelectionModal(user) {
        const modal = document.getElementById('roleSelectionModal');
        
        // Update stats in modal
        updateModalStats(user);
        
        // Show modal
        modal.style.display = 'flex';

        // Handle role selection
        const roleCards = modal.querySelectorAll('.role-selection-card');
        roleCards.forEach(card => {
            card.addEventListener('click', () => {
                const selectedRole = card.dataset.role;
                const rememberChoice = document.getElementById('rememberRoleChoice').checked;

                if (rememberChoice) {
                    // Save preference to localStorage
                    localStorage.setItem('preferredRole', selectedRole);
                }

                // Hide modal and redirect
                modal.style.display = 'none';
                redirectToDashboard(selectedRole);
            });
        });
    }

    function updateModalStats(user) {
        // This would update the stats shown in the modal
        // In production, use actual data from API
        console.log('Updating modal with user data:', user);
    }

    function redirectToDashboard(role) {
        console.log(`Redirecting to ${role} dashboard`);
        
        if (role === 'student') {
            window.location.href = '../student/dashboard.html';
        } else if (role === 'instructor') {
            window.location.href = '../instructor/dashboard.html';
        }
    }

    // ========================================
    // CHECK PREFERRED ROLE ON LOAD
    // ========================================
    
    function checkPreferredRole() {
        // If user previously selected a preferred role and chose to remember it
        const preferredRole = localStorage.getItem('preferredRole');
        
        // You can use this to auto-select or skip modal in future logins
        if (preferredRole) {
            console.log('User preferred role:', preferredRole);
        }
    }

    // ========================================
    // SOCIAL LOGIN WITH ROLE SELECTION
    // ========================================
    
    function initSocialLogin() {
        const socialButtons = document.querySelectorAll('.social-btn');

        socialButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const provider = btn.dataset.provider;
                console.log(`${provider} login initiated`);
                
                // Simulate OAuth flow
                setTimeout(() => {
                    // Simulate successful OAuth with user having both roles
                    const user = {
                        email: `user@${provider}.com`,
                        roles: ['student', 'instructor'],
                        studentData: { activeCourses: 12, certificates: 5 },
                        instructorData: { courses: 8, students: 2547 }
                    };
                    
                    handleSuccessfulLogin(user);
                }, 1000);
            });
        });
    }

    // ========================================
    // PASSWORD VISIBILITY TOGGLE
    // ========================================
    
    function initPasswordToggle() {
        const toggleButtons = document.querySelectorAll('.toggle-password');

        toggleButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const input = btn.previousElementSibling;
                const icon = btn.querySelector('i');

                if (input.type === 'password') {
                    input.type = 'text';
                    icon.classList.remove('fa-eye');
                    icon.classList.add('fa-eye-slash');
                } else {
                    input.type = 'password';
                    icon.classList.remove('fa-eye-slash');
                    icon.classList.add('fa-eye');
                }
            });
        });
    }

    // ========================================
    // PASSWORD STRENGTH CHECKER
    // ========================================
    
    function initPasswordStrength() {
        const passwordInput = document.getElementById('registerPassword');
        const strengthIndicator = document.getElementById('passwordStrength');

        if (!passwordInput || !strengthIndicator) return;

        passwordInput.addEventListener('input', () => {
            const password = passwordInput.value;
            const strength = calculatePasswordStrength(password);

            strengthIndicator.classList.remove('weak', 'medium', 'strong', 'very-strong');

            if (password.length > 0) {
                strengthIndicator.classList.add(strength.class);
                strengthIndicator.querySelector('.strength-text').textContent = strength.text;
            } else {
                strengthIndicator.querySelector('.strength-text').textContent = 'Password strength';
            }
        });
    }

    function calculatePasswordStrength(password) {
        let score = 0;

        if (password.length >= 8) score++;
        if (password.length >= 12) score++;
        if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
        if (/\d/.test(password)) score++;
        if (/[^a-zA-Z\d]/.test(password)) score++;

        if (score <= 1) {
            return { class: 'weak', text: 'Weak password' };
        } else if (score === 2) {
            return { class: 'medium', text: 'Medium strength' };
        } else if (score === 3 || score === 4) {
            return { class: 'strong', text: 'Strong password' };
        } else {
            return { class: 'very-strong', text: 'Very strong!' };
        }
    }

    // ========================================
    // PASSWORD MATCH VALIDATION
    // ========================================
    
    function initPasswordMatch() {
        const passwordInput = document.getElementById('registerPassword');
        const confirmInput = document.getElementById('confirmPassword');

        if (!passwordInput || !confirmInput) return;

        confirmInput.addEventListener('input', () => {
            if (confirmInput.value === '') {
                confirmInput.style.borderColor = 'var(--border-color)';
            } else if (passwordInput.value === confirmInput.value) {
                confirmInput.style.borderColor = 'var(--accent-green)';
            } else {
                confirmInput.style.borderColor = 'var(--accent-red)';
            }
        });
    }

    // ========================================
    // REGISTER FORM SUBMISSION
    // ========================================
    
    function initRegisterForm() {
        const registerForm = document.getElementById('registerForm');

        if (!registerForm) return;

        registerForm.addEventListener('submit', (e) => {
            e.preventDefault();

            const formData = new FormData(registerForm);
            const password = formData.get('password');
            const confirmPassword = formData.get('confirmPassword');

            if (password !== confirmPassword) {
                alert('Passwords do not match!');
                return;
            }

            if (!formData.get('terms')) {
                alert('Please accept the Terms of Service');
                return;
            }

            console.log('Registration data:', Object.fromEntries(formData));

            setTimeout(() => {
                alert('Registration successful! Please check your email to verify your account.');
                window.location.href = 'login.html';
            }, 500);
        });
    }

    // ========================================
    // FORGOT PASSWORD FORM
    // ========================================
    
    function initForgotPasswordForm() {
        const forgotForm = document.getElementById('forgotPasswordForm');

        if (!forgotForm) return;

        forgotForm.addEventListener('submit', (e) => {
            e.preventDefault();

            const formData = new FormData(forgotForm);
            const email = formData.get('email');

            console.log('Password reset requested for:', email);

            setTimeout(() => {
                alert('Password reset link sent! Please check your email.');
                window.location.href = 'login.html';
            }, 500);
        });
    }

    // ========================================
    // INITIALIZE
    // ========================================
    
    function init() {
        checkPreferredRole();
        initPasswordToggle();
        initPasswordStrength();
        initPasswordMatch();
        initLoginForm();
        initRegisterForm();
        initForgotPasswordForm();
        initSocialLogin();

        console.log('Auth page initialized');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();