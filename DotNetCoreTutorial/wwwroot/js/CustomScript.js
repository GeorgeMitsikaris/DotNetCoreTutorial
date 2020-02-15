function ConfirmDelete(userId, isDeleted) {
    const idToDelete = `delete_${userId}`;
    const idToConfirmDelete = `confirmDelete_${userId}`;

    if (isDeleted) {
        $(`#${idToDelete}`).hide();
        $(`#${idToConfirmDelete}`).show();
    } else {
        $(`#${idToDelete}`).show();
        $(`#${idToConfirmDelete}`).hide();
    }
}