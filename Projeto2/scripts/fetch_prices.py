import yfinance as yf
import pandas as pd
from pathlib import Path


TICKERS_DOW_JONES = [
    "AAPL", "AMGN", "AMZN", "AXP", "BA", "CAT", "CRM", "CSCO", "CVX", "DIS",
    "GS", "HD", "HON", "IBM", "JNJ", "JPM", "KO", "MCD", "MMM", "MRK",
    "MSFT", "NKE", "NVDA", "PG", "SHW", "TRV", "UNH", "V", "VZ", "WMT"
]


DATA_INICIO = "2025-07-01"
DATA_FIM = "2026-01-01"  
# yfinance usa data final como exclusiva.
# Então, para incluir 31/12/2025, usamos 2026-01-01.


def baixar_precos():
    linhas = []

    for ticker in TICKERS_DOW_JONES:
        print(f"Baixando dados de {ticker}...")

        dados = yf.download(
            ticker,
            start=DATA_INICIO,
            end=DATA_FIM,
            progress=False,
            auto_adjust=True
        )

        if dados.empty:
            print(f"Aviso: nenhum dado encontrado para {ticker}")
            continue

        for data, linha in dados.iterrows():
            preco_fechamento = float(linha["Close"])

            linhas.append({
                "Data": data.strftime("%Y-%m-%d"),
                "Ticker": ticker,
                "PrecoFechamento": preco_fechamento
            })

    df = pd.DataFrame(linhas)

    df = df.sort_values(by=["Ticker", "Data"])

    pasta_saida = Path("data")
    pasta_saida.mkdir(exist_ok=True)

    caminho_saida = pasta_saida / "prices.csv"
    df.to_csv(caminho_saida, index=False)

    print()
    print(f"Arquivo salvo em: {caminho_saida}")
    print(f"Total de linhas: {len(df)}")
    print(f"Total de tickers: {df['Ticker'].nunique()}")


if __name__ == "__main__":
    baixar_precos()