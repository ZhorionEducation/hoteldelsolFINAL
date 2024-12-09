$(document).ready(function () {
    $('#reservaForm').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        var submitButton = $("button[type=submit]:focus").val();
        var formData = new FormData(form[0]);
        formData.append('submitButton', submitButton);

        $.ajax({
            type: form.attr('method'),
            url: form.attr('action'),
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    if (submitButton === 'Pagar') {
                        window.location.href = response.pagarUrl;
                    } else if (submitButton === 'Guardar') {
                        Swal.fire({
                            title: 'Reserva Guardada',
                            text: 'Recuerda que tienes 24 horas para abonar y subir comprobante, de lo contrario la reserva sera cancelada.',
                            icon: 'warning',
                            confirmButtonText: 'Entendido',
                            allowOutsideClick: false,
                            allowEscapeKey: false,
                            showConfirmButton: true
                        
                        }).then((result) => {
                            if (result.isConfirmed) {
                                window.location.href = response.redirectUrl;
                            }
                        });
                    }
                } else {
                    Swal.fire('Error', response.message || "Error al crear la reserva.", 'error');
                }
            },
            error: function () {
                Swal.fire('Error', "Verifica los campos del formulario.", 'warning');
                // Swal.fire('Error', "Error al procesar la solicitud.", 'error');
            }
        });
    });
});