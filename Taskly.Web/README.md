# Taskly Web

Interface web do Taskly para organizar equipes, projetos e tarefas em um quadro Kanban.

## Tecnologias

- React 19 e TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form e Zod
- Vitest, Testing Library e MSW
- CSS responsivo sem biblioteca visual externa

## Pré-requisitos

- Node.js 22.12 ou superior (a versão recomendada pelo projeto está no `.nvmrc`)
- API Taskly executando em `http://localhost:5219`

Com NVM, selecione a versão correta com:

```bash
nvm install
nvm use
```

## Configuração

O Vite lê variáveis de arquivos `.env`. Em desenvolvimento, o arquivo `.env.development` já contém:

```env
VITE_API_URL=http://localhost:5219
```

Para outro ambiente, copie `.env.example`, ajuste a URL pública da API e disponibilize `VITE_API_URL` no momento do build. Variáveis `VITE_*` ficam embutidas no bundle e não devem conter segredos.

O build de produção falha deliberadamente quando `VITE_API_URL` estiver
ausente, não for HTTPS ou não representar somente a origem da API. Isso evita
publicar um bundle que tente acessar o backend local.

## Executando

```bash
npm install
npm run dev
```

A interface estará disponível em `http://localhost:5173`, origem já liberada pelo CORS da API no ambiente de desenvolvimento.

O modo noturno é utilizado por padrão. O botão de tema fica disponível na tela
de login e na área autenticada, permitindo alternar entre os modos noturno e
claro. A preferência escolhida é salva no navegador e restaurada nos próximos
acessos.

## Comandos

| Comando | Finalidade |
| --- | --- |
| `npm run dev` | Servidor de desenvolvimento com atualização automática |
| `npm run build` | Verificação TypeScript e bundle de produção |
| `npm run preview` | Prévia local do bundle de produção |
| `npm run lint` | Análise estática do código |
| `npm test` | Suíte de testes uma vez |
| `npm run test:watch` | Testes em modo interativo |

## Produção no Render

O frontend é publicado como **Static Site** e entregue pelo CDN do Render. A
configuração fica no `render.yaml` da raiz do repositório e inclui:

- Node.js 24 e instalação reproduzível com `npm ci`;
- build otimizado do Vite em `dist`;
- cache longo para assets versionados e revalidação do `index.html`;
- cabeçalhos de segurança para todas as páginas;
- rewrite de `/*` para `/index.html`, necessário para abrir diretamente rotas
  como `/teams` e `/projects/:projectId`;
- URL HTTPS da API incorporada no bundle durante o build.

Para reproduzir o build do Render localmente:

```bash
VITE_API_URL=https://taskly-api-samueldantasoliveira.onrender.com npm run build
```

Não coloque senhas, tokens ou strings de conexão em variáveis `VITE_*`, pois
elas são públicas no JavaScript entregue ao navegador.

## Organização

```text
src/
├── app/                  # shell, navegação e proteção das rotas
├── features/             # autenticação e clientes HTTP por domínio
├── pages/                # páginas acessadas pelo roteador
├── shared/
│   ├── api/              # cliente HTTP central
│   ├── components/       # componentes reutilizáveis
│   ├── lib/              # utilitários e chaves de cache
│   └── types/            # contratos compartilhados com a API
├── styles/               # tema e layout responsivo
└── test/                 # configuração global dos testes
```

O token JWT é mantido em `sessionStorage`: ele sobrevive à atualização da aba, mas é removido ao encerrar a sessão do navegador. Respostas `401` limpam automaticamente a sessão e levam o usuário de volta ao login.
