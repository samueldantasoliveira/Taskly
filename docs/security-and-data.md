# Segurança e integridade

## Sessões

Cada requisição autenticada verifica a conta ativa e a versão da sessão no MongoDB.
Trocar a senha invalida todos os tokens anteriores; excluir a conta impede seu uso.
Tokens anteriores à implantação deste recurso não possuem versão e exigirão novo login.
Documentos de usuários antigos continuam compatíveis e recebem nova versão ao trocar a senha.
Uma falha do banco não autoriza o acesso e deve ser tratada como indisponibilidade.

Cadastro e troca de senha aceitam de 6 a 128 caracteres, sem senhas compostas apenas
de espaços (compatível com o mínimo já utilizado no frontend). Login não restringe
o mínimo para preservar o acesso de contas antigas.

Login tem limite de 20 tentativas/minuto por IP; cadastro, 10. Valores configuráveis
por `AuthenticationLimits__LoginPerMinute` e `AuthenticationLimits__RegistrationPerMinute`.
O excesso retorna 429 com `Retry-After`. Os limites são locais a cada instância;
ao escalar horizontalmente, deve-se utilizar controle compartilhado ou no proxy.
No Render, cabeçalhos encaminhados só são seguros enquanto a API estiver acessível
exclusivamente pelo proxy confiável da plataforma. Testes isolados usam limites maiores.

Recuperação de senha por e-mail e confirmação de e-mail ainda não estão implementadas.

## Concorrência

Usuários, equipes, projetos e tarefas possuem `Version`. O repositório grava apenas
se a versão lida ainda for atual e incrementa a versão no mesmo comando MongoDB.
Conflitos retornam 409, sem sobrescrever a primeira gravação. Documentos antigos
sem o campo são tratados como versão zero.

As respostas e DTOs de edição incluem `version`; o frontend envia a versão exibida.
Clientes antigos que não enviam a versão continuam compatíveis, mas somente ficam
protegidos contra concorrência durante a operação do servidor. Para detectar uma
edição de tela desatualizada, envie sempre a versão recebida. Após 409, recarregue e
revise os dados antes de reenviar; não repita a escrita automaticamente.

Esse controle é por documento. Ele não substitui transações ou coordenação para
regras envolvendo várias coleções, nem implementa restauração de registros excluídos.

## Saída e responsabilidades

O proprietário pode transferir a equipe ou projeto para outro membro existente pela
tela de edição. Transferir a equipe mantém o antigo proprietário como membro; ele
pode sair depois. O proprietário da equipe também pode recuperar a propriedade de
projetos de membros que saíram.

Remover um membro ou sair da equipe revoga o acesso imediatamente. Depois, as tarefas
Todo/InProgress desse membro na equipe ficam sem responsável, mantendo seu status;
Done/Cancelled preservam o histórico. A exclusão da conta retorna 409 enquanto
houver equipes, projetos ou tarefas ativas sob sua responsabilidade, desconsiderando
recursos cujos projetos/equipes foram excluídos. A exclusão é lógica, não apagamento
físico de todos os dados pessoais.

Limitação: verificações de responsabilidade e alterações em coleções diferentes não
são uma transação. Se houver falha após remover o membro, o acesso continua revogado,
mas pode ser necessário liberar manualmente suas atribuições. Operações simultâneas
entre coleções ainda exigem coordenação; não há garantia global de serialização.
