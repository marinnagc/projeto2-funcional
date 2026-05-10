namespace Projeto2
// Types.fs: É o dicionário de tipos do projeto. 
// Define as estruturas de dados base (record types),
// como o que compõe um Ativo, uma Carteira (lista de ativos e pesos)
// e os resultados financeiros (ResultadoCarteira), garantindo que todos
// os outros arquivos falem a mesma "língua".
module Types =

    type Ativo = {
        Ticker: string
    }

    type PrecoAtivo = {
        Data: System.DateTime
        Ticker: string
        PrecoFechamento: float
    }

    type RetornoAtivo = {
        Data: System.DateTime
        Ticker: string
        Retorno: float
    }

    type Carteira = {
        Ativos: string list
        Pesos: float list
    }

    type ResultadoCarteira = {
        Carteira: Carteira
        RetornoAnualizado: float 
        VolatilidadeAnualizada: float
        Sharpe: float // Sharpe Ratio
    }

// PrecoAtivo = dado bruto de preço
// RetornoAtivo = dado calculado a partir do preço
// Carteira = ativos + pesos sorteados
// ResultadoCarteira = carteira + métricas calculadas


// CSV/API
// ↓
// PrecoAtivo
// ↓
// RetornoAtivo
// ↓
// Carteira simulada
// ↓
// ResultadoCarteira
// ↓
// Melhor carteira pelo maior Sharpe