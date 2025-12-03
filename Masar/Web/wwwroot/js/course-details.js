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

// Enroll Function
function enrollCourse(courseId) {
    // TODO: Implement enrollment logic
    fetch(`/api/enrollment/enroll/${courseId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            window.location.href = `/student/course/${courseId}`;
        } else {
            alert('Failed to enroll. Please try again.');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred. Please try again.');
    });
}