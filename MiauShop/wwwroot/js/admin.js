// Painel MiauShop — só comportamento visual. Nenhuma regra de negócio aqui: quem valida é o servidor.

// Pré-visualização da imagem do produto enquanto o administrador digita a URL.
document.querySelectorAll('[data-preview-origem]').forEach(campo => {
    const imagem = document.querySelector('[data-preview-imagem]');
    campo.addEventListener('change', () => { imagem.src = campo.value.trim(); });
});
