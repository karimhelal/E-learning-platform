let allCourses = [];
let selectedCourseIds = [];

document.addEventListener('DOMContentLoaded', async () => {
    console.log('Page loaded, loading courses...');
    await loadCourses();
    setupDragAndDrop();
    document.getElementById('createTrackForm').addEventListener('submit', handleSubmit);
});

async function loadCourses() {
    const availableList = document.getElementById('availableCourses');

    try {
        console.log('Fetching courses from /api/admin/courses/simple');
        const res = await fetch('/api/admin/courses/simple');

        console.log('Response status:', res.status);
        console.log('Response OK?:', res.ok);

        if (!res.ok) {
            throw new Error(`HTTP error! status: ${res.status}`);
        }

        allCourses = await res.json();
        console.log('Courses loaded:', allCourses);
        console.log('Number of courses:', allCourses.length);

        if (allCourses.length === 0) {
            console.log('No courses found');
            availableList.innerHTML = '<div class="loading-text">No courses available</div>';
            return;
        }

        renderAvailableCourses(allCourses);

    } catch (error) {
        console.error('Error loading courses:', error);
        availableList.innerHTML = '<div class="loading-text">Failed to load courses: ' + error.message + '</div>';
    }
}

function renderAvailableCourses(courses) {
    console.log('Rendering available courses:', courses);
    const availableList = document.getElementById('availableCourses');
    
    // Filter out already selected courses
    const available = courses.filter(c => !selectedCourseIds.includes(c.id));

    console.log('Available courses after filtering:', available);

    if (available.length === 0) {
        availableList.innerHTML = '<div class="loading-text">No more courses available</div>';
        return;
    }

    availableList.innerHTML = available.map(course => `
        <div class="course-item" draggable="true" data-id="${course.id}" data-title="${course.title}">
            <i class="fas fa-grip-vertical drag-handle"></i>
            <span class="course-title">${course.title}</span>
        </div>
    `).join('');

    console.log('Rendered HTML:', availableList.innerHTML);

    // Re-attach drag events
    attachDragEvents();
}

function renderSelectedCourses() {
    const selectedList = document.getElementById('selectedCourses');
    const countEl = document.getElementById('selectedCount');

    if (selectedCourseIds.length === 0) {
        selectedList.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-hand-pointer"></i>
                <p>Drag courses here</p>
            </div>
        `;
        countEl.textContent = '0 selected';
        return;
    }

    selectedList.innerHTML = selectedCourseIds.map((id, index) => {
        const course = allCourses.find(c => c.id === id);
        return `
            <div class="course-item" draggable="true" data-id="${id}" data-title="${course?.title || 'Unknown'}">
                <i class="fas fa-grip-vertical drag-handle"></i>
                <span class="course-title">${course?.title || 'Unknown'}</span>
                <span class="course-order">#${index + 1}</span>
                <button type="button" class="remove-btn" onclick="removeCourse(${id})">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
    }).join('');

    countEl.textContent = `${selectedCourseIds.length} selected`;

    // Re-attach drag events for reordering
    attachDragEvents();
}

function setupDragAndDrop() {
    const availableList = document.getElementById('availableCourses');
    const selectedList = document.getElementById('selectedCourses');

    // Drop zone for selected courses
    selectedList.addEventListener('dragover', (e) => {
        e.preventDefault();
        selectedList.classList.add('drag-over');
    });

    selectedList.addEventListener('dragleave', () => {
        selectedList.classList.remove('drag-over');
    });

    selectedList.addEventListener('drop', (e) => {
        e.preventDefault();
        selectedList.classList.remove('drag-over');

        const courseId = parseInt(e.dataTransfer.getData('text/plain'));
        const sourceList = e.dataTransfer.getData('source');

        if (sourceList === 'available' && !selectedCourseIds.includes(courseId)) {
            selectedCourseIds.push(courseId);
            renderSelectedCourses();
            renderAvailableCourses(allCourses);
        } else if (sourceList === 'selected') {
            // Reorder within selected list
            handleReorder(e, courseId);
        }
    });

    // Drop zone for available courses (to remove from selected)
    availableList.addEventListener('dragover', (e) => {
        e.preventDefault();
        availableList.classList.add('drag-over');
    });

    availableList.addEventListener('dragleave', () => {
        availableList.classList.remove('drag-over');
    });

    availableList.addEventListener('drop', (e) => {
        e.preventDefault();
        availableList.classList.remove('drag-over');

        const courseId = parseInt(e.dataTransfer.getData('text/plain'));
        const sourceList = e.dataTransfer.getData('source');

        if (sourceList === 'selected') {
            removeCourse(courseId);
        }
    });
}

function attachDragEvents() {
    document.querySelectorAll('#availableCourses .course-item').forEach(item => {
        item.addEventListener('dragstart', (e) => {
            e.target.classList.add('dragging');
            e.dataTransfer.setData('text/plain', e.target.dataset.id);
            e.dataTransfer.setData('source', 'available');
        });

        item.addEventListener('dragend', (e) => {
            e.target.classList.remove('dragging');
        });
    });

    document.querySelectorAll('#selectedCourses .course-item').forEach(item => {
        item.addEventListener('dragstart', (e) => {
            e.target.classList.add('dragging');
            e.dataTransfer.setData('text/plain', e.target.dataset.id);
            e.dataTransfer.setData('source', 'selected');
        });

        item.addEventListener('dragend', (e) => {
            e.target.classList.remove('dragging');
        });
    });
}

function handleReorder(e, draggedId) {
    const selectedList = document.getElementById('selectedCourses');
    const items = [...selectedList.querySelectorAll('.course-item')];
    
    const draggedIndex = selectedCourseIds.indexOf(draggedId);
    
    // Find the drop position
    let dropIndex = items.length;
    for (let i = 0; i < items.length; i++) {
        const rect = items[i].getBoundingClientRect();
        if (e.clientY < rect.top + rect.height / 2) {
            dropIndex = i;
            break;
        }
    }

    // Remove from old position and insert at new position
    selectedCourseIds.splice(draggedIndex, 1);
    if (dropIndex > draggedIndex) dropIndex--;
    selectedCourseIds.splice(dropIndex, 0, draggedId);

    renderSelectedCourses();
}

function removeCourse(courseId) {
    selectedCourseIds = selectedCourseIds.filter(id => id !== courseId);
    renderSelectedCourses();
    renderAvailableCourses(allCourses);
}

function filterCourses() {
    const searchTerm = document.getElementById('courseSearch').value.toLowerCase();
    const filtered = allCourses.filter(c =>
        c.title.toLowerCase().includes(searchTerm)
    );
    renderAvailableCourses(filtered);
}

async function handleSubmit(e) {
    e.preventDefault();

    const title = document.getElementById('title').value.trim();

    if (!title) {
        Swal.fire({
            title: 'Error',
            text: 'Title is required.',
            icon: 'warning',
            background: '#151b38',
            color: '#f8fafc'
        });
        return;
    }

    const payload = {
        title: title,
        description: document.getElementById('description').value.trim() || null,
        status: parseInt(document.getElementById('status').value),
        courseIds: selectedCourseIds
    };

    console.log('Submitting payload:', payload);

    try {
        const res = await fetch('/api/admin/tracks', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        console.log('Response status:', res.status);

        if (res.ok) {
            await Swal.fire({
                title: 'Success!',
                text: 'Track created successfully.',
                icon: 'success',
                background: '#151b38',
                color: '#f8fafc',
                confirmButtonColor: '#10b981'
            });
            window.location.href = '/Admin/Tracks';
        } else {
            const err = await res.json();
            console.error('Error response:', err);
            Swal.fire({
                title: 'Error',
                text: err.message || 'Failed to create track.',
                icon: 'error',
                background: '#151b38',
                color: '#f8fafc'
            });
        }
    } catch (error) {
        console.error('Submit error:', error);
        Swal.fire({
            title: 'Error',
            text: 'Connection failed: ' + error.message,
            icon: 'error',
            background: '#151b38',
            color: '#f8fafc'
        });
    }
}