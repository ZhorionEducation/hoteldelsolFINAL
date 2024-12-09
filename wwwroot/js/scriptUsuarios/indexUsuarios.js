document.getElementById('searchInput').addEventListener('input', function (e) {
    const searchTerm = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const cells = row.querySelectorAll('td');
        let found = false;

        cells.forEach(cell => {
            if (cell.textContent.toLowerCase().includes(searchTerm)) {
                found = true;
            }
        });

        row.style.display = found ? '' : 'none';
    });
});

// Función loadEditForm actualizada
function loadEditForm(id) {
    $.ajax({
        url: '/Usuarios/Edit/' + id,
        type: 'GET',
        success: function (result) {
            $('#editFormContainer').html(result);
            $('#editModal').show();
            $('#detailsModal').hide();
            // Reinicializar validación
            $.validator.unobtrusive.parse('#editFormContainer');
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar el formulario de edición:', error);
            alert('Error al cargar el formulario de edición.');
        }
    });
}

function closeEditModal() {
    $('#editModal').hide();
    $('#detailsModal').show();
}


$(document).ready(function () {
    $('.close').click(function () {
        closeEditModal();
        closeDetailsModal();
        closeDeleteModal();
    });

    // Manejo del envío del formulario de edición
    $(document).on('submit', '#editForm', function (e) {
        e.preventDefault();
        
        // Asegurarse de que el formulario tenga validación inicializada
        if ($(this).data('validator')) {
            if (!$(this).data('validator').valid()) {
                return;
            }
        }
    
        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: $(this).serialize(),
            success: function (result) {
                if (result.success) {
                    closeEditModal();
                    Swal.fire({
                        icon: 'success',
                        title: 'Usuario editado exitosamente.',
                        showConfirmButton: false,
                        timer: 1500
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    $('#editFormContainer').html(result);
                    $.validator.unobtrusive.parse('#editForm');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error al guardar los cambios:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Ha ocurrido un error guardando los cambios.',
                    text: 'Intenta de nuevo.',
                });
            }
        });
    });

    // Manejo del envío del formulario de eliminación
    
});

function loadDetailsModal(id) {
    $.ajax({
        url: '/Usuarios/Details/' + id,
        type: 'GET',
        success: function (result) {
            $('#detailsContainer').html(result);
            $('#detailsModal').show();
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar los detalles:', error);
            alert('Error al cargar los detalles del usuario.');
        }
    });
}

function closeDetailsModal() {
    $('#detailsModal').hide();
}

function loadDeleteForm(id) {
    Swal.fire({
        title: '¿Estás seguro?',
        text: "¡No podrás revertir esto!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminarlo!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Usuarios/Delete/' + id,
                type: 'POST',
                data: {
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (result) {
                    if (result.success) {
                        Swal.fire(
                            'Eliminado!',
                            'El usuario ha sido eliminado.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire(
                            'Error!',
                            'Ha ocurrido un error eliminando el usuario.',
                            'error'
                        );
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error al eliminar el usuario:', error);
                    Swal.fire(
                        'Error!',
                        'Ha ocurrido un error eliminando el usuario.',
                        'error'
                    );
                }
            });
        }
    });
}

function closeDeleteModal() {
    $('#deleteModal').hide();
}
