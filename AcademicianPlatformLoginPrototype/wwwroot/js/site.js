$('.edit-btn').click(function () {
    var announcementId = $(this).data('id');
    var announcementTitle = $(this).data('title');
    var announcementContent = $(this).data('content');
    $('#title').val(announcementTitle);
    $('#content').val(announcementContent);
    $('#editModal').modal('show');
});

$('#editForm').submit(function (e) {
    $('#editModal').modal('hide');
});

