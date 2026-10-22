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

// Troca de tamanho: pede ao servidor o bloco de preço daquela variação.
document.querySelectorAll('[data-url-preco]').forEach(form => {
    const area = document.querySelector('[data-area-preco]');

    form.querySelectorAll('input[name="variacaoId"]').forEach(opcao => {
        opcao.addEventListener('change', async () => {
            const resposta = await fetch(`${form.dataset.urlPreco}?variacaoId=${opcao.value}`);
            if (resposta.ok) area.innerHTML = await resposta.text();
        });
    });
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

    campoCep.addEventListener('change', async () => {
        status.textContent = 'Buscando endereço...';

        const resposta = await fetch(`${campoCep.dataset.cepUrl}?cep=${encodeURIComponent(campoCep.value)}`);
        if (!resposta.ok) {
            status.textContent = 'CEP não encontrado. Preencha o endereço abaixo.';
            return;
        }

        const endereco = await resposta.json();
        campo('Logradouro').value = endereco.logradouro;
        campo('Bairro').value = endereco.bairro;
        campo('Cidade').value = endereco.cidade;
        campo('Uf').value = endereco.uf;
        status.textContent = '';
        campo('Numero').focus();
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

// "Adicionar ao carrinho" sem recarregar a página: o cliente continua comprando.
document.querySelectorAll('[data-form-carrinho]').forEach(form => {
    form.addEventListener('submit', async evento => {
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

            window.miauAtualizarCarrinho(dados.quantidade);
            window.miauAviso('Adicionado ao carrinho!', true);
        } catch (erro) {
            window.miauAviso(erro.message || 'Não foi possível adicionar. Tente de novo.', false);
        } finally {
            botao.disabled = false;
        }
    });
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
