export interface LoginRequest {
  email: string;
  senha: string;
}

export interface UsuarioAutenticado {
  id: number;
  nome: string;
  email: string;
  papeis: string[];
}

export interface AuthResult {
  accessToken: string;
  accessTokenExpiraEm: string;
  refreshToken: string;
  refreshTokenExpiraEm: string;
  usuario: UsuarioAutenticado;
}
