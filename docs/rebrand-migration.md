# Migração de Taskly para Rivulus

A aplicação, a solution, os projetos, namespaces, pacotes e documentação usam a
marca **Rivulus**. Os identificadores públicos antigos permanecem temporariamente
para que links já divulgados continuem funcionando.

## Compatibilidade mantida

- Os serviços e URLs do Render continuam com o prefixo `taskly-`, evitando uma
  mudança de origem sem coordenação de CORS, CSP e frontend.
- O frontend migra automaticamente `taskly.session` e `taskly.theme` para as
  novas chaves `rivulus.*`.
- O monitor aceita tanto `RIVULUS_HEALTH_URL` quanto a variável legada
  `TASKLY_HEALTH_URL`.

Esses nomes são detalhes internos e não aparecem como marca na interface.

## Reinicialização dos dados

O banco configurado passa a ser `Rivulus`. Essa decisão inicia a aplicação sem
as contas e projetos do banco anterior. O emissor e a audiência dos novos tokens
também passam a ser `Rivulus.Api`.

O banco `Taskly` antigo não é apagado automaticamente e pode ser mantido por um
curto período como rollback. Depois de validar o novo deploy e confirmar que os
dados antigos não são necessários, ele pode ser removido manualmente no Atlas.

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
7. Validar a criação dos índices e uma nova conta no banco `Rivulus`; somente
   depois decidir se o banco antigo pode ser removido no Atlas.
8. Após estabilizar a marca, atualizar currículo, perfil do GitHub e demais
   referências públicas uma única vez.

## Próxima integração

O provedor de e-mail deve ser configurado já com a marca Rivulus. Remetente,
templates e futuro domínio não devem reutilizar o nome anterior.
