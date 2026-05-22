const API_BASE = process.env.API_BASE ?? 'http://127.0.0.1:5253';

function fail(message) {
  const error = new Error(message);
  error.isTestFailure = true;
  throw error;
}

function assert(condition, message) {
  if (!condition) fail(message);
}

async function readJsonSafe(response) {
  const text = await response.text();
  try {
    return { ok: true, json: JSON.parse(text) };
  } catch {
    return { ok: false, text };
  }
}

async function http(path, { method = 'GET', token, json } = {}) {
  const headers = {};
  if (token) headers.Authorization = `Bearer ${token}`;
  if (json !== undefined) headers['Content-Type'] = 'application/json';

  const res = await fetch(`${API_BASE}${path}`, {
    method,
    headers,
    body: json !== undefined ? JSON.stringify(json) : undefined,
  });

  return res;
}

async function login(email, senha = '123456') {
  const res = await http('/api/auth/login', { method: 'POST', json: { email, senha } });
  const body = await readJsonSafe(res);
  assert(res.ok, `Login falhou para ${email}: HTTP ${res.status} ${body.ok ? JSON.stringify(body.json) : body.text}`);
  assert(body.ok, `Login retornou não-JSON para ${email}`);
  const token = body.json.token ?? body.json.Token;
  const usuarioId = body.json.usuarioId ?? body.json.UsuarioId;
  assert(typeof token === 'string' && token.length > 10, `Login não retornou token para ${email}`);
  assert(Number.isFinite(Number(usuarioId)), `Login não retornou usuarioId para ${email}`);
  return { token, usuarioId: Number(usuarioId) };
}

async function getIdeiaDemoId() {
  const res = await http('/api/ideias?termo=Ideia%20Demo%20(Docs%20%2B%20Proposta%20Aceita)');
  const body = await readJsonSafe(res);
  assert(res.ok, `Listar ideias falhou: HTTP ${res.status}`);
  assert(Array.isArray(body.json), 'Listar ideias não retornou lista');
  const ideia = body.json[0];
  assert(ideia && (ideia.idaId ?? ideia.IdaId), 'Ideia demo não encontrada');
  return Number(ideia.idaId ?? ideia.IdaId);
}

async function createIdeia(tokenEmpreendedor, { nome, cnpjBase }) {
  const payload = {
    categoriaId: 1,
    estagioId: 2,
    nome,
    regiao: 'SP',
    cnpj: `${cnpjBase}-${Date.now()}`,
    descricao: 'Ideia criada pelo smoke test',
    linkVideo: null,
    imagem: null,
    fatia: 10,
    valorCaptacao: 10000,
  };

  const res = await http('/api/ideias', { method: 'POST', token: tokenEmpreendedor, json: payload });
  const body = await readJsonSafe(res);
  assert(res.ok, `Criar ideia falhou: HTTP ${res.status} ${body.ok ? JSON.stringify(body.json) : body.text}`);
  const ideiaId = body.json.idaId ?? body.json.IdaId;
  assert(Number.isFinite(Number(ideiaId)), 'Criar ideia não retornou idaId');
  return Number(ideiaId);
}

async function enviarProposta(tokenInvestidor, ideiaId, { mensagem, valor, fatiaPret }) {
  const res = await http(`/api/ideias/${ideiaId}/propostas`, {
    method: 'POST',
    token: tokenInvestidor,
    json: { mensagem, valor, fatiaPret },
  });
  const body = await readJsonSafe(res);
  assert(res.ok, `Enviar proposta falhou: HTTP ${res.status} ${body.ok ? JSON.stringify(body.json) : body.text}`);
  const propostaId = body.json.prpId ?? body.json.PrpId;
  assert(Number.isFinite(Number(propostaId)), 'Enviar proposta não retornou prpId');
  return Number(propostaId);
}

async function responderEmpreendedor(tokenEmpreendedor, propostaId, { aceiteId, retorno }) {
  const res = await http(`/api/propostas/${propostaId}/responder`, {
    method: 'POST',
    token: tokenEmpreendedor,
    json: { aceiteId, retorno },
  });
  const body = await readJsonSafe(res);
  assert(res.ok, `Responder (empreendedor) falhou: HTTP ${res.status} ${body.ok ? JSON.stringify(body.json) : body.text}`);
  return body.json;
}

async function responderInvestidor(tokenInvestidor, propostaId, { aceiteId, retorno }) {
  const res = await http(`/api/propostas/${propostaId}/responder-investidor`, {
    method: 'POST',
    token: tokenInvestidor,
    json: { aceiteId, retorno },
  });
  const body = await readJsonSafe(res);
  assert(res.ok, `Responder (investidor) falhou: HTTP ${res.status} ${body.ok ? JSON.stringify(body.json) : body.text}`);
  return body.json;
}

async function ensureChatWorks(tokenA, tokenB, { userAId, userBId, ideiaId }) {
  const textoA = `Msg A ${Date.now()}`;
  const textoB = `Msg B ${Date.now()}`;

  const resA = await http('/api/chat/mensagens', {
    method: 'POST',
    token: tokenA,
    json: { paraUsuarioId: userBId, ideiaId, texto: textoA },
  });
  assert(resA.ok, `Chat: envio inicial falhou: HTTP ${resA.status}`);

  const conversasA = await http('/api/chat/conversas', { token: tokenA });
  const conversasABody = await readJsonSafe(conversasA);
  assert(conversasA.ok, `Chat: listar conversas falhou: HTTP ${conversasA.status}`);
  const listaA = conversasABody.json;
  assert(Array.isArray(listaA), 'Chat: conversas não retornou lista');
  const convA = listaA.find(c =>
    Number(c.id ?? c.Id) > 0 &&
    Number(c.outroUsuarioId ?? c.OutroUsuarioId) === Number(userBId) &&
    Number(c.ideiaId ?? c.IdeiaId) === Number(ideiaId)
  );
  assert(convA, 'Chat: conversa não foi criada/encontrada para a ideia');
  const conversaId = Number(convA.id ?? convA.Id);

  const resB = await http(`/api/chat/conversas/${conversaId}/mensagens`, {
    method: 'POST',
    token: tokenB,
    json: { paraUsuarioId: null, ideiaId, texto: textoB },
  });
  assert(resB.ok, `Chat: resposta na conversa falhou: HTTP ${resB.status}`);

  const msgs = await http(`/api/chat/conversas/${conversaId}/mensagens`, { token: tokenA });
  const msgsBody = await readJsonSafe(msgs);
  assert(msgs.ok, `Chat: listar mensagens falhou: HTTP ${msgs.status}`);
  assert(Array.isArray(msgsBody.json), 'Chat: mensagens não retornou lista');
  const textos = msgsBody.json.map(m => m.texto ?? m.Texto);
  assert(textos.includes(textoA), 'Chat: mensagem A não encontrada');
  assert(textos.includes(textoB), 'Chat: mensagem B não encontrada');

  return conversaId;
}

async function ensureBasicEntrepreneurLimit(tokenEmpBasico) {
  const base = `CNPJ-TESTE-${Date.now()}`;
  const mk = (n) => ({
    categoriaId: 1,
    estagioId: 2,
    nome: `Ideia Basico Limite ${Date.now()}-${n}`,
    regiao: 'SP',
    cnpj: `${base}-${n}`,
    descricao: 'Teste limite plano básico',
    linkVideo: null,
    imagem: null,
    fatia: 10,
    valorCaptacao: 10000,
  });

  let successes = 0;
  for (let i = 1; i <= 4; i += 1) {
    const r = await http('/api/ideias', { method: 'POST', token: tokenEmpBasico, json: mk(i) });
    if (r.ok) {
      successes += 1;
      continue;
    }
    if (r.status === 403) {
      assert(successes <= 2, `Plano básico deveria bloquear a partir de 3 ideias ativas (sucessos=${successes}, tentativa=${i})`);
      const r2 = await http('/api/ideias', { method: 'POST', token: tokenEmpBasico, json: mk(i + 100) });
      assert(r2.status === 403, `Plano básico deveria continuar bloqueando após limite (esperado 403), veio ${r2.status}`);
      return;
    }
    fail(`Plano básico retornou status inesperado ao criar ideia: ${r.status}`);
  }

  fail('Plano básico não bloqueou criação após múltiplas tentativas (esperado 403 em algum momento).');
}

async function main() {
  const step = async (name, fn) => {
    process.stdout.write(`- ${name}... `);
    await fn();
    process.stdout.write('OK\n');
  };

  let admin;
  let investidorBasico;
  let investidorElite;
  let empreendedorBasico;
  let empreendedorPro;

  await step('Login Admin', async () => { admin = await login('admin@tcc.local'); });
  await step('Login Investidor Básico', async () => { investidorBasico = await login('investidor@tcc.local'); });
  await step('Login Investidor Elite', async () => { investidorElite = await login('elite@tcc.local'); });
  await step('Login Empreendedor Básico', async () => { empreendedorBasico = await login('empreendedor@tcc.local'); });
  await step('Login Empreendedor Pro', async () => { empreendedorPro = await login('pro@tcc.local'); });

  let ideiaDemoId;
  await step('Encontrar ideia demo', async () => { ideiaDemoId = await getIdeiaDemoId(); });

  await step('Bloqueio filtro Elite para investidor básico', async () => {
    const res = await http('/api/ideias?apenasComDocumentos=true', { token: investidorBasico.token });
    assert(res.status === 403, `Esperado 403, veio ${res.status}`);
  });

  await step('Filtro Elite liberado para investidor Elite', async () => {
    const res = await http('/api/ideias?apenasComDocumentos=true', { token: investidorElite.token });
    const body = await readJsonSafe(res);
    assert(res.ok, `Esperado 200, veio ${res.status}`);
    assert(Array.isArray(body.json), 'Resposta não é lista');
    assert(body.json.some(i => Number(i.idaId ?? i.IdaId) === ideiaDemoId), 'Ideia demo não apareceu no filtro de documentos');
  });

  await step('Relatório Elite bloqueado para investidor básico', async () => {
    const res = await http(`/api/ideias/${ideiaDemoId}/relatorio`, { token: investidorBasico.token });
    assert(res.status === 403, `Esperado 403, veio ${res.status}`);
  });

  await step('Relatório Elite liberado para investidor Elite', async () => {
    const res = await http(`/api/ideias/${ideiaDemoId}/relatorio`, { token: investidorElite.token });
    const text = await res.text();
    assert(res.ok, `Esperado 200, veio ${res.status}`);
    assert(text.includes('RELATÓRIO') || text.includes('Relatório') || text.includes('#'), 'Conteúdo do relatório parece inválido');
  });

  let propostaEliteId;
  await step('Obter proposta do investidor Elite', async () => {
    const res = await http('/api/propostas/minhas', { token: investidorElite.token });
    const body = await readJsonSafe(res);
    assert(res.ok, `Esperado 200, veio ${res.status}`);
    assert(Array.isArray(body.json), 'Resposta não é lista');
    const proposta = body.json.find(p => Number(p.prpIdeiaId ?? p.PrpIdeiaId) === ideiaDemoId);
    assert(proposta, 'Proposta do Elite para ideia demo não encontrada');
    propostaEliteId = Number(proposta.prpId ?? proposta.PrpId);
  });

  await step('Contrato bloqueado para investidor básico', async () => {
    const res = await http(`/api/propostas/${propostaEliteId}/contrato`, { token: investidorBasico.token });
    assert(res.status === 403, `Esperado 403, veio ${res.status}`);
  });

  await step('Contrato liberado para investidor Elite', async () => {
    const res = await http(`/api/propostas/${propostaEliteId}/contrato`, { token: investidorElite.token });
    const text = await res.text();
    assert(res.ok, `Esperado 200, veio ${res.status}`);
    assert(text.length > 50, 'Contrato retornou conteúdo muito pequeno');
  });

  await step('Contrato liberado para empreendedor Pro (dono da ideia)', async () => {
    const res = await http(`/api/propostas/${propostaEliteId}/contrato`, { token: empreendedorPro.token });
    const text = await res.text();
    assert(res.ok, `Esperado 200, veio ${res.status}`);
    assert(text.length > 50, 'Contrato retornou conteúdo muito pequeno');
  });

  await step('Admin dashboard acessível para admin', async () => {
    const res = await http('/api/admin/dashboard', { token: admin.token });
    assert(res.ok, `Esperado 200, veio ${res.status}`);
  });

  await step('Admin dashboard bloqueado para não-admin', async () => {
    const res = await http('/api/admin/dashboard', { token: investidorElite.token });
    assert(res.status === 403, `Esperado 403, veio ${res.status}`);
  });

  await step('Fluxo: empreendedor cria ideia → investidor propõe → empreendedor aceita', async () => {
    const ideiaId = await createIdeia(empreendedorPro.token, {
      nome: `Ideia Smoke Aceite Direto ${Date.now()}`,
      cnpjBase: 'CNPJ-SMOKE-ACEITE-DIRETO',
    });

    const propostaId = await enviarProposta(investidorBasico.token, ideiaId, {
      mensagem: 'Proposta inicial (smoke test)',
      valor: 50000,
      fatiaPret: 12,
    });

    const updated = await responderEmpreendedor(empreendedorPro.token, propostaId, { aceiteId: 1, retorno: null });
    const infos = updated.infos ?? updated.Infos ?? [];
    const ultimo = infos[infos.length - 1] ?? {};
    const aceiteNome = (ultimo.aceiteNome ?? ultimo.AceiteNome ?? '').toLowerCase();
    assert(aceiteNome.includes('aceit'), `Status esperado "aceita", veio "${aceiteNome}"`);
  });

  await step('Fluxo: contato (chat) entre investidor e empreendedor após aceite', async () => {
    const ideiaId = await createIdeia(empreendedorPro.token, {
      nome: `Ideia Smoke Chat ${Date.now()}`,
      cnpjBase: 'CNPJ-SMOKE-CHAT',
    });

    const propostaId = await enviarProposta(investidorBasico.token, ideiaId, {
      mensagem: 'Proposta para habilitar chat (smoke test)',
      valor: 25000,
      fatiaPret: 8,
    });

    await responderEmpreendedor(empreendedorPro.token, propostaId, { aceiteId: 1, retorno: null });

    await ensureChatWorks(investidorBasico.token, empreendedorPro.token, {
      userAId: investidorBasico.usuarioId,
      userBId: empreendedorPro.usuarioId,
      ideiaId,
    });
  });

  await step('Fluxo: contraproposta → investidor aceita', async () => {
    const ideiaId = await createIdeia(empreendedorPro.token, {
      nome: `Ideia Smoke Contra Aceitar ${Date.now()}`,
      cnpjBase: 'CNPJ-SMOKE-CONTRA-ACEITA',
    });

    const propostaId = await enviarProposta(investidorBasico.token, ideiaId, {
      mensagem: 'Proposta inicial para contraproposta (smoke test)',
      valor: 80000,
      fatiaPret: 15,
    });

    await responderEmpreendedor(empreendedorPro.token, propostaId, { aceiteId: 4, retorno: 'Topo, mas quero 12% em vez de 15%.' });
    const updated = await responderInvestidor(investidorBasico.token, propostaId, { aceiteId: 1, retorno: null });
    const infos = updated.infos ?? updated.Infos ?? [];
    const ultimo = infos[infos.length - 1] ?? {};
    const aceiteNome = (ultimo.aceiteNome ?? ultimo.AceiteNome ?? '').toLowerCase();
    assert(aceiteNome.includes('aceit'), `Status esperado "aceita", veio "${aceiteNome}"`);
  });

  await step('Fluxo: contraproposta → investidor recusa', async () => {
    const ideiaId = await createIdeia(empreendedorPro.token, {
      nome: `Ideia Smoke Contra Recusar ${Date.now()}`,
      cnpjBase: 'CNPJ-SMOKE-CONTRA-RECUSA',
    });

    const propostaId = await enviarProposta(investidorBasico.token, ideiaId, {
      mensagem: 'Proposta inicial para recusar contraproposta (smoke test)',
      valor: 30000,
      fatiaPret: 5,
    });

    await responderEmpreendedor(empreendedorPro.token, propostaId, { aceiteId: 4, retorno: 'Só fecho se subir para 10%.' });
    const updated = await responderInvestidor(investidorBasico.token, propostaId, { aceiteId: 2, retorno: null });
    const infos = updated.infos ?? updated.Infos ?? [];
    const ultimo = infos[infos.length - 1] ?? {};
    const aceiteNome = (ultimo.aceiteNome ?? ultimo.AceiteNome ?? '').toLowerCase();
    assert(aceiteNome.includes('recus'), `Status esperado "recusada", veio "${aceiteNome}"`);
  });

  await step('Limite do plano básico (empreendedor)', async () => {
    await ensureBasicEntrepreneurLimit(empreendedorBasico.token);
  });

  process.stdout.write('\nSmoke tests OK\n');
}

main().catch((e) => {
  process.stderr.write(`\nTeste falhou: ${e?.message ?? e}\n`);
  process.exit(1);
});
