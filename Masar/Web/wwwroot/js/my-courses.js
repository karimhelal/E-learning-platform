// ========================================
// MY COURSES PAGE - JavaScript
// ========================================

(function() {
    'use strict';

    // ========================================
    // STATE MANAGEMENT
    // ========================================
    
    let state = {
        activeFilter: 'all',
        searchTerm: ''
    };

    // ========================================
    // DOM ELEMENTS
    // ========================================
    
    const elements = {
        filterTabs: document.querySelectorAll('.filter-tab'),
        courseCards: document.querySelectorAll('.course-card-full'),
        searchInput: document.getElementById('courseSearchInput'),
        coursesGrid: document.querySelector('.courses-grid-full')
    };

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

    function saveToStorage(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
        } catch (e) {
            console.warn('Could not save to localStorage:', e);
        }
    }

    function getFromStorage(key, defaultValue) {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : defaultValue;
        } catch (e) {
            console.warn('Could not read from localStorage:', e);
            return defaultValue;
        }
    }

    // ========================================
    // FILTER FUNCTIONALITY
    // ========================================
    
    function initFilters() {
        if (!elements.filterTabs.length) return;

        elements.filterTabs.forEach(tab => {
            tab.addEventListener('click', handleFilterClick);
        });

        // Load saved filter preference
        const savedFilter = getFromStorage('myCourses_filter', 'all');
        if (savedFilter !== 'all') {
            const savedTab = document.querySelector(`[data-filter="${savedFilter}"]`);
            if (savedTab) {
                // Trigger click without animation on page load
                handleFilterClick({ currentTarget: savedTab }, true);
            }
        }
    }

    function handleFilterClick(e, silent = false) {
        const tab = e.currentTarget;
        const filter = tab.getAttribute('data-filter');

        // Update active state
        elements.filterTabs.forEach(t => {
            t.classList.remove('active');
        });
        
        tab.classList.add('active');

        // Update state
        state.activeFilter = filter;
        
        // Save preference
        saveToStorage('myCourses_filter', filter);

        // Apply filters
        applyFilters();

        if (!silent) {
            console.log(`Filter changed to: ${filter}`);
        }
    }

    function getFilteredCount(filter) {
        if (filter === 'all') return elements.courseCards.length;
        return Array.from(elements.courseCards).filter(card => {
            const status = card.getAttribute('data-status');
            return status === filter || status === filter.replace('-', '');
        }).length;
    }

    // ========================================
    // SEARCH FUNCTIONALITY
    // ========================================
    
    function initSearch() {
        if (!elements.searchInput) return;

        // Debounced search
        const debouncedSearch = debounce(handleSearch, 300);
        elements.searchInput.addEventListener('input', debouncedSearch);

        // Clear search on Escape key
        elements.searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                elements.searchInput.value = '';
                handleSearch({ target: elements.searchInput });
                elements.searchInput.blur();
            }
        });
    }

    function handleSearch(e) {
        const searchTerm = e.target.value.toLowerCase().trim();
        state.searchTerm = searchTerm;

        applyFilters();

        // Log search results
        const visibleCount = Array.from(elements.courseCards).filter(card => 
            card.style.display !== 'none'
        ).length;

        if (searchTerm) {
            console.log(`Search: "${searchTerm}" - Found ${visibleCount} course(s)`);
        }
    }

    // ========================================
    // APPLY FILTERS
    // ========================================
    
    function applyFilters() {
        let visibleCount = 0;

        elements.courseCards.forEach((card) => {
            const status = card.getAttribute('data-status');
            const title = card.querySelector('.course-card-title')?.textContent.toLowerCase() || '';
            const description = card.querySelector('.course-card-description')?.textContent.toLowerCase() || '';
            const instructor = card.querySelector('.meta-item span')?.textContent.toLowerCase() || '';

            // Normalize status for comparison (handle both "in-progress" and "inprogress")
            const normalizedStatus = status ? status.replace('-', '').toLowerCase() : '';
            const normalizedFilter = state.activeFilter.replace('-', '').toLowerCase();

            // Check filter match
            const matchesFilter = state.activeFilter === 'all' || 
                                normalizedStatus === normalizedFilter ||
                                status === state.activeFilter;
            
            // Check search match
            const matchesSearch = !state.searchTerm || 
                title.includes(state.searchTerm) || 
                description.includes(state.searchTerm) ||
                instructor.includes(state.searchTerm);

            if (matchesFilter && matchesSearch) {
                card.style.display = '';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Show/hide empty state
        updateEmptyState(visibleCount);
    }

    // ========================================
    // EMPTY STATE
    // ========================================
    
    function updateEmptyState(visibleCount) {
        let emptyState = elements.coursesGrid?.querySelector('.empty-state-dynamic');
        
        if (visibleCount === 0 && (state.searchTerm || state.activeFilter !== 'all')) {
            // Create dynamic empty state for search/filter
            if (!emptyState) {
                emptyState = document.createElement('div');
                emptyState.className = 'empty-state empty-state-dynamic';
                elements.coursesGrid?.appendChild(emptyState);
            }

            const message = state.searchTerm 
                ? `No courses found matching "<strong>${escapeHtml(state.searchTerm)}</strong>"`
                : `No ${state.activeFilter.replace('-', ' ')} courses found`;

            emptyState.innerHTML = `
                <i class="fas fa-search"></i>
                <p>${message}</p>
                <button class="btn btn-secondary" onclick="window.myCoursesApp.clearFilters()">
                    <i class="fas fa-redo"></i>
                    <span>Clear Filters</span>
                </button>
            `;
            
            emptyState.style.display = 'flex';
        } else if (emptyState) {
            emptyState.remove();
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // ========================================
    // CLEAR FILTERS
    // ========================================
    
    function clearFilters() {
        // Reset search
        if (elements.searchInput) {
            elements.searchInput.value = '';
        }
        
        state.searchTerm = '';

        // Reset to "All" filter
        const allTab = document.querySelector('[data-filter="all"]');
        if (allTab && !allTab.classList.contains('active')) {
            allTab.click();
        } else {
            state.activeFilter = 'all';
            applyFilters();
        }

        console.log('Filters cleared');
    }

    // ========================================
    // COURSE CARD INTERACTIONS
    // ========================================
    
    function initCardInteractions() {
        elements.courseCards.forEach(card => {
            // Animate progress bars on viewport entry
            const progressBar = card.querySelector('.progress-bar');
            if (progressBar) {
                const targetWidth = progressBar.style.width;
                progressBar.style.width = '0%';
                
                const observer = new IntersectionObserver((entries) => {
                    entries.forEach(entry => {
                        if (entry.isIntersecting) {
                            setTimeout(() => {
                                progressBar.style.width = targetWidth;
                            }, 100);
                            observer.unobserve(entry.target);
                        }
                    });
                }, { threshold: 0.1 });
                
                observer.observe(card);
            }
        });
    }

    // ========================================
    // KEYBOARD SHORTCUTS
    // ========================================
    
    function initKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Don't trigger if user is typing in an input
            if (e.target.matches('input, textarea')) return;

            // Ctrl/Cmd + K: Focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                elements.searchInput?.focus();
            }

            // Number keys (1-3): Switch filters
            if (e.key === '1') {
                document.querySelector('[data-filter="all"]')?.click();
            } else if (e.key === '2') {
                document.querySelector('[data-filter="in-progress"]')?.click();
            } else if (e.key === '3') {
                document.querySelector('[data-filter="completed"]')?.click();
            }
        });
    }

    // ========================================
    // INITIALIZE
    // ========================================
    
    function init() {
        console.log('🎓 My Courses - Initializing...');

        // Check if required elements exist
        if (!elements.coursesGrid) {
            console.warn('Courses grid not found');
            return;
        }

        // Initialize all features
        initFilters();
        initSearch();
        initCardInteractions();
        initKeyboardShortcuts();

        // Log stats
        console.log('📊 Courses loaded:', {
            total: elements.courseCards.length,
            inProgress: getFilteredCount('in-progress'),
            completed: getFilteredCount('completed')
        });

        console.log('✅ My Courses - Ready!');
    }

    // ========================================
    // PUBLIC API
    // ========================================
    
    window.myCoursesApp = {
        clearFilters: clearFilters,
        applyFilters: applyFilters,
        getState: () => state
    };

    // ========================================
    // AUTO-INITIALIZE
    // ========================================
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Filter tabs functionality
    const filterTabs = document.querySelectorAll('.filter-tab');
    const courseCards = document.querySelectorAll('.course-card-full');

    filterTabs.forEach(tab => {
        tab.addEventListener('click', function() {
            // Remove active class from all tabs
            filterTabs.forEach(t => t.classList.remove('active'));
            
            // Add active class to clicked tab
            this.classList.add('active');
            
            // Get filter value
            const filter = this.dataset.filter;
            
            // Filter courses
            courseCards.forEach(card => {
                const status = card.dataset.status;
                
                if (filter === 'all') {
                    card.style.display = 'flex';
                } else if (filter === 'not-started' && status === 'notstarted') {
                    card.style.display = 'flex';
                } else if (filter === 'in-progress' && status === 'inprogress') {
                    card.style.display = 'flex';
                } else if (filter === 'completed' && status === 'completed') {
                    card.style.display = 'flex';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    });

    // Search functionality
    const searchInput = document.getElementById('courseSearchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            
            courseCards.forEach(card => {
                const title = card.querySelector('.course-card-title').textContent.toLowerCase();
                const description = card.querySelector('.course-card-description').textContent.toLowerCase();
                const instructor = card.querySelector('.meta-item span').textContent.toLowerCase();
                
                if (title.includes(searchTerm) || description.includes(searchTerm) || instructor.includes(searchTerm)) {
                    card.style.display = 'flex';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }

})();