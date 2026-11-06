# Atendimento ao cliente: assistente da loja e próxima etapa com IA

## O que já está pronto (gratuito, sem IA)

O chat **"Ajuda"** da loja atende em tempo real com um roteiro programado no servidor:

| Opção do menu | O que faz | De onde vem a resposta |
|---|---|---|
| **1. Dúvidas frequentes** | Lista as perguntas cadastradas; o número escolhido mostra a resposta | Painel (Admin > Assistente) |
| **2. Procurar um produto** | Busca no catálogo e mostra até 3 produtos com foto, preço e link | Mesmo serviço do catálogo da loja |
| **3. Calcular frete e prazo** | Pede o CEP e mostra valor, distância e prazo de entrega | Mesmo cálculo do carrinho e do checkout |
| **4. Acompanhar meu pedido** | Pede número + e-mail e mostra o status atual | Mesma regra da tela "Acompanhar pedido" |
| **5. Falar com uma pessoa** | Explica o canal humano (numa loja real, WhatsApp/e-mail) | Texto fixo |

Também entende texto livre, sem IA: compara as palavras da mensagem com as **palavras-chave** de cada resposta
cadastrada (sem acentos, sem maiúsculas, "troca" reconhece "trocar"), reconhece um CEP escrito na frase e
pedidos como "rastrear" ou "falar com atendente". O que ele não souber responder fica anotado no painel, e a loja
cria a resposta com um clique ("Criar resposta").

**Por que não inventa preço nem prazo:** valores de produto, frete e status de pedido vêm sempre dos serviços
do servidor (`ICatalogoService`, `IFreteService`, `IPedidoService`), os mesmos usados pelo resto da loja.

### Frete por distância

O frete é calculado pela distância entre o **CEP de origem** (padrão: Bragança Paulista, 12900-002; o
administrador muda em Admin > Frete) e o CEP do cliente:

- coordenadas do CEP pela AwesomeAPI (gratuita, sem chave); se ela falhar, usa a capital do estado (via ViaCEP);
- distância em linha reta (fórmula de Haversine) × 1,3 para aproximar o caminho por estrada;
- frete = valor base + km × valor por km, até um valor máximo (os três editáveis no painel);
- prazo = envio do fornecedor + 1 dia útil a cada 500 km; frete grátis acima de R$ 150 continua valendo.

## Próxima etapa (opcional): agente de IA

O assistente atual foi feito **sem IA de propósito, para o projeto continuar gratuito**. Mas a estrutura já
está pronta para um agente de IA, o que mostra ao cliente o potencial da solução:

```
Chat na loja ─► POST /Assistente/Mensagem (ASP.NET Core) ─► API de um modelo de linguagem (LLM)
                         │                                         │
                         └──── ferramentas (C#) ◄──────────────────┘
                               • BuscarProdutos  → ICatalogoService
                               • CalcularFrete   → IFreteService
                               • ConsultarPedido → IPedidoService
```

- O LLM só conversa; números continuam vindo das ferramentas (as mesmas regras de hoje).
- A chave da API fica no servidor (variável de ambiente no Render).
- As respostas cadastradas no painel viram a base de conhecimento do agente.
- Se a cota gratuita da API acabar, o chat volta sozinho para o roteiro atual.

| Risco | Como tratar |
|---|---|
| IA "inventar" informação | Preço, frete e pedido só pelas ferramentas; instrução para dizer "não sei" e oferecer atendente |
| Abuso / custo | Limite de mensagens por IP (já existe: 20 por minuto) e tamanho máximo da mensagem (200 caracteres) |
| Dados pessoais (LGPD) | O assistente já não guarda e-mail, telefone nem números longos nas dúvidas anotadas |
