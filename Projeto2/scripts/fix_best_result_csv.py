from __future__ import annotations

from pathlib import Path


def normalize_number(text: str) -> str:
    return text.strip().replace(",", ".")


def main() -> None:
    base_dir = Path(__file__).resolve().parents[1]
    input_file = base_dir / "results" / "best_result.csv"

    if not input_file.exists():
        raise FileNotFoundError(f"Arquivo não encontrado: {input_file}")

    lines = input_file.read_text(encoding="utf-8").splitlines()
    if not lines:
        raise ValueError("CSV vazio")

    output_lines: list[str] = ["Ticker;Peso"]
    index = 1

    while index < len(lines):
        line = lines[index].strip()
        index += 1

        if not line:
            break

        if ";" in line:
            parts = line.split(";", 1)
        else:
            parts = line.split(",", 1)

        if len(parts) != 2:
            raise ValueError(f"Linha inválida na seção de pesos: {line}")

        ticker, peso = parts
        output_lines.append(f"{ticker};{normalize_number(peso)}")

    metrics = {
        "RetornoAnualizado": None,
        "VolatilidadeAnualizada": None,
        "Sharpe": None,
    }

    for line in lines[index:]:
        line = line.strip()
        if not line:
            continue

        if ";" in line:
            parts = line.split(";", 1)
        else:
            parts = line.split(",", 1)

        if len(parts) != 2:
            raise ValueError(f"Linha inválida na seção de métricas: {line}")

        key, value = parts
        if key in metrics:
            metrics[key] = normalize_number(value)

    output_lines.append("")
    for key in ["RetornoAnualizado", "VolatilidadeAnualizada", "Sharpe"]:
        value = metrics[key]
        if value is None:
            raise ValueError(f"Métrica ausente: {key}")
        output_lines.append(f"{key};{value}")

    input_file.write_text("\n".join(output_lines) + "\n", encoding="utf-8")
    print(f"Arquivo corrigido: {input_file}")


if __name__ == "__main__":
    main()
