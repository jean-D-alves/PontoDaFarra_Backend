# AGENTS.md

## Visão geral
Backend da Ponto da Farra (API REST).

## Stack
- ASP.NET Core, .NET 10
- PostgreSQL com EF Core
- Docker / docker-compose

## Comandos
- Build: `dotnet build`
- Testes: `dotnet test`
- Subir ambiente: `docker compose up -d`

## Arquitetura (Controller → Service → Repository)
- `Controllers/`: apenas HTTP (receber, validar entrada, devolver resposta)
- `Services/`: regras de negócio
- `Repositories/`: acesso ao banco
- `Models/`: entidades das tabelas
- Controllers devolvem DTOs, nunca entidades

## Padrão de código
- Nomes em inglês
- PascalCase: classes, métodos, propriedades. camelCase: variáveis locais e parâmetros. Interfaces com prefixo `I`
- Métodos com até 20 linhas (exceto `Program.cs`)
- Nenhuma lógica de negócio no controller
- Métodos de I/O sempre async

## Segurança
- Nunca logar tokens, senhas ou dados sensíveis
- Não enfraquecer rate limiting nem as regras de autenticação
- Secrets apenas por variável de ambiente

## Restrições
- Não instalar pacotes sem aprovação
- Não criar nem alterar migrations; se o model mudar, avisar para eu criar
- Não usar `Console.WriteLine` em produção; usar `ILogger`