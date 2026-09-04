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

## Executando

```bash
npm install
npm run dev
```

A interface estará disponível em `http://localhost:5173`, origem já liberada pelo CORS da API no ambiente de desenvolvimento.

## Comandos

| Comando | Finalidade |
| --- | --- |
| `npm run dev` | Servidor de desenvolvimento com atualização automática |
| `npm run build` | Verificação TypeScript e bundle de produção |
| `npm run preview` | Prévia local do bundle de produção |
| `npm run lint` | Análise estática do código |
| `npm test` | Suíte de testes uma vez |
| `npm run test:watch` | Testes em modo interativo |

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
