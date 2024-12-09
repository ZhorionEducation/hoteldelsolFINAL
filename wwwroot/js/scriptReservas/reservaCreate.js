$(document).ready(function () {
    // Animaci�n de entrada para el formulario
    $('.container').hide().fadeIn(1000);
    // Efecto hover para los campos del formulario
    $('.form-control').hover(
        function () { $(this).addClass('shadow-sm'); },
        function () { $(this).removeClass('shadow-sm'); }
    );
    // Validaci�n de fechas
    $('#FechaInicio, #FechaFin').change(function () {
        var fechaInicio = new Date($('#FechaInicio').val());
        var fechaFin = new Date($('#FechaFin').val());
        if (fechaFin <= fechaInicio) {
            Swal.fire({
                icon: 'error',
                title: 'Error en las fechas',
                text: 'La fecha de fin debe ser posterior a la fecha de inicio',
                confirmButtonColor: '#3085d6',
                confirmButtonText: 'Aceptar'
            }).then((result) => {
                $('#FechaFin').val('');
            });
            return;
        }
    });
    // Calculo automatico de dias y precio total
    $('#FechaInicio, #FechaFin, #PrecioTotal').change(function () {
        var fechaInicio = new Date($('#FechaInicio').val());
        var fechaFin = new Date($('#FechaFin').val());
        var precioTotal = parseFloat($('#PrecioTotal').val());
        if (!isNaN(fechaInicio) && !isNaN(fechaFin) && !isNaN(precioTotal)) {
            var dias = (fechaFin - fechaInicio) / (1000 * 60 * 60 * 24);
            var precioPorDia = precioTotal / dias;
            if (dias > 0) {
                alert('Estancia de ' + dias + ' dias. Precio por dia: $' + precioPorDia.toFixed(2));
            }
        }
    });
});


$(document).ready(function () {
    // Initially disable date inputs
    $('#FechaInicio, #FechaFin').prop('disabled', true);

    $('#HabitacionId').change(function () {
        var habitacionId = $(this).val();
        
        // Clear and disable dates if no room selected
        if (!habitacionId) {
            $('#FechaInicio, #FechaFin').prop('disabled', true)
                                       .val('');
            fechasOcupadas = [];
            return;
        }

        // Enable date inputs only after room selection
        $('#FechaInicio, #FechaFin').prop('disabled', false)
                                   .val('');

        // Get occupied dates for selected room
        $.ajax({
            url: '/Reservas/GetFechasOcupadas',
            type: 'GET',
            data: { habitacionId: habitacionId },
            success: function (data) {
                fechasOcupadas = data.map(function (item) {
                    var start = new Date(item.fechaInicio);
                    var end = new Date(item.fechaFin);
                    
                    start.setDate(start.getDate());
                    end.setDate(end.getDate() + 2);
                    
                    return {
                        start: start,
                        end: end
                    };
                });
                
                $('#FechaInicio, #FechaFin').datepicker('refresh');
            },
            error: function () {
                console.error('Error fetching fechas ocupadas.');
            }
        });
    });

    // Rest of your existing datepicker code...
});


$(document).ready(function () {
    $('#HabitacionId').change(function () {
        var selectedOption = $(this).find('option:selected');
        var habitacionId = $(this).val();
        
        // Hacer una llamada AJAX para obtener los detalles de la habitación
        if (habitacionId) {
            $.ajax({
                url: '/Reservas/GetHabitacionDetails',
                type: 'GET',
                data: { id: habitacionId },
                success: function (response) {
                    var maxAcompanantes;
                    switch(response.tipoHabitacion) {
                        case 'Individual':
                            maxAcompanantes = 1; // 1 acompañante (capacidad total 2)
                            break;
                        case 'Doble':
                            maxAcompanantes = 2; // 2 acompañantes (capacidad total 3)
                            break;
                        case 'Deluxe':
                            maxAcompanantes = 5; // 5 acompañantes (capacidad total 6)
                            break;
                        default:
                            maxAcompanantes = 0;
                    }
                    
                    // Actualizar atributos del input numeroAcompanantes
                    $('#numeroAcompanantes')
                        .attr('max', maxAcompanantes)
                        .attr('min', 0)
                        .val('')
                        .prop('disabled', false);
                }
            });
        } else {
            $('#numeroAcompanantes')
                .val('')
                .prop('disabled', true);
        }
    });

    // Validar el número de acompañantes al cambiar
    $('#numeroAcompanantes').on('input', function() {
        var value = parseInt($(this).val());
        var max = parseInt($(this).attr('max'));
        
        if (value > max) {
            $(this).val(max);
            Swal.fire({
                icon: 'warning',
                title: 'Límite excedido',
                text: `Esta habitación permite máximo ${max} acompañantes.`
            });
        }
    });
});


$(document).ready(function() {
    $('#HabitacionId').change(function() {
        var habitacionId = $(this).val();
        if (habitacionId) {
            $.ajax({
                url: '/Reservas/GetComodidadesByHabitacion',
                type: 'GET',
                data: { habitacionId: habitacionId },
                success: function(comodidades) {
                    // Clear existing selected items first
                    $('#selectedComodidades').empty();
                    
                    // Desmarcar todos los checkboxes primero
                    $('input[name="Comodidades"]').prop('checked', false);
                    
                    // Marcar los checkboxes correspondientes y agregar items
                    $('input[name="Comodidades"]').each(function() {
                        if (comodidades.includes($(this).val())) {
                            $(this).prop('checked', true);
                            
                            // Add to selected items container
                            const span = document.createElement('span');
                            span.classList.add('selected-item');
                            span.textContent = $(this).next('label').text();
                            span.dataset.id = this.id;
                            document.getElementById('selectedComodidades').appendChild(span);
                        }
                    });
                    
                    // Forzar la actualización del precio total después de marcar los checkboxes
                    // Usar setTimeout para asegurar que los checkboxes ya están marcados
                    setTimeout(function() {
                        actualizarPrecioTotal();
                    }, 100);
                },
                error: function() {
                    console.error('Error fetching comodidades.');
                }
            });
        } else {
            $('input[name="Comodidades"]').prop('checked', false);
            $('#selectedComodidades').empty();
            actualizarPrecioTotal();
        }
    });
});

document.addEventListener('DOMContentLoaded', function() {
    const checkboxes = document.querySelectorAll('input[type="checkbox"]');
    const selectedServicios = document.getElementById('selectedServicios');
    const selectedComodidades = document.getElementById('selectedComodidades');

    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const container = this.name === 'Servicios' ? selectedServicios : selectedComodidades;
            if (this.checked) {
                const span = document.createElement('span');
                span.classList.add('selected-item');
                span.textContent = this.nextElementSibling.textContent;
                span.dataset.id = this.id;
                container.appendChild(span);
            } else {
                const span = container.querySelector(`[data-id="${this.id}"]`);
                if (span) span.remove();
            }
        });
    });
});

$(document).ready(function () {
    var fechasOcupadas = [];

    $('#HabitacionId').change(function () {
        var habitacionId = $(this).val();
        if (habitacionId) {
            $.ajax({
                url: '/Reservas/GetFechasOcupadas',
                type: 'GET',
                data: { habitacionId: habitacionId },
                success: function (data) {
                    // Convertir fechas de ocupación a objetos Date y extender el rango
                    fechasOcupadas = data.map(function (item) {
                        var start = new Date(item.fechaInicio);
                        var end = new Date(item.fechaFin);
                        
                        // Ajustar fechas para incluir días adyacentes
                        start.setDate(start.getDate() );  //Quitamos el -1
                        end.setDate(end.getDate() + 2);
                        
                        
                        return {
                            start: start,
                            end: end
                        };
                    });
                    
                    $('#FechaInicio, #FechaFin').datepicker('refresh');
                },
                error: function () {
                    console.error('Error fetching fechas ocupadas.');
                }
            });
        } else {
            fechasOcupadas = [];
            $('#FechaInicio, #FechaFin').datepicker('refresh');
        }
    });

    function disableDates(date) {
        // Comparación con redondeo a medianoche
        date.setHours(0, 0, 0, 0);
        
        for (var i = 0; i < fechasOcupadas.length; i++) {
            var occupied = fechasOcupadas[i];
            var occupiedStart = new Date(occupied.start);
            var occupiedEnd = new Date(occupied.end);
            
            occupiedStart.setHours(0, 0, 0, 0);
            occupiedEnd.setHours(0, 0, 0, 0);
            
            if (date >= occupiedStart && date <= occupiedEnd) {
                return [false, 'occupied-date', 'Fecha ocupada'];
            }
        }
        return [true, '', ''];
    }

    $('#FechaInicio, #FechaFin').datepicker({
        dateFormat: 'yy-mm-dd',
        minDate: 0,
        beforeShowDay: disableDates,
        onSelect: function () {
            $(this).trigger('change'); // Disparar el evento 'change'
            $(this).datepicker("hide");
    
            var fechaInicio = new Date($('#FechaInicio').val());
            var fechaFin = new Date($('#FechaFin').val());
    
            if (fechaFin <= fechaInicio) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error en las fechas',
                    text: 'La fecha de fin debe ser posterior a la fecha de inicio',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'Aceptar'
                }).then((result) => {
                    $('#FechaFin').val('');
                });
                return;
            }
    
            if (isDateOccupied(fechaInicio, fechaFin)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Fechas no disponibles',
                    text: 'Las fechas seleccionadas están ocupadas. Por favor, elija otras fechas.',
                    confirmButtonColor: '#3085d6',
                    confirmButtonText: 'Aceptar'
                }).then((result) => {
                    $('#FechaInicio').val('');
                    $('#FechaFin').val('');
                });
            }
        }
    });

    function isDateOccupied(startDate, endDate) {
        // Normalize dates to start of day
        startDate.setHours(0, 0, 0, 0);
        endDate.setHours(0, 0, 0, 0);
    
        // Check each occupied range
        for (let i = 0; i < fechasOcupadas.length; i++) {
            let occupied = fechasOcupadas[i];
            let occupiedStart = new Date(occupied.start);
            let occupiedEnd = new Date(occupied.end);
            
            occupiedStart.setHours(0, 0, 0, 0);
            occupiedEnd.setHours(0, 0, 0, 0);
    
            // Only return true if there's a real overlap
            // For dates 6-10 occupied, this will allow 5 and 11 to be selected
            if (
                (startDate < occupiedEnd && endDate > occupiedStart)
            ) {
                return true;
            }
        }
        return false;
    }
});

//Precio de reserva:

$(document).ready(function() {
    const IVA = 0.19; // Porcentaje de IVA (19%)

    

    function calcularDias(fechaInicio, fechaFin) {
        if (!fechaInicio || !fechaFin) {
            return 1;
        }


        var inicio = new Date(fechaInicio);
        var fin = new Date(fechaFin);

        // Verificar si las fechas son válidas
        if (isNaN(inicio.getTime()) || isNaN(fin.getTime())) {
            console.error("Fechas inválidas:", fechaInicio, fechaFin);
            return 1;
        }

        // Asegurar que la hora no afecte el cálculo
        inicio.setHours(12, 0, 0, 0);
        fin.setHours(12, 0, 0, 0);

        var diferencia = fin - inicio;
        var dias = Math.ceil(diferencia / (1000 * 60 * 60 * 24));

        console.log("Dias calculados:", dias);

        return dias > 0 ? dias : 1;
    }

    $('.servicio-checkbox').change(function () {
        updateSelectedServicios();
        actualizarPrecioTotal(); // Recalculate total when servicios change
    });

    // Function to Update Selected Servicios and Total
    function updateSelectedServicios() {
        var selected = [];
        var totalServicios = 0;

        $('.servicio-checkbox:checked').each(function () {
            var servicioName = $(this).siblings('label').text();
            var servicioPrice = parseFloat($(this).data('price'));

            if (!isNaN(servicioPrice)) {
                selected.push(servicioName);
                totalServicios += servicioPrice;
            }
        });

        // Update the list of selected servicios
        $('#listServicios').empty();
        if (selected.length > 0) {
            $.each(selected, function (index, servicio) {
                $('#listServicios').append('<li>' + servicio + '</li>');
            });
        } else {
            $('#listServicios').append('<li>Ningún servicio seleccionado.</li>');
        }

        function formatCurrency(value) {
            return value.toLocaleString('es-CO', { style: 'currency', currency: 'COP' }).replace(/,00$/, '');
        }

        // Update the total Servicios price
        $('#totalServicios').text(formatCurrency(totalServicios));

        return totalServicios; // Return totalServicios for further calculations
    }

    function formatToCOP(value) {
        return value.toLocaleString('es-CO', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    }
    
    function parseCOP(value) {
        return parseFloat(value.replace(/\./g, ''));
    }
    
    function actualizarPrecioTotal() {
        var precioTotal = 0;
        var IVA = 0.19;
    
        // Obtener las fechas de inicio y fin
        var fechaInicio = $('#FechaInicio').val();
        var fechaFin = $('#FechaFin').val();
        var dias = calcularDias(fechaInicio, fechaFin);
    
        var totalServicios = updateSelectedServicios();
        precioTotal += totalServicios;
    
        // Obtener el precio de la habitación seleccionada
        var habitacionId = $('#HabitacionId').val();
        if (habitacionId) {
            $.ajax({
                url: '/Reservas/GetPrecioHabitacion',
                type: 'GET',
                data: { id: habitacionId },
                success: function(precioHabitacion) {
                    var precioHabitacionFloat = parseFloat(precioHabitacion);
                    if (isNaN(precioHabitacionFloat)) {
                        console.error("Precio de habitación inválido:", precioHabitacion);
                        precioHabitacionFloat = 0;
                    }
                    precioTotal += precioHabitacionFloat * dias;
    
                    // Calcular el precio de las comodidades
                    actualizarPrecioComodidades(precioTotal, dias, function(totalComodidades) {
                        var precioConIVA = precioTotal * (1 + IVA);
    
                        // Actualizar los inputs con valores desformateados
                        $('#precioTotalSinIVA').val(precioTotal);
                        $('#precioTotalConIVA').val(precioConIVA);
    
                        // Actualizar los spans con formato
                        $('#formattedPrecioSinIVA').text(`COP ${formatToCOP(precioTotal)}`);
                        $('#formattedPrecioConIVA').text(`COP ${formatToCOP(precioConIVA)}`);
                    });
                },
                error: function() {
                    console.error("Error al obtener el precio de la habitación:", habitacionId);
                    $('#precioTotalSinIVA').val(precioTotal);
                    $('#precioTotalConIVA').val(precioTotal * (1 + IVA));
    
                    $('#formattedPrecioSinIVA').text(`COP ${formatToCOP(precioTotal)}`);
                    $('#formattedPrecioConIVA').text(`COP ${formatToCOP(precioTotal * (1 + IVA))}`);
                }
            });
        } else {
            var precioConIVA = precioTotal + (precioTotal * IVA);
    
            $('#precioTotalSinIVA').val(precioTotal);
            $('#precioTotalConIVA').val(precioConIVA);
    
            $('#formattedPrecioSinIVA').text(`COP ${formatToCOP(precioTotal)}`);
            $('#formattedPrecioConIVA').text(`COP ${formatToCOP(precioConIVA)}`);
        }
    }

    function actualizarPrecioComodidades(precioTotal, dias, callback) {
        var comodidadesSeleccionadas = $('input[name="Comodidades"]:checked');
        var totalComodidades = comodidadesSeleccionadas.length;
        var comodidadesProcesadas = 0;

        if (totalComodidades === 0) {
            var precioConIVA = precioTotal + (precioTotal * IVA);
            $('#precioTotalSinIVA').val(precioTotal);
            $('#precioTotalConIVA').val(precioConIVA);
    
            $('#formattedPrecioSinIVA').text(`COP ${formatToCOP(precioTotal)}`);
            $('#formattedPrecioConIVA').text(`COP ${formatToCOP(precioConIVA)}`);
            console.log("Total sin comodidades:", precioTotal, "Total con IVA:", precioConIVA);
            return;
        }

        comodidadesSeleccionadas.each(function() {
            var comodidadId = $(this).val();
            $.ajax({
                url: '/Reservas/GetPrecioComodidad',
                type: 'GET',
                data: { id: comodidadId },
                success: function(precioComodidad) {
                    var precioComodidadFloat = parseFloat(precioComodidad);
                    if (isNaN(precioComodidadFloat)) {
                        console.error("Precio de comodidad inválido:", precioComodidad);
                        precioComodidadFloat = 0;
                    }
                    // Si la comodidad también se cobra por día, multiplicar por 'dias'
                    precioTotal += precioComodidadFloat * dias;
                    comodidadesProcesadas++;

                    console.log("Precio comodidad:", precioComodidadFloat, "Total después de comodidad:", precioTotal);

                    if (comodidadesProcesadas === totalComodidades) {
                        // Calcular el IVA
                        var precioConIVA = precioTotal + (precioTotal * IVA);
                        $('#precioTotalSinIVA').val(precioTotal);
            $('#precioTotalConIVA').val(precioConIVA);
    
            $('#formattedPrecioSinIVA').text(`COP ${formatToCOP(precioTotal)}`);
            $('#formattedPrecioConIVA').text(`COP ${formatToCOP(precioConIVA)}`);
                        console.log("Total final sin IVA:", precioTotal, "Total con IVA:", precioConIVA);
                        if (callback) callback(precioTotal);
                    }
                },
                error: function() {
                    console.error("Error al obtener el precio de la comodidad:", comodidadId);
                    comodidadesProcesadas++;
                    if (comodidadesProcesadas === totalComodidades) {
                        var precioConIVA = precioTotal + (precioTotal * IVA);
                        $('#precioTotalSinIVA').val(precioTotal);
            $('#precioTotalConIVA').val(precioConIVA);
    
            $('#formattedPrecioSinIVA').text(`COP ${formatToCOP(precioTotal)}`);
            $('#formattedPrecioConIVA').text(`COP ${formatToCOP(precioConIVA)}`);
                        if (callback) callback(precioTotal);
                    }
                }
            });
        });
    }

    // Actualizar el precio total cuando se cambian las fechas
    $('#FechaInicio, #FechaFin').change(function() {
        actualizarPrecioTotal();
    });

    // Actualizar el precio total cuando se cambia la habitación
    $('#HabitacionId').change(function() {
        actualizarPrecioTotal();
    });

    // Actualizar el precio total cuando se seleccionan/deseleccionan servicios
    $('input[name="Servicios"]').change(function() {
        actualizarPrecioTotal();
    });

    // Actualizar el precio total cuando se seleccionan/deseleccionan comodidades
    $('input[name="Comodidades"]').change(function() {
        actualizarPrecioTotal();
    });

    // Inicializar el precio total
    actualizarPrecioTotal();
});





