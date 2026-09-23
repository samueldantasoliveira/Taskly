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
