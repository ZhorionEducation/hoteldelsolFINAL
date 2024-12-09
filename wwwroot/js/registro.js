document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('registroForm');

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        // Validar que los emails coincidan
        const email = document.getElementById('email').value;
        const confirmarEmail = document.getElementById('confirmarEmail').value;

        if (email !== confirmarEmail) {
            alert('Los emails no coinciden');
            return;
        }

        // Simulación de envío del formulario
        this.style.opacity = '0.5';
        this.querySelector('button').textContent = 'Enviando...';

        setTimeout(() => {
            alert('¡Registro exitoso!');
            this.reset();
            this.style.opacity = '1';
            this.querySelector('button').textContent = 'Registrarse';
        }, 2000);
    });

    // Animación de etiquetas flotantes
    document.querySelectorAll('input, select').forEach(element => {
        element.addEventListener('focus', function () {
            this.previousElementSibling.style.color = '#b8960c';
            this.previousElementSibling.style.transform = 'translateY(-25px) scale(0.8)';
        });

        element.addEventListener('blur', function () {
            if (!this.value) {
                this.previousElementSibling.style.color = '';
                this.previousElementSibling.style.transform = '';
            }
        });

        // Verificar si el campo ya tiene valor al cargar la página
        if (element.value) {
            element.previousElementSibling.style.transform = 'translateY(-25px) scale(0.8)';
        }
    });

    // Establecer la fecha máxima permitida (18 años atrás desde hoy)
    const fechaNacimiento = document.getElementById('fechaNacimiento');
    const hoy = new Date();
    const maxDate = new Date(hoy.getFullYear() - 18, hoy.getMonth(), hoy.getDate());
    fechaNacimiento.max = maxDate.toISOString().split('T')[0];
});