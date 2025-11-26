// Matches our backend AlertCreateDto
export interface AlertCreateDto {
  ticker: string;
  condition: "ABOVE" | "BELOW";
  targetPrice: number;
}

// Matches our backend AlertDetailsDto
export interface AlertDetailsDto {
  id: number;
  ticker: string;
  condition: string;
  targetPrice: number;
  status: string;
  createdAt: string;
}