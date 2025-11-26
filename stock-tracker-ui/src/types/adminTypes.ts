import type { OrderDetailsDto, PortfolioHoldingDto } from "./tradeTypes";
import type { WalletBalanceDto } from "./walletTypes";

// This is for the GET /api/admin/kyc-requests
export interface KycRequestDetailsDto {
  userId: number;
  fullName: string;
  email: string;
  panNumber: string;
  bankName: string;
  bankAccountNumber: string;
  bankIfscCode: string;
  submittedAt: string;
}

// This is for the GET /api/admin/users
export interface AdminUserListDto {
  id: number;
  fullName: string;
  email: string;
  role: string;
  kycStatus: string;
  isActive: boolean;
}

// This is for the GET /api/admin/users/{id}
export interface AdminUserDetailsDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  gender?: string;
  dateOfBirth?: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLogin?: string;
  kycStatus: string;
  panNumber?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankIfscCode?: string;
  hasProfileImage: boolean;
  wallet: WalletBalanceDto | null;
  portfolio: PortfolioHoldingDto[];
  orders: OrderDetailsDto[];
}

// This is for the GET /api/admin/stats
export interface AdminStatsDto {
  totalUsers: number;
  activeUsers: number;
  topHeldStocks: StockStatDto[];
  topAlertedStocks: StockStatDto[];
}

export interface StockStatDto {
  ticker: string;
  count: number;
}