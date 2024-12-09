function getSelectedFilters() {
    const checkboxes = document.querySelectorAll('input[name="exportFilters"]:checked');
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;
    
    const filters = Array.from(checkboxes).map(cb => cb.value);
    
    if (dateFrom) filters.push(`dateFrom=${dateFrom}`);
    if (dateTo) filters.push(`dateTo=${dateTo}`);
    
    return filters;
}

function exportToPdf() {
    const filters = getSelectedFilters();
    window.location.href = `/Habitaciones/ExportToPdf?filters=${filters.join(',')}`;
}

function exportToExcel() {
    const filters = getSelectedFilters();
    window.location.href = `/Habitaciones/ExportToExcel?filters=${filters.join(',')}`;
}

// document.getElementById('searchInput').addEventListener('input', function (e) {
//     const searchTerm = e.target.value.toLowerCase();
//     const rows = document.querySelectorAll('tbody tr');

//     rows.forEach(row => {
//         const cells = row.querySelectorAll('td');
//         let found = false;

//         cells.forEach(cell => {
//             if (cell.textContent.toLowerCase().includes(searchTerm)) {
//                 found = true;
//             }
//         });

//         row.style.display = found ? '' : 'none';
//     });
// });

function loadEditForm(id) {
    $.ajax({
        url: '/Habitaciones/Edit/' + id,
        type: 'GET',
        success: function (result) {
            $('#editFormContainer').html(result);
            $('#editModal').show();
            $('#detailsModal').hide(); // Ocultar el contenedor de detalles
        }
    });
}

function closeEditModal() {
    $('#editModal').hide();
    $('#detailsModal').show(); // Mostrar el contenedor de detalles
}

$(document).ready(function () {
    $('.close').click(function () {
        closeEditModal();
    });

    $(document).on('submit', '#editForm', function (e) {
        e.preventDefault();
        var formData = new FormData(this);
    
        Swal.fire({
            title: '¿Estás seguro?',
            text: '¿Deseas guardar los cambios?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sí, guardar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: $(this).attr('action'),
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (result) {
                        if (result.success) {
                            $('#editModal').hide();
                            Swal.fire({
                                icon: 'success',
                                title: '¡Guardado!',
                                text: 'Los cambios han sido guardados correctamente.',
                                showConfirmButton: false,
                                timer: 1500
                            }).then(() => {
                                loadDetailsModal(result.id);
                                location.reload();
                            });
                        } else if (result.message === "No changes") {
                            $('#editModal').hide();
                            Swal.fire({
                                icon: 'info',
                                title: 'Sin cambios',
                                text: 'No se detectaron cambios para guardar.',
                                showConfirmButton: false,
                                timer: 1500
                            }).then(() => {
                                loadDetailsModal(result.id);
                            });
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: result.message || 'No se pudieron guardar los cambios.'
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error:', error);
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'Ha ocurrido un error guardando los cambios. Intenta de nuevo.'
                        });
                    }
                });
            }
        });
    });
});

function loadDetailsModal(id) {
    $.ajax({
        url: '/Habitaciones/Details/' + id,
        type: 'GET',
        success: function (result) {
            $('#detailsContainer').html(result);
            $('#detailsModal').show();
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Ha ocurrido un error cargando los detalles. Intenta de nuevo.'
            });
        }
    });
}

function showFullImage(src) {
    // Elimina cualquier modal existente para evitar duplicados
    const existingModal = document.querySelector('.full-image-modal');
    if (existingModal) {
        existingModal.remove();
    }

    // Crea el contenedor del modal
    const modal = document.createElement('div');
    modal.className = 'full-image-modal';
    modal.innerHTML = `<img src="${src}" class="full-image" />`;

    // Añade un evento para cerrar el modal al hacer clic fuera de la imagen
    modal.onclick = () => modal.remove();

    // Añade el modal al cuerpo del documento
    document.body.appendChild(modal);
    modal.style.display = 'block';
}

$(document).ready(function () {
    $('.close').click(function () {
        closeDetailsModal();
    });
});

function closeDetailsModal() {
    $('#detailsModal').hide();
}