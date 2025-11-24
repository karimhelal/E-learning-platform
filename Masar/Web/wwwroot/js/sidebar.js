// ========================================
// SIDEBAR COMPONENT - JavaScript
// ======================================== ==

(function () {
    'use strict';

    // ========================================
    // MOBILE MENU TOGGLE
    // ========================================

    const mobileToggle = document.getElementById('mobileToggle');
    const sidebar = document.getElementById('sidebar');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    if (mobileToggle) {
        mobileToggle.addEventListener('click', toggleSidebar);
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', closeSidebar);
    }

    function toggleSidebar() {
        sidebar?.classList.toggle('active');
        sidebarOverlay?.classList.toggle('active');

        const icon = mobileToggle?.querySelector('i');
        if (icon) {
            icon.classList.toggle('fa-bars');
            icon.classList.toggle('fa-times');
        }
    }

    function closeSidebar() {
        sidebar?.classList.remove('active');
        sidebarOverlay?.classList.remove('active');

        const icon = mobileToggle?.querySelector('i');
        if (icon) {
            icon.classList.remove('fa-times');
            icon.classList.add('fa-bars');
        }
    }

    // ========================================
    // ACTIVE NAVIGATION LINK
    // ========================================

    function setActiveNavLink() {
        const currentPath = window.location.pathname;
        const currentPage = currentPath.split('/').pop() || 'index.html';
        const navLinks = document.querySelectorAll('.nav-link');

        navLinks.forEach(link => {
            const linkHref = link.getAttribute('href');
            const linkPage = linkHref ? linkHref.split('/').pop() : '';

            // Remove active class from all links
            link.classList.remove('active');

            // Add active class to matching link
            if (linkPage === currentPage) {
                link.classList.add('active');
            }
        });
    }

    // ========================================
    // NAVIGATION LINK CLICK HANDLER
    // ========================================

    function initNavLinkHandlers() {
        const navLinks = document.querySelectorAll('.nav-link');

        navLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                // Update active state for instant feedback
                navLinks.forEach(l => l.classList.remove('active'));
                this.classList.add('active');

                // Close mobile menu
                if (window.innerWidth <= 1024) {
                    setTimeout(closeSidebar, 100);
                }
            });
        });
    }

    // ========================================
    // CLOSE SIDEBAR ON ESC KEY
    // ========================================

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && sidebar?.classList.contains('active')) {
            closeSidebar();
        }
    });

    // ========================================
    // HANDLE WINDOW RESIZE
    // ========================================

    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (window.innerWidth > 1024) {
                closeSidebar();
            }
        }, 250);
    });

    // ========================================
    // INITIALIZE ON PAGE LOAD
    // ========================================

    function init() {
        setActiveNavLink();
        initNavLinkHandlers();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();