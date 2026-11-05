# 0008 — Desenvolvedor único, commits direto na main

- **Data:** 2026-10-26
- **Autor:** Daniel/Claude
- **Status:** Aceita

## Contexto

O projeto foi montado para uma equipe de alunos: branch por tarefa, Pull Request com 1 aprovação,
`main` protegida e aviso antes de editar uma cena. A equipe não vai mais existir: o Daniel passa a ser
o único desenvolvedor. Sem revisor, o PR virou só burocracia, e três branches encadeadas
(`docs/especificacao-v1` → `asset/modelos-projeto-antigo` → `feature/cena-voo`) ficaram um mês sem merge.

## Decisão

- Os commits vão **direto na `main`**, sem branch de tarefa e sem Pull Request.
- As três branches pendentes foram levadas para a `main` por fast-forward (a `main` passou de `c5b9a6f` para `f77f4c2`).
- Saem as regras de equipe: proteger a `main`, convidar colaboradores e avisar antes de editar uma cena.
- Continuam valendo: `git pull` no início e `git push` no fim (backup, e o projeto pode ser aberto em outro PC),
  Conventional Commits, Git LFS, nada de `push --force` ou `reset --hard`, documentação obrigatória e
  o Claude pedindo permissão antes de cada push.

## Alternativas consideradas

- **Manter branch + PR sem revisor:** só adiciona passos; o histórico mostra que os PRs ficavam parados.
- **Branch por tarefa com merge local, sem PR:** ainda é um passo a mais sem ganho para uma pessoa só.
  Fica como opção apenas para experimentos que talvez sejam descartados.

## Consequências

- Menos passos por tarefa e nada de branches esquecidas.
- Um commit ruim vai direto para a `main`: revisar com `git status`/`git diff` antes de commitar ficou mais
  importante. Para desfazer, usar `git revert` (nunca reescrever a história já enviada).
- Se alguém voltar a colaborar, reavaliar esta decisão (o fluxo antigo está no histórico do `CONTRIBUINDO.md`).
- O modelo `.github/pull_request_template.md` e as branches antigas (local e remoto) foram apagados.
