# MiauShop 🐾

Loja de dropshipping pet **demonstrativa** — projeto acadêmico com checkout simulado (sem pagamento real).

## Stack

- ASP.NET Core MVC (.NET 10 LTS, C#)
- Entity Framework Core + PostgreSQL (via Docker)
- Bootstrap 5 com identidade visual própria

## Arquitetura

```
MiauShop/
├── Controllers/    # recebem a requisição e escolhem a resposta
├── Models/
│   ├── Entities/   # classes que viram tabelas no banco
│   └── ViewModels/ # dados montados para cada tela
├── Views/          # telas Razor (.cshtml)
├── Services/       # regras de negócio
├── Repositories/   # acesso ao banco (interfaces + implementações)
├── Data/           # AppDbContext, migrations e dados de exemplo
└── wwwroot/        # CSS, JS e imagens
```

Fluxo de uma requisição: **Controller → Service → Repository → DbContext → PostgreSQL**, e a resposta volta para a **View**.

> ⚠️ Projeto demonstrativo: nenhum pedido é real e nenhum pagamento é processado.
