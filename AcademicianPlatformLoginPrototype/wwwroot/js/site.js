//TinyMCE
tinymce.init({
    selector: 'textarea#AnnouncementContent',
    plugins: 'ai tinycomments mentions anchor autolink charmap codesample emoticons image link lists media searchreplace table visualblocks wordcount checklist mediaembed casechange export formatpainter pageembed permanentpen footnotes advtemplate advtable advcode editimage tableofcontents mergetags powerpaste tinymcespellchecker autocorrect a11ychecker typography inlinecss',
    toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link image media table mergetags | align lineheight | tinycomments | checklist numlist bullist indent outdent | emoticons charmap | removeformat',
    tinycomments_mode: 'embedded',
    tinycomments_author: 'Author name',
    mergetags_list: [
        { value: 'First.Name', title: 'First Name' },
        { value: 'Email', title: 'Email' },
    ],
    ai_request: (request, respondWith) => respondWith.string(() => Promise.reject("See docs to implement AI Assistant")),
});

//Announcement edit code
$('.edit-btn').click(function () {
    var announcementId = $(this).data('id');
    var announcementTitle = $(this).data('title');
    var announcementFaculty = $(this).data('faculty')
    var announcementContent = $(this).data('content');
    $('#title').val(announcementTitle);
    $('#faculty').val(announcementFaculty);
    $('#content').val(announcementContent);
    $('#editModal').modal('show');
});

$('#editForm').submit(function (e) {
    $('#editModal').modal('hide');
});

//User delete confirmation window code
$('.delete-user-btn').click(function () {
    var userId = $(this).data('user-id');
    $('#deleteUserId').val(userId);
    $('#deleteUserModal').modal('show');
});

$('#deleteUserForm').submit(function (e) {
    $('#deleteUserModal').modal('hide');
});

//Reply to support ticket window code
$('.reply-ticket-btn').click(function () {
    console.log("Work!");
    var ticketId = $(this).data('ticket-id');
    var userNameWhoReplies = $(this).data('user-name');
    $('#replyTicketId').val(ticketId);
    $('#userNameWhoReplies').val(userNameWhoReplies);
    $('#replyTicketModal').modal('show');
});

$('#replyTicketForm').submit(function (e) {
    $('#replyTicketModal').modal('hide');
});

//Announcement filtering code
document.getElementById('facultySelect').addEventListener('change', function () {
    var selectedOption = this.options[this.selectedIndex];
    var action = selectedOption.value;
    var faculty = selectedOption.getAttribute('data-faculty');

    if (action === 'IndexFaculty') {
        window.location.href = '/Home/' + action + '?announcementFaculty=' + encodeURIComponent(faculty);
    }
    else if (action == 'Index') {
        window.location.href = '/Home/';
    }
});




