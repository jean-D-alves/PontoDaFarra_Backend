# AGENTS.md

## Project Overview
Backend for Ponto da Farra

## Stack
- Asp.net 
- Net10.0
- postgres

## Estrutura de pastas
- /Controller: so HTTP
- /Services: regras de negócio
- /repositories: banco de dados
- /models: modelo de tabelas
- */Interface: Interface para busca

## Padrão de codigo
- nomes em inglês, camelCase
- funções com ate 20 linhas
- nada de logica no controller

## Restrictions
- Não instalar dependências sem aprovação.
- Não mexer em migrations
- Não modificar `package.json` diretamente.
- Não usar `console.log` em produção.