// Matches our backend RegisterUserDto
export interface RegisterUserDto {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber?: string;
}

// Matches our backend LoginDto
export interface LoginDto {
  email: string;
  password: string;
}

// Matches our backend ForgotPasswordDto
export interface ForgotPasswordDto {
  email: string;
}

// Matches our backend ResetPasswordDto
export interface ResetPasswordDto {
  email: string;
  code: string;
  newPassword: string;
}