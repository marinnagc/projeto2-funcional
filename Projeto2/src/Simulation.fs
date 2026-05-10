namespace Projeto2

// É o criador de cenários aleatórios.
// Ele sorteia (valida e normaliza para base 100%) 
// as milhões de combinações possíveis de pesos dos ativos 
// utilizando geradores aleatórios validos dentro dos limites do 
// projeto (peso máximo = 0.2). Ele que invoca os cálculos do Portfolio a 
// cada geração e retorna a "melhor" carteira do bloco simulado.

module Simulation =

    open System
    open Projeto2.Types
    open Projeto2.Portfolio

    let private normalizarPesos (valores: float list) : float list =
        let soma = valores |> List.sum

        if soma = 0.0 then
            valores
        else
            valores |> List.map (fun v -> v / soma)

    let gerarPesosAleatorios 
        (quantidade: int) 
        (pesoMaximo: float) 
        (seed: int) 
        : float list =

        let random = Random(seed)

        let rec tentarGerar tentativa =
            let pesos =
                List.init quantidade (fun _ -> random.NextDouble())
                |> normalizarPesos

            let valido =
                pesos |> List.forall (fun p -> p <= pesoMaximo)

            if valido then
                pesos
            else
                tentarGerar (tentativa + 1)

        tentarGerar 0

    let criarCarteira 
        (ativos: string list) 
        (pesos: float list) 
        : Carteira =

        {
            Ativos = ativos
            Pesos = pesos
        }

    let simularCarteira 
        (retornos: RetornoAtivo list) 
        (ativos: string list) 
        (pesoMaximo: float) 
        (seed: int) 
        : ResultadoCarteira option =

        let pesos =
            gerarPesosAleatorios ativos.Length pesoMaximo seed

        let carteira =
            criarCarteira ativos pesos

        avaliarCarteira retornos pesoMaximo carteira

    let simularCombinacao 
        (retornos: RetornoAtivo list) 
        (quantidadeSimulacoes: int) 
        (pesoMaximo: float) 
        (ativos: string list) 
        : ResultadoCarteira option =

        seq { 1 .. quantidadeSimulacoes }
        |> Seq.choose (fun i ->
            let seed =
                abs (hash (ativos, i))

            simularCarteira retornos ativos pesoMaximo seed
        )
        |> Seq.filter (fun resultado -> resultado.Sharpe > 0.0)
        |> Seq.sortByDescending (fun resultado -> resultado.Sharpe)
        |> Seq.tryHead