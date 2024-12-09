$(document).ready(function () {
    $('#formpagos').submit(function (event) {
        event.preventDefault();

        // Get input values
        const montoInput = $('[name="Monto"]');
        const monto = parseFloat(montoInput.val());
        const minimoAbono = parseFloat(montoInput.attr('min'));
        const fileInput = $('#comprobante');

        // Validate minimum amount
        if (monto < minimoAbono) {
            Swal.fire({
                icon: 'error',
                title: 'Monto Insuficiente',
                text: `El monto mínimo de abono debe ser ${minimoAbono.toLocaleString('es-CO', {
                    style: 'currency',
                    currency: 'COP'
                })}`,
                confirmButtonText: 'Entendido',
                confirmButtonColor: '#3085d6'
            });
            return;
        }

        // Validate file upload
        if (fileInput.get(0).files.length === 0) {
            Swal.fire({
                title: 'Error en el formulario',
                text: 'Por favor seleccione una imagen.',
                icon: 'error',
                confirmButtonText: 'Entendido'
            });
            return;
        }

        // Show loading state
        Swal.fire({
            title: 'Enviando formulario...',
            text: 'Por favor, espera mientras procesamos tu solicitud.',
            icon: 'info',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false
        });

        // Process form submission
        var form = $(this);
        var formData = new FormData(form[0]);

        $.ajax({
            type: form.attr('method'),
            url: form.attr('action'),
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Comprobante enviado',
                        text: 'Un administrador revisará tu comprobante y te notificará si la reserva fue aceptada.',
                        icon: 'info',
                        confirmButtonText: 'Entendido',
                        allowOutsideClick: false,
                        allowEscapeKey: false
                    }).then((result) => {
                        if (result.isConfirmed && response.redirectUrl) {
                            window.location.href = response.redirectUrl;
                        }
                    });
                } else {
                    // Swal.fire('Error', response.message || "Error al procesar el pago.", 'error');
                    Swal.fire('Error', response.message || "Revisa nuevamente los campos solicitados.", 'warning');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error AJAX:', status, error);
                Swal.fire('Error', "Error al procesar la solicitud.", 'error');
            }
        });
    });
});




