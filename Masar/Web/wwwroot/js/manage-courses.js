let currentPage = 1;
let totalPages = 1;

async function loadCourses(page) {
    currentPage = page;
    const search = document.getElementById('searchCourses').value;
    const category = document.getElementById('categoryFilter').value;

    try {
        const res = await fetch(`/api/admin/courses?page=${page}&search=${search}&category=${category}`);
        const data = await res.json();

        totalPages = data.totalPages;
        renderTable(data.items);
        updateUI(data.totalCount);
    } catch (err) { console.error(err); }
}

function renderTable(courses) {
    const tbody = document.getElementById('coursesTableBody');
    tbody.innerHTML = '';
    
    if(!courses.length) { tbody.innerHTML = '<tr><td colspan="7" class="text-center">No courses found</td></tr>'; return; }

    courses.forEach(c => {
        tbody.innerHTML += `
            <tr>
                <td>#${c.id}</td>
                <td class="fw-bold">${c.title}</td>
                <td>
                    <div class="d-flex align-items-center">
                        <div class="user-avatar-sm me-2 bg-primary text-white rounded-circle d-flex justify-content-center align-items-center" style="width:25px;height:25px;font-size:12px">
                             ${c.instructorName.charAt(0)}
                        </div>
                        ${c.instructorName}
                    </div>
                </td>
                <td><span class="badge bg-info text-dark">${c.categories}</span></td>
                <td><i class="fas fa-users me-1 text-muted"></i> ${c.studentsCount}</td>
                <td>${new Date(c.createdDate).toLocaleDateString()}</td>
                <td>
                    <button class="btn btn-sm btn-danger" onclick="deleteCourse(${c.id})"><i class="fas fa-trash"></i></button>
                </td>
            </tr>`;
    });
}

async function deleteCourse(id) {
    if(!confirm("Delete this course?")) return;
    const res = await fetch(`/api/admin/courses/${id}`, { method: 'DELETE' });
    if(res.ok) loadCourses(currentPage);
}

function changePage(dir) {
    if(currentPage + dir > 0 && currentPage + dir <= totalPages) loadCourses(currentPage + dir);
}

function updateUI(count) {
    document.getElementById('coursesCount').innerText = `${count} courses`;
    document.getElementById('paginationInfo').innerText = `Page ${currentPage} of ${totalPages}`;
    document.getElementById('prevBtn').disabled = currentPage === 1;
    document.getElementById('nextBtn').disabled = currentPage === totalPages || totalPages === 0;
}

document.addEventListener('DOMContentLoaded', () => loadCourses(1));