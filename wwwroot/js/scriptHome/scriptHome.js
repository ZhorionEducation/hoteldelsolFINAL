
document.querySelector('a[href="/#Comodidades"]').addEventListener('click', function(event) {
    event.preventDefault();
    const targetId = this.getAttribute('href').substring(2);
    const targetElement = document.getElementById(targetId);
    const offset = 100; 
    const elementPosition = targetElement.getBoundingClientRect().top;
    const offsetPosition = elementPosition + window.pageYOffset - offset;

    window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
    });
});


document.querySelector('a[href="/#Servicios"]').addEventListener('click', function(event) {
    event.preventDefault();
    const targetId = this.getAttribute('href').substring(2);
    const targetElement = document.getElementById(targetId);
    const offset = 300; 
    const elementPosition = targetElement.getBoundingClientRect().top;
    const offsetPosition = elementPosition + window.pageYOffset - offset;

    window.scrollTo({
        top: offsetPosition,
        behavior: 'smooth'
    });
});
