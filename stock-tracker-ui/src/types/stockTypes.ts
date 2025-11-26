// Matches our backend MarketIndexDto

export interface MarketIndexDto {
  ticker: string;
  name: string;
  currentPrice: number;
  change: number;
  percentChange: number;
}

// Matches our backend StockQuoteCardDto
export interface StockQuoteCardDto {
  ticker: string;
  currentPrice: number;
  change: number;
  percentChange: number;
}

// We'll also add the other stock DTOs we'll need soon
// Matches backend CompanyProfileDto
export interface CompanyProfileDto {
  name: string;
  ticker: string;
  logoUrl: string;
  industry: string;
  websiteUrl: string;
}

// Matches backend StockSearchResultDto
export interface StockSearchResultDto {
  ticker: string;
  description: string;
}

// Matches backend FinnhubQuoteDto
export interface FinnhubQuoteDto {
  c: number;
  h: number;
  l: number;
  o: number;
  pc: number;
}