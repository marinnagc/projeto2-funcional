namespace Projeto2

// É o seu maestro ou função principal (main). 
// Responsável por inicializar as variáveis gerais, invocar a leitura (DataLoader), 
// passar essa carga de leitura para ser distribuída em paralelo (ParallelRunner) e 
// capturar o melhor resultado global do processamento, expondo por último um "print" 
// no seu terminal e salvando um novo CSV contendo essa glória na sua pasta de results. 
// É onde o motor "Puro" cruza com as saídas "Impuras"


module Program =

    open System
    open System.Diagnostics
    open System.IO
    open System.Globalization
    open Projeto2.DataLoader
    open Projeto2.ParallelRunner

    open Projeto2.Types

    let tickersDowJones = [
        "AAPL"; "AMGN"; "AMZN"; "AXP"; "BA"; "CAT"; "CRM"; "CSCO"; "CVX"; "DIS"
        "GS"; "HD"; "HON"; "IBM"; "JNJ"; "JPM"; "KO"; "MCD"; "MMM"; "MRK"
        "MSFT"; "NKE"; "NVDA"; "PG"; "SHW"; "TRV"; "UNH"; "V"; "VZ"; "WMT"
    ]

    let salvarResultado (caminho: string) (resultado: ResultadoCarteira) =
        let pasta = Path.GetDirectoryName(caminho)

        if not (String.IsNullOrWhiteSpace(pasta)) then
            Directory.CreateDirectory(pasta) |> ignore

        let linhas =
            [
                "Ticker;Peso"
                yield!
                    List.zip resultado.Carteira.Ativos resultado.Carteira.Pesos
                    |> List.map (fun (ticker, peso) ->
                        $"{ticker};{peso.ToString("G17", CultureInfo.InvariantCulture)}"
                    )

                ""
                $"RetornoAnualizado;{resultado.RetornoAnualizado.ToString("G17", CultureInfo.InvariantCulture)}"
                $"VolatilidadeAnualizada;{resultado.VolatilidadeAnualizada.ToString("G17", CultureInfo.InvariantCulture)}"
                $"Sharpe;{resultado.Sharpe.ToString("G17", CultureInfo.InvariantCulture)}"
            ]

        File.WriteAllLines(caminho, linhas)

    let resolverCaminhoCsv () =
        let candidatos = [
            "data/dow30_returns.csv"
            "data/prices.csv"
            "../data/prices.csv"
            "../dow30_returns.csv"
            "dow30_returns.csv"
        ]

        candidatos
        |> List.tryFind File.Exists
        |> Option.defaultWith (fun () -> candidatos.Head)

    [<EntryPoint>]
    let main argv =
        let caminhoCsv = resolverCaminhoCsv ()

        let dataInicio = DateTime(2025, 7, 1)
        let dataFim = DateTime(2025, 12, 31)

        let quantidadeAcoesNaCarteira = 25

        // ============ ESCOLHA DO MODO ============
        // Descomente o modo que deseja usar:

        // MODO TESTE: executa rápido para validação (~3-5 minutos)
        // let quantidadeAmostrasCombinacoes = 50
        // let quantidadeSimulacoesPorCombinacao = 200

        // MODO ENTREGA: resultado mais robusto (~30+ minutos)
        let quantidadeAmostrasCombinacoes = 500
        let quantidadeSimulacoesPorCombinacao = 2000
        // ========================================

        let pesoMaximo = 0.20

        printfn "Carregando retornos..."

        let retornos =
            carregarRetornosDoPeriodo caminhoCsv dataInicio dataFim

        printfn "Total de retornos carregados: %d" retornos.Length
        printfn "Iniciando simulação paralela..."
        printfn "Ações por carteira: %d" quantidadeAcoesNaCarteira
        printfn "Combinações amostradas: %d" quantidadeAmostrasCombinacoes
        printfn "Simulações por combinação: %d" quantidadeSimulacoesPorCombinacao
        printfn "Peso máximo por ativo: %.2f" pesoMaximo

        let stopwatch = Stopwatch.StartNew()

        let melhorCarteira =
            executarParaleloAmostrado
                retornos
                tickersDowJones
                quantidadeAcoesNaCarteira
                quantidadeAmostrasCombinacoes
                quantidadeSimulacoesPorCombinacao
                pesoMaximo

        stopwatch.Stop()

        match melhorCarteira with
        | Some resultado ->
            printfn ""
            printfn "Melhor carteira encontrada:"
            printfn "Sharpe: %.6f" resultado.Sharpe
            printfn "Retorno anualizado: %.6f" resultado.RetornoAnualizado
            printfn "Volatilidade anualizada: %.6f" resultado.VolatilidadeAnualizada
            printfn "Tempo de execução: %.2f segundos" stopwatch.Elapsed.TotalSeconds

            printfn ""
            printfn "Pesos:"
            List.zip resultado.Carteira.Ativos resultado.Carteira.Pesos
            |> List.iter (fun (ticker, peso) ->
                printfn "%s -> %.4f" ticker peso
            )

            salvarResultado "results/best_result.csv" resultado

            printfn ""
            printfn "Resultado salvo em results/best_result.csv"

        | None ->
            printfn "Nenhuma carteira válida encontrada."

        0