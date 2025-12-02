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
    if (!tbody) return;

    tbody.innerHTML = '';

    if (!users || users.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center" style="padding:40px; color: #94a3b8;">No users found</td></tr>';
        return;
    }

    users.forEach(u => {
        // 1. قراءة البيانات بأمان
        const id = u.id || u.Id;
        const fullName = u.fullName || u.FullName || "Unknown";
        const email = u.email || u.Email || "";
        const joinedDate = u.joinedDate || u.JoinedDate;

        // 2. تجهيز مصفوفة الرولز
        let rolesArray = [];
        if (Array.isArray(u.roles)) rolesArray = u.roles;
        else if (Array.isArray(u.Roles)) rolesArray = u.Roles;
        else if (typeof u.roles === 'string') rolesArray = [u.roles];

        // 3. فحص هل المستخدم أدمن؟ (الخطوة الأهم) 🔥
        // بنحول الكلام لحروف صغيرة عشان نضمن المقارنة (Admin = admin)
        const isAdmin = rolesArray.some(r => r.toLowerCase() === 'admin');

        // 4. تجهيز HTML الرولز (Badges)
        let rolesHtml = '';
        rolesArray.forEach(r => {
            let badgeClass = 'student';
            const lowerRole = r.toLowerCase();

            if (lowerRole === 'admin') badgeClass = 'admin';
            else if (lowerRole === 'instructor') badgeClass = 'instructor';

            rolesHtml += `<span class="role-badge ${badgeClass} me-1">${r}</span>`;
        });

        const dateDisplay = joinedDate ? new Date(joinedDate).toLocaleDateString() : '-';

        // 5. تحديد شكل زرار الأكشن بناءً على هل هو أدمن ولا لأ 🔥
        let actionHtml = '';

        if (isAdmin) {
            // لو أدمن: اظهر كلمة "محمي" مع أيقونة درع
            actionHtml = `
                <div style="color: #64748b; font-size: 0.85rem; display: flex; align-items: center; gap: 6px;">
                    <i class="fas fa-user-shield" style="color: #f59e0b;"></i> 
                    <span>Protected</span>
                </div>
            `;
        } else {
            // لو مش أدمن: اظهر زر الحذف عادي
            actionHtml = `
                <button class="btn-delete-outline" onclick="deleteUser('${id}')">
                    <i class="fas fa-trash-alt"></i> Delete
                </button>
            `;
        }

        // 6. رسم الصف
        tbody.innerHTML += `
            <tr>
                <td><span class="user-id-text">U${id}</span></td>
                <td style="font-weight: 500;">${fullName}</td>
                <td style="color: #94a3b8;">${email}</td>
                <td>${rolesHtml}</td>
                <td>${dateDisplay}</td>
                <td>${actionHtml}</td> </tr>
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