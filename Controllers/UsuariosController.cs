using Hotel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hotel.ViewModels;
using Hotel.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Google;
using BCrypt.Net;


namespace Hotel.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HotelContext _context;

        // In-memory store for reset tokens (for demonstration purposes)
        private const int PageSize = 10; //Cuantos items por pagina
        private static Dictionary<string, string> _resetTokens = new Dictionary<string, string>();

        public UsuariosController(HotelContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            int pageNumber = page ?? 1;
            const int PageSize = 5;

            var usuarios = _context.Usuarios
                .Include(u => u.Genero)
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usuarios = usuarios.Where(u =>
                    u.NombreUsuario.Contains(searchString) ||
                    u.Nombre.Contains(searchString) ||
                    u.Apellido.Contains(searchString) ||
                    u.Telefono.Contains(searchString)
                );
            }

            var paginatedList = await PaginatedList<Usuario>.CreateAsync(usuarios.AsNoTracking(), pageNumber, PageSize);

            ViewData["CurrentFilter"] = searchString;
            return View(paginatedList);
        }

        // GET: Usuarios/Details/5
        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Genero)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsUsuarios", usuario); // Vista parcial con los detalles del usuario
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre");
            ViewData["RolId"] = new SelectList(_context.Roles, "Id", "Nombre", new Guid("0B9A405E-EFB9-4E0F-A3AF-29854CBB0A1E"));
            ViewData["TipoDocumentoId"] = new SelectList(_context.TiposDocumento, "Id", "Nombre");

            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreUsuario,CorreoElectronico,Contrasena,Nombre,Apellido,Telefono,FechaNacimiento,GeneroId,RolId,ImagenPerfil,FechaRegistro,Activo,NumeroDocumento,TipoDocumentoId")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.Id = Guid.NewGuid();
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
                usuario.Activo = true;
                usuario.FechaRegistro = DateOnly.FromDateTime(DateTime.Now);

                if (!User?.IsInRole("Administrador") ?? true)
                {
                    usuario.RolId = new Guid("9E658190-A781-4DAA-A16D-BAB03BBA319D"); // Cliente rol
                }

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Only authenticate if not admin
                if (!User?.IsInRole("Administrador") ?? true)
                {
                    var userWithRole = await _context.Usuarios
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.Id == usuario.Id);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, userWithRole.Rol.Nombre),
                new Claim("UserId", usuario.Id.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // If admin, redirect to users list
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre", usuario.GeneroId);
            ViewData["RolId"] = new SelectList(_context.Roles, "Id", "Nombre", usuario.RolId);
            ViewData["TipoDocumentoId"] = new SelectList(_context.TiposDocumento, "Id", "Nombre", usuario.TipoDocumentoId);

            return View(usuario);
        }

        // GET: Usuarios/Edit/5

        [HttpGet]
        [AuthorizePermission("Usuarios")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var editModel = new UsuarioEditViewModel
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Telefono = usuario.Telefono,
                GeneroId = usuario.GeneroId,
                RolId = usuario.RolId,
                Activo = usuario.Activo,
                NumeroDocumento = usuario.NumeroDocumento,
                TipoDocumentoId = usuario.TipoDocumentoId,

                // Población de Select Lists
                Generos = _context.Generos.Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Nombre
                }).ToList(),

                Roles = _context.Roles.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Nombre
                }).ToList(),

                TiposDocumento = _context.TiposDocumento.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre
                }).ToList()
            };

            return PartialView("_EditUsuarios", editModel);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Usuarios")]
        public async Task<IActionResult> Edit(Guid id, UsuarioEditViewModel editModel)
        {
            if (id != editModel.Id)
            {
                return Json(new { success = false, message = "ID no coincide." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Usuarios.FindAsync(id);
                    if (usuario == null)
                    {
                        return Json(new { success = false, message = "Usuario no encontrado." });
                    }

                    usuario.NombreUsuario = editModel.NombreUsuario;
                    usuario.Nombre = editModel.Nombre;
                    usuario.Apellido = editModel.Apellido;
                    usuario.Telefono = editModel.Telefono;
                    usuario.GeneroId = editModel.GeneroId;
                    usuario.RolId = editModel.RolId;
                    usuario.Activo = editModel.Activo;

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Json(new { success = false, message = "Error de concurrencia al actualizar." });
                }
            }

            return Json(new { success = false, message = "Modelo inválido", errors = ModelState.Values.SelectMany(v => v.Errors) });
        }

        // GET: Usuarios/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nombreUsuario, string contrasena)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .ThenInclude(r => r.Permisos)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario != null && BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena))
            {
                // Verificar si el usuario está activo
                if (usuario.Activo)
                {
                    // Crear claims para la autenticación
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
                new Claim("UserId", usuario.Id.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true // Puedes ajustar esta propiedad según tus necesidades
                    };

                    // Iniciar sesión el usuario
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Si el usuario está inactivo, agregar un error al ModelState
                    ModelState.AddModelError(string.Empty, "Tu cuenta está inactiva. Por favor, contacta al administrador.");
                    return View();
                }
            }

            // Si el usuario no se encuentra o la contraseña es incorrecta
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View();
        }

        // POST: Usuarios/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Add new action method

        [HttpGet]
        public IActionResult ExternalLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(
                    action: "ExternalLoginCallback",
                    controller: "Usuarios",
                    values: null,
                    protocol: Request.Scheme),
                AllowRefresh = true,
                IsPersistent = true
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login", new { error = "External authentication failed" });
            }

            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == email);

            if (user == null)
            {
                // Extract user's name from authentication result
                var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);

                // Create new user
                user = new Usuario
                {
                    Id = Guid.NewGuid(),
                    CorreoElectronico = email,                    // From Google
                    NombreUsuario = email,                        // Using email as username
                    Nombre = name?.Split(' ').FirstOrDefault(),
                    Contrasena = Guid.NewGuid().ToString(), // First part of full name
                    Apellido = name?.Split(' ').Skip(1)          // Rest of the name parts
                    .FirstOrDefault() ?? "PendienteActualizar",
                    Telefono = "0000000000",                              // Not provided by Google
                    FechaNacimiento = DateOnly.FromDateTime(DateTime.Now), // Not provided by Google
                    GeneroId = new Guid("DFE73A3B-CD79-4A5E-A102-6A6EA60B3ACE"),                              // Not provided by Google
                    RolId = new Guid("9E658190-A781-4DAA-A16D-BAB03BBA319D"),
                    ImagenPerfil = authenticateResult.Principal.FindFirstValue("picture"), // Google profile picture
                    FechaRegistro = DateOnly.FromDateTime(DateTime.Now),
                    Activo = true,
                    NumeroDocumento = 0,                       // Not provided by Google
                    TipoDocumentoId = new Guid("76726924-4BEA-4EE4-9C9F-0A2BB48CAC1A")                // Not provided by Google
                };

                _context.Usuarios.Add(user);
                await _context.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.NombreUsuario),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", $"{user.Nombre} {user.Apellido}"),
                new Claim(ClaimTypes.Role, user.Rol?.Nombre ?? "User"),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // GET: Usuarios/Delete/5
        [AuthorizePermission("Usuarios")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Genero)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DeleteUsuarios", usuario);
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizePermission("Usuarios")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        // GET: ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST: ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == email);


            if (usuario == null)
            {
                return Json(new { success = false, message = "No se encontró ninguna cuenta con ese correo electrónico" });
            }

            try
            {
                string token = GeneratePasswordResetToken();
                _resetTokens[token] = email;

                var resetLink = Url.Action("ResetPasswordLink", "Usuarios",
                    new { token = token, email = email }, Request.Scheme);

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Deshabilita la validación del certificado

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Hotel Del Sol", "newhoteldelsol@gmail.com")); // Your Gmail
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = "Restablecer Contraseña";
                    message.Body = new TextPart("html")
                    {
                        Text = $@"
                            <h2>Restablecer Contraseña</h2>
                            <p>Para restablecer tu contraseña, haz clic en el siguiente enlace:</p>
                            <p><a href='{resetLink}'>Restablecer Contraseña</a></p>"
                    };

                    // Configure SMTP client
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("newhoteldelsol@gmail.com", "tzao pwlr zmaz xtct"); // Use App Password here
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al enviar el correo: " + ex.Message });
            }
        }

        // GET: ResetPasswordConfirmation
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // GET: ResetPasswordLink
        public IActionResult ResetPasswordLink(string token, string email)
        {
            if (!_resetTokens.ContainsKey(token))
            {
                return BadRequest("Token inválido o expirado.");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        // POST: ResetPasswordLink
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordLinkConfirmed([FromBody] ResetPasswordModel model)
        {
            if (!_resetTokens.ContainsKey(model.Token))
            {
                return BadRequest(new { success = false, message = "Token inválido o expirado." });
            }

            string email = _resetTokens[model.Token];
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoElectronico == email);

            if (usuario != null)
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                _resetTokens.Remove(model.Token);

                return Ok(new { success = true, message = "Contraseña actualizada correctamente" });
            }

            return BadRequest(new { success = false, message = "Usuario no encontrado." });
        }

        private string GeneratePasswordResetToken()
        {
            return Guid.NewGuid().ToString();
        }

        private async Task UpdateUserPassword(string email, string newPassword)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CorreoElectronico == email);

            if (usuario != null)
            {
                // Aquí deberías hashear la contraseña en producción
                usuario.Contrasena = newPassword;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
        }

        // GET: Usuarios/Count
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Count()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Genero)
                .Include(u => u.TipoDocumento)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

            if (usuario == null)
            {
                return NotFound();
            }

            // Populate ViewBag data for dropdowns
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Nombre", usuario.GeneroId);
            ViewData["TipoDocumentoId"] = new SelectList(_context.TiposDocumento, "Id", "Nombre", usuario.TipoDocumentoId);

            return View(usuario);
        }

        // POST: Usuarios/Count
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Count([FromForm] Usuario model)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (userId == null)
                {
                    return Json(new { success = false, message = "Usuario no identificado" });
                }

                var usuario = await _context.Usuarios.FindAsync(Guid.Parse(userId));
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Actualizar solo los campos permitidos
                usuario.Nombre = model.Nombre;
                usuario.Apellido = model.Apellido;
                usuario.Telefono = model.Telefono;
                usuario.GeneroId = model.GeneroId;
                usuario.NumeroDocumento = model.NumeroDocumento;
                usuario.TipoDocumentoId = model.TipoDocumentoId;

                // Actualizar la contraseña si se proporciona una nueva
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cambios guardados correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar los cambios: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerificarDatoUnico(string campo, string valor)
        {
            bool existe = false;
            switch (campo.ToLower())
            {
                case "nombreusuario":
                    existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == valor);
                    break;
                case "correoelectronico":
                    existe = await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == valor);
                    break;
                case "numerodocumento":
                    if (long.TryParse(valor, out long numero))
                    {
                        existe = await _context.Usuarios.AnyAsync(u => u.NumeroDocumento == numero);
                    }
                    break;
            }
            return Json(new { existe });
        }
    }

    public class ResetPasswordModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
