(function () {
    'use strict';

    // ========================================
    // Password visibility toggle
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
    // Password strength indicator
    // ========================================
    function initPasswordStrength() {
        const passwordInput = document.getElementById('registerpassword');
        const strengthIndicator = document.getElementById('passwordStrength');

        if (!passwordInput || !strengthIndicator) return;

        passwordInput.addEventListener('input', () => {
            const password = passwordInput.value;
            const strength = calculatePasswordStrength(password);

            strengthIndicator.classList.remove('weak', 'medium', 'strong', 'very-strong');

            if (password.length > 0) {
                strengthIndicator.classList.add(strength.class);
                const textSpan = strengthIndicator.querySelector('.strength-text');
                if (textSpan) textSpan.textContent = strength.text;
            } else {
                const textSpan = strengthIndicator.querySelector('.strength-text');
                if (textSpan) textSpan.textContent = 'Password strength';
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
    // Password match validation
    // ========================================
    function initPasswordMatch() {
        const passwordInput = document.getElementById('registerpassword');
        const confirmInput = document.getElementById('confirmpassword');

        if (!passwordInput || !confirmInput) return;

        confirmInput.addEventListener('input', () => {
            if (confirmInput.value === '') {
                confirmInput.style.borderColor = '';
            } else if (passwordInput.value === confirmInput.value) {
                confirmInput.style.borderColor = 'green';
            } else {
                confirmInput.style.borderColor = 'red';
            }
        });
    }

    // ========================================
    // Initialize
    // ========================================
    function init() {
        initPasswordToggle();
        initPasswordStrength();
        initPasswordMatch();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();