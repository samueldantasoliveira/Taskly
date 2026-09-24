# Migração de Taskly para Rivulus

A aplicação, a solution, os projetos, namespaces, pacotes e documentação usam a
marca **Rivulus**. Alguns identificadores operacionais antigos permanecem
temporariamente para que a mudança não interrompa a produção nem esconda os
dados existentes.

## Compatibilidade mantida

- O banco de produção continua chamado `Taskly`; trocar apenas o nome da
  configuração faria a aplicação abrir um banco vazio.
- O emissor e a audiência JWT continuam como `Taskly.Api`, preservando tokens
  emitidos antes do deploy da nova marca.
- Os serviços e URLs do Render continuam com o prefixo `taskly-`, evitando uma
  mudança de origem sem coordenação de CORS, CSP e frontend.
- O frontend migra automaticamente `taskly.session` e `taskly.theme` para as
  novas chaves `rivulus.*`.
- O monitor aceita tanto `RIVULUS_HEALTH_URL` quanto a variável legada
  `TASKLY_HEALTH_URL`.

Esses nomes são detalhes internos e não aparecem como marca na interface.

## Etapas externas após o merge

1. Validar o deploy nos serviços atuais e confirmar login, Kanban, convites e
   health check.
2. Renomear o repositório no GitHub para `Rivulus`.
3. Atualizar o remoto local:

   ```bash
   git remote set-url origin git@github.com:samueldantasoliveira/Rivulus.git
   ```

4. Atualizar no README os links de clone e badges para o novo endereço.
5. Criar a variável de repositório `RIVULUS_HEALTH_URL`; o fallback antigo pode
   ser removido depois que o novo monitor executar com sucesso.
6. Manter os nomes atuais dos serviços Render até existir um domínio próprio.
   Com um domínio estável, migrar API e frontend sem expor o hostname interno ao
   usuário e atualizar, em conjunto, `AllowedHosts`, CORS, `VITE_API_URL` e CSP.
7. Não renomear o banco diretamente. Se o identificador precisar mudar, criar
   backup verificado, restaurar em `Rivulus`, validar contagens e índices e só
   então alterar `MongoDb__DatabaseName`.
8. Após estabilizar a marca, atualizar currículo, perfil do GitHub e demais
   referências públicas uma única vez.

## Próxima integração

O provedor de e-mail deve ser configurado já com a marca Rivulus. Remetente,
templates e futuro domínio não devem reutilizar o nome anterior.
