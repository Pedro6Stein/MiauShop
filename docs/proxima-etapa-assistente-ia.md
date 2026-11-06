# Próxima etapa: atendimento ao cliente com IA

Proposta para a próxima fase do MiauShop (para constar no TAP como etapa futura).

## Objetivo

Atender o cliente em tempo real, a qualquer hora, respondendo dúvidas sobre produtos, frete, prazos e pedidos,
sem aumentar custo fixo e sem abrir mão das regras que já ficam no servidor (preço, frete, status do pedido).

## Como seria

| Fase | O que entrega | Custo |
|---|---|---|
| **1. Assistente de perguntas frequentes** | Janelinha de chat na loja com respostas prontas para as dúvidas mais comuns (frete grátis, prazo de envio, trocas, como acompanhar o pedido) e botão "falar com atendente" (WhatsApp). | Zero: roda no próprio servidor, sem serviço externo. |
| **2. Agente de IA conectado à loja** | Um modelo de linguagem (LLM) conversa com o cliente e usa **ferramentas** do próprio sistema: buscar produtos no catálogo, calcular frete e consultar o status de um pedido (com número + e-mail, mesmo limite de tentativas de hoje). | Plano gratuito de uma API de LLM no início; depois, custo por uso. |
| **3. Melhoria contínua** | Painel com as conversas (sem dados pessoais), perguntas que a IA não soube responder e taxa de resolução, para alimentar novas respostas. | Zero. |

## Arquitetura (fase 2)

```
Chat na loja ─► POST /Assistente/Mensagem (ASP.NET Core) ─► API do LLM
                         │                                      │
                         └──── ferramentas (C#) ◄───────────────┘
                               • BuscarProdutos(termo, animal)   → ICatalogoService
                               • CalcularFrete(subtotal)         → ICalculadoraFrete
                               • ConsultarPedido(numero, email)  → IPedidoService
```

- A chave da API fica só no servidor (variável de ambiente no Render), nunca no navegador.
- **O LLM não inventa preço nem prazo**: esses números sempre vêm das ferramentas, que usam as mesmas regras de hoje.
- As descrições, a ficha técnica e as avaliações dos produtos servem de base de conhecimento (RAG simples).

## Cuidados

| Risco | Como tratar |
|---|---|
| IA "inventar" informação | Respostas de preço, frete e pedido só pelas ferramentas; instrução para dizer "não sei" e oferecer atendente. |
| Abuso / custo | Limite de mensagens por IP (o mesmo `RateLimiter` já usado no login) e tamanho máximo da mensagem. |
| Dados pessoais (LGPD) | Não guardar e-mail nem número do pedido no histórico; conversas anonimizadas. |
| Limite do plano gratuito | Cair para a fase 1 (respostas prontas) quando a cota acabar. |

## Indicadores de sucesso

- % de conversas resolvidas sem atendente humano
- Tempo médio de resposta
- Nota de satisfação ao fim da conversa (1 a 5 estrelas, como nas avaliações de produto)

## Estimativa

Cerca de 3 sprints de 1 semana: fase 1 (1 sprint), fase 2 (2 sprints). A fase 3 acompanha o uso real.
