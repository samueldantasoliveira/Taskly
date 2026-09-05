![.NET](https://img.shields.io/badge/.NET-10-blue)
![MongoDB](https://img.shields.io/badge/MongoDB-Database-green)
![React](https://img.shields.io/badge/React-19-61dafb)
![xUnit](https://img.shields.io/badge/Tests-xUnit-success)
# 🗂️ Taskly

Aplicação para gerenciamento de usuários, equipes, projetos e tarefas, com API em .NET 10, MongoDB e interface web em React.

O projeto foi criado com foco em organização de código, separação de responsabilidades e aplicação prática de conceitos utilizados no desenvolvimento back-end.

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

O arquivo `compose.yaml` cria dois containers MongoDB independentes:

| Serviço | Porta | Uso | Dados |
| ------- | ----- | --- | ----- |
| `mongodb` | `27017` | Execução local da API | Persistentes no volume `mongodb-data` |
| `mongodb-test` | `27018` | Testes de integração | Descartáveis em memória |

Com Docker:

```bash
docker compose up -d
```

Com Podman:

```bash
podman compose up -d
```

Para conferir o estado dos bancos:

```bash
docker compose ps
# ou
podman compose ps
```

As configurações padrão do projeto já apontam a API para `localhost:27017` e os testes de integração para `localhost:27018`.

O container `mongodb-test` é reutilizado durante a execução da suíte. Cada classe de testes de integração recebe um banco lógico exclusivo, com nome no formato `TasklyIntegrationTests_<guid>`. O MongoDB cria esse banco na primeira gravação, e a `TasklyApiFactory` o remove automaticamente quando a classe termina. Assim, os testes podem executar isoladamente sem acumular dados entre execuções e sem criar um novo container para cada teste.

### 3. Restaurar as dependências

```bash
dotnet restore
```

### 4. Executar a aplicação

Em um terminal, inicie a API:

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
npm run build
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

---

# 📚 Próximos Passos

* Expandir cobertura de testes unitários
* Expandir cobertura dos testes de integração
* Adicionar paginação e filtros nas consultas
* Realizar deploy da aplicação

---

# 📫 Contato

* LinkedIn: https://linkedin.com/in/samuel-dantas-de-oliveira
* GitHub: https://github.com/samueldantasoliveira
