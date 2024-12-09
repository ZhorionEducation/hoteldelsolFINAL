function openNav() {
    document.getElementById("mySidenav").style.width = "250px"; // Expande el sidenav
    document.getElementById("main-content").style.marginLeft = "250px"; // Desplaza el contenido principal hacia la derecha
    document.getElementById("main-content").classList.add("shrink"); // Aplica el escalado
    
}

function closeNav() {
    document.getElementById("mySidenav").style.width = "0"; // Contrae el sidenav
    document.getElementById("main-content").style.marginLeft = "0"; // Restaura el contenido principal a su posición original
    document.getElementById("main-content").classList.remove("shrink"); // Quita el escalado
    
}