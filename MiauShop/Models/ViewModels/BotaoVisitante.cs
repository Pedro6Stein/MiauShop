namespace MiauShop.Models.ViewModels;

// Botão "ver o painel como visitante" usado em vários lugares da loja (faixa, cabeçalho, início, rodapé).
// Com o acesso de demonstração ligado, um clique já entra no painel (somente leitura);
// desligado, vira um link comum para a tela de login.
// Complemento: texto extra só para leitores de tela (ex.: "como visitante, sem senha").
public record BotaoVisitante(string Texto, string Classe, string Icone = "bi-speedometer2", string? Complemento = null);
