// ========================================
// MY TRACKS PAGE - JavaScript
// ======================================== ==

(function() {
    'use strict';

    // ========================================
    // FILTER FUNCTIONALITY
    // ========================================
    
    function initFilters() {
        const filterTabs = document.querySelectorAll('.filter-tab');
        const trackCards = document.querySelectorAll('.track-card-detailed');

        filterTabs.forEach(tab => {
            tab.addEventListener('click', function() {
                const filter = this.getAttribute('data-filter');
                
                // Update active tab
                filterTabs.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
                
                // Filter tracks
                trackCards.forEach(card => {
                    const status = card.getAttribute('data-status');
                    
                    if (filter === 'all') {
                        card.style.display = '';
                        card.classList.add('fade-in');
                    } else if (filter === status) {
                        card.style.display = '';
                        card.classList.add('fade-in');
                    } else {
                        card.style.display = 'none';
                        card.classList.remove('fade-in');
                    }
                });
            });
        });
    }

    // ========================================
    // INITIALIZE ON PAGE LOAD
    // ========================================
    
    function init() {
        initFilters();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();