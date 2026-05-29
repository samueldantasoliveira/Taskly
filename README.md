![.NET](https://img.shields.io/badge/.NET-8-blue)
![MongoDB](https://img.shields.io/badge/MongoDB-Database-green)
![xUnit](https://img.shields.io/badge/Tests-xUnit-success)
# 🗂️ Taskly API

API REST para gerenciamento de usuários, equipes, projetos e tarefas, desenvolvida com .NET 8 e MongoDB.

O projeto foi criado com foco em organização de código, separação de responsabilidades e aplicação prática de conceitos utilizados no desenvolvimento back-end.

---

# 🚀 Tecnologias

* .NET 8 (C#)
* ASP.NET Core
* MongoDB
* xUnit
* Moq
* Dependency Injection
* JWT Authentication
* MongoDB.Driver

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
 └── Taskly.Tests/
```

### Camadas

| Camada | Responsabilidade |
|---------|------------------|
| Controllers | Endpoints da API |
| Application | Serviços e regras de aplicação |
| Domain | Entidades e regras de negócio |
| Infrastructure | Persistência e integrações externas |

---

# 📌 Funcionalidades

| Funcionalidade | Descrição | Status |
|---------------|-----------|--------|
| Gerenciamento de Usuários (User) | Cadastro, atualização e remoção de usuários | ✅ |
| Autenticação JWT | Login com geração de token JWT | ✅ |
| Autorização | Proteção de rotas com `[Authorize]` | ✅ |
| Gerenciamento de Equipes (Team) | Criação de equipes e adição de membros | ✅ |
| Gerenciamento de Projetos (Project) | Criação, atualização e remoção de projetos | ✅ |
| Gerenciamento de Tarefas (TodoTask) | Criação, atualização e atribuição de tarefas | ✅ |
| Relacionamento entre Entidades | Usuários, equipes, projetos e tarefas integrados | ✅ |
| Hash de Senha | Armazenamento seguro de credenciais | ✅ |
| Soft Delete | Exclusão lógica utilizando `DeletedAt` | ✅ |
| Tratamento de Erros | Retornos padronizados com `StructuredOperationResult` | ✅ |

---

# 🏗️ Arquitetura e Boas Práticas

| Implementação | Descrição | Status |
|--------------|-----------|--------|
| Arquitetura em Camadas | Separação entre Controllers, Application, Domain e Infrastructure | ✅ |
| DTOs | Separação entre contratos da API e entidades de domínio | ✅ |
| Repository Pattern | Persistência desacoplada através de interfaces | ✅ |
| Injeção de Dependência | Serviços e repositórios registrados via DI | ✅ |
| Validação de Dados | Regras de validação para entidades e operações | ✅ |
| Testes Unitários | Cobertura de regras de negócio com xUnit e Moq | ✅ |
| Result Pattern | Retornos padronizados utilizando `StructuredOperationResult` | ✅ |

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

* APIs REST
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

---

# ▶️ Como executar o projeto

### Pré-requisitos

- .NET 8 SDK
- MongoDB

### 1. Clonar o repositório

```bash
git clone https://github.com/samueldantasoliveira/Taskly.git
cd Taskly
```

### 2. Configurar a conexão com o MongoDB

No arquivo `appsettings.json`, ajuste a string de conexão conforme seu ambiente:

```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017"
  }
}
```

### 3. Restaurar as dependências

```bash
dotnet restore
```

### 4. Executar a aplicação

```bash
dotnet run
```

### 5. Acessar a documentação da API

```text
https://localhost:<porta>/swagger
```

---

# 📚 Próximos Passos

* Expandir cobertura de testes unitários
* Implementar endpoints de consulta e listagem para projetos e equipes
* Adicionar paginação e filtros nas consultas
* Corrigir problemas de configuração do Swagger
* Realizar deploy da aplicação

---

# 📫 Contato

* LinkedIn: https://linkedin.com/in/samuel-dantas-de-oliveira
* GitHub: https://github.com/samueldantasoliveira
