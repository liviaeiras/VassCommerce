# VassCommerce API

API de e-commerce construída com ASP.NET Core, Entity Framework Core e SQL Server.

## Configuração local

Configure uma chave JWT fora do repositório. A chave deve ter pelo menos 32 bytes:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "SUBSTITUA-POR-UMA-CHAVE-FORTE-DE-PELO-MENOS-32-BYTES"
dotnet user-secrets set "Jwt:Issuer" "VassCommerce.Api"
dotnet user-secrets set "Jwt:Audience" "VassCommerce.Client"
dotnet user-secrets set "Jwt:ExpiresMinutes" "120"
```

O projeto já possui um `UserSecretsId`; executar `dotnet user-secrets init` só é
necessário se o identificador não estiver presente. Também é possível usar
variáveis de ambiente, como `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` e
`Jwt__ExpiresMinutes`. A API falha na inicialização se a chave, o emissor, a
a audiência ou um tempo de expiração positivo não estiverem configurados.

Defina a conexão SQL Server em `ConnectionStrings:DefaultConnection` usando
User Secrets ou `ConnectionStrings__DefaultConnection` no ambiente. Para
provisionar um administrador inicial opcional, configure também `Admin:Email`
e `Admin:Password` fora do repositório; o cadastro público sempre cria somente
clientes. Em bancos de desenvolvimento existentes, rotacione ou remova a conta
administrativa legada antes de disponibilizar a aplicação.

## Banco de dados

Instale a ferramenta EF CLI, se necessário, e aplique as migrations do projeto:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update
```

Na inicialização, a API também aplica migrations pendentes. Não foi necessária
uma migration para as mudanças de autenticação.

## Executar a API e abrir o Swagger

Na pasta `VassCommerce.Api`:

```powershell
dotnet restore
dotnet run
```

Com o perfil HTTP do `launchSettings.json`, abra
`http://localhost:5163/swagger`. Sem um perfil de lançamento, a URL padrão é
`http://localhost:5080/swagger`.

## Cadastro e login

O cadastro público aceita somente os dados de cliente. CPF e data de nascimento
continuam necessários para criar o perfil de cliente; propriedades adicionais,
como perfil administrativo, não são aceitas:

```http
POST /auth/register
Content-Type: application/json

{
  "nomeCompleto": "Pessoa Exemplo",
  "email": "pessoa@example.com",
  "senha": "SUBSTITUA-POR-UMA-SENHA-FORTE",
  "dataNascimento": "1995-04-12T00:00:00",
  "cpf": "111.222.333-44"
}
```

O login usa apenas e-mail e senha:

```http
POST /auth/login
Content-Type: application/json

{
  "email": "pessoa@example.com",
  "senha": "SUBSTITUA-PELA-SENHA-DO-CADASTRO"
}
```

As senhas são armazenadas com BCrypt. O cadastro responde `201 Created`, e o
login responde `200 OK`; credenciais inválidas respondem `401 Unauthorized`.

## Autorizar no Swagger

Faça login, copie o valor de `token` da resposta, clique em **Authorize** no
Swagger e informe somente o token JWT, sem adicionar o prefixo `Bearer`.
Endpoints privados validam assinatura, emissor, audiência e expiração. Consultas
públicas do catálogo continuam acessíveis sem autenticação.

## Testes

Os testes usam banco EF Core InMemory isolado, sem dependência do banco SQL
Server da desenvolvedora:

```powershell
dotnet test .\tests\VassCommerce.Api.Tests\VassCommerce.Api.Tests.csproj
```
