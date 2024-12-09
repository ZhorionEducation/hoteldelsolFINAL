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
        url: '/Reservas/Edit/' + id,
        type: 'GET',
        success: function (result) {
            $('#editFormContainer').html(result);
            $('#editModal').show();
            $('#detailsModal').hide();
            
            // Inicializar el calendario después de cargar el formulario
            initializeEditCalendar(id);
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
        const form = $(this);
    
        // First validate huespedes
        if (!validateHuespedes()) {
            return false;
        }
        
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
                    url: form.attr('action'),
                    type: 'POST',
                    data: form.serialize(),
                    success: function (result) {
                        if (result.success) {
                            Swal.fire({
                                icon: 'success',
                                title: '¡Guardado!',
                                text: 'Los cambios han sido guardados correctamente.',
                                showConfirmButton: false,
                                timer: 1500
                            }).then(() => {
                                closeEditModal();
                                loadDetailsModal(result.id);
                                location.reload();
                            });
                        } else {
                            // Instead of replacing the entire form, just show validation errors
                            const tempDiv = $(result);
                            const validationSummary = tempDiv.find('.validation-summary-errors');
                            if (validationSummary.length) {
                                $('#editForm .validation-summary-errors').html(validationSummary.html());
                            } else {
                                // If there's no validation summary, preserve the form
                                $('#editFormContainer').html(result);
                                // Restore huespedes data
                                const huespedesData = form.find('.huesped-item').map(function() {
                                    return {
                                        nombre: $(this).find('input[name*="Nombre"]').val(),
                                        documento: $(this).find('input[name*="DocumentoIdentidad"]').val()
                                    };
                                }).get();
                                
                                huespedesData.forEach((huesped, index) => {
                                    $(`input[name="Huespedes[${index}].Nombre"]`).val(huesped.nombre);
                                    $(`input[name="Huespedes[${index}].DocumentoIdentidad"]`).val(huesped.documento);
                                });
                            }
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
        url: '/Reservas/Details/' + id,
        type: 'GET',
        success: function (result) {
            $('#detailsContainer').html(result);
            $('#detailsModal').show();
        }
    });
}

function openPaymentDetailsModal(pagoId) {
    $.get('/Reservas/GetPaymentDetails/' + pagoId, function (data) {
        $('#paymentDetailsModal .modal-body').html(data);
        $('#paymentDetailsModal').show();
    });
}

function savePaymentDetails() {
    var form = $('#paymentDetailsForm');

    // Log form data for debugging
    console.log('Form data:', form.serialize());

    $.ajax({
        url: '/Reservas/UpdatePaymentDetails',
        type: 'POST',
        data: form.serialize(),
        success: function(response) {
            console.log('Response:', response); // Debug log

            if (response && response.success) {
                $('#paymentDetailsModal').modal('hide');
                location.reload();
            } else {
                let errorMessage = 'Error al guardar los cambios';
                if (response && response.message) {
                    errorMessage = response.message;
                }
                alert('Error: ' + errorMessage);
            }
        },
        error: function(xhr, status, error) {
            console.error('Error details:', {
                status: status,
                error: error,
                response: xhr.responseText
            });
            alert('Error en la solicitud. Por favor, intente nuevamente.');
        }
    });
}

function closePaymentModal() {
    $('#paymentDetailsModal').hide();
}

function showFullImage(src) {
    const modal = document.createElement('div');
    modal.className = 'full-image-modal';
    modal.innerHTML = `<img src="${src}" class="full-image" />`;
    modal.onclick = () => modal.remove();
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