# Projeto 2 - Otimização de Portfólio com F#

Simulação paralela de Monte Carlo para encontrar a melhor carteira de investimentos (maior Sharpe Ratio) dentre 30 ações do índice Dow Jones. Implementado em **F#** com princípios de programação funcional: funções puras, imutabilidade, processamento paralelo e pipelines de dados.

## O que o projeto faz
- Seleciona carteiras com **25 ativos ou mais** entre as 30 disponíveis.
- Gera todas as combinações possíveis de ativos (etapa combinatória).
- Amostra um subconjunto das combinações para executar em tempo viável.
- Para cada combinação amostrada, simula múltiplas alocações de pesos aleatórios.
- Calcula retorno anualizado, volatilidade anualizada e Sharpe Ratio para cada carteira.
- Identifica a carteira com o melhor Sharpe Ratio.
- Respeita restrições: long-only, peso máximo de 20% por ativo, soma de pesos = 1.
- Usa **paralelismo** (Array.Parallel.choose) para acelerar a avaliação.

## Requisitos e Dependências
- **.NET SDK 10.0+** para compilar e executar o código F#.
- Arquivo de dados históricos: `Projeto2/data/dow30_returns.csv` (fornecido pelo professor).

## Como Instalar e Executar

### 1. Preparar os Dados
Coloque o arquivo `dow30_returns.csv` fornecido pelo professor em `Projeto2/data/`.
```
Projeto2/
└── data/
    └── dow30_returns.csv
```

### 2. Compilar e Rodar
```powershell
cd Projeto2
dotnet run
```

O programa carregará os retornos diários do período 01/07/2025 a 31/12/2025, executará a simulação e salvará o resultado em `results/best_result.csv`.

## Modos de Execução
O projeto oferece dois modos que você alterna comentando/descomentando linhas no `Program.fs`:

- **Modo Teste**: 50 combinações, 200 simulações (~3-5 minutos)
- **Modo Entrega**: 500 combinações, 2.000 simulações (~30+ minutos)

Por padrão, está no Modo Teste. Para trocar, localize o bloco de configuração em `Program.fs` e descomente a versão de Entrega.

## Estrutura do Projeto
```
Projeto2/
├── data/
│   └── dow30_returns.csv          # Arquivo de entrada (retornos diários)
├── results/
│   └── best_result.csv            # Resultado: melhor carteira encontrada
├── src/
│   ├── Types.fs                   # Tipos de domínio (Ativo, Carteira, ResultadoCarteira)
│   ├── DataLoader.fs              # Carregamento e validação do CSV
│   ├── Portfolio.fs               # Cálculos puros: retorno, volatilidade, Sharpe
│   ├── Simulation.fs              # Geração de pesos e simulação de carteiras
│   ├── ParallelRunner.fs          # Combinações, amostragem e paralelismo
│   └── Program.fs                 # Main: orquestração e I/O
└── Projeto2.fsproj               # Configuração do projeto .NET
```

## Resultados Esperados
A execução produz:
1. **Log no console** com informações da simulação (ativos por carteira, combinações amostradas, tempo total).
2. **Melhor carteira encontrada** exibida com: Sharpe Ratio, Retorno Anualizado, Volatilidade Anualizada, Pesos por ativo.
3. **Arquivo CSV** (`results/best_result.csv`) com a carteira ótima e suas métricas, pronto para análise posterior.

O tempo de execução depende do modo escolhido:
- Modo Teste: poucos minutos
- Modo Entrega: 30 a 60+ minutos (máquina dependente)

## Notas sobre a Implementação
- **Funções Puras**: Cálculos financeiros em `Portfolio.fs` não possuem efeitos colaterais.
- **Paralelismo**: Avaliação de combinações usa `Array.Parallel.choose` para multithreading seguro.
- **Amostragem Controlada**: Após gerar todas as combinações, amostra um subconjunto determinístico (seed fixa).
- **Modo Leve**: Prioriza execução viável em máquina comum sem sacrificar a lógica funcional.
