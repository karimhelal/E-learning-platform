// ========================================
// BROWSE COURSES PAGE - JavaScript
// ======================================== ==

(function() {
    'use strict';

    // ========================================
    // VIEW TOGGLE (Grid/List)
    // ========================================
    
    function initViewToggle() {
        const viewBtns = document.querySelectorAll('.view-btn');
        const coursesCatalog = document.getElementById('coursesCatalog');

        viewBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const view = this.getAttribute('data-view');
                
                viewBtns.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                
                if (view === 'list') {
                    coursesCatalog.classList.add('list-view');
                } else {
                    coursesCatalog.classList.remove('list-view');
                }
                
                Masar.saveToStorage('preferredView', view);
            });
        });

        // Load saved preference
        const savedView = Masar.getFromStorage('preferredView');
        if (savedView === 'list') {
            document.querySelector('[data-view="list"]')?.click();
        }
    }

    // ========================================
    // DROPDOWN FILTERS
    // ========================================
    
    function initDropdowns() {
        const dropdowns = document.querySelectorAll('.filter-dropdown');

        dropdowns.forEach(dropdown => {
            const btn = dropdown.querySelector('.filter-dropdown-btn');
            const options = dropdown.querySelectorAll('.filter-option');

            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                
                // Close other dropdowns
                dropdowns.forEach(d => {
                    if (d !== dropdown) d.classList.remove('active');
                });
                
                dropdown.classList.toggle('active');
            });

            options.forEach(option => {
                option.addEventListener('click', function() {
                    // Update active state
                    options.forEach(o => o.classList.remove('active'));
                    this.classList.add('active');
                    
                    // Update button text
                    const text = this.textContent.trim();
                    const span = btn.querySelector('span');
                    if (span) {
                        span.textContent = text;
                    }
                    
                    // Close dropdown
                    dropdown.classList.remove('active');
                    
                    // Apply filter
                    applyFilters();
                });
            });
        });

        // Close dropdowns on outside click
        document.addEventListener('click', () => {
            dropdowns.forEach(d => d.classList.remove('active'));
        });
    }

    // ========================================
    // SEARCH FUNCTIONALITY
    // ========================================
    
    function initSearch() {
        const searchInput = document.getElementById('courseSearch');
        
        if (searchInput) {
            searchInput.addEventListener('input', Masar.debounce(function() {
                applyFilters();
            }, 300));
        }
    }

    // ========================================
    // APPLY FILTERS
    // ========================================
    
    function applyFilters() {
        const searchTerm = document.getElementById('courseSearch')?.value.toLowerCase() || '';
        const category = document.querySelector('[data-category].active')?.getAttribute('data-category') || 'all';
        const level = document.querySelector('[data-level].active')?.getAttribute('data-level') || 'all';
        
        const courseCards = document.querySelectorAll('.catalog-course-card');
        let visibleCount = 0;

        courseCards.forEach(card => {
            const cardCategory = card.getAttribute('data-category');
            const cardLevel = card.getAttribute('data-level');
            const title = card.querySelector('.course-card-title')?.textContent.toLowerCase() || '';
            const description = card.querySelector('.course-card-description')?.textContent.toLowerCase() || '';

            const matchesSearch = !searchTerm || title.includes(searchTerm) || description.includes(searchTerm);
            const matchesCategory = category === 'all' || cardCategory === category;
            const matchesLevel = level === 'all' || cardLevel === level;

            if (matchesSearch && matchesCategory && matchesLevel) {
                card.style.display = '';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Update results count
        const resultsCount = document.getElementById('resultsCount');
        if (resultsCount) {
            resultsCount.textContent = visibleCount;
        }

        // Show/hide clear filters button
        const hasFilters = searchTerm || category !== 'all' || level !== 'all';
        const clearBtn = document.getElementById('clearFilters');
        if (clearBtn) {
            clearBtn.style.display = hasFilters ? 'flex' : 'none';
        }
    }

    // ========================================
    // CLEAR FILTERS
    // ========================================
    
    function initClearFilters() {
        const clearBtn = document.getElementById('clearFilters');
        
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                // Reset search
                const searchInput = document.getElementById('courseSearch');
                if (searchInput) searchInput.value = '';

                // Reset dropdowns
                document.querySelectorAll('.filter-option').forEach(option => {
                    option.classList.remove('active');
                    if (option.getAttribute('data-category') === 'all' || 
                        option.getAttribute('data-level') === 'all' ||
                        option.getAttribute('data-sort') === 'popular') {
                        option.classList.add('active');
                    }
                });

                // Reset dropdown button texts
                updateDropdownTexts();

                // Apply filters
                applyFilters();

                Masar.showNotification('Filters cleared', 'info', 2000);
            });
        }
    }

    function updateDropdownTexts() {
        document.querySelector('#categoryDropdown span').textContent = 'All Categories';
        document.querySelector('#levelDropdown span').textContent = 'All Levels';
        document.querySelector('#sortDropdown span').textContent = 'Sort by: Popular';
    }

    // ========================================
    // ENROLL FUNCTIONALITY
    // ========================================
    
    function initEnrollButtons() {
        const enrollBtns = document.querySelectorAll('.enroll-btn');

        enrollBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                const courseId = this.getAttribute('data-course-id');
                const courseTitle = this.closest('.catalog-course-card').querySelector('.course-card-title').textContent;

                Masar.confirm(
                    `Are you sure you want to enroll in "${courseTitle}"?`,
                    () => {
                        enrollInCourse(courseId, courseTitle);
                    },
                    {
                        title: 'Enroll in Course',
                        confirmText: 'Enroll Now'
                    }
                );
            });
        });
    }

    function enrollInCourse(courseId, courseTitle) {
        // Show loading
        const btn = document.querySelector(`[data-course-id="${courseId}"]`);
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>Enrolling...</span>';
        }

        // Simulate enrollment (replace with actual form submission in MVC)
        setTimeout(() => {
            // In MVC, you would submit a form here
            /*
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Student/Course/Enroll';
            
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'courseId';
            input.value = courseId;
            form.appendChild(input);
            
            document.body.appendChild(form);
            form.submit();
            */

            // For demo, just show notification
            if (btn) {
                btn.innerHTML = '<i class="fas fa-check"></i><span>Enrolled!</span>';
                btn.classList.remove('btn-primary');
                btn.classList.add('btn-success');
            }

            Masar.showNotification(`Successfully enrolled in "${courseTitle}"!`, 'success');

            // Redirect after delay
            setTimeout(() => {
                // window.location.href = '/Student/MyCourses';
            }, 2000);
        }, 1500);
    }

    // ========================================
    // PAGINATION
    // ========================================
    
    function initPagination() {
        const paginationBtns = document.querySelectorAll('.pagination-btn:not([disabled])');

        paginationBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                paginationBtns.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                
                // Scroll to top
                window.scrollTo({ top: 0, behavior: 'smooth' });
                
                // In real implementation, load new page of results
                console.log('Loading page:', this.textContent);
            });
        });
    }

    // ========================================
    // INITIALIZE ON PAGE LOAD
    // ========================================
    
    function init() {
        initViewToggle();
        initDropdowns();
        initSearch();
        initClearFilters();
        initEnrollButtons();
        initPagination();

        // Initial filter application
        applyFilters();

        console.log('Browse Courses page initialized');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();