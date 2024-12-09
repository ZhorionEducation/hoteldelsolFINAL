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

function loadEditForm(id) {
    $.ajax({
        url: '/ServiciosAdicionales/Edit/' + id,
        type: 'GET',
        headers: { "X-Requested-With": "XMLHttpRequest" },
        success: function (result) {
            $('#editFormContainer').html(result);
            $('#editModal').show();
            $('#detailsModal').hide();
            $.validator.unobtrusive.parse('#editForm');
        },
        error: function (xhr, status, error) {
            console.error('Error al cargar el formulario de edición:', error);
            alert('Error al cargar el formulario de edición.');
        }
    });
}

function closeEditModal() {
    $('#editModal').hide();
    $('#editFormContainer').html('');
}

$(document).ready(function () {
    // Manejador para cerrar modales
    $('.close').click(function () {
        closeEditModal();
        closeDetailsModal();
        closeDeleteModal();
    });

    // Manejador del formulario de edición
    
});

function submitEditForm(formId) {
    const form = document.getElementById(formId);
    const formData = new FormData(form);
    
    // Always include the current image value
    const currentImage = form.querySelector('input[name="Imagen"]').value;
    formData.set('Imagen', currentImage);

    fetch(form.action, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            Swal.fire({
                icon: 'success',
                title: '¡Éxito!',
                text: 'Servicio actualizado correctamente'
            }).then(() => {
                location.reload();
            });
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: data.message || 'Error al actualizar el servicio'
            });
        }
    })
    .catch(error => {
        console.error('Error:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Error al procesar la solicitud'
        });
    });

    return false;
}

function loadDetailsModal(id) {
    $.ajax({
        url: '/ServiciosAdicionales/Details/' + id,
        type: 'GET',
        success: function (data) {
            $('#detailsContainer').html(data);
            $('#detailsModal').show();
        }
    });
}

function closeDetailsModal() {
    $('#detailsModal').hide();
    $('#detailsContainer').html('');
}

function loadDeleteForm(id) {
    $.ajax({
        url: '/ServiciosAdicionales/Delete/' + id,
        type: 'GET',
        success: function (data) {
            $('#deleteFormContainer').html(data);
            $('#deleteModal').show();
        }
    });
}

function closeDeleteModal() {
    $('#deleteModal').hide();
    $('#deleteFormContainer').html('');
}

$(document).ready(function () {
    $('.close').click(function () {
        closeDeleteModal();
    });

    
});

function handleDelete(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: "¡No podrá revertir esta acción!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/ServiciosAdicionales/Delete/' + id,
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function(result) {
                    if (result.success) {
                        Swal.fire({
                            icon: 'success',
                            title: '¡Eliminado!',
                            text: 'Servicio eliminado correctamente'
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: result.message || 'No se pudo eliminar el servicio'
                        });
                    }
                },
                error: function(xhr, status, error) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'No se puede eliminar el servicio porque tiene reservas asociadas'
                    });
                }
            });
        }
    });
}

// Function to submit delete form
function submitDeleteForm(formId) {
    const form = document.getElementById(formId);
    $.ajax({
        url: $(form).attr('action'),
        type: 'POST',
        data: $(form).serialize(),
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: 'success',
                    title: '¡Eliminado!',
                    text: 'Servicio eliminado correctamente'
                }).then(() => {
                    closeDeleteModal();
                    removeServiceRow(result.id);
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'No se pudo eliminar el servicio'
                });
                $('#deleteFormContainer').html(result);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Ha ocurrido un error eliminando el servicio. Intente de nuevo'
            });
        }
    });
}

// Update the form handler
$(document).on('submit', '#deleteForm', function (e) {
    e.preventDefault();
    handleDelete(this.id);
});

$(document).ready(function () {
    $('.close').click(function () {
        closeDetailsModal();
    });
});

function updateServiceRow(service) {
    const row = document.querySelector(`tr[data-id="${service.id}"]`);
    if (row) {
        row.querySelector('.service-name').textContent = service.nombre;
        row.querySelector('.service-description').textContent = service.descripcion;
        row.querySelector('.service-image').src = service.imagenUrl;
    }
}

function removeServiceRow(id) {
    const row = document.querySelector(`tr[data-id="${id}"]`);
    if (row) {
        row.remove();
    }
}