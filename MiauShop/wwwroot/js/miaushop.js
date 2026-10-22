// MiauShop — animações (GSAP). Tudo é desligado se o sistema pedir "reduzir movimento".

const reduzirMovimento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

if (window.gsap && !reduzirMovimento) {
    gsap.registerPlugin(ScrollTrigger);

    // Entrada do banner principal: os textos sobem um depois do outro.
    gsap.from('[data-anima="hero"] > *', {
        y: 30,
        opacity: 0,
        duration: .8,
        stagger: .12,
        ease: 'power3.out'
    });

    // Elementos que aparecem ao rolar a página (cards, seções).
    ScrollTrigger.batch('[data-anima="surgir"]', {
        start: 'top 90%',
        once: true,
        onEnter: elementos => gsap.from(elementos, {
            y: 40,
            opacity: 0,
            duration: .7,
            stagger: .08,
            ease: 'power3.out'
        })
    });
}

// Regra deste arquivo: o JavaScript NÃO calcula preço nem guarda regra de negócio.
// Ele só pede ao servidor um pedaço de HTML já pronto e coloca na tela.

// ---------- Página do produto ----------

// Botões − e +: usam o min/max que o servidor escreveu no campo (stepDown/stepUp respeitam esses limites).
document.querySelectorAll('[data-quantidade]').forEach(caixa => {
    const campo = caixa.querySelector('input');
    caixa.querySelector('[data-menos]').addEventListener('click', () => campo.stepDown());
    caixa.querySelector('[data-mais]').addEventListener('click', () => campo.stepUp());
});

// Troca de tamanho (página do produto ou janelinha): pede ao servidor o bloco de preço daquela variação.
// Delegado no document porque a janelinha é criada depois que a página carrega.
document.addEventListener('change', async evento => {
    const opcao = evento.target;
    const form = opcao.closest?.('[data-url-preco]');
    if (!form || opcao.name !== 'variacaoId') return;

    const area = form.closest('[data-escopo-produto]').querySelector('[data-area-preco]');
    const resposta = await fetch(`${form.dataset.urlPreco}?variacaoId=${opcao.value}`);
    if (resposta.ok) area.innerHTML = await resposta.text();
});

// "+" de card com tamanho: busca no servidor o conteúdo da janelinha e abre.
document.addEventListener('click', async evento => {
    const botao = evento.target.closest('[data-escolher-url]');
    if (!botao) return;

    evento.preventDefault();
    const resposta = await fetch(botao.dataset.escolherUrl);
    if (!resposta.ok) {
        window.location.href = botao.href; // sem a janelinha, vai para a página do produto
        return;
    }

    document.querySelector('[data-modal-conteudo]').innerHTML = await resposta.text();
    bootstrap.Modal.getOrCreateInstance('#ms-modal-escolha').show();
});

// ---------- Página do carrinho ----------

// Formulários do carrinho (−, +, remover): envia ao servidor e troca a área pelo HTML que ele devolve.
// Delegado no document porque o conteúdo é substituído a cada alteração.
document.addEventListener('submit', async evento => {
    const form = evento.target.closest('[data-form-parcial]');
    const area = document.querySelector('[data-area-carrinho]');
    if (!form || !area) return;

    evento.preventDefault();
    area.classList.add('ms-carregando');

    try {
        const resposta = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form, evento.submitter), // inclui o botão clicado (quantidade nova) e o token
            headers: { 'X-Parcial': '1' }
        });

        if (!resposta.ok) throw new Error();

        area.innerHTML = await resposta.text();
        const total = area.querySelector('[data-total-itens]');
        if (total) window.miauAtualizarCarrinho(parseInt(total.dataset.totalItens, 10));
    } catch {
        window.miauAviso('Não foi possível atualizar o carrinho. Tente de novo.', false);
    } finally {
        area.classList.remove('ms-carregando');
    }
});

// ---------- Checkout ----------

// CEP: ao sair do campo, pede o endereço ao servidor do MiauShop (que consulta o ViaCEP)
// e coloca nos campos o que o servidor devolveu.
document.querySelectorAll('[data-cep-url]').forEach(campoCep => {
    const form = campoCep.form;
    const status = form.querySelector('[data-cep-status]');
    const campo = nome => form.querySelector(`[name="Dados.${nome}"]`);

    // Rua e bairro ficam travados quando o CEP já os traz; senão o cliente digita.
    // (Cidade e UF são sempre travadas na própria tela.) Quem decide no final é o servidor.
    const preencher = (nome, valor) => {
        const entrada = campo(nome);
        entrada.value = valor ?? '';
        entrada.readOnly = Boolean(valor);
    };

    campoCep.addEventListener('change', async () => {
        status.textContent = 'Buscando endereço...';

        const resposta = await fetch(`${campoCep.dataset.cepUrl}?cep=${encodeURIComponent(campoCep.value)}`);
        if (!resposta.ok) {
            ['Logradouro', 'Bairro'].forEach(nome => preencher(nome, ''));
            campo('Cidade').value = '';
            campo('Uf').value = '';
            status.textContent = 'CEP não encontrado. Confira os números.';
            return;
        }

        const endereco = await resposta.json();
        preencher('Logradouro', endereco.logradouro);
        preencher('Bairro', endereco.bairro);
        campo('Cidade').value = endereco.cidade;
        campo('Uf').value = endereco.uf;
        status.textContent = '';
        (endereco.logradouro ? campo('Numero') : campo('Logradouro')).focus();
    });
});

// Payment Brick do Mercado Pago (modo teste). Chave, valor e e-mail vêm escritos pelo servidor.
// Ao clicar em "Pagar", NADA é enviado ao Mercado Pago: guardamos só o método escolhido
// e mostramos a janela "pagamento bloqueado".
// Espera o "load" porque o SDK do Mercado Pago é carregado depois deste arquivo.
window.addEventListener('load', () => {
const areaBrick = document.getElementById('mercado-pago-brick');

if (areaBrick && window.MercadoPago) {
    const mp = new MercadoPago(areaBrick.dataset.mpPublicKey, { locale: 'pt-BR' });

    mp.bricks().create('payment', 'mercado-pago-brick', {
        initialization: {
            amount: Number(areaBrick.dataset.mpValor),
            payer: { email: areaBrick.dataset.mpEmail }
        },
        customization: {
            paymentMethods: { bankTransfer: 'all', creditCard: 'all', debitCard: 'all', ticket: 'all' },
            visual: { style: { customVariables: { baseColor: '#C92D63' } } }
        },
        callbacks: {
            onReady: () => { },
            onError: erro => console.error('Mercado Pago:', erro),
            onSubmit: ({ selectedPaymentMethod }) => {
                document.querySelector('[data-metodo-escolhido]').value = selectedPaymentMethod;
                bootstrap.Modal.getOrCreateInstance('#modal-demo').show();
                return Promise.resolve(); // encerra o "carregando" do botão sem processar pagamento
            }
        }
    });
}
});

// "Adicionar ao carrinho" sem recarregar a página (página do produto, "+" do card e janelinha).
// Delegado no document porque a janelinha é criada depois que a página carrega.
document.addEventListener('submit', async evento => {
    const form = evento.target.closest('[data-form-carrinho]');
    if (!form) return;

    evento.preventDefault();
    const botao = form.querySelector('[type="submit"]');
    botao.disabled = true;

    try {
        const resposta = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form), // inclui o token antifalsificação
            headers: { Accept: 'application/json' }
        });
        const dados = await resposta.json();

        if (!resposta.ok) throw new Error(dados.erro);

        bootstrap.Modal.getInstance('#ms-modal-escolha')?.hide();
        window.miauAtualizarCarrinho(dados.quantidade);
        window.miauAviso('Adicionado ao carrinho!', true);
    } catch (erro) {
        window.miauAviso(erro.message || 'Não foi possível adicionar. Tente de novo.', false);
    } finally {
        botao.disabled = false;
    }
});

// Aviso rápido no canto da tela (toast do Bootstrap).
window.miauAviso = (texto, comLinkCarrinho) => {
    const aviso = document.getElementById('ms-aviso');
    if (!aviso) return;

    aviso.querySelector('[data-aviso-texto]').textContent = texto;
    aviso.querySelector('[data-aviso-link]').hidden = !comLinkCarrinho;
    bootstrap.Toast.getOrCreateInstance(aviso, { delay: 4000 }).show();
};

// Faz o ícone do carrinho "pular" e atualiza o número (usado ao adicionar produtos).
window.miauAtualizarCarrinho = quantidade => {
    const contador = document.querySelector('[data-carrinho-contador]');
    const icone = document.querySelector('.ms-carrinho');

    if (contador) {
        contador.textContent = quantidade;
        contador.hidden = quantidade === 0;
    }

    if (icone && !reduzirMovimento) {
        icone.classList.remove('ms-pulando');
        void icone.offsetWidth; // reinicia a animação
        icone.classList.add('ms-pulando');
    }
};
