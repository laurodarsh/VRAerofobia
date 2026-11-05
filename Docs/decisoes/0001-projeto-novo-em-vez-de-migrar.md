# 0001 — Criar um projeto novo em vez de limpar o antigo

- **Data:** 2026-09-23
- **Autor:** Daniel Ramalho (com Claude Code)
- **Status:** Aceita

## Contexto

O projeto antigo (`Aerofobia atual (22-03-2024)`) foi analisado. Os detalhes estão em [ANALISE_PROJETO_ANTIGO.md](../ANALISE_PROJETO_ANTIGO.md):
- Unity 2022.3.7f1, Built-in RP, Google Cardboard (celular), Photon multiplayer;
- 655 MB de assets, dos quais ~450 MB sem uso; 3.923 materiais (metade duplicada);
- apenas ~400 linhas de código próprio, com bugs, e nenhuma parte funcionando de verdade em VR;
- sem controle de versão.

O alvo agora é **Meta Quest 2/3**, o que muda plataforma XR, input, UI e pipeline de renderização.

## Decisão

Criar um projeto novo, do zero, com configuração limpa para Quest. Do projeto antigo, migrar
**apenas** os assets necessários, de forma seletiva (via `.unitypackage` com dependências).
A lógica de voo será reescrita.

## Alternativas consideradas

- **Limpar o projeto antigo e converter:** exigiria remover Photon, Cardboard, Visual Scripting,
  converter para URP e reconfigurar XR, input e UI. Na prática, é o mesmo trabalho de um projeto novo,
  só que carregando lixo e configurações antigas escondidas em `ProjectSettings/`.

## Consequências

- Histórico Git limpo desde o primeiro commit.
- O projeto antigo continua intacto como referência (não é apagado).
- É preciso migrar e otimizar os assets manualmente (Fase 1 do roadmap).
