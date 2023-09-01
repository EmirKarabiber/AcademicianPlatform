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



