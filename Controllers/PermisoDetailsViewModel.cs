namespace Hotel.ViewModels;
public class PermisoDetailsViewModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public int CantidadRoles { get; set; }
    public List<string> NombresRoles { get; set; }
}