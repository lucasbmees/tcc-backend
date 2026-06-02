# Plano de Testes — Fluxo Completo (TCC Shark Tank)

Este plano descreve a ordem recomendada para testar o sistema ponta a ponta (front + API), com usuários específicos, dados de teste e validações por perfil/plano.

## 0) Pré-requisitos

- Front: `http://127.0.0.1:5173/`
- API: `http://127.0.0.1:5253/` (Swagger na raiz)
- Banco (SQLite) em desenvolvimento: [tcc_sharktank.db](file:///Users/lucas/Desktop/faculdade/tcc-26/TCC-2026/src/TccSharkTank.WebApi/tcc_sharktank.db)

### Usuários de teste (seed automático em Development)

Senha padrão para todos: `123456`

- **Admin (adm)**: `admin@tcc.local`
- **Investidor Básico (basico)**: `investidor@tcc.local`
- **Investidor Elite (elite)**: `elite@tcc.local`
- **Empreendedor Básico (basico)**: `empreendedor@tcc.local`
- **Empreendedor Pro (pro)**: `pro@tcc.local`

### Dados seed para acelerar o fluxo

- Ideia do empreendedor PRO: **"Ideia Demo (Docs + Proposta Aceita)"**
  - Possui **documento** (para testar filtro Elite “somente com documentos”)
  - Possui **proposta do Investidor Elite já aceita** (para liberar chat/contrato e priorização)

## 1) Smoke test de infraestrutura (obrigatório antes de tudo)

1. Abrir `http://127.0.0.1:5253/` e validar Swagger carregando.
2. Abrir `http://127.0.0.1:5173/` e validar Home/Dashboard renderizando.
3. Validar que o front está chamando a API (login deve retornar token).

Critério de aceite: não há erro 5xx e o login funciona.

### 1.1 Smoke test automatizado (API)

Arquivo: [smoke.test.mjs](file:///Users/lucas/Desktop/faculdade/tcc-26/TCC-2026/smoke.test.mjs)

Execução:
- `node smoke.test.mjs`

Cobertura (automatizada):
- Empreendedor cria ideia
- Investidor envia proposta
- Empreendedor aceita proposta
- Chat entre investidor e empreendedor (após aceite)
- Contraproposta: empreendedor envia e investidor aceita
- Contraproposta: empreendedor envia e investidor recusa

## 2) Autenticação e segurança (Auth)

### 2.1 Login

1. Logar com `investidor@tcc.local`.
2. Validar navegação para a área autenticada e token salvo.
3. Repetir com `empreendedor@tcc.local`, `elite@tcc.local`, `pro@tcc.local`, `admin@tcc.local`.

Critérios:
- Token contém `role` e `plan` (o sistema usa `claim plan`).

### 2.2 Cadastro (Register)

1. Criar um usuário novo (escolher cargo “investidor” e outro “empreendedor”).
2. Validar conflitos:
   - cadastrar com e-mail já existente → deve falhar (409)
   - cadastrar com cpf já existente → deve falhar (409)
   - cadastrar com telefone já existente → deve falhar (409)

### 2.3 Recuperação de senha

1. Executar “Recuperar senha” para um e-mail válido.
2. Validar retorno de token (ambiente simulado).
3. Executar “Redefinir senha” com token retornado.
4. Logar com a nova senha.

## 3) Perfil do usuário

Executar com **investidor** e com **empreendedor**.

1. Abrir Perfil.
2. Editar:
   - Nome / sobrenome / telefone / e-mail
   - Campos de perfil (descrição, CEP, data, link)
   - Preferências de notificação (e-mail)
3. Salvar e recarregar, validando persistência.

Critérios:
- Usuário só consegue editar o próprio perfil (exceto admin).

## 4) Planos e pagamentos (Básico, Pro, Elite)

### 4.1 Listagem de planos e “meu plano”

1. Logar como `investidor@tcc.local` e abrir página Premium.
2. Validar “Meu plano” e regalias retornadas pela API.

### 4.2 Assinatura de plano (troca de token)

1. Logar como `investidor@tcc.local` → assinar **Elite**.
2. Validar que a resposta retorna **novo token** e que o front salva/usa este token.
3. Deslogar/logar novamente e validar que o plano continua Elite.

Repetir com `empreendedor@tcc.local` → assinar **Pro**.

### 4.3 Pagamentos (simulação)

1. Com usuário autenticado, simular pagamento.
2. Validar listagem “meus pagamentos”.

## 5) Ideias (empreendedor)

### 5.1 Criar ideia (Básico: limite; Pro: ilimitado)

1. Logar como **Empreendedor Básico** (`empreendedor@tcc.local`).
2. Criar 2 ideias ativas → deve permitir.
3. Criar a 3ª ideia ativa → deve bloquear (limite do básico).

4. Logar como **Empreendedor Pro** (`pro@tcc.local`).
5. Criar 3+ ideias ativas → deve permitir.

### 5.2 Editar ideia

1. Editar nome, estágio, categoria, região, descrição, captação.
2. Validar que o dono consegue editar e outro empreendedor não.

### 5.3 Upload de documentos

1. Fazer upload de documento PDF (ex: pitch deck, contrato social, demonstrativos, etc.) em uma ideia.
2. Validar que o documento aparece no detalhe da ideia para o dono e para planos pagos.

## 6) Explorar ideias (visitante e investidor)

### 6.1 Listagem + filtros públicos

1. Acessar Explorar Ideias deslogado → deve redirecionar/bloquear e exigir login.
2. Testar filtros (logado): termo, categoria, estágio, região, valor min/max.
3. Abrir detalhe de uma ideia.

### 6.2 Comentários

1. Logar como `investidor@tcc.local`.
2. Comentar em uma ideia (comentário raiz).
3. Responder comentário (reply).
4. Validar que comentários aparecem e persistem.

### 6.3 Filtro exclusivo Elite: “Somente com documentos”

1. Logar como `investidor@tcc.local` (basico) e abrir filtros avançados:
   - o checkbox não deve aparecer; se forçar a query, a API deve retornar 403.
2. Logar como `elite@tcc.local` e ativar o checkbox:
   - deve listar apenas ideias com documentos (usar a ideia demo como referência).

## 7) Propostas (fluxo principal de negócio)

### 7.1 Enviar proposta (investidor)

1. Logar como `investidor@tcc.local`.
2. Abrir uma ideia e enviar proposta (valor, fatia, mensagem).
3. Validar em “Minhas Propostas” que a proposta aparece com status pendente.

### 7.2 Receber e responder (empreendedor)

1. Logar como `empreendedor@tcc.local`.
2. Abrir “Propostas recebidas” e/ou “Propostas da ideia”.
3. Aceitar ou recusar.
4. Validar que o status atualiza para ambos os lados.

### 7.3 Contraproposta (empreendedor → investidor)

1. O empreendedor envia uma contraproposta.
2. O investidor responde à contraproposta.
3. Validar histórico de infos da proposta (linha do tempo).

### 7.4 Prioridade Elite nas propostas (visual/ordenação)

1. Logar como `pro@tcc.local` e abrir propostas da ideia demo.
2. Validar que proposta do `elite@tcc.local` aparece primeiro e com indicador “Elite”.

### 7.5 Encerrar proposta (investidor)

1. Com proposta ativa, encerrar.
2. Validar que não aparece mais como ativa/que o status foi atualizado.

## 8) Chat (após aceite)

1. Garantir que existe uma proposta aceita (a ideia demo já tem).
2. Logar como investidor e entrar em Chat:
   - listar conversas
   - listar mensagens
   - enviar mensagem
3. Logar como empreendedor e validar recebimento e resposta.

Critério:
- Chat só deve funcionar entre usuários que têm proposta aceita (controle do serviço).

## 9) Relatórios e documentos jurídicos (pagos)

### 9.1 Contrato (Pro e Elite)

1. Logar como `elite@tcc.local`.
2. Ir em “Minhas Propostas” na proposta aceita e baixar contrato.
3. Logar como `pro@tcc.local` (empreendedor da ideia) e baixar contrato da proposta aceita.

Negativo:
- Investidor básico não deve conseguir baixar contrato (403).
- Empreendedor básico não deve conseguir baixar contrato (403).

### 9.2 Relatório Elite (download)

1. Logar como `elite@tcc.local`.
2. Abrir a ideia demo e baixar “Relatório (Elite)”.

Negativo:
- Investidor básico deve receber bloqueio (403).

## 10) Notificações

1. Logar como investidor e listar “minhas notificações”.
2. Disparar notificação (de um usuário para outro) e validar o destinatário recebendo.
3. Marcar como lida e validar atualização.

## 11) Governança (denúncia)

1. Logar como investidor ou empreendedor.
2. Denunciar uma ideia informando o motivo.
3. Logar como admin e listar denúncias.
4. Analisar denúncia (aprovar/recusar) e validar persistência de status/observação.

## 12) Administração (adm)

### 12.1 Dashboard admin

1. Logar como `admin@tcc.local`.
2. Abrir dashboard e validar:
   - volume mensal
   - total de startups
   - taxa de conversão
   - total investido (propostas aceitas)

### 12.2 Gestão de usuários

1. Listar usuários.
2. Inativar/ativar usuário.
3. Validar que usuário inativo não consegue logar.

### 12.3 Moderação de ideias

1. Alterar status de uma ideia (aprovar/reprovar/pendente conforme lookup).
2. Validar que listagens refletem o novo status.

### 12.4 Logs (adm)

1. Registrar um log manualmente via endpoint (ou validar logs gerados por ações).

## 13) Checklist final (aceite)

- Planos: Básico, Pro e Elite bloqueiam/liberam corretamente.
- Segurança: endpoints com role/plan retornam 401/403 quando indevido.
- Fluxo de negócio: ideia → proposta → aceite/contraproposta → chat → contrato/relatório.
- Admin: dashboard, denúncias, moderação e usuários funcionam.
