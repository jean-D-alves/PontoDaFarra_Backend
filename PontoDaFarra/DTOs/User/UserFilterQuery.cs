using System;

namespace PontoDaFarra.DTOs.User
{
    public class UserFilterQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Quando null/vazio, o filtro não é aplicado.
        public string? Role { get; set; }

        // Busca por nome ou e-mail.
        public string? Search { get; set; }
    }
}
