# Sincro — API

Backend do **Syncrow**, plataforma de visibilidade operacional para pequenas facções têxteis do Vale do Itajaí (SC). Conecta o dono da facção às confecções contratantes: Kanban de produção, timeline de cada pedido, alertas, relatórios e um portal público para o cliente acompanhar os pedidos dele.

O front-end fica em outro repositório: [frontend-Sincro](https://github.com/igorfriese/frontend-Sincro).

## Stack

- .NET 8 · ASP.NET Core Web API
- EF Core (Code First) · SQL Server LocalDB
- ASP.NET Core Identity · JWT Bearer com refresh token
- Swagger

## Arquitetura

```
Sincro.Domain          entidades e interfaces dos repositórios (não depende de nada)
Sincro.Application     services com as regras de negócio, DTOs e exceções
Sincro.Infrastructure  DbContext, migrations, repositórios, Unit of Work, seeds
Sincro.Presentation    controllers, Program.cs, middleware de erros
```

- Os **controllers** só traduzem HTTP; as regras ficam nos **services** (`PedidoService`, `EtapaService`, `EventoService`, `RelatorioService`, `ClienteService`, `ProdutoService`, `UsuarioService`, `AuthService`).
- Os **repositórios** não salvam: quem salva é o `IUnitOfWork.SalvarAsync()`, chamado pelo service no fim da operação.
- Os services lançam exceções (`NaoEncontradoException`, `RegraDeNegocioException`...) e o `ExceptionHandlingMiddleware` converte em HTTP. **Todo erro sai como `{ "erro": "mensagem" }`**, inclusive os de validação dos DTOs.

## Como rodar

**Pré-requisitos:** SDK do .NET (8 ou mais novo) com o runtime do ASP.NET Core 8, SQL Server LocalDB e o certificado HTTPS de desenvolvimento (`dotnet dev-certs https --trust`).

**1. Configurar os segredos (uma vez por máquina).** A chave do JWT e a connection string **não vão para o Git** — ficam nos user-secrets:

```bash
dotnet user-secrets set "Jwt:Key" "uma-frase-aleatoria-com-32-caracteres-ou-mais" --project Sincro.Presentation
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=SincroDb;Trusted_Connection=True;TrustServerCertificate=True;" --project Sincro.Presentation
```

No Visual Studio dá para fazer o mesmo em: botão direito no `Sincro.Presentation` → **Gerenciar Segredos do Usuário** (lá a barra é dupla: `(localdb)\\mssqllocaldb`).

**2. Rodar.** No Visual Studio, perfil **https** e **F5**. Ou pelo terminal:

```bash
dotnet run --project Sincro.Presentation --launch-profile https
```

A API sobe em `https://localhost:7206` e o Swagger abre em `https://localhost:7206/swagger`. Na primeira vez as migrations são aplicadas sozinhas.

**3. Dados de exemplo.** Em Development, com o banco sem clientes nem pedidos, o `DemoSeeder` cria clientes, produtos, 16 pedidos (em andamento e finalizados, no prazo e atrasados) com timeline, e os usuários abaixo. Todos com a senha `Sincro@123`:

| Email | Perfil |
|---|---|
| `admin@sincro.com` | Administrador |
| `gestor@sincro.com` | Gestor |
| `vendedor@sincro.com` | Vendedor (Júlia) |
| `vendedor2@sincro.com` | Vendedor (Marcos) |

No Swagger: faça `POST /api/auth/login`, copie o `token` e cole em **Authorize** (sem o prefixo `Bearer`).

## Perfis

| | Administrador | Gestor | Vendedor |
|---|---|---|---|
| Pedidos | todos | todos | **só os dele** (responsável) |
| Excluir pedido | ✓ | | |
| Clientes e produtos | cadastra, edita, exclui | cadastra e edita | só consulta (sem link do portal) |
| Configurar colunas do Kanban | ✓ | | |
| Usuários | ✓ | | |
| Relatórios | ✓ | ✓ | |

## Regras do Kanban

- As colunas são as **etapas** (configuráveis). Mover entre elas é livre; a **última etapa** (maior ordem, "Finalizado") é terminal: o pedido sai do Kanban e fica só no Histórico.
- Mudar de etapa é **só** pelo `PUT /api/pedidos/{id}/etapa`, que valida a regra e grava o evento na timeline (com quem moveu). O `PUT /api/pedidos/{id}` comum não mexe na coluna.
- Etapa nova entra antes da final; a chave de uma etapa não muda; não dá para excluir a etapa final nem uma etapa com pedidos.

## Rotas

| Rota | Quem |
|---|---|
| `POST /api/auth/login` · `POST /api/auth/refresh-token` | público |
| `GET /api/pedidos` · `GET /api/pedidos/{id}` · `POST /api/pedidos` · `PUT /api/pedidos/{id}` | logado (Vendedor só os dele) |
| `PUT /api/pedidos/{id}/etapa` `{ etapa, observacao }` | logado (Vendedor só os dele) |
| `DELETE /api/pedidos/{id}` | Administrador |
| `GET /api/pedidos/publico/cliente/{token}` | **público** (portal do cliente) |
| `GET /api/eventos/pedido/{pedidoId}` · `POST /api/eventos` (anotação) | logado (Vendedor só os dele) |
| `GET /api/eventos/publico/cliente/{token}/pedido/{pedidoId}` | **público** |
| `GET /api/etapas` | **público** (o portal desenha as etapas) |
| `POST` · `PUT /{id}` · `PUT /reordenar` · `DELETE /{id}` em `/api/etapas` | Administrador |
| `GET /api/clientes` · `GET /api/produtos` | logado |
| `POST` · `PUT` em `/api/clientes` e `/api/produtos` | Administrador, Gestor |
| `DELETE` em `/api/clientes` e `/api/produtos` | Administrador |
| `GET /api/usuarios/responsaveis` | Administrador, Gestor |
| `GET` · `POST` · `PUT /{id}` · `DELETE /{id}` em `/api/usuarios` | Administrador |
| `GET /api/usuarios/me/dados` · `PUT /api/usuarios/me` · `PUT /api/usuarios/me/senha` | logado |
| `GET /api/relatorios/producao?dataInicio&dataFim` · `GET /api/relatorios/pontualidade` | Administrador, Gestor |

## Migrations

No Console do Gerenciador de Pacotes (projeto padrão: `Sincro.Presentation`):

```
Add-Migration NomeDaMigration -Project Sincro.Infrastructure
Update-Database -Project Sincro.Infrastructure
```
