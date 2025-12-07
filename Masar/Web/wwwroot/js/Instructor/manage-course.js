const CourseManager = { // Rename 'PageManager' to your specific feature (e.g., CurriculumBuilder)

    // 1. STATE: Variables that used to be global 'let' or 'var'
    state: {
        activeTab: 'curriculum',
        courseId: null,
        isEditing: false,
        tempData: null
    },

    // 2. ACTIONS: The dictionary of things the User can DO
    actions: {
        // Example: 'save-form': function(element, event) { ... }
        'toggle-module': function (element, event) {
            event.stopPropagation();

            // 1. Toggle the Header (for arrow rotation)
            const parentModuleItem = element.closest('.module-item');
            if (parentModuleItem)
                parentModuleItem.classList.toggle('expanded');
        },

        'open-add-module': function () {
            // Reset form
            document.getElementById("moduleForm").reset();
            document.getElementById("moduleIdInput").value = "0";
            document.getElementById("moduleModalTitle").innerText = "Add New Module";

            // Show modal
            CourseManager.functions.showModal("moduleModal");
        },

        'open-edit-module': function (element) {
            // gather info (already have the data in the DOM)
            const moduleId = element.dataset.moduleId;
            const moduleTitle = element.dataset.moduleTitle;

            // Populate form
            document.getElementById('moduleIdInput').value = moduleId;
            document.getElementById('moduleTitleInput').value = moduleTitle;
            document.getElementById('moduleModalTitle').innerText = "Edit Module";

            // show modal
            CourseManager.functions.showModal("moduleModal");
        },

        'open-add-lesson': function (element, event) {
            event.stopPropagation();

            const moduleId = element.dataset.moduleId;

            // Reset form
            document.getElementById("lessonForm").reset();
            document.getElementById("moduleIdInput").value = `"${moduleId}"`;
            document.getElementById("lessonModalTitle").innerText = "Add New Lesson";

            // show modal
            CourseManager.functions.showModal("lessonModal");
        },

        'open-edit-lesson': function (element, event) {
            event.stopPropagation();

            // Use jQuery .data() to get attributes
            const lessonId = $(element).data('lesson-id');
            const moduleId = $(element).data('module-id');

            // AJAX call
            $.get(`/instructor/manage-course/lesson/${lessonId}`, function (data) {

                // --- A. Populate Common Fields ---
                // Use .text() for text content
                $('#lessonModalTitle').text("Edit Lesson");

                // Use .val() for input values
                $('#lessonIdInput').val(data.lessonId);
                $('#lessonTitleInput').val(data.lessonTitle);
                $('#contentTypeInput').val(data.contentType);

                // --- B. Handle Content Type Switching ---
                // Find the option using jQuery selector and trigger click
                const $typeOption = $(`.lesson-type-option[data-type="${data.contentType}"]`);
                if ($typeOption.length > 0) {
                    $typeOption.trigger('click');
                }

                // --- C. Populate Specific Fields ---
                if (data.contentType === 'Video') {
                    // Populate Video Fields
                    $('input[name="VideoUrl"]').val(data.videoUrl || "");
                    $('input[name="DurationMinutes"]').val(data.durationInMinutes || "");
                }
                else if (data.contentType === 'Article') {
                    // Populate Article Fields
                    const content = data.articleContent || "";

                    // 1. Update the hidden textarea
                    $('#hiddenArticleContent').val(content);

                    // 2. Update the visual editor div (use .html() for innerHTML)
                    $('#articleContentEditor').html(content);
                }

                // --- D. Show Modal ---
                CourseManager.functions.showModal("lessonModal");

            }).fail(function () {
                alert("Error fetching lesson details.");
            });
        },

        'select-lesson-type': function (element, event) {
            event.stopPropagation();

            const lessonType = element.dataset.type;
            document.getElementById("contentTypeInput").value = lessonType;

            document.querySelectorAll('.lesson-type-option').forEach(el =>
                el.classList.remove('selected')
            );
            element.classList.add('selected');

            if (lessonType == "Video") {
                document.getElementById("videoFields").classList.remove("hidden");
                document.getElementById("articleFields").classList.add("hidden");
            }
            else if (lessonType == "Article") {
                document.getElementById("articleFields").classList.remove("hidden");
                document.getElementById("videoFields").classList.add("hidden");
            }
        },

        'close-modal': function (element) {
            const targetId = element.dataset.target;

            document.getElementById(targetId).classList.remove("show");
        },

        'switch-tab': function (element, even) {
            // 1. Get tab id
            const tabId = element.dataset.tabId;

            // 2. Call the function to update UI
            CourseManager.functions.activateTab(element, tabId);
        },
    },

    // 3. FUNCTIONS: Re-usable logic (AJAX calls, UI updates)
    functions: {
        // Example: loadData: function() { ... }
        showModal: function (modalId) {
            document.getElementById(modalId).classList.add("show");
        },

        hideModal: function (modalId) {
            document.getElementById(modalId).classList.remove("show");
        },

        activateTab: function (clickedBtn, tabId) {
            // 1. Deactivate all Buttons
            document.querySelectorAll('.tab-btn').forEach(btn =>
                btn.classList.remove('active')
            );

            // 2. Deactivate all Content Areas
            document.querySelectorAll('.tab-content').forEach(content =>
                content.classList.remove('active')
            );

            // 3. Activate the clicked Button
            clickedBtn.classList.add('active');

            // 4. Activate the target Content
            const targetContent = document.getElementById(`tab-${tabId}`);
            if (targetContent) {
                targetContent.classList.add('active');
            }

            // 5. Update State
            CourseManager.state.activeTab = tabId;

            // Optional: If you use Lazy Loading later, you can add check here:
            // if (!targetContent.innerHTML.trim()) { loadTabAjax(tabId); }
        },

        getLesson: function (lessonId) {
        }
    },

    // 4. ROUTER: The Event Delegation Logic (Copy this exactly)
    bindEvents: function () {
        document.addEventListener("click", (e) => {
            const trigger = e.target.closest("[data-action]");

            if (trigger) {
                const actionName = trigger.dataset.action;
                if (this.actions[actionName]) {
                    this.actions[actionName](trigger, e);
                } else {
                    console.warn(`Action '${actionName}' not defined.`);
                }
            }
        });

        // Add other global listeners here (change, input, etc.) if needed
    },

    // 5. START
    init: function () {
        console.log("Initializing...");
        this.bindEvents();
        // Any startup logic (like loading initial data) goes here
    }
};

document.addEventListener("DOMContentLoaded", () => {
    CourseManager.init();
});