# 0003 — Escopo da v1: experiência sentada, controles do médico, clima e voo em circuito

- **Data:** 2026-09-23
- **Autor:** Daniel Ramalho (com Claude Code)
- **Status:** Aceita

## Contexto

O projeto foi recriado para o Quest ([ADR 0001](0001-projeto-novo-em-vez-de-migrar.md)) e as cenas ainda
são placeholders. Antes de implementar, foi preciso fechar como o paciente e o médico interagem,
como o voo funciona e como o médico controla a dificuldade. A especificação completa está em
[ESPECIFICACAO.md](../ESPECIFICACAO.md).

Restrições: aplicação terapêutica (conforto e ausência de sustos vêm primeiro), Quest 2 como
hardware mais fraco (72 FPS) e uma equipe de alunos (a solução tem que ser simples de manter).

## Decisão

1. **Experiência 100% sentada, sem locomoção.** Nenhum movimento contínuo, teleporte, pulo ou giro
   pelo analógico. O usuário só olha em volta com a cabeça. Os providers de locomoção e giro do
   `XR_Player` ficam desligados.
2. **Divisão dos controles.** O paciente usa o controle direito (raio + gatilho) no menu e na compra.
   Na sala de embarque e no voo, o **médico** usa o controle esquerdo:
   gatilho segurado por 1 s = decolar/pousar (ou chamar o embarque, na cena 2);
   X = chuva; Y = noite; grip = turbulência leve; clique do analógico = voltar ao normal; Menu (≡) = pausa.
   O mapeamento fica num Input Action Asset.
3. **Clima combinável e gradual.** Chuva, noite e turbulência são liga/desliga **independentes**
   (podem ser combinados) e mudam em 5–10 s. Nuvens estão sempre presentes. Sem trovão, relâmpago
   ou turbulência forte na v1.
4. **Voo em circuito fechado, sem teleporte.** O avião decola, sobe acima das nuvens e faz um oval
   amplo (inclinação ≤ 10–15°). Quando o médico manda pousar, o avião termina a volta até alinhar
   com a pista (espera de até ~60 s) e pousa **no mesmo aeroporto**. Quem se move é o avião, e o
   `XR_Player` é filho dele.
5. **Destino escolhido passa entre as cenas** por uma classe estática simples (`SessionData`), sem
   `DontDestroyOnLoad` nem singleton MonoBehaviour. Quem pula a compra recebe o destino padrão (Paris).
6. **Menu `00_Menu` permanece** como ponto de entrada, permitindo começar do início ou ir direto a uma cena.
7. **Exceção à regra de distância da UI (1,5–3 m) na cena 1:** a tela do computador fica num monitor
   grande a 0,9–1,2 m, com texto maior que o padrão, para que a cena pareça real (mesa com computador).
   As demais UIs continuam seguindo a regra.

## Alternativas consideradas

- **Níveis fixos de dificuldade (0 a 4) em vez de condições combináveis:** mais simples de explicar,
  mas tira do médico o controle fino de cada estímulo.
- **Pouso imediato (o avião vira na hora para a pista):** mais responsivo, porém exige calcular
  uma rota dinâmica e aumenta o risco de curvas bruscas (enjoo). A espera de até 60 s é aceitável.
- **Teleportar o aeroporto para a frente do avião no pouso (como no projeto antigo):** evita o
  circuito, mas pode aparecer para o usuário e causa desorientação.
- **Mundo se movendo em volta de um avião parado:** barato, mas complica física, sons 3D e a volta à mesma pista.
- **Médico controlando por um app/tablet separado:** mais confortável para o médico, mas exige rede
  e outro aplicativo. Fica para uma versão futura.
- **Paciente com os controles também no voo:** risco de ativar comandos sem querer.

## Consequências

- O `XR_Player` precisa de ajustes (desligar locomoção, giro e, nas cenas 2 e 3, o raio).
- Precisamos de um Input Action Asset próprio para o médico e de um painel de pausa comum a todas as cenas.
- O controlador de voo é uma máquina de estados (Pronto → Decolagem → Subida → Cruzeiro →
  Aproximação → Pouso → Concluído) movendo o avião por waypoints/curvas, com aceleração suave.
  Se for usado o pacote Splines para a rota, isso exige uma ADR própria (pacote novo).
- O médico acompanha pelo espelhamento do headset (casting); não há HUD para ele na cena.
- Qualquer evento súbito novo (trovão, alarme etc.) precisa de nova ADR e aprovação do professor/terapeuta.
