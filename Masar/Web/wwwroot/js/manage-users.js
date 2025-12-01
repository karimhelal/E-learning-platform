// متغيرات للتحكم في الصفحات
let currentPage = 1;
let totalPages = 1;

// ==================== 1. LOAD USERS (Main Function) ====================
async function loadUsers(page) {
    currentPage = page;

    // محاولة قراءة عناصر البحث (لو موجودة في الصفحة)
    const searchInput = document.getElementById('searchUsers');
    const roleInput = document.getElementById('roleFilter');

    const search = searchInput ? searchInput.value : "";
    const role = roleInput ? roleInput.value : "all";

    try {
        // الاتصال بالـ API
        const response = await fetch(`/api/admin/users?page=${page}&search=${search}&role=${role}`);

        if (!response.ok) throw new Error("Failed to fetch data");

        const data = await response.json();

        // تحديث المتغيرات العامة
        totalPages = data.totalPages;

        // رسم الجدول
        renderTable(data.items);

        // تحديث أزرار التنقل والعداد (لو العناصر موجودة في الـ HTML)
        if (document.getElementById('usersCount'))
            document.getElementById('usersCount').innerText = `${data.totalCount} users`;

        if (document.getElementById('paginationInfo'))
            document.getElementById('paginationInfo').innerText = `Page ${currentPage} of ${totalPages}`;

        const prevBtn = document.getElementById('prevBtn');
        const nextBtn = document.getElementById('nextBtn');

        if (prevBtn) prevBtn.disabled = currentPage === 1;
        if (nextBtn) nextBtn.disabled = currentPage === totalPages || totalPages === 0;

    } catch (error) {
        console.error("Error:", error);
        // عرض رسالة خطأ داخل الجدول في حالة فشل الاتصال
        const tbody = document.getElementById('usersTableBody');
        if (tbody) tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger p-4">Error loading data. Check console.</td></tr>`;
    }
}

// ==================== 2. RENDER TABLE (New Design) ====================
function renderTable(users) {
    const tbody = document.getElementById('usersTableBody');
    if (!tbody) return; // حماية لو الجدول مش موجود

    tbody.innerHTML = '';

    if (!users || users.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center" style="padding:40px; color: #94a3b8;">No users found</td></tr>';
        return;
    }

    users.forEach(u => {
        // قراءة البيانات بأمان (التعامل مع الحروف الكبيرة والصغيرة)
        const id = u.id || u.Id;
        const fullName = u.fullName || u.FullName || "Unknown";
        const email = u.email || u.Email || "";
        const joinedDate = u.joinedDate || u.JoinedDate;

        // التأكد من أن الرولز مصفوفة
        let rolesArray = [];
        if (Array.isArray(u.roles)) rolesArray = u.roles;
        else if (Array.isArray(u.Roles)) rolesArray = u.Roles;
        else if (typeof u.roles === 'string') rolesArray = [u.roles];

        // تنسيق التاريخ
        const dateDisplay = joinedDate ? new Date(joinedDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : '-';

        // تنسيق الرولز (Badges)
        let rolesHtml = '';
        rolesArray.forEach(r => {
            let badgeClass = 'student'; // Default styling class
            const lowerRole = r.toLowerCase();

            if (lowerRole === 'admin') badgeClass = 'admin';
            else if (lowerRole === 'instructor') badgeClass = 'instructor';

            rolesHtml += `<span class="role-badge ${badgeClass} me-1">${r}</span>`;
        });

        // رسم الصف (بالتصميم الجديد)
        tbody.innerHTML += `
            <tr>
                <td><span class="user-id-text">U${id}</span></td>
                <td style="font-weight: 500;">${fullName}</td>
                <td style="color: #94a3b8;">${email}</td>
                <td>${rolesHtml}</td>
                <td>${dateDisplay}</td>
                <td>
                    <button class="btn-delete-outline" onclick="deleteUser('${id}')">
                        <i class="fas fa-trash-alt"></i> Delete
                    </button>
                </td>
            </tr>
        `;
    });
}

// ==================== 3. DELETE USER (With SweetAlert) ====================
async function deleteUser(id) {
    const result = await Swal.fire({
        title: 'Are you sure?',
        text: "You are about to delete this user permanently.",
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
            const response = await fetch(`/api/admin/users/${id}`, { method: 'DELETE' });

            if (response.ok) {
                await Swal.fire({
                    title: 'Deleted!',
                    text: 'User has been removed.',
                    icon: 'success',
                    background: '#151b38',
                    color: '#f8fafc',
                    confirmButtonColor: '#10b981'
                });
                loadUsers(currentPage); // إعادة تحميل البيانات
            } else {
                let errorMessage = "Could not delete user. They might have related data.";
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

// ==================== 4. PAGINATION HELPER ====================
function changePage(direction) {
    let newPage = currentPage + direction;
    if (newPage > 0 && newPage <= totalPages) {
        loadUsers(newPage);
    }
}

// ==================== 5. INITIALIZE (تشغيل عند البدء) ====================
document.addEventListener('DOMContentLoaded', () => {
    loadUsers(1);
});

function changePage(dir) {
    if(currentPage + dir > 0 && currentPage + dir <= totalPages) loadUsers(currentPage + dir);
}

function updateUI(count) {
    document.getElementById('usersCount').innerText = `${count} users`;
    document.getElementById('paginationInfo').innerText = `Page ${currentPage} of ${totalPages}`;
    document.getElementById('prevBtn').disabled = currentPage === 1;
    document.getElementById('nextBtn').disabled = currentPage === totalPages || totalPages === 0;
}

document.addEventListener('DOMContentLoaded', () => loadUsers(1));