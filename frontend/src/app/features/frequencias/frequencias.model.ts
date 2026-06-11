export interface FrequenciaEntrega {
  id: number;
  nome: string;
  diasCiclo: number | null;
  descricao: string;
  personalizada: boolean;
  ativo: boolean;
}

export interface SalvarFrequenciaRequest {
  nome: string;
  diasCiclo: number | null;
  descricao: string;
  personalizada: boolean;
  ativo: boolean;
}

/** Pré-visualização (cliente) da agenda a partir de hoje — espelha o serviço do backend. */
export function previewAgenda(diasCiclo: number, horizonteDias = 28): string[] {
  if (!diasCiclo || diasCiclo <= 0) {
    return [];
  }
  const datas: string[] = [];
  const limite = new Date();
  limite.setDate(limite.getDate() + horizonteDias);
  for (let d = new Date(); d <= limite; d.setDate(d.getDate() + diasCiclo)) {
    datas.push(d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }));
  }
  return datas;
}
