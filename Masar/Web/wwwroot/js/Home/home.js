// Mobile Menu Toggle
const mobileToggle = document.getElementById("mobileToggle");
const navLinks = document.getElementById("navLinks");

mobileToggle.addEventListener("click", () => {
    navLinks.classList.toggle("active");
});

// Close mobile menu when clicking on a link
document.querySelectorAll(".nav-links a").forEach((link) => {
    link.addEventListener("click", () => {
        navLinks.classList.remove("active");
    });
});

// Navbar scroll effect
const navbar = document.getElementById("navbar");
let lastScroll = 0;

window.addEventListener("scroll", () => {
    const currentScroll = window.pageYOffset;

    if (currentScroll > 100) {
        navbar.classList.add("scrolled");
    } else {
        navbar.classList.remove("scrolled");
    }

    lastScroll = currentScroll;
});

// Testimonial Carousel
let currentTestimonial = 0;
const testimonials = document.querySelectorAll(".testimonial");
const dots = document.querySelectorAll(".dot");

function showTestimonial(index) {
    testimonials.forEach((t) => t.classList.remove("active"));
    dots.forEach((d) => d.classList.remove("active"));

    testimonials[index].classList.add("active");
    dots[index].classList.add("active");
    currentTestimonial = index;
}

function changeTestimonial(direction) {
    let newIndex = currentTestimonial + direction;
    if (newIndex < 0) newIndex = testimonials.length - 1;
    if (newIndex >= testimonials.length) newIndex = 0;
    showTestimonial(newIndex);
}

// Auto-rotate testimonials
setInterval(() => {
    changeTestimonial(1);
}, 6000);

// Scroll Animations
const observerOptions = {
    threshold: 0.15,
    rootMargin: "0px 0px -80px 0px",
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
        if (entry.isIntersecting) {
            entry.target.classList.add("visible");
        }
    });
}, observerOptions);

document.querySelectorAll(".fade-in-up").forEach((el) => {
    observer.observe(el);
});

// Smooth Scrolling - Handle both same-page anchors and navigation from other pages
document.querySelectorAll('a[href*="#"]').forEach((anchor) => {
    anchor.addEventListener("click", function (e) {
        const href = this.getAttribute("href");
        
        // Check if this is a same-page anchor (starts with # or /#)
        if (href.startsWith("#")) {
            // Same page anchor
            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                target.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                });
            }
        } else if (href.startsWith("/#")) {
            // Link with path + anchor (e.g., /#how-it-works)
            const isHomePage = window.location.pathname === "/" || window.location.pathname === "";
            
            if (isHomePage) {
                // Already on home page, just scroll
                e.preventDefault();
                const targetId = href.substring(1); // Remove the leading /
                const target = document.querySelector(targetId);
                if (target) {
                    target.scrollIntoView({
                        behavior: "smooth",
                        block: "start",
                    });
                }
            }
            // Otherwise, let the browser navigate to the home page with the anchor
        }
    });
});

// Handle scroll to anchor on page load (when navigating from another page)
document.addEventListener("DOMContentLoaded", function() {
    if (window.location.hash) {
        setTimeout(() => {
            const target = document.querySelector(window.location.hash);
            if (target) {
                target.scrollIntoView({
                    behavior: "smooth",
                    block: "start",
                });
            }
        }, 100);
    }
});