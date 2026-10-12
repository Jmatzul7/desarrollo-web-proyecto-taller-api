using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string token, DateTime expira) GenerarToken(Usuario usuario, string nombreRol, List<string>? permisos = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Role, nombreRol),
                new("nombreCompleto", usuario.NombreCompleto)
            };

            // Permisos granulares asignados al usuario (tabla UsuarioPermiso), además de su rol.
            if (permisos != null)
                claims.AddRange(permisos.Select(p => new Claim("permiso", p)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expira = DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expira,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}
