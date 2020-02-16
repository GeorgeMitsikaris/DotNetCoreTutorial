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

function deleteRole(roleId, isDeleted) {
    const idToDelete = `delete_${roleId}`;
    const idToConfirmDelete = `confirmDelete_${roleId}`;

    if (isDeleted) {
        $(`#${idToDelete}`).hide();
        $(`#${idToConfirmDelete}`).show();
    } else {
        $(`#${idToDelete}`).show();
        $(`#${idToConfirmDelete}`).hide();
    }
}