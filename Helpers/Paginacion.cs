using TallerMecanicoAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace TallerMecanicoAPI.Helpers
{
    // Parámetros de paginación y filtro para los listados (?pagina=1&tamano=10&buscar=...).
    // Si NO se envía "pagina", los endpoints siguen devolviendo la lista completa (compatibilidad).
    public class ConsultaPaginada
    {
        public const int TamanoPorDefecto = 10;
        public const int TamanoMaximo = 100;

        private int? _tamano;

        public int? Pagina { get; set; }

        public int Tamano
        {
            get => Math.Clamp(_tamano ?? TamanoPorDefecto, 1, TamanoMaximo);
            set => _tamano = value;
        }

        // Texto libre: se busca en los campos relevantes de cada entidad.
        public string? Buscar { get; set; }

        // Filtros opcionales (cada listado usa los que le aplican).
        public int? Estado { get; set; }        // órdenes: IdEstadoOrden
        public bool? Activo { get; set; }       // clientes, vehículos, repuestos, usuarios
        public bool? StockBajo { get; set; }    // repuestos: existencia <= 5
        public int? IdMecanico { get; set; }    // órdenes: solo las asignadas a ese mecánico

        public string? TextoBusqueda => string.IsNullOrWhiteSpace(Buscar) ? null : Buscar.Trim();
    }

    public class ResultadoPaginado<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int Tamano { get; set; }
        public int TotalPaginas { get; set; }

        public ResultadoPaginado<TDestino> Convertir<TDestino>(Func<T, TDestino> mapa) => new()
        {
            Items = Items.Select(mapa).ToList(),
            Total = Total,
            Pagina = Pagina,
            Tamano = Tamano,
            TotalPaginas = TotalPaginas
        };

        // Cuenta el total y trae solo la página pedida. Si la página pedida ya no existe
        // (p. ej. se borró el último registro de la última página) devuelve la última.
        public static async Task<ResultadoPaginado<T>> CrearAsync(IQueryable<T> consulta, ConsultaPaginada parametros)
        {
            var tamano = parametros.Tamano;
            var total = await consulta.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
            var pagina = Math.Clamp(parametros.Pagina ?? 1, 1, totalPaginas);

            var items = await consulta.Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

            return new ResultadoPaginado<T>
            {
                Items = items,
                Total = total,
                Pagina = pagina,
                Tamano = tamano,
                TotalPaginas = totalPaginas
            };
        }
    }
}
