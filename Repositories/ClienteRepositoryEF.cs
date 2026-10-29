using Microsoft.EntityFrameworkCore;
using TallerMecanicoAPI.Data;
using TallerMecanicoAPI.Helpers;
using TallerMecanicoAPI.Models;

namespace TallerMecanicoAPI.Repositories
{
    public class ClienteRepositoryEF : IClienteRepository
    {
        private readonly AppDbContext _context;
        public ClienteRepositoryEF(AppDbContext context) => _context = context;

        public async Task<List<Cliente>> GetAll() => await _context.Clientes.ToListAsync();

        public async Task<ResultadoPaginado<Cliente>> GetPaginado(ConsultaPaginada consulta)
        {
            var q = _context.Clientes.AsQueryable();
            var b = consulta.TextoBusqueda;
            if (b != null)
                q = q.Where(c => c.Nombre.Contains(b) ||
                                 (c.Email != null && c.Email.Contains(b)) ||
                                 (c.Telefono != null && c.Telefono.Contains(b)));
            if (consulta.Activo.HasValue) q = q.Where(c => c.Activo == consulta.Activo.Value);
            return await ResultadoPaginado<Cliente>.CrearAsync(q.OrderBy(c => c.Nombre).ThenBy(c => c.IdCliente), consulta);
        }

        public async Task<Cliente?> GetById(int id) => await _context.Clientes.FindAsync(id);

        public async Task Add(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Cliente cliente)
        {
            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return;

            _context.Clientes.Remove(cliente);
            await DbErrorHandler.GuardarOManejarConflicto(() => _context.SaveChangesAsync(), "cliente");
        }
    }
}
