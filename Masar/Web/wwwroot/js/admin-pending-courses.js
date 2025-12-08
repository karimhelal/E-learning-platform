let allCourses = [];

document.addEventListener('DOMContentLoaded', () => {
    loadPendingCourses();
});

// 1. Load Data
async function loadPendingCourses() {
    const container = document.getElementById('pendingCoursesList');
    container.innerHTML = '<div class="text-center p-5"><i class="fas fa-spinner fa-spin fa-3x text-muted"></i></div>';

    try {
        const response = await fetch('/api/admin/pending-courses');
        if (!response.ok) throw new Error("Failed to load");

        allCourses = await response.json();
        
        document.getElementById('stats-pending').innerText = allCourses.length;

        renderCourses(allCourses);

    } catch (error) {
        console.error(error);
        container.innerHTML = '<div class="text-center text-danger p-5">Error loading courses.</div>';
    }
}

// 2. Render Cards (رسم الكروت)
function renderCourses(courses) {
    const container = document.getElementById('pendingCoursesList');
    const emptyState = document.getElementById('emptyState');

    container.innerHTML = '';

    if (courses.length === 0) {
        container.style.display = 'none';
        emptyState.style.display = 'block';
        return;
    }

    container.style.display = 'grid'; // تأكد من الستايل في الـ CSS
    emptyState.style.display = 'none';

    courses.forEach(c => {
        // تنسيق التاريخ والوقت
        const date = new Date(c.createdDate).toLocaleDateString();
        const timeAgo = getTimeAgo(new Date(c.createdDate));

        const card = `
            <div class="pending-course-card">
                <div class="course-review-header">
                    <div class="course-thumbnail-small">
                        <img src="${c.thumbnailImageUrl}" alt="Thumb" onerror="this.src='/images/default-course.png'">
                    </div>
                    <div class="course-main-info">
                        <div class="course-title-row">
                            <h3 class="course-title">${c.title}</h3>
                            <span class="time-badge"><i class="fas fa-clock"></i> ${timeAgo}</span>
                        </div>
                        <div class="instructor-info">
                            <div class="avatar avatar-sm" style="background:#7c3aed;color:white;">${c.instructorName.charAt(0)}</div>
                            <div class="instructor-details">
                                <div class="instructor-name">${c.instructorName}</div>
                                <div class="instructor-meta">
                                    <span><i class="fas fa-book"></i> ${c.instructorCoursesCount} courses</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="course-review-details">
                    <div class="course-meta-grid">
                        <div class="meta-item"><i class="fas fa-layer-group"></i> <span class="meta-value">${c.category || 'General'}</span></div>
                        <div class="meta-item"><i class="fas fa-book-open"></i> <span class="meta-value">${c.modulesCount} Modules</span></div>
                        <div class="meta-item"><i class="fas fa-play-circle"></i> <span class="meta-value">${c.lessonsCount} Lessons</span></div>
                        <div class="meta-item"><i class="fas fa-signal"></i> <span class="meta-value">${c.level}</span></div>
                    </div>
                </div>

                <div class="course-review-actions">
                    <a href="/Course/${c.id}" target="_blank" class="btn btn-secondary btn-view">
                        <i class="fas fa-eye"></i> View
                    </a>
                    <button class="btn btn-danger btn-reject" onclick="openRejectModal(${c.id})">
                        <i class="fas fa-times"></i> Reject
                    </button>
                    <button class="btn btn-success btn-approve" onclick="approveCourse(${c.id})">
                        <i class="fas fa-check"></i> Approve
                    </button>
                </div>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', card);
    });
}

// 3. Approve Action
async function approveCourse(id) {
    const result = await Swal.fire({
        title: 'Approve Course?',
        text: "This course will be published immediately.",
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#10b981',
        confirmButtonText: 'Yes, Publish!'
    });

    if (result.isConfirmed) {
        try {
            await fetch(`/api/admin/courses/${id}/approve`, { method: 'POST' });
            Swal.fire('Published!', 'Course is live now.', 'success');
            loadPendingCourses(); // Refresh
        } catch (e) {
            Swal.fire('Error', 'Something went wrong', 'error');
        }
    }
}

// 4. Reject Logic (Modal)
function openRejectModal(id) {
    document.getElementById('rejectCourseId').value = id;
    document.getElementById('rejectionModal').style.display = 'flex';
}

function closeRejectionModal() {
    document.getElementById('rejectionModal').style.display = 'none';
}

async function submitRejection() {
    const id = document.getElementById('rejectCourseId').value;

    // تجميع السبب من القائمة ومن النص
    const reasonSelect = document.getElementById('rejectionReason').value;
    const reasonText = document.getElementById('rejectionDetails').value;
    const fullReason = reasonSelect ? `${reasonSelect}: ${reasonText}` : reasonText;

    if (!fullReason) {
        alert("Please provide a reason.");
        return;
    }

    try {
        // التعديل هنا: إضافة Headers و Body
        const response = await fetch(`/api/admin/courses/${id}/reject`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json' // مهم جداً
            },
            body: JSON.stringify({ reason: fullReason }) // إرسال الـ DTO
        });

        if (!response.ok) throw new Error("Failed");

        closeRejectionModal();
        // استخدام SweetAlert اللي انت ضايفه
        Swal.fire('Rejected', 'Course has been rejected.', 'success');
        loadPendingCourses();

        // تصفير النموذج
        document.getElementById('rejectionForm').reset();

    } catch (e) {
        console.error(e);
        Swal.fire('Error', 'Error rejecting course', 'error');
    }
}

// 5. Search & Filter (Client-Side for speed)
function filterCourses() {
    const search = document.getElementById('courseSearch').value.toLowerCase();
    const category = document.getElementById('categoryFilter').value;

    const filtered = allCourses.filter(c => {
        const matchesSearch = c.title.toLowerCase().includes(search) || c.instructorName.toLowerCase().includes(search);
        const matchesCategory = category === 'all' || c.category === category;
        return matchesSearch && matchesCategory;
    });

    renderCourses(filtered);
}

// Helper: Time Ago
function getTimeAgo(date) {
    const seconds = Math.floor((new Date() - date) / 1000);
    let interval = seconds / 31536000;
    if (interval > 1) return Math.floor(interval) + " years ago";
    interval = seconds / 2592000;
    if (interval > 1) return Math.floor(interval) + " months ago";
    interval = seconds / 86400;
    if (interval > 1) return Math.floor(interval) + " days ago";
    interval = seconds / 3600;
    if (interval > 1) return Math.floor(interval) + " hours ago";
    interval = seconds / 60;
    if (interval > 1) return Math.floor(interval) + " minutes ago";
    return "Just now";
}