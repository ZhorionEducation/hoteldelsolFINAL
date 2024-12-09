using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }    // Página actual
        public int TotalPages { get; private set; }   // Total de páginas

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            // Calcula el total de páginas
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Agrega los ítems a la lista
            this.AddRange(items);
        }

        // Indica si hay página anterior
        public bool HasPreviousPage => PageIndex > 1;

        // Indica si hay página siguiente
        public bool HasNextPage => PageIndex < TotalPages;

        // Método estático para crear una instancia de PaginatedList
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await Task.FromResult(source.Count()); // Obtener el total de ítems
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(); // Obtener los ítems de la página actual
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}