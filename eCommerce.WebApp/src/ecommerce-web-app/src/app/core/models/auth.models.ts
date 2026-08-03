export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  firstName: string;
  lastName: string;
}

export interface TokenResponse {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshExpiresAtUtc: string;
}

export interface AuthUser {
  id: string;
  email: string;
  role: 'Admin' | 'Customer';
  userId: string | null;
}
