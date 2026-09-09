![.NET](https://img.shields.io/badge/.NET-10-blue)
![MongoDB](https://img.shields.io/badge/MongoDB-Database-green)
![React](https://img.shields.io/badge/React-19-61dafb)
![xUnit](https://img.shields.io/badge/Tests-xUnit-success)
[![Backend CI](https://github.com/samueldantasoliveira/Taskly/actions/workflows/backend-ci.yml/badge.svg?branch=main)](https://github.com/samueldantasoliveira/Taskly/actions/workflows/backend-ci.yml?query=branch%3Amain)

# 🗂️ Taskly

Aplicação para gerenciamento de usuários, equipes, projetos e tarefas, com API em .NET 10, MongoDB e interface web em React.

O projeto foi criado com foco em organização de código, separação de responsabilidades e aplicação prática de conceitos utilizados no desenvolvimento back-end.

---

# 🌐 Demonstração

| Recurso | URL |
| ------- | --- |
| Aplicação web | [Abrir o Taskly](https://taskly-web-samueldantasoliveira.onrender.com) |
| API | [Health check público](https://taskly-api-samueldantasoliveira.onrender.com/health/ready) |

> A API utiliza o plano gratuito do Render e pode levar aproximadamente um
> minuto para responder ao primeiro acesso após um período sem atividade.

---

# 🚀 Tecnologias

* .NET 10 LTS (C#)
* ASP.NET Core
* MongoDB
* xUnit
* Moq
* Dependency Injection
* JWT Authentication
* MongoDB.Driver
* Microsoft.AspNetCore.Mvc.Testing
* React 19 e TypeScript
* Vite
* TanStack Query
* React Hook Form e Zod
* Vitest e Testing Library

---

# 🧱 Arquitetura

O projeto está organizado em arquitetura em camadas, separando responsabilidades entre aplicação, domínio, infraestrutura e endpoints da API.

```text
Taskly/
 ├── Taskly.API/
 │   ├── Application/
 │   ├── Controllers/
 │   ├── Domain/
 │   └── Infrastructure/
 │
 ├── Taskly.UnitTests/
 │   ├── Application/
 │   └── Domain/
 │
 ├── Taskly.IntegrationTests/
 │
 └── Taskly.Web/
     └── src/
```

### Camadas

| Camada         | Responsabilidade                    |
| -------------- | ----------------------------------- |
| Controllers    | Endpoints da API                    |
| Application    | Serviços e regras de aplicação      |
| Domain         | Entidades e regras de negócio       |
| Infrastructure | Persistência e integrações externas |

---

# 📌 Funcionalidades

| Funcionalidade                      | Descrição                                             | Status |
| ----------------------------------- | ----------------------------------------------------- | ------ |
| Gerenciamento de Usuários (User)    | Cadastro, atualização e remoção de usuários           | ✅      |
| Autenticação JWT                    | Login com geração de token JWT                        | ✅      |
| Autorização                         | Proteção de rotas com `[Authorize]`                   | ✅      |
| Gerenciamento de Equipes (Team)     | Criação de equipes e adição de membros                | ✅      |
| Gerenciamento de Projetos (Project) | Criação, atualização e remoção de projetos            | ✅      |
| Gerenciamento de Tarefas (TodoTask) | Criação, atualização e atribuição de tarefas          | ✅      |
| Relacionamento entre Entidades      | Usuários, equipes, projetos e tarefas integrados      | ✅      |
| Hash de Senha                       | Armazenamento seguro de credenciais                   | ✅      |
| Soft Delete                         | Exclusão lógica utilizando `DeletedAt`                | ✅      |
| Tratamento de Erros                 | Retornos padronizados com `StructuredOperationResult` | ✅      |
| Interface Web Responsiva            | Fluxos de autenticação, equipes, projetos e perfil     | ✅      |
| Quadro Kanban                       | Criação, atribuição e mudança de estado das tarefas    | ✅      |

---

# 🏗️ Arquitetura e Boas Práticas

| Implementação          | Descrição                                                         | Status |
| ---------------------- | ----------------------------------------------------------------- | ------ |
| Arquitetura em Camadas | Separação entre Controllers, Application, Domain e Infrastructure | ✅      |
| DTOs                   | Separação entre contratos da API e entidades de domínio           | ✅      |
| Repository Pattern     | Persistência desacoplada através de interfaces                    | ✅      |
| Injeção de Dependência | Serviços e repositórios registrados via DI                        | ✅      |
| Validação de Dados     | Regras de validação para entidades e operações                    | ✅      |
| Testes Unitários       | Cobertura de regras de negócio com xUnit e Moq                    | ✅      |
| Testes de Integração   | Testes HTTP utilizando `WebApplicationFactory`                    | ✅      |
| Result Pattern         | Retornos padronizados utilizando `StructuredOperationResult`      | ✅      |

---

# 📊 Modelo de Domínio

```text
User
├─ participa de Teams
├─ pode ser Owner de Projects
└─ pode ser responsável por TodoTasks

Team
├─ possui membros (Users)
└─ possui Projects

Project
├─ pertence a uma Team
├─ possui um Owner (User)
└─ possui TodoTasks

TodoTask
├─ pertence a um Project
└─ pode ser atribuída a um User
```

---

# 🧪 Conceitos Aplicados

* API
* DTOs
* Repository Pattern
* Injeção de Dependência
* Arquitetura em Camadas
* Separação de Responsabilidades
* Regras de Negócio Centralizadas em Services
* Tratamento Padronizado de Erros
* Autenticação JWT
* Autorização com `[Authorize]`
* Soft Delete
* Testes Unitários com xUnit e Moq
* Testes de Integração com `WebApplicationFactory`
* Testes de endpoints HTTP

---

# ▶️ Como executar o projeto

### Pré-requisitos

* .NET 10 SDK
* Docker Compose ou Podman Compose
* Node.js 22.12 ou superior (Node 24 recomendado)

### 1. Clonar o repositório

```bash
git clone https://github.com/samueldantasoliveira/Taskly.git
cd Taskly
```

### 2. Iniciar os bancos MongoDB

O arquivo `compose.yaml` constrói a imagem da API e inicia três containers:

| Serviço | Porta | Uso | Dados |
| ------- | ----- | --- | ----- |
| `api` | `5219` | API ASP.NET Core em um container .NET 10 | Sem dados locais |
| `mongodb` | `27017` | Execução local da API | Persistentes no volume `mongodb-data` |
| `mongodb-test` | `27018` | Testes de integração | Descartáveis em memória |

Com Docker:

```bash
docker compose up --build -d
```

Com Podman:

```bash
podman compose up --build -d
```

Para conferir o estado dos bancos:

```bash
docker compose ps
# ou
podman compose ps
```

A API containerizada usa o endereço interno `mongodb:27017`. A porta `5219` da máquina é encaminhada para a porta `8080` do container. Para escolher outras portas locais, defina `TASKLY_API_PORT`, `TASKLY_MONGO_PORT` e `TASKLY_MONGO_TEST_PORT`, por exemplo:

```bash
TASKLY_API_PORT=5220 TASKLY_MONGO_PORT=27019 docker compose up --build -d
```

As configurações executadas diretamente na máquina continuam apontando para `localhost:27017`, e os testes de integração usam `localhost:27018`.
O `appsettings.Development.json` não é copiado para a imagem; o Compose injeta
os valores locais equivalentes como variáveis de ambiente.

O container `mongodb-test` é reutilizado durante a execução da suíte. Cada classe de testes de integração recebe um banco lógico exclusivo, com nome no formato `TasklyIntegrationTests_<guid>`. O MongoDB cria esse banco na primeira gravação, e a `TasklyApiFactory` o remove automaticamente quando a classe termina. Assim, os testes podem executar isoladamente sem acumular dados entre execuções e sem criar um novo container para cada teste.

### 3. Restaurar as dependências (execução sem container)

Esta etapa é necessária somente se você quiser executar a API diretamente com o SDK instalado:

```bash
dotnet restore
```

### 4. Executar a aplicação

Se você iniciou o Compose completo na etapa 2, a API já estará disponível. Para executá-la diretamente com o SDK em vez do container:

```bash
dotnet run --project Taskly.API/Taskly.API.csproj
```

Em outro terminal, inicie o frontend:

```bash
cd Taskly.Web
nvm use
npm install
npm run dev
```

A aplicação estará em `http://localhost:5173` e a API em `http://localhost:5219`.

### 5. Executar os testes e verificações

```bash
dotnet test
cd Taskly.Web
npm test
npm run lint
VITE_API_URL=https://taskly-api-samueldantasoliveira.onrender.com npm run build
```

Os testes unitários e de integração são executados a partir da solução principal.

Para remover os containers ao terminar:

```bash
docker compose down
# ou
podman compose down
```

O volume de desenvolvimento é preservado. Para também apagar os dados locais:

```bash
docker compose down --volumes
# ou
podman compose down --volumes
```

### 6. Acessar a documentação da API

```text
http://localhost:5219/swagger
```

O Swagger é habilitado somente quando `ASPNETCORE_ENVIRONMENT` está como
`Development`. Em produção, suas rotas retornam `404`.

---

# ☁️ Produção no Render

O projeto está publicado no Render. O `render.yaml` da raiz funciona como o
Blueprint responsável pelos dois serviços:

| Serviço | Tipo | Status | Configuração principal |
| ------- | ---- | ------ | ---------------------- |
| `taskly-api-samueldantasoliveira` | Web Service Docker | ✅ Online | `Taskly.API/Dockerfile` e health check em `/health/ready` |
| `taskly-web-samueldantasoliveira` | Static Site | ✅ Online | Node.js 24, `npm ci && npm run build` e publicação de `dist` |

O frontend recebe cache otimizado para assets, cabeçalhos de segurança e o
rewrite de `/*` para `/index.html` exigido pelas rotas do React. Sua variável
`VITE_API_URL` aponta para a URL HTTPS pública da API e não contém segredos.

O Render fornece `PORT` automaticamente ao backend; a API lê esse valor e
escuta em `0.0.0.0:<PORT>`.

As configurações específicas e os segredos de produção não ficam em arquivos
versionados. O Blueprint configura estas variáveis:

| Variável | Origem |
| -------- | ------ |
| `ASPNETCORE_ENVIRONMENT` | Definida como `Production` no Blueprint |
| `AllowedHosts` | Host público exato da API |
| `Cors__AllowedOrigins__0` | URL HTTPS pública exata do frontend |
| `MongoDb__ConnectionString` | Solicitada de forma secreta ao criar o Blueprint |
| `MongoDb__DatabaseName` | Definida como `Taskly` no Blueprint |
| `Jwt__Key` | Gerada automaticamente pelo Render como Base64 de 256 bits |

Para fazer uma rotação manual futura da chave JWT, gere uma nova com:

```bash
openssl rand -base64 32
```

Não salve o resultado no repositório. Se houver mais de uma origem pública
permitida, use `Cors__AllowedOrigins__1`, `Cors__AllowedOrigins__2` e assim por
diante. Em produção, a API rejeita origens HTTP, locais ou inválidas, o curinga
em `AllowedHosts` e a chave JWT conhecida de desenvolvimento.

O TLS é encerrado pelo proxy do Render. A aplicação aceita os cabeçalhos
encaminhados pelo proxy antes de aplicar HSTS e redirecionamento HTTPS, de modo
que reconhece corretamente a requisição original como HTTPS.

Endpoints de saúde:

| Endpoint | Verificação |
| -------- | ----------- |
| `/health/live` | Processo da API está respondendo |
| `/health/ready` | Conexão real com o MongoDB por meio de `ping` |
| `/health` | Alias compatível para o readiness |

---

# 📚 Próximos Passos

* ✅ Migrar a API para o .NET 10 LTS
* ✅ Preparar a API e sua imagem Docker para produção
* ✅ Preparar o frontend para produção, com build otimizado e configuração da URL da API
* ✅ Provisionar o MongoDB e publicar o Blueprint no Render
* ⏭️ Configurar integração e deploy contínuos
* Adicionar paginação e filtros nas consultas
* Expandir a cobertura dos testes automatizados

---

# 📫 Contato

* LinkedIn: https://linkedin.com/in/samuel-dantas-de-oliveira
* GitHub: https://github.com/samueldantasoliveira
