// This is a TypeScript interface for our UserDetailsDto from the backend
export interface UserDetailsDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  gender?: string;
  dateOfBirth?: string; // We'll handle date conversion
  panNumber?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankIfscCode?: string;
  kycStatus: string;
  role: string;
  createdAt: string;
}

export interface KycSubmitDto {
  panNumber: string;
  bankName: string;
  bankAccountNumber: string;
  bankIfscCode: string;
}

// Matches our backend ProfileUpdateDto
export interface ProfileUpdateDto {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  gender?: string;
  dateOfBirth?: Date | null; // Date object for the picker
}