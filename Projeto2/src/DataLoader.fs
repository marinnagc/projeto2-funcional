namespace Projeto2

// É o módulo de entrada de dados (I/O). 
// Lê o arquivo CSV de retornos diários já calculados (matriz com tickers no header).
// Reconstrói as datas começando de 01/07/2025 e gera a lista de RetornoAtivo.

module DataLoader =

    open System
    open System.Globalization
    open System.IO
    open Projeto2.Types

    let private parseFloat (texto: string) =
        Double.Parse(texto, CultureInfo.InvariantCulture)

    let carregarRetornosDoPeriodo 
        (caminhoArquivo: string) 
        (dataInicio: DateTime) 
        (dataFim: DateTime) 
        : RetornoAtivo list =

        if not (File.Exists(caminhoArquivo)) then
            failwith $"Arquivo não encontrado: {caminhoArquivo}"

        let linhas = File.ReadAllLines(caminhoArquivo)

        if linhas.Length < 2 then
            failwith "CSV vazio ou com apenas header"

        // Primeira linha: tickers
        let tickers =
            linhas.[0].Split(',')
            |> Array.map (fun t -> t.Trim())
            |> Array.toList

        // Linhas restantes: retornos diários
        let retornosPorLinha =
            linhas
            |> Array.skip 1
            |> Array.filter (fun linha -> not (String.IsNullOrWhiteSpace(linha)))
            |> Array.mapi (fun indice linha ->
                let data = dataInicio.AddDays(float indice)
                let retornosStr = linha.Split(',')

                if retornosStr.Length <> tickers.Length then
                    failwith $"Linha {indice + 2}: número de colunas ({retornosStr.Length}) não bate com tickers ({tickers.Length})"

                (data, retornosStr)
            )

        // Construir lista de RetornoAtivo
        retornosPorLinha
        |> Array.collect (fun (data, retornosStr) ->
            tickers
            |> List.mapi (fun col ticker ->
                let retornoStr = retornosStr.[col].Trim()
                let retorno = parseFloat retornoStr

                {
                    Data = data
                    Ticker = ticker
                    Retorno = retorno
                }
            )
            |> List.toArray
        )
        |> Array.filter (fun r -> r.Data >= dataInicio && r.Data <= dataFim)
        |> Array.toList