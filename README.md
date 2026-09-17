![.NET](https://img.shields.io/badge/.NET-10-blue)
![MongoDB](https://img.shields.io/badge/MongoDB-Database-green)
![React](https://img.shields.io/badge/React-19-61dafb)
![xUnit](https://img.shields.io/badge/Tests-xUnit-success)
[![Backend CI](https://github.com/samueldantasoliveira/Taskly/actions/workflows/backend-ci.yml/badge.svg?branch=main)](https://github.com/samueldantasoliveira/Taskly/actions/workflows/backend-ci.yml?query=branch%3Amain)
[![Frontend CI](https://github.com/samueldantasoliveira/Taskly/actions/workflows/frontend-ci.yml/badge.svg?branch=main)](https://github.com/samueldantasoliveira/Taskly/actions/workflows/frontend-ci.yml?query=branch%3Amain)
[![API Container CI](https://github.com/samueldantasoliveira/Taskly/actions/workflows/api-container-ci.yml/badge.svg?branch=main)](https://github.com/samueldantasoliveira/Taskly/actions/workflows/api-container-ci.yml?query=branch%3Amain)
[![E2E CI](https://github.com/samueldantasoliveira/Taskly/actions/workflows/e2e-ci.yml/badge.svg?branch=main)](https://github.com/samueldantasoliveira/Taskly/actions/workflows/e2e-ci.yml?query=branch%3Amain)

# 🗂️ Taskly

Aplicação para gerenciamento de usuários, equipes, projetos e tarefas, com API em .NET 10, MongoDB e interface web em React.

O projeto foi criado com foco em organização de código, separação de responsabilidades e aplicação prática de conceitos utilizados no desenvolvimento back-end.

---

# 🌐 Demonstração

| Recurso | URL |
| ------- | --- |
| Aplicação web | <a href="https://taskly-web-samueldantasoliveira.onrender.com" target="_blank" rel="noopener noreferrer">Abrir o Taskly</a> |
| API | <a href="https://taskly-api-samueldantasoliveira.onrender.com/health/ready" target="_blank" rel="noopener noreferrer">Health check público</a> |

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
* Playwright (testes E2E com Chromium)

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
 ├── Taskly.E2ETests/
 │   └── tests/
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
| Temas Noturno e Claro               | Noturno por padrão, com alternância e preferência salva no navegador | ✅      |
| Quadro Kanban                       | Filtros, ordenação e histórico incremental por status | ✅      |

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
| Testes E2E            | Fluxos no Chromium com frontend, API e MongoDB reais              | ✅      |
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
| `mongodb-test` | `27018` | Testes de integração e E2E | Descartáveis em memória |

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

#### Testes de ponta a ponta (E2E)

A suíte em `Taskly.E2ETests` utiliza Playwright e Chromium, com frontend, API
e MongoDB reais. Os seis cenários cobrem:

* Cadastro com login automático.
* Login com sessão mantida após recarregar a página.
* Criação de equipe, projeto e tarefa pelo navegador.
* Atribuição, início e conclusão de uma tarefa.
* Filtros combinados por título e responsável, incluindo limpeza dos filtros.
* Carregamento independente dos históricos de concluídas e canceladas.

Cada cenário prepara seus próprios dados e não depende da execução dos demais.
Os helpers criam dados pela API real; as ações do fluxo testado acontecem no navegador.

Partindo da raiz do repositório, inicie o MongoDB de teste e instale as dependências:

```bash
docker compose up -d --wait mongodb-test
# Alternativa com Podman:
# podman-compose up -d mongodb-test

npm ci --prefix Taskly.Web
cd Taskly.E2ETests
nvm use
npm ci
npx playwright install --with-deps chromium
npm run typecheck
npm test
```

O Playwright inicia a API em `http://127.0.0.1:5220` e o frontend em
`http://127.0.0.1:4173`, aguardando ambos ficarem disponíveis. A API utiliza
o banco `TasklyE2E` no MongoDB da porta `27018`. Reserve essas portas para o E2E.
Os dados desse banco não são apagados ao final de cada teste; usuários exclusivos
evitam conflitos entre execuções, e o armazenamento do container é temporário.

Dentro de `Taskly.E2ETests`, também é possível acompanhar e depurar os testes:

```bash
npm run test:ui       # Interface para executar e inspecionar os testes
npm run test:headed   # Executar com o navegador visível
npx playwright test tests/task-creation.spec.ts --debug
npm run report       # Abrir o relatório da última execução
```

Relatórios, screenshots e traces ficam em `playwright-report/` e `test-results/`,
ignorados pelo Git. Screenshots e traces são preservados quando um teste falha.

Volte à raiz antes dos comandos de encerramento abaixo.

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

# 🔄 CI/CD

O GitHub Actions executa quatro workflows em Pull Requests destinados à `main`
e em pushes nessa branch. Os badges no topo mostram os resultados na `main`.

| Workflow | Validação |
| -------- | --------- |
| [Backend CI](.github/workflows/backend-ci.yml) | Restore, build em Release, testes unitários e de integração com MongoDB temporário |
| [Frontend CI](.github/workflows/frontend-ci.yml) | Instalação pelo lockfile, lint, testes e build do frontend |
| [API Container CI](.github/workflows/api-container-ci.yml) | Build do Dockerfile e inicialização da API em Production com MongoDB e chave JWT temporários, verificando `/health/ready` |
| [E2E CI](.github/workflows/e2e-ci.yml) | Verificação TypeScript e testes Playwright no Chromium, com frontend, API e MongoDB reais |

O E2E roda com um worker e até duas novas tentativas por teste que falhar. O
workflow publica o artefato `e2e-results` com relatórios e evidências disponíveis,
mantidos por sete dias na execução do GitHub Actions.

O fluxo de contribuição e publicação é:

```text
Branch de trabalho → PR → Checks aprovados → Merge na main
                                              ↓
                                    CI do commit na main
                                              ↓
                                    Deploy automático no Render
```

O ruleset da `main`, configurado no GitHub, exige PR e os checks configurados antes do
merge. Após a primeira execução do E2E, adicione `E2E - fluxos principais` aos
checks obrigatórios do ruleset; criar o workflow não altera essa configuração.
O `render.yaml` define `autoDeployTrigger: checksPass` nos dois serviços:
após o merge, o Render aguarda os checks do novo commit na `main` antes de iniciar
o deploy automático. São controles separados: um protege o merge e o outro, a
publicação automática.

O Render considera todos os checks detectados, não apenas os exigidos pelo
ruleset. Não inicia o deploy automático se nenhum check for detectado ou se algum
falhar; resultados `success`, `neutral` e `skipped` são aceitos pela plataforma.
Essa configuração não bloqueia deploys manuais.
Veja a [documentação de integração com CI do Render](https://render.com/docs/deploys#integrating-with-ci).

Os filtros de build continuam limitando os deploys aos arquivos relevantes de
cada serviço. Alterações apenas no README não exigem uma nova publicação, embora
a CI continue executando.

Ao aplicar mudanças no Blueprint, confirme sua sincronização no painel do Render
e verifique se os dois serviços exibem **Auto-Deploy: After CI Checks Pass**.
Renomear os jobs dos workflows também exige atualizar os checks do ruleset no
GitHub.

---

# 🤝 Como contribuir

Antes de começar, salve suas alterações em andamento e atualize a `main`:

```bash
git switch main
git pull --ff-only origin main
git switch -c docs/nome-da-mudanca
```

Use um nome que descreva a alteração, com prefixos como `feature/`, `fix/`,
`docs/` ou `ci/`. Faça mudanças com escopo pequeno e execute as verificações
locais relevantes descritas em **Executar os testes e verificações**.

Confira o diff e adicione apenas os arquivos da mudança. Por exemplo, para
uma atualização de documentação:

```bash
git diff -- README.md
git diff --check
git add README.md
git commit -m "docs: describe the change"
git push -u origin docs/nome-da-mudanca
```

No GitHub, abra um Pull Request da sua branch para a `main`, descrevendo a
mudança e como foi validada. Aguarde a aprovação dos checks de backend,
frontend, container da API e E2E. Se o PR precisar ser atualizado com a `main`,
atualize a branch e aguarde a nova execução da CI antes do merge.

Depois do merge, apague a branch remota pelo GitHub. Com o diretório de trabalho
limpo, atualize sua cópia local e remova a branch concluída:

```bash
git switch main
git pull --ff-only origin main
git branch -d docs/nome-da-mudanca
```

Se o Git não reconhecer a branch como integrada, confira o método de merge do
PR antes de forçar a exclusão. Não inclua senhas reais, tokens ou conexões de
produção nos commits; use variáveis de ambiente para esses valores.

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

### Recuperar um deploy com problema

1. **Identificar a falha:** no painel do Render, abra o serviço afetado
   (`taskly-api-samueldantasoliveira` ou `taskly-web-samueldantasoliveira`).
   Confira os logs do deploy e, para erros durante o uso da API, a aba **Logs**.
   Consulte também o [health check da API](https://taskly-api-samueldantasoliveira.onrender.com/health/ready).
   No plano gratuito, aguarde a inicialização após um período sem atividade.
2. **Voltar à versão anterior:** na página **Deploys** do serviço, escolha um
   deploy bem-sucedido e conhecido como funcional, clique em **Rollback** e
   confirme em **Rollback to this deploy**. Só é possível usar versões cujos
   artefatos ainda estejam disponíveis no Render. API e frontend são serviços
   separados; verifique a compatibilidade entre as versões.
3. **Corrigir o código:** o rollback no painel desativa os deploys automáticos
   desse serviço. Corrija o problema ou reverta a alteração por uma nova PR
   para `main` e aguarde os checks. O rollback do Render não altera o Git.
   Após o merge da correção, reative **Auto-Deploy: After CI Checks Pass** em
   **Settings**. Confirme que o commit corrigido foi publicado; se necessário,
   use **Manual Deploy → Deploy latest commit** somente após os checks passarem.
4. **Validar a recuperação:** confirme HTTP `200` no `/health/ready`, faça login
   no frontend e abra um projeto para conferir o Kanban. Revise os logs da API
   para verificar se a falha deixou de ocorrer.

O rollback da aplicação **não restaura os dados do MongoDB**. Problemas de dados
ou de variáveis de ambiente precisam ser tratados separadamente.

Referências: [logs no Render](https://render.com/docs/logging) e
[rollback e reativação dos deploys automáticos](https://render.com/docs/rollbacks).

---

# 📚 Próximos Passos

* ✅ Migrar a API para o .NET 10 LTS
* ✅ Preparar a API e sua imagem Docker para produção
* ✅ Preparar o frontend para produção, com build otimizado e configuração da URL da API
* ✅ Provisionar o MongoDB e publicar o Blueprint no Render
* ✅ Configurar CI do backend, frontend e container da API, com deploy automático condicionado aos checks no Blueprint
* ✅ Adicionar paginação, filtros e ordenação às consultas de tarefas
* ✅ Adicionar testes E2E dos fluxos principais com Playwright e workflow no GitHub Actions
* Expandir a cobertura dos testes automatizados

---

# 📫 Contato

* LinkedIn: https://linkedin.com/in/samuel-dantas-de-oliveira
* GitHub: https://github.com/samueldantasoliveira
