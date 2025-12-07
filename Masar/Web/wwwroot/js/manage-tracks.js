let currentPage = 1;
let totalPages = 1;

async function loadTracks(page) {
    currentPage = page;

    const searchInput = document.getElementById('searchTracks');
    const search = searchInput ? searchInput.value : "";

    try {
        const response = await fetch(`/api/admin/tracks?page=${page}&search=${search}`);

        if (!response.ok) throw new Error("Failed to fetch data");

        const data = await response.json();

        totalPages = data.totalPages;

        renderTable(data.items);
        updateUI(data.totalCount);

    } catch (error) {
        console.error("Error:", error);
        const tbody = document.getElementById('tracksTableBody');
        if (tbody) tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger p-4">Error loading data. Check console.</td></tr>`;
    }
}

function renderTable(tracks) {
    const tbody = document.getElementById('tracksTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (!tracks || tracks.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-center" style="padding:40px; color: #94a3b8;">No tracks found</td></tr>';
        return;
    }

    tracks.forEach(t => {
        const id = t.id || t.Id;
        const title = t.title || t.Title || "Untitled";
        const description = t.description || t.Description || "-";
        const truncatedDesc = description.length > 50 ? description.substring(0, 50) + "..." : description;
        const coursesCount = t.coursesCount ?? t.CoursesCount ?? 0;
        const studentsCount = t.studentsCount ?? t.StudentsCount ?? 0;
        const createdDate = t.createdDate || t.CreatedDate;
        const dateDisplay = createdDate ? new Date(createdDate).toLocaleDateString() : '-';

        tbody.innerHTML += `
            <tr>
                <td><span class="user-id-text">T${id}</span></td>
                <td style="font-weight: 500; color: #f8fafc;">${title}</td>
                <td style="color: #94a3b8;" title="${description}">${truncatedDesc}</td>
                <td>
                    <span class="role-badge instructor">
                        <i class="fas fa-book me-1"></i> ${coursesCount}
                    </span>
                </td>
                <td>
                    <span class="role-badge student">
                        <i class="fas fa-users me-1"></i> ${studentsCount}
                    </span>
                </td>
                <td style="color: #94a3b8;">${dateDisplay}</td>
                <td>
                    <button class="btn-delete-outline" onclick="deleteTrack(${id})">
                        <i class="fas fa-trash-alt"></i> Delete
                    </button>
                </td>
            </tr>
        `;
    });
}

async function deleteTrack(id) {
    const result = await Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this track permanently.",
        icon: 'warning',
        background: '#151b38',
        color: '#f8fafc',
        showCancelButton: true,
        confirmButtonColor: '#ef4444',
        cancelButtonColor: '#334155',
        confirmButtonText: 'Yes, delete it!'
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(`/api/admin/tracks/${id}`, { method: 'DELETE' });

            if (response.ok) {
                await Swal.fire({
                    title: 'Deleted!',
                    text: 'Track has been removed.',
                    icon: 'success',
                    background: '#151b38',
                    color: '#f8fafc',
                    confirmButtonColor: '#10b981'
                });
                loadTracks(currentPage);
            } else {
                let errorMessage = "Could not delete track. It might have related data.";
                try {
                    const errorData = await response.json();
                    if (errorData.message) errorMessage = errorData.message;
                } catch (e) { }

                Swal.fire({
                    title: 'Action Failed',
                    text: errorMessage,
                    icon: 'error',
                    background: '#151b38',
                    color: '#f8fafc',
                    confirmButtonColor: '#ef4444',
                    confirmButtonText: 'Understood'
                });
            }
        } catch (error) {
            console.error(error);
            Swal.fire({
                title: 'System Error',
                text: 'Connection failed.',
                icon: 'error',
                background: '#151b38',
                color: '#f8fafc'
            });
        }
    }
}

function changePage(direction) {
    let newPage = currentPage + direction;
    if (newPage > 0 && newPage <= totalPages) {
        loadTracks(newPage);
    }
}

function updateUI(count) {
    document.getElementById('tracksCount').innerText = `${count} tracks`;
    document.getElementById('paginationInfo').innerText = `Page ${currentPage} of ${totalPages}`;
    document.getElementById('prevBtn').disabled = currentPage === 1;
    document.getElementById('nextBtn').disabled = currentPage === totalPages || totalPages === 0;
}

document.addEventListener('DOMContentLoaded', () => loadTracks(1));