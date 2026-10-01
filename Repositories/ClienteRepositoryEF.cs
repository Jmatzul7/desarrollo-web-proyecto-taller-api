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
