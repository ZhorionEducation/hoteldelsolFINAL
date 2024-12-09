using Hotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class AuthorizePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _permissions;

    public AuthorizePermissionAttribute(string permissions)
    {
        _permissions = permissions.Split(',').Select(p => p.Trim()).ToArray();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = Guid.Parse(userIdClaim);
        var dbContext = context.HttpContext.RequestServices.GetService(typeof(HotelContext)) as HotelContext;
        var usuario = dbContext.Usuarios
            .Include(u => u.Rol)
            .ThenInclude(r => r.Permisos)
            .FirstOrDefault(u => u.Id == userId);

        if (usuario == null || !_permissions.Any(permission => 
            usuario.Rol.Permisos.Any(p => p.Nombre == permission)))
        {
            context.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/AccessDenied.cshtml",
                StatusCode = 403
            };
        }
    }
}