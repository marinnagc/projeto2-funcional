namespace Projeto2

// Lida com a lógica de rotas e performance. 
// Primeiro ele gera todas as possíveis "mãos" 
// combinatórias que podem ser escolhidas com a união das 
// 20 ações dentre 30 disponíveis. Depois, orquestra essa 
// simulação utilizando bibliotecas de concorrência (Array.Parallel.choose) 
// para acelerar a validação das carteiras de forma paralela via multithreading.

module ParallelRunner =

    open System
    open Projeto2.Types
    open Projeto2.Simulation

    let rec combinacoes tamanho lista =
        match tamanho, lista with
        | 0, _ -> [ [] ]
        | _, [] -> []
        | k, x :: xs ->
            let comX =
                combinacoes (k - 1) xs
                |> List.map (fun combinacao -> x :: combinacao)

            let semX =
                combinacoes k xs

            comX @ semX

    let private embaralharComSeed (seed: int) (itens: 'a list) =
        let aleatorio = Random(seed)

        itens
        |> List.map (fun item -> aleatorio.NextDouble(), item)
        |> List.sortBy fst
        |> List.map snd

    let private amostrarCombinacoes
        (ativosDisponiveis: string list)
        (quantidadeAcoesNaCarteira: int)
        (quantidadeAmostras: int)
        : string list list =

        let seed = quantidadeAcoesNaCarteira * 104729 + quantidadeAmostras * 7919

        ativosDisponiveis
        |> combinacoes quantidadeAcoesNaCarteira
        |> embaralharComSeed seed
        |> List.truncate quantidadeAmostras

    let avaliarCombinacoes
        (retornos: RetornoAtivo list)
        (combinações: string list list)
        (quantidadeSimulacoesPorCombinacao: int)
        (pesoMaximo: float)
        (usarParalelo: bool)
        : ResultadoCarteira option =

        let resultados =
            if usarParalelo then
                combinações
                |> List.toArray
                |> Array.Parallel.choose (fun ativos ->
                    simularCombinacao
                        retornos
                        quantidadeSimulacoesPorCombinacao
                        pesoMaximo
                        ativos
                )
                |> Array.toList
            else
                combinações
                |> List.choose (fun ativos ->
                    simularCombinacao
                        retornos
                        quantidadeSimulacoesPorCombinacao
                        pesoMaximo
                        ativos
                )

        resultados
        |> List.sortByDescending (fun resultado -> resultado.Sharpe)
        |> List.tryHead

    let executarSequencial 
        (retornos: RetornoAtivo list) 
        (ativosDisponiveis: string list) 
        (quantidadeAcoesNaCarteira: int) 
        (quantidadeSimulacoesPorCombinacao: int) 
        (pesoMaximo: float) 
        : ResultadoCarteira option =

        avaliarCombinacoes
            retornos
            (ativosDisponiveis |> combinacoes quantidadeAcoesNaCarteira)
            quantidadeSimulacoesPorCombinacao
            pesoMaximo
            false

    let executarSequencialAmostrado
        (retornos: RetornoAtivo list)
        (ativosDisponiveis: string list)
        (quantidadeAcoesNaCarteira: int)
        (quantidadeAmostrasCombinacoes: int)
        (quantidadeSimulacoesPorCombinacao: int)
        (pesoMaximo: float)
        : ResultadoCarteira option =

        avaliarCombinacoes
            retornos
            (amostrarCombinacoes ativosDisponiveis quantidadeAcoesNaCarteira quantidadeAmostrasCombinacoes)
            quantidadeSimulacoesPorCombinacao
            pesoMaximo
            false

    let executarParalelo 
        (retornos: RetornoAtivo list) 
        (ativosDisponiveis: string list) 
        (quantidadeAcoesNaCarteira: int) 
        (quantidadeSimulacoesPorCombinacao: int) 
        (pesoMaximo: float) 
        : ResultadoCarteira option =

        avaliarCombinacoes
            retornos
            (ativosDisponiveis |> combinacoes quantidadeAcoesNaCarteira)
            quantidadeSimulacoesPorCombinacao
            pesoMaximo
            true

    let executarParaleloAmostrado
        (retornos: RetornoAtivo list)
        (ativosDisponiveis: string list)
        (quantidadeAcoesNaCarteira: int)
        (quantidadeAmostrasCombinacoes: int)
        (quantidadeSimulacoesPorCombinacao: int)
        (pesoMaximo: float)
        : ResultadoCarteira option =

        avaliarCombinacoes
            retornos
            (amostrarCombinacoes ativosDisponiveis quantidadeAcoesNaCarteira quantidadeAmostrasCombinacoes)
            quantidadeSimulacoesPorCombinacao
            pesoMaximo
            true