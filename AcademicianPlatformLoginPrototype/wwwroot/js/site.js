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

