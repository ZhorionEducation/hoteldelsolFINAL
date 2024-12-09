
$(document).ready(function () {
    initializeHuespedesLimit();

    $('#HabitacionId').change(function () {
        var habitacionId = $(this).val();
        if (habitacionId) {
            $.ajax({
                url: '/Reservas/GetHabitacionDetails',
                type: 'GET',
                data: { id: habitacionId },
                success: function (response) {
                    if (response) {
                        updateMaxHuespedes(response.tipoHabitacion);
                        $('#maxHuespedesActual').val(maxHuespedes);
                        $('#tipoHabitacionActual').val(response.tipoHabitacion);
                        $('#huespedesContainer').empty();
                        $('#numeroAcompanantes').val(0);
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'No se pudo obtener los detalles de la habitación.'
                        });
                    }
                }
            });
        }
    });

    $('form').submit(function (e) {
        if (!validateHuespedes()) {
            e.preventDefault();
        }
    });
});

let maxHuespedes = 0;
let isAdmin = false;

function updateMaxHuespedes(tipoHabitacion) {
    switch (tipoHabitacion) {
        case 'Individual':
            maxHuespedes = 1;
            break;
        case 'Doble':
            maxHuespedes = 2;
            break;
        case 'Deluxe':
            maxHuespedes = 5;
            break;
        default:
            maxHuespedes = 0;
    }

    if (isAdmin) {
        maxHuespedes += 1;
    }

    console.log(`Max huespedes updated to: ${maxHuespedes}`);
}

function initializeHuespedesLimit() {
    const tipoHabitacionActual = $('#tipoHabitacionActual').val();
    updateMaxHuespedes(tipoHabitacionActual);
}

function addHuesped() {
    var index = $('#huespedesContainer .huesped-item').length;
    const maxHuespedesActual = parseInt($('#maxHuespedesActual').val()) || maxHuespedes;

    if (index >= maxHuespedesActual) {
        Swal.fire({
            icon: 'error',
            title: 'Límite de huéspedes',
            text: `Esta habitación solo permite ${maxHuespedesActual} acompañante(s).`
        });
        return;
    }

    var newHuesped = `
        <div class="huesped-item" data-index="${index}">
            <input type="hidden" name="Huespedes[${index}].Id" value="00000000-0000-0000-0000-000000000000" />
            <div class="form-group">
                <label>Nombre</label>
                <input type="text" name="Huespedes[${index}].Nombre" class="form-control" />
            </div>
            <div class="form-group">
                <label>Documento Identidad</label>
                <input type="text" name="Huespedes[${index}].DocumentoIdentidad" class="form-control" />
            </div>
            <button type="button" class="btn btn-danger" onclick="confirmRemoveHuesped(this)">Eliminar</button>
        </div>`;
    $('#huespedesContainer').append(newHuesped);
    actualizarNumeroAcompanantes();
}

function confirmRemoveHuesped(button) {
    Swal.fire({
        title: '¿Estás seguro?',
        text: "No podrás revertir esto",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminarlo',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            removeHuesped(button);
            Swal.fire(
                'Eliminado',
                'El huésped ha sido eliminado.',
                'success'
            );
        }
    });
}

function removeHuesped(button) {
    $(button).closest('.huesped-item').remove();
    actualizarNumeroAcompanantes();
}

function actualizarNumeroAcompanantes() {
    var cantidad = $('#huespedesContainer .huesped-item').length;
    $('#numeroAcompanantes').val(cantidad);
}

function validateHuespedes() {
    var isValid = true;
    var huespedes = $('#huespedesContainer .huesped-item');
    var documentos = [];

    if (huespedes.length === 0) return true;

    huespedes.each(function () {
        var nombre = $(this).find('input[name*="Nombre"]').val().trim();
        var documento = $(this).find('input[name*="DocumentoIdentidad"]').val().trim();

        if (nombre === '' || !/^[a-zA-Z\s]{1,20}$/.test(nombre)) {
            isValid = false;
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'El nombre del huésped debe ser alfabético y no exceder los 20 caracteres.'
            });
            return false;
        }

        if (documento === '' || !/^\d{7,10}$/.test(documento)) {
            isValid = false;
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'El documento de identidad debe ser numérico y tener de 7 a 10 dígitos.'
            });
            return false;
        }

        if (documentos.includes(documento)) {
            isValid = false;
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'El documento de identidad debe ser único y no puede repetirse.'
            });
            return false;
        }

        documentos.push(documento);
    });
    return isValid;
}

function initializeEditCalendar(currentReservaId) {
    var fechasOcupadas = [];

    function loadFechasOcupadas(habitacionId) {
        if (habitacionId) {
            $.ajax({
                url: '/Reservas/GetFechasOcupadas',
                type: 'GET',
                data: { habitacionId: habitacionId },
                success: function (data) {
                    // Filtrar para excluir las fechas de la reserva actual
                    fechasOcupadas = data
                        .filter(item => item.reservaId !== currentReservaId)
                        .map(function (item) {
                            var start = new Date(item.fechaInicio);
                            var end = new Date(item.fechaFin);
                            start.setDate(start.getDate() );
                            end.setDate(end.getDate() + 2);
                            return {
                                start: start,
                                end: end
                            };
                        });
                    
                    $('#FechaInicio, #FechaFin').datepicker('refresh');
                }
            });
        }
    }

    function disableDates(date) {
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
            var fechaInicio = new Date($('#FechaInicio').val());
            var fechaFin = new Date($('#FechaFin').val());
            $(this).datepicker("hide");

            if (fechaFin && fechaFin <= fechaInicio) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error en fechas',
                    text: 'La fecha de fin debe ser posterior a la fecha de inicio'
                });
                $('#FechaFin').val('');
                return;
            }

            if (isDateRangeOccupied(fechaInicio, fechaFin)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Fechas no disponibles',
                    text: 'El rango de fechas seleccionado está ocupado'
                });
                $('#FechaInicio').val('');
                $('#FechaFin').val('');
            }
        }
    });

    function isDateRangeOccupied(startDate, endDate) {
        if (!startDate || !endDate) return false;
        
        startDate.setHours(0, 0, 0, 0);
        endDate.setHours(0, 0, 0, 0);

        for (var i = 0; i < fechasOcupadas.length; i++) {
            var occupied = fechasOcupadas[i];
            var occupiedStart = new Date(occupied.start);
            var occupiedEnd = new Date(occupied.end);
            
            occupiedStart.setHours(0, 0, 0, 0);
            occupiedEnd.setHours(0, 0, 0, 0);

            if ((startDate >= occupiedStart && startDate <= occupiedEnd) ||
                (endDate >= occupiedStart && endDate <= occupiedEnd) ||
                (startDate <= occupiedStart && endDate >= occupiedEnd)) {
                return true;
            }
        }
        return false;
    }

    // Cargar fechas ocupadas cuando cambia la habitación
    $('#HabitacionId').change(function() {
        loadFechasOcupadas($(this).val());
    });

    // Cargar fechas ocupadas inicialmente
    loadFechasOcupadas($('#HabitacionId').val());
}