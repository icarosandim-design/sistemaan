/**
 * Exportação CSV simples (UTF-8 com BOM, compatível com Excel pt-BR).
 * Sem dependências externas.
 */
export function exportarCsv(nomeArquivo: string, colunas: string[], linhas: (string | number | null | undefined)[][]): void {
  const escape = (v: string | number | null | undefined): string => {
    const s = v === null || v === undefined ? '' : String(v);
    return /[";\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
  };

  const sep = ';'; // Excel pt-BR usa ; como separador padrão
  const conteudo = [colunas, ...linhas]
    .map((linha) => linha.map(escape).join(sep))
    .join('\r\n');

  const blob = new Blob(['﻿' + conteudo], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = nomeArquivo.endsWith('.csv') ? nomeArquivo : `${nomeArquivo}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}

/** Formata número pt-BR para exibição/CSV. */
export function num(v: number | null | undefined, casas = 2): string {
  if (v === null || v === undefined) return '';
  return v.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
}
