// permisoRol.js

function openModal(modalId, itemId) {
    var modal = document.getElementById(modalId);
    if (!modal) return;

    modal.style.display = "block";
    setTimeout(() => {
        modal.style.opacity = "1";
        modal.style.visibility = "visible";
        modal.querySelector('.modal-content').style.transform = "translateY(0)";
    }, 10);

    if (modalId === 'editPermisoModal') {
        document.getElementById('editPermisoId').value = itemId;
    } else if (modalId === 'editRolModal') {
        document.getElementById('editRolId').value = itemId;
        
        // Cargar datos del rol y sus permisos
        fetch(`/PermisoRol/GetRolDetails/${itemId}`)
            .then(response => response.json())
            .then(data => {
                document.getElementById('editRolNombre').value = data.nombre;
                
                // Desmarcar todos los checkboxes primero
                document.querySelectorAll('input[name="PermisosIds"]').forEach(checkbox => {
                    checkbox.checked = false;
                });

                // Marcar los checkboxes de los permisos existentes
                data.permisos.forEach(permisoId => {
                    const checkbox = document.querySelector(`input[name="PermisosIds"][value="${permisoId}"]`);
                    if (checkbox) {
                        checkbox.checked = true;
                    }
                });
            });
    } else if (modalId === 'deletePermisoModal') {
        document.getElementById('deletePermisoId').value = itemId;
    } else if (modalId === 'deleteRolModal') {
        document.getElementById('deleteRolId').value = itemId;
    } else if (modalId === 'detailsPermisoModal') {
        loadDetails(itemId, true);
    } else if (modalId === 'detailsRolModal') {
        loadDetails(itemId, false);
    }
}

function closeModal(modalId) {
    var modal = document.getElementById(modalId);
    var modalContent = modal.querySelector('.modal-content');

    modalContent.style.transform = "translateY(-20px)";
    modal.style.opacity = "0";

    setTimeout(() => {
        modal.style.display = "none";
        modal.style.visibility = "hidden";
        modalContent.style.transform = "translateY(0)";
    }, 200);
}

function loadDetails(id, isPermiso) {
    var url = '/PermisoRol/Details';
    url += "?id=" + id + "&isPermiso=" + isPermiso;

    fetch(url)
        .then(response => response.text())
        .then(data => {
            if (isPermiso) {
                document.getElementById('detailsPermisoContent').innerHTML = data;
            } else {
                document.getElementById('detailsRolContent').innerHTML = data;
            }
        })
        .catch(error => console.error('Error loading details:', error));
}

let currentRolId;
let currentRolEstado;

function changeEstadoRol(id, estadoActual) {
    Swal.fire({
        title: '¿Cambiar estado?',
        text: `¿Está seguro que desea ${!estadoActual ? 'activar' : 'desactivar'} este rol?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, cambiar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/PermisoRol/ChangeEstadoRol/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ estado: !estadoActual })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const checkbox = document.querySelector(`input[onclick*="changeEstadoRol('${id}')"]`);
                    if (checkbox) {
                        checkbox.checked = !estadoActual;
                        Swal.fire({
                            icon: 'success',
                            title: '¡Estado actualizado!',
                            text: 'El estado del rol ha sido modificado correctamente',
                            timer: 1500,
                            showConfirmButton: false
                        });
                    }
                } else {
                    throw new Error('Error al cambiar estado');
                }
            })
            .catch(error => {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'No se pudo cambiar el estado del rol'
                });
                const checkbox = document.querySelector(`input[onclick*="changeEstadoRol('${id}')"]`);
                if (checkbox) {
                    checkbox.checked = estadoActual;
                }
            });
        } else {
            const checkbox = document.querySelector(`input[onclick*="changeEstadoRol('${id}')"]`);
            if (checkbox) {
                checkbox.checked = estadoActual;
            }
        }
    });
}

// Event listeners para cerrar modales
document.addEventListener('DOMContentLoaded', function () {
    var modals = document.getElementsByClassName('modal');
    for (var i = 0; i < modals.length; i++) {
        modals[i].addEventListener('click', function (event) {
            if (event.target === this) {
                closeModal(this.id);
            }
        });
    }

    // Animaci�n para los botones
    var buttons = document.getElementsByTagName('button');
    for (var i = 0; i < buttons.length; i++) {
        buttons[i].addEventListener('mouseover', function () {
            this.style.transform = 'scale(1.05)';
            this.style.transition = 'transform 0.1s ease';
        });
        buttons[i].addEventListener('mouseout', function () {
            this.style.transform = 'scale(1)';
            this.style.transition = 'transform 0.1s ease';
        });
    }
});

document.addEventListener('DOMContentLoaded', function () {
    var createPermisoForm = document.getElementById('createPermisoForm');
    if (createPermisoForm) {
        createPermisoForm.addEventListener('submit', createPermiso);
    }

    var createRolForm = document.getElementById('createRolForm');
    if (createRolForm) {
        createRolForm.addEventListener('submit', createRol);
    }
});

document.addEventListener('DOMContentLoaded', function () {


    // Evento para el bot�n de confirmaci�n en el modal de cambio de estado
    document.getElementById('confirmChangeEstado').addEventListener('click', confirmChangeEstadoRol);

    // Animaci�n para los botones de cambio de estado
    var changeEstadoButtons = document.querySelectorAll('.btn-warning, .btn-success');
    for (var i = 0; i < changeEstadoButtons.length; i++) {
        changeEstadoButtons[i].addEventListener('mouseover', function () {
            this.style.transform = 'scale(1.05)';
            this.style.transition = 'transform 0.1s ease';
        });
        changeEstadoButtons[i].addEventListener('mouseout', function () {
            this.style.transform = 'scale(1)';
            this.style.transition = 'transform 0.1s ease';
        });
    }
});

// Funci�n para manejar la creaci�n de permisos
function createPermiso(event) {
    event.preventDefault();
    
    const form = event.target;
    const formData = new FormData(form);
    
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenElement) {
        formData.append('__RequestVerificationToken', tokenElement.value);
    }

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        closeModal('addPermisoModal');
        if (data.success) {
            Swal.fire({
                icon: 'success',
                title: '¡Éxito!',
                text: data.message,
                timer: 2000,
                showConfirmButton: false
            }).then(() => {
                location.reload();
            });
        } else {
            throw new Error(data.message || 'Error al crear el permiso');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message || 'No se pudo crear el permiso'
        });
    });
}

// Función para manejar la creación de roles
document.addEventListener('DOMContentLoaded', function() {
    const addRolForm = document.querySelector('#addRolForm');
    if (addRolForm) {
        addRolForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            // Validación del nombre
            const nombreInput = document.getElementById('addRolNombreInput');
            const nombre = nombreInput.value.trim();
            
            // Expresión regular para validar solo letras y espacios
            const regex = /^[A-Za-zñÑ\s]{1,25}$/;
            
            if (!nombre) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'El nombre del rol es requerido'
                });
                return;
            }

            // Validación con regex
            if (!regex.test(nombre)) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'El nombre del rol solo puede contener letras y espacios (1-25 caracteres)'
                });
                return;
            }

            // Validación de permisos
            const permisosSeleccionados = Array.from(
                document.querySelectorAll('#addRolModal input[name="PermisosIds"]:checked')
            ).map(permiso => permiso.value);
            
            if (permisosSeleccionados.length === 0) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Debe seleccionar al menos un permiso'
                });
                return;
            }

            try {
                const formData = new FormData();
                formData.append('Nombre', nombre);
                formData.append('PermisosIds', JSON.stringify(permisosSeleccionados));

                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenElement) {
                    formData.append('__RequestVerificationToken', tokenElement.value);
                }

                const response = await fetch('/PermisoRol/CreateRol', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Error en la respuesta del servidor');
                }

                const data = await response.json();
                
                if (data.success) {
                    closeModal('addRolModal');
                    await Swal.fire({
                        icon: 'success',
                        title: '¡Éxito!',
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false
                    });
                    location.reload();
                } else {
                    throw new Error(data.message || 'Error al crear el rol');
                }
            } catch (error) {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.message || 'Error al crear el rol'
                });
            }
        });
    }
});

// Asignar los manejadores de eventos a los formularios cuando el DOM est� cargado
document.addEventListener('DOMContentLoaded', function () {
    var createPermisoForm = document.getElementById('createPermisoForm');
    if (createPermisoForm) {
        createPermisoForm.addEventListener('submit', createPermiso);
    }

    var createRolForm = document.getElementById('createRolForm');
    if (createRolForm) {
        createRolForm.addEventListener('submit', createRol);
    }
});

// Función para manejar el envío del formulario de edición
document.addEventListener('DOMContentLoaded', function() {
    const editRolForm = document.querySelector('#editRolModal form');
    if (editRolForm) {
        editRolForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            try {
                const formData = new FormData();
                formData.append('Id', document.getElementById('editRolId').value);
                formData.append('Nombre', document.getElementById('editRolNombre').value);
                
                const permisosSeleccionados = Array.from(
                    document.querySelectorAll('input[name="PermisosIds"]:checked')
                ).map(cb => cb.value);
                
                formData.append('PermisosIds', JSON.stringify(permisosSeleccionados));

                // Si usas tokens antiforgery:
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenElement) {
                    formData.append('__RequestVerificationToken', tokenElement.value);
                }

                const response = await fetch('/PermisoRol/EditRol', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
                }

                const data = await response.json();
                
                if (data.success) {
                    closeModal('editRolModal');
                    await Swal.fire({
                        icon: 'success',
                        title: '¡Éxito!',
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false
                    });
                    location.reload();
                } else {
                    throw new Error(data.message || 'Error al actualizar el rol');
                }
            } catch (error) {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.message || 'Error al actualizar el rol'
                });
            }
        });
    }
});

// Add this for delete permiso
function deletePermiso(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: "Esta acción no se puede revertir",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/PermisoRol/DeletePermiso/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => {
                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: '¡Eliminado!',
                        text: 'El permiso ha sido eliminado correctamente',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    throw new Error('Error al eliminar');
                }
            })
            .catch(error => {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'No se pudo eliminar el permiso'
                });
            });
        }
    });
}

// Add this for delete rol
function deleteRol(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: "Esta acción no se puede revertir",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/PermisoRol/DeleteRol/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then(response => {
                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: '¡Eliminado!',
                        text: 'El rol ha sido eliminado correctamente',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        location.reload();
                    });
                } else {
                    throw new Error('Error al eliminar');
                }
            })
            .catch(error => {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'No se pudo eliminar el rol'
                });
            });
        }
    });
};

// Add event listener for editing permissions
document.addEventListener('DOMContentLoaded', function() {
    const editPermisoForm = document.querySelector('#editPermisoModal form');
    if (editPermisoForm) {
        editPermisoForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            try {
                const formData = new FormData(this);
                const response = await fetch('/PermisoRol/EditPermiso', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (data.success) {
                    closeModal('editPermisoModal');
                    await Swal.fire({
                        icon: 'success',
                        title: '¡Éxito!',
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false
                    });
                    location.reload();
                } else {
                    // Mostrar mensaje de error específico
                    await Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: data.message
                    });
                }
            } catch (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'No se pudo actualizar el permiso'
                });
            }
        });
    }
});

// Función para manejar la creación de permisos
document.addEventListener('DOMContentLoaded', function() {
    const addPermisoForm = document.querySelector('#createPermisoForm');
    if (addPermisoForm) {
        addPermisoForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            // Validación del nombre
            const nombreInput = document.querySelector('#createPermisoForm input[name="Nombre"]');
            if (!nombreInput.value.trim()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'El nombre del permiso es requerido'
                });
                return;
            }

            try {
                const formData = new FormData(this);
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenElement) {
                    formData.append('__RequestVerificationToken', tokenElement.value);
                }

                const response = await fetch('/PermisoRol/CreatePermiso', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const data = await response.json();

                if (data.success) {
                    closeModal('addPermisoModal');
                    await Swal.fire({
                        icon: 'success',
                        title: '¡Éxito!',
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false
                    });
                    location.reload();
                } else {
                    throw new Error(data.message || 'No se pudo crear el permiso');
                }

            } catch (error) {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.message || 'No se pudo crear el permiso'
                });
            }
        });
    }

    const createRolForm = document.getElementById('createRolForm');
    if (createRolForm) {
        createRolForm.addEventListener('submit', async function(e) {
            e.preventDefault();

            // Validación del nombre
            const nombreInput = document.querySelector('#createRolForm input[name="Nombre"]');
            if (!nombreInput.value.trim()) {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'El nombre del rol es requerido'
                });
                return;
            }

            try {
                const formData = new FormData(this);
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenElement) {
                    formData.append('__RequestVerificationToken', tokenElement.value);
                }

                const response = await fetch('/PermisoRol/CreateRol', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const data = await response.json();

                if (data.success) {
                    closeModal('addRolModal');
                    await Swal.fire({
                        icon: 'success',
                        title: '¡Éxito!',
                        text: data.message,
                        timer: 2000,
                        showConfirmButton: false
                    });
                    location.reload();
                } else {
                    throw new Error(data.message || 'No se pudo crear el rol');
                }

            } catch (error) {
                console.error('Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: error.message || 'No se pudo crear el rol'
                });
            }
        });
    }
});

// Asegúrate de que solo haya un event listener
document.addEventListener('DOMContentLoaded', function() {
    const createPermisoForm = document.getElementById('createPermisoForm');
    if (createPermisoForm) {
        // Remover event listeners previos
        const newForm = createPermisoForm.cloneNode(true);
        createPermisoForm.parentNode.replaceChild(newForm, createPermisoForm);
        newForm.addEventListener('submit', createPermiso);
    }
});

// Agregar al archivo permisoRol.js

function filterTable(tableType) {
    const searchInput = document.getElementById(tableType === 'roles' ? 'searchRol' : 'searchPermiso');
    const searchText = searchInput.value.toLowerCase();
    const selector = tableType === 'roles' ? '.role-row' : '.permission-row';
    const rows = document.querySelectorAll(selector);
    
    // Reset currentPages
    currentPages[tableType] = 1;
    
    let visibleRows = 0;
    let currentPage = 1;
    
    rows.forEach(row => {
        const name = row.querySelector('td:first-child').textContent.toLowerCase();
        const matchesSearch = name.includes(searchText);
        
        if (matchesSearch) {
            visibleRows++;
            const newPage = Math.ceil(visibleRows / itemsPerPage);
            row.setAttribute('data-page', newPage.toString());
            row.style.display = newPage === 1 ? 'table-row' : 'none';
        } else {
            row.style.display = 'none';
        }
    });
    
    const totalVisiblePages = Math.ceil(visibleRows / itemsPerPage);
    
    // Update pagination display
    const paginationInfo = document.getElementById(`${tableType}CurrentPage`);
    if (paginationInfo) {
        paginationInfo.textContent = `Página 1 de ${totalVisiblePages || 1}`;
    }
    
    // Enable/disable pagination buttons
    const prevBtn = document.querySelector(`button[onclick="changePage('${tableType}', 'prev')"]`);
    const nextBtn = document.querySelector(`button[onclick="changePage('${tableType}', 'next')"]`);
    
    if (prevBtn) prevBtn.disabled = totalVisiblePages <= 1;
    if (nextBtn) nextBtn.disabled = totalVisiblePages <= 1;
}

// Modificar la función changePage existente
function changePage(tableType, direction) {
    const searchInput = document.getElementById(tableType === 'roles' ? 'searchRol' : 'searchPermiso');
    const searchText = searchInput.value.toLowerCase();
    const selector = tableType === 'roles' ? '.role-row' : '.permission-row';
    const rows = document.querySelectorAll(selector);
    
    let visibleRows = Array.from(rows).filter(row => {
        const name = row.querySelector('td:first-child').textContent.toLowerCase();
        return name.includes(searchText);
    });
    
    const totalPages = Math.ceil(visibleRows.length / itemsPerPage);
    
    if (direction === 'prev' && currentPages[tableType] > 1) {
        currentPages[tableType]--;
    } else if (direction === 'next' && currentPages[tableType] < totalPages) {
        currentPages[tableType]++;
    }
    
    visibleRows.forEach((row, index) => {
        const rowPage = Math.ceil((index + 1) / itemsPerPage);
        row.style.display = rowPage === currentPages[tableType] ? 'table-row' : 'none';
    });
    
    // Update pagination display
    const paginationInfo = document.getElementById(`${tableType}CurrentPage`);
    if (paginationInfo) {
        paginationInfo.textContent = `Página ${currentPages[tableType]} de ${totalPages || 1}`;
    }
}

// Modificar la función showPage existente
function showPage(tableType, pageNumber) {
    const searchInput = document.getElementById(tableType === 'roles' ? 'searchRol' : 'searchPermiso');
    const searchText = searchInput.value.toLowerCase();
    const selector = tableType === 'roles' ? '.role-row' : '.permission-row';
    const rows = document.querySelectorAll(selector);
    
    let visibleRows = Array.from(rows).filter(row => {
        const name = row.querySelector('td:first-child').textContent.toLowerCase();
        return name.includes(searchText);
    });
    
    visibleRows.forEach((row, index) => {
        const rowPage = Math.ceil((index + 1) / itemsPerPage);
        row.style.display = rowPage === pageNumber ? 'table-row' : 'none';
    });
    
    const totalPages = Math.ceil(visibleRows.length / itemsPerPage);
    document.getElementById(`${tableType}CurrentPage`).textContent = 
        `Página ${pageNumber} de ${totalPages || 1}`;
}