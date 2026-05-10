namespace Projeto2

// Concentra as fórmulas financeiras. Possui apenas cálculos
// puros para avaliar matematicamente
// a carteira gerando valores tangíveis: descobre o retorno anualizado,
// calcula o desvio padrão (volatilidade) e aplica a fórmula final para
// calcular rigorosamente o Sharpe Ratio com base no histórico.

module Portfolio =

    open Projeto2.Types

    let private media (valores: float list) : float =
        if List.isEmpty valores then
            0.0
        else
            valores |> List.average

    let private desvioPadrao (valores: float list) : float =
        if valores.Length <= 1 then
            0.0
        else
            let m = media valores

            let variancia =
                valores
                |> List.averageBy (fun x -> (x - m) ** 2.0)

            sqrt variancia

    let carteiraValida (pesoMaximo: float) (carteira: Carteira) : bool =
        let somaPesos = carteira.Pesos |> List.sum

        let pesosValidos =
            carteira.Pesos
            |> List.forall (fun peso -> peso >= 0.0 && peso <= pesoMaximo)

        let somaValida =
            abs (somaPesos - 1.0) < 0.000001

        carteira.Ativos.Length = carteira.Pesos.Length
        && pesosValidos
        && somaValida

    let calcularRetornosDiariosCarteira 
        (retornos: RetornoAtivo list) 
        (carteira: Carteira) 
        : float list =

        let mapaPesos =
            List.zip carteira.Ativos carteira.Pesos
            |> Map.ofList

        let ativosCarteira =
            carteira.Ativos |> Set.ofList

        retornos
        |> List.filter (fun r -> ativosCarteira.Contains r.Ticker)
        |> List.groupBy (fun r -> r.Data)
        |> List.choose (fun (_, retornosDoDia) ->

            let tickersDoDia =
                retornosDoDia
                |> List.map (fun r -> r.Ticker)
                |> Set.ofList

            if Set.isSubset ativosCarteira tickersDoDia then
                let retornoCarteiraDia =
                    retornosDoDia
                    |> List.sumBy (fun r ->
                        let peso = mapaPesos.[r.Ticker]
                        peso * r.Retorno
                    )

                Some retornoCarteiraDia
            else
                None
        )

    let calcularRetornoAnualizado 
        (retornos: RetornoAtivo list) 
        (carteira: Carteira) 
        : float =

        let retornosDiarios =
            calcularRetornosDiariosCarteira retornos carteira

        if List.isEmpty retornosDiarios then
            0.0
        else
            (media retornosDiarios) * 252.0

    let calcularVolatilidadeAnualizada 
        (retornos: RetornoAtivo list) 
        (carteira: Carteira) 
        : float =

        let retornosDiarios =
            calcularRetornosDiariosCarteira retornos carteira

        if List.isEmpty retornosDiarios then
            0.0
        else
            (desvioPadrao retornosDiarios) * sqrt 252.0

    let calcularSharpe 
        (retornoAnualizado: float) 
        (volatilidadeAnualizada: float) 
        : float =

        if volatilidadeAnualizada = 0.0 then
            System.Double.NegativeInfinity
        else
            retornoAnualizado / volatilidadeAnualizada

    let avaliarCarteira 
        (retornos: RetornoAtivo list) 
        (pesoMaximo: float) 
        (carteira: Carteira) 
        : ResultadoCarteira option =

        if not (carteiraValida pesoMaximo carteira) then
            None
        else
            let retornoAnualizado =
                calcularRetornoAnualizado retornos carteira

            let volatilidadeAnualizada =
                calcularVolatilidadeAnualizada retornos carteira

            let sharpe =
                calcularSharpe retornoAnualizado volatilidadeAnualizada

            Some {
                Carteira = carteira
                RetornoAnualizado = retornoAnualizado
                VolatilidadeAnualizada = volatilidadeAnualizada
                Sharpe = sharpe
            }