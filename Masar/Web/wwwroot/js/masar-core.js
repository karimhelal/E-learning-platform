// ========================================
// MASAR - Core JavaScript Functions
// Optimized for .NET MVC (No API Calls)
// ========================================

const Masar = (function () {
    'use strict';

    // ========================================
    // UTILITY FUNCTIONS
    // ========================================

    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    function formatDuration(minutes) {
        if (minutes < 60) {
            return `${minutes} minutes`;
        }
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
    }

    function formatDate(dateString) {
        const date = new Date(dateString);
        const options = { year: 'numeric', month: 'short', day: 'numeric' };
        return date.toLocaleDateString('en-US', options);
    }

    function getInitials(name) {
        return name
            .split(' ')
            .slice(0, 2)
            .map(n => n[0])
            .join('')
            .toUpperCase();
    }

    function slugify(text) {
        return text
            .toString()
            .toLowerCase()
            .trim()
            .replace(/\s+/g, '-')
            .replace(/[^\w\-]+/g, '')
            .replace(/\-\-+/g, '-');
    }

    // ========================================
    // NOTIFICATIONS
    // ========================================

    function showNotification(message, type = 'info', duration = 3000) {
        const existing = document.querySelector('.notification');
        if (existing) {
            existing.remove();
        }

        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-${getNotificationIcon(type)}"></i>
                <span>${message}</span>
            </div>
            <button class="notification-close" onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        `;

        if (!document.getElementById('notification-styles')) {
            const style = document.createElement('style');
            style.id = 'notification-styles';
            style.textContent = `
                .notification {
                    position: fixed;
                    top: 2rem;
                    right: 2rem;
                    min-width: 300px;
                    padding: 1rem 1.5rem;
                    background: var(--card-bg);
                    border: 1px solid var(--border-color);
                    border-radius: var(--border-radius-md);
                    box-shadow: var(--shadow-lg);
                    z-index: 9999;
                    display: flex;
                    align-items: center;
                    gap: 1rem;
                    opacity: 0;
                    transform: translateX(400px);
                    transition: all 0.3s ease;
                }
                .notification.show {
                    opacity: 1;
                    transform: translateX(0);
                }
                .notification-content {
                    display: flex;
                    align-items: center;
                    gap: 0.8rem;
                    flex: 1;
                    color: var(--text-primary);
                }
                .notification-success { border-left: 4px solid var(--accent-green); }
                .notification-error { border-left: 4px solid var(--accent-red); }
                .notification-warning { border-left: 4px solid var(--accent-orange); }
                .notification-info { border-left: 4px solid var(--accent-cyan); }
                .notification-success i { color: var(--accent-green); }
                .notification-error i { color: var(--accent-red); }
                .notification-warning i { color: var(--accent-orange); }
                .notification-info i { color: var(--accent-cyan); }
                .notification-close {
                    background: none;
                    border: none;
                    color: var(--text-muted);
                    cursor: pointer;
                    padding: 0.5rem;
                    transition: color 0.3s;
                }
                .notification-close:hover {
                    color: var(--text-primary);
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(notification);
        setTimeout(() => notification.classList.add('show'), 10);

        if (duration > 0) {
            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => notification.remove(), 300);
            }, duration);
        }

        return notification;
    }

    function getNotificationIcon(type) {
        const icons = {
            success: 'check-circle',
            error: 'exclamation-circle',
            warning: 'exclamation-triangle',
            info: 'info-circle'
        };
        return icons[type] || icons.info;
    }

    // ========================================
    // LOADING STATES
    // ========================================

    function showLoading(element, message = 'Loading...') {
        if (!element) return;

        const loader = document.createElement('div');
        loader.className = 'loading-overlay';
        loader.innerHTML = `
            <div class="spinner"></div>
            <p>${message}</p>
        `;

        if (!document.getElementById('loading-styles')) {
            const style = document.createElement('style');
            style.id = 'loading-styles';
            style.textContent = `
                .loading-overlay {
                    position: absolute;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(10, 14, 39, 0.9);
                    display: flex;
                    flex-direction: column;
                    align-items: center;
                    justify-content: center;
                    gap: 1rem;
                    z-index: 100;
                    backdrop-filter: blur(4px);
                }
                .spinner {
                    width: 40px;
                    height: 40px;
                    border: 4px solid rgba(124, 58, 237, 0.2);
                    border-top-color: var(--accent-purple);
                    border-radius: 50%;
                    animation: spin 0.8s linear infinite;
                }
                @keyframes spin {
                    to { transform: rotate(360deg); }
                }
                .loading-overlay p {
                    color: var(--text-secondary);
                    font-size: 0.95rem;
                }
            `;
            document.head.appendChild(style);
        }

        element.style.position = 'relative';
        element.appendChild(loader);

        return loader;
    }

    function hideLoading(element) {
        if (!element) return;
        const loader = element.querySelector('.loading-overlay');
        if (loader) {
            loader.remove();
        }
    }

    // ========================================
    // MODAL HANDLER
    // ========================================

    function showModal(title, content, options = {}) {
        const modal = document.createElement('div');
        modal.className = 'modal-overlay';
        modal.innerHTML = `
            <div class="modal-container">
                <div class="modal-header">
                    <h3>${title}</h3>
                    <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    ${content}
                </div>
                ${options.footer ? `<div class="modal-footer">${options.footer}</div>` : ''}
            </div>
        `;

        if (!document.getElementById('modal-styles')) {
            const style = document.createElement('style');
            style.id = 'modal-styles';
            style.textContent = `
                .modal-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(0, 0, 0, 0.8);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    z-index: 10000;
                    backdrop-filter: blur(4px);
                    animation: fadeIn 0.3s ease;
                }
                .modal-container {
                    background: var(--card-bg);
                    border: 1px solid var(--border-color);
                    border-radius: var(--border-radius-lg);
                    max-width: 600px;
                    width: 90%;
                    max-height: 90vh;
                    overflow-y: auto;
                    animation: slideInRight 0.3s ease;
                }
                .modal-header {
                    padding: 1.5rem;
                    border-bottom: 1px solid var(--border-color);
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                }
                .modal-header h3 {
                    margin: 0;
                    font-size: 1.3rem;
                    color: var(--text-primary);
                }
                .modal-close {
                    background: none;
                    border: none;
                    color: var(--text-muted);
                    cursor: pointer;
                    padding: 0.5rem;
                    font-size: 1.2rem;
                    transition: color 0.3s;
                }
                .modal-close:hover {
                    color: var(--text-primary);
                }
                .modal-body {
                    padding: 1.5rem;
                    color: var(--text-secondary);
                }
                .modal-footer {
                    padding: 1.5rem;
                    border-top: 1px solid var(--border-color);
                    display: flex;
                    gap: 1rem;
                    justify-content: flex-end;
                }
            `;
            document.head.appendChild(style);
        }

        document.body.appendChild(modal);

        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                modal.remove();
            }
        });

        const escHandler = function (e) {
            if (e.key === 'Escape') {
                modal.remove();
                document.removeEventListener('keydown', escHandler);
            }
        };
        document.addEventListener('keydown', escHandler);

        return modal;
    }

    // ========================================
    // CONFIRM DIALOG
    // ========================================

    function confirm(message, onConfirm, options = {}) {
        const modal = showModal(
            options.title || 'Confirm Action',
            `<p style="color: var(--text-primary);">${message}</p>`,
            {
                footer: `
                    <button class="btn btn-secondary" onclick="this.closest('.modal-overlay').remove()">
                        Cancel
                    </button>
                    <button class="btn btn-primary confirm-btn">
                        ${options.confirmText || 'Confirm'}
                    </button>
                `
            }
        );

        modal.querySelector('.confirm-btn').addEventListener('click', function () {
            onConfirm();
            modal.remove();
        });
    }

    // ========================================
    // LOCAL STORAGE HELPERS
    // ========================================

    function saveToStorage(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
            return true;
        } catch (error) {
            console.error('Storage Error:', error);
            return false;
        }
    }

    function getFromStorage(key, defaultValue = null) {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : defaultValue;
        } catch (error) {
            console.error('Storage Error:', error);
            return defaultValue;
        }
    }

    function removeFromStorage(key) {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (error) {
            console.error('Storage Error:', error);
            return false;
        }
    }

    // ========================================
    // FORM HELPERS (for MVC Forms)
    // ========================================

    function serializeForm(form) {
        const formData = new FormData(form);
        const data = {};

        for (let [key, value] of formData.entries()) {
            if (data[key]) {
                if (!Array.isArray(data[key])) {
                    data[key] = [data[key]];
                }
                data[key].push(value);
            } else {
                data[key] = value;
            }
        }

        return data;
    }

    function validateForm(form) {
        const inputs = form.querySelectorAll('input[required], textarea[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('error');
                isValid = false;
            } else {
                input.classList.remove('error');
            }
        });

        return isValid;
    }

    // ========================================
    // PUBLIC API
    // ========================================

    return {
        // Utilities
        debounce,
        formatDuration,
        formatDate,
        getInitials,
        slugify,

        // UI
        showNotification,
        showLoading,
        hideLoading,
        showModal,
        confirm,

        // Storage
        saveToStorage,
        getFromStorage,
        removeFromStorage,

        // Forms (for MVC)
        serializeForm,
        validateForm
    };

})();

// Make available globally
window.Masar = Masar;

// ========================================
// AUTO-DISPLAY SERVER MESSAGES (TempData)
// ========================================
document.addEventListener('DOMContentLoaded', function () {
    // Check for server-side messages (from TempData)
    const successMsg = document.querySelector('[data-success-message]');
    const errorMsg = document.querySelector('[data-error-message]');
    const warningMsg = document.querySelector('[data-warning-message]');
    const infoMsg = document.querySelector('[data-info-message]');

    if (successMsg) {
        Masar.showNotification(successMsg.dataset.successMessage, 'success');
        successMsg.remove();
    }
    if (errorMsg) {
        Masar.showNotification(errorMsg.dataset.errorMessage, 'error');
        errorMsg.remove();
    }
    if (warningMsg) {
        Masar.showNotification(warningMsg.dataset.warningMessage, 'warning');
        warningMsg.remove();
    }
    if (infoMsg) {
        Masar.showNotification(infoMsg.dataset.infoMessage, 'info');
        infoMsg.remove();
    }
});