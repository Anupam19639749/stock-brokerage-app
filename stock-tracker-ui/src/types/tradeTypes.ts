// Matches backend OrderRequestDto
export interface OrderRequestDto {
  ticker: string;
  quantity: number;
  type: "BUY" | "SELL"; // Corresponds to our OrderType enum
}

// Matches backend OrderDetailsDto
export interface OrderDetailsDto {
  id: number;
  ticker: string;
  quantity: number;
  pricePerShare: number;
  totalValue: number;
  type: string;
  status: string;
  timestamp: string;
}

// Matches backend PortfolioHoldingDto
export interface PortfolioHoldingDto {
  id: number;
  ticker: string;
  quantity: number;
  averageCostPrice: number;
  totalCost: number;
}

export interface FinnhubQuoteDto {
  c: number;
  h: number;
  l: number;
  o: number;
  pc: number;
}