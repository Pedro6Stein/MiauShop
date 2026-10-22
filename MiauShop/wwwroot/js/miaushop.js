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
