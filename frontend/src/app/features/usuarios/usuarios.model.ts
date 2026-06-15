export type Perfil = 'Administrador' | 'Operador' | 'Cozinha';

export interface Usuario {
  id: number;
  nome: string;
  email: string;
  telefone: string | null;
  observacoes: string | null;
  perfil: string;
  ativo: boolean;
  atualizadoEm: string | null;
}

export interface PerfilOpcao {
  nome: string;
  descricao: string;
}

export interface CriarUsuarioRequest {
  nome: string;
  email: string;
  senha: string;
  perfil: string;
  telefone?: string | null;
  observacoes?: string | null;
}

export interface AtualizarUsuarioRequest {
  nome: string;
  email: string;
  perfil: string;
  telefone?: string | null;
  observacoes?: string | null;
}
