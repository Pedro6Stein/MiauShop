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

// ---------- Página do produto ----------

// Seletor de quantidade (− e +), sempre entre 1 e 10.
document.querySelectorAll('[data-quantidade]').forEach(caixa => {
    const campo = caixa.querySelector('input');
    const limitar = valor => Math.min(Math.max(parseInt(valor, 10) || 1, 1), parseInt(campo.max, 10));

    caixa.querySelector('[data-menos]').addEventListener('click', () => campo.value = limitar(+campo.value - 1));
    caixa.querySelector('[data-mais]').addEventListener('click', () => campo.value = limitar(+campo.value + 1));
    campo.addEventListener('change', () => campo.value = limitar(campo.value));
});

// Troca de tamanho: atualiza o preço mostrado (o valor que vale é sempre o do servidor).
document.querySelectorAll('input[name="variacaoId"][data-preco]').forEach(opcao => {
    opcao.addEventListener('change', () => {
        document.querySelector('[data-preco]:not(input)').textContent = opcao.dataset.preco;
        const antigo = document.querySelector('[data-preco-antigo]:not(input)');
        if (antigo) antigo.textContent = opcao.dataset.precoAntigo;
    });
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
