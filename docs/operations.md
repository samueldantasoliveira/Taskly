# Operação: diagnóstico, backup e restauração

## Diagnóstico e disponibilidade

Erros inesperados geram log JSON com exceção e `RequestId`. A resposta 500 contém
mensagem pública e `traceId`, também enviado como `X-Request-ID`. Use esse código
nos logs do Render; não compartilhe logs completos que possam conter dados pessoais.
O middleware não registra corpos, senhas, tokens nem connection strings. Detalhes
de exceções ainda devem ser tratados como informação restrita.

`bash scripts/check-health.sh https://SUA-API/health/ready` exige HTTP 200 e tenta
três vezes. O endpoint verifica a conexão com o MongoDB. O health check do Render
e um alerta externo têm propósitos diferentes: o primeiro controla a instância;
o segundo avisa você. Veja [health checks do Render](https://render.com/docs/health-checks).

O workflow **API Health Monitor** está preparado para executar a cada seis horas:

1. Após o merge, crie a variável de repositório `RIVULUS_HEALTH_URL` com a URL HTTPS
   completa do `/health/ready` (sem credenciais).
2. Ative notificações de falha de Actions na sua conta e execute o workflow manualmente.
3. Confirme que recebe uma notificação de falha antes de considerar o alerta ativo.

Sem a variável, o job é ignorado. Não é monitoramento em tempo real nem garantia
de entrega de alertas. Agendamentos podem atrasar. O serviço gratuito pode demorar
a iniciar depois de inatividade; veja [limitações do plano gratuito](https://render.com/docs/free).
Não adicione esse workflow ou o drill com filtro de caminhos aos checks obrigatórios de PR.

## Backup de produção (configuração externa pendente)

Ainda não há backup de produção agendado por este repositório. Antes de depender
do serviço para dados reais, configure backup e armazenamento privado fora do
container da API, com criptografia, retenção e aviso em caso de falha.
Ponto de partida a aprovar: backup diário, retenção de sete dias e drill mensal.
Isso sugere perda máxima de até 24 horas apenas se os backups realmente ocorrerem;
não há RPO/RTO garantidos ou medidos em produção.

Para um dump manual, use MongoDB Database Tools e um arquivo de configuração fora
do repositório, legível somente pelo dono (`chmod 600`), contendo a URI com usuário
de backup de menor privilégio. Não coloque a senha no comando, Git ou saída de CI.
Use diretório novo e privado e registre data, versão do MongoDB e origem junto do arquivo:

```bash
umask 077
backup_dir=$(mktemp -d /tmp/rivulus-backup.XXXXXXXX)
mongodump --config /CAMINHO/PRIVADO/backup.yml --db Taskly \
  --gzip --archive="$backup_dir/rivulus.archive.gz"
sha256sum "$backup_dir/rivulus.archive.gz"
```

`Taskly` permanece como identificador interno do banco de produção durante a
migração de marca para evitar perda aparente dos dados. Confirme o nome real do
banco antes de executar. Transfira o arquivo para armazenamento
criptografado; `/tmp` não é retenção. Um dump de banco com escritas concorrentes não
garante consistência entre coleções: planeje janela sem escritas ou use mecanismo
de snapshot consistente compatível com seu cluster. Documentação: [mongodump](https://www.mongodb.com/docs/database-tools/mongodump/).

## Recuperação segura

Nunca teste restore no banco de produção. Use outro cluster/banco vazio, credenciais
próprias e nome como `RivulusRestore_20260923`. Confira o checksum antes de restaurar:

```bash
mongorestore --config /CAMINHO/PRIVADO/restore.yml \
  --gzip --archive=/CAMINHO/backup/rivulus.archive.gz \
  --nsInclude 'Taskly.*' --nsFrom 'Taskly.*' --nsTo 'RivulusRestore_20260923.*' \
  --stopOnError
```

Não use `--drop`. O destino deve estar vazio e isolado. Valide contagens, índices,
referências e login/criação/leitura em uma API local apontando para o banco restaurado.
Somente depois planeje uma eventual troca de produção, com aprovação e rollback.
Os namespaces são renomeados por `nsFrom`/`nsTo`: [mongorestore](https://www.mongodb.com/docs/database-tools/mongorestore/).

Para ensaiar sem tocar dados reais:

```bash
bash scripts/recovery-drill.sh
# Ou:
CONTAINER_ENGINE=podman bash scripts/recovery-drill.sh
```

O script cria um MongoDB sem rede externa, portas ou volumes do host; usa dados
sintéticos, faz dump/restore em outro banco e verifica documentos, referências e
índice único. O container temporário e seus dados são removidos no final, inclusive
em falhas comuns. O workflow **Recovery Drill** repete esse ensaio nas mudanças do
script ou por execução manual. Isso valida o procedimento, não um backup real do Atlas.

Rollback da aplicação continua descrito na seção de recuperação do [README](../README.md).
