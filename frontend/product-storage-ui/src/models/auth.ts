export interface AuthResponse {
  token: string;
  expiresAt: Date;
  fullname: string;
  email: string;
}

export interface RegisterRequest {
  fullname: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}