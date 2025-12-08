// Course Details Page JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Tab Navigation
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const tabId = btn.dataset.tab;
            
            // Remove active class from all tabs
            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));
            
            // Add active class to clicked tab
            btn.classList.add('active');
            document.getElementById(tabId).classList.add('active');
        });
    });

    // Module Toggle
    const moduleItems = document.querySelectorAll('.module-item');
    
    moduleItems.forEach(item => {
        const header = item.querySelector('.module-header');
        
        header.addEventListener('click', () => {
            item.classList.toggle('open');
        });
    });
});

// Enroll in Course Function
function enrollCourse(courseId) {
    const enrollBtn = document.querySelector(`[onclick="enrollCourse(${courseId})"]`);
    
    // Check if user is logged in first
    fetch(`/api/enrollment/status/${courseId}`)
        .then(response => response.json())
        .then(statusData => {
            if (!statusData.isLoggedIn) {
                // Redirect to login with return URL
                window.location.href = `/Account/Login?ReturnUrl=/course/${courseId}`;
                return;
            }
            
            if (statusData.isEnrolled) {
                // Already enrolled, redirect to course
                window.location.href = statusData.courseUrl || `/student/course/details/${courseId}`;
                return;
            }
            
            // Show loading state
            if (enrollBtn) {
                enrollBtn.disabled = true;
                enrollBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Enrolling...';
            }
            
            // Proceed with enrollment
            fetch(`/api/enrollment/enroll/${courseId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Show success message
                    showToast(data.message || 'Successfully enrolled!', 'success');
                    
                    // Update button to show enrolled state
                    if (enrollBtn) {
                        enrollBtn.innerHTML = '<i class="fas fa-check"></i> Enrolled!';
                        enrollBtn.classList.remove('btn-primary');
                        enrollBtn.classList.add('btn-success');
                    }
                    
                    // Redirect to course after a short delay
                    setTimeout(() => {
                        window.location.href = data.redirectUrl || `/student/course/details/${courseId}`;
                    }, 1500);
                } else {
                    showToast(data.message || 'Failed to enroll. Please try again.', 'error');
                    
                    // Reset button
                    if (enrollBtn) {
                        enrollBtn.disabled = false;
                        enrollBtn.innerHTML = '<i class="fas fa-plus"></i> Enroll Now';
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showToast('An error occurred. Please try again.', 'error');
                
                // Reset button
                if (enrollBtn) {
                    enrollBtn.disabled = false;
                    enrollBtn.innerHTML = '<i class="fas fa-plus"></i> Enroll Now';
                }
            });
        })
        .catch(error => {
            // If status check fails, try direct enrollment
            console.error('Status check error:', error);
            
            // Show loading state
            if (enrollBtn) {
                enrollBtn.disabled = true;
                enrollBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Enrolling...';
            }
            
            fetch(`/api/enrollment/enroll/${courseId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message || 'Successfully enrolled!', 'success');
                    setTimeout(() => {
                        window.location.href = data.redirectUrl || `/student/course/details/${courseId}`;
                    }, 1500);
                } else {
                    showToast(data.message || 'Failed to enroll. Please try again.', 'error');
                    if (enrollBtn) {
                        enrollBtn.disabled = false;
                        enrollBtn.innerHTML = '<i class="fas fa-plus"></i> Enroll Now';
                    }
                }
            })
            .catch(err => {
                console.error('Enrollment error:', err);
                showToast('An error occurred. Please try again.', 'error');
                if (enrollBtn) {
                    enrollBtn.disabled = false;
                    enrollBtn.innerHTML = '<i class="fas fa-plus"></i> Enroll Now';
                }
            });
        });
}

// Enroll in Track Function
function enrollTrack(trackId) {
    const enrollBtn = document.querySelector(`[onclick="enrollTrack(${trackId})"]`);
    
    // Check if user is logged in first
    fetch(`/api/enrollment/track-status/${trackId}`)
        .then(response => response.json())
        .then(statusData => {
            if (!statusData.isLoggedIn) {
                // Redirect to login with return URL
                window.location.href = `/Account/Login?ReturnUrl=/track/${trackId}`;
                return;
            }
            
            if (statusData.isEnrolled) {
                // Already enrolled, redirect to track
                window.location.href = statusData.trackUrl || `/student/track/${trackId}`;
                return;
            }
            
            // Show loading state
            if (enrollBtn) {
                enrollBtn.disabled = true;
                enrollBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Enrolling...';
            }
            
            // Proceed with enrollment
            fetch(`/api/enrollment/enroll-track/${trackId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message || 'Successfully enrolled in the track!', 'success');
                    
                    if (enrollBtn) {
                        enrollBtn.innerHTML = '<i class="fas fa-check"></i> Enrolled!';
                        enrollBtn.classList.remove('btn-primary');
                        enrollBtn.classList.add('btn-success');
                    }
                    
                    setTimeout(() => {
                        window.location.href = data.redirectUrl || `/student/track/${trackId}`;
                    }, 1500);
                } else {
                    showToast(data.message || 'Failed to enroll. Please try again.', 'error');
                    
                    if (enrollBtn) {
                        enrollBtn.disabled = false;
                        enrollBtn.innerHTML = '<i class="fas fa-route"></i> Start Track';
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showToast('An error occurred. Please try again.', 'error');
                
                if (enrollBtn) {
                    enrollBtn.disabled = false;
                    enrollBtn.innerHTML = '<i class="fas fa-route"></i> Start Track';
                }
            });
        })
        .catch(error => {
            console.error('Status check error:', error);
            // Redirect to login if status check fails (likely not logged in)
            window.location.href = `/Account/Login?ReturnUrl=/track/${trackId}`;
        });
}

// Toast notification helper
function showToast(message, type = 'success') {
    // Check if toast element exists, create one if not
    let toast = document.getElementById('toast');
    
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'toast';
        toast.className = 'toast';
        toast.innerHTML = `
            <i class="fas fa-check-circle"></i>
            <span id="toastMessage">${message}</span>
        `;
        document.body.appendChild(toast);
        
        // Add styles if not already present
        if (!document.getElementById('toast-styles')) {
            const style = document.createElement('style');
            style.id = 'toast-styles';
            style.textContent = `
                .toast {
                    position: fixed;
                    bottom: 2rem;
                    left: 50%;
                    transform: translateX(-50%) translateY(100px);
                    padding: 1rem 1.5rem;
                    background: #1a1a2e;
                    border: 1px solid rgba(124, 58, 237, 0.3);
                    border-radius: 12px;
                    color: white;
                    font-size: 0.9rem;
                    display: flex;
                    align-items: center;
                    gap: 0.75rem;
                    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
                    z-index: 10000;
                    opacity: 0;
                    transition: all 0.3s ease;
                }
                .toast.show {
                    transform: translateX(-50%) translateY(0);
                    opacity: 1;
                }
                .toast.success { border-color: #10b981; }
                .toast.success i { color: #10b981; }
                .toast.error { border-color: #ef4444; }
                .toast.error i { color: #ef4444; }
            `;
            document.head.appendChild(style);
        }
    }
    
    const toastMessage = toast.querySelector('#toastMessage') || toast.querySelector('span');
    const icon = toast.querySelector('i');
    
    if (toastMessage) toastMessage.textContent = message;
    if (icon) {
        icon.className = type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle';
    }
    
    toast.className = `toast ${type} show`;
    
    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}