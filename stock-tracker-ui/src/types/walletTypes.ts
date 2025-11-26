// Matches backend WalletBalanceDto
export interface WalletBalanceDto {
  walletId: number;
  balance: number;
}

// Matches backend AddMoneyRequestDto
export interface AddMoneyRequestDto {
  amount: number;
  password: string;
}

// Matches backend WalletTransactionDto
export interface WalletTransactionDto {
  id: number;
  amount: number;
  type: string;
  timestamp: string;
}