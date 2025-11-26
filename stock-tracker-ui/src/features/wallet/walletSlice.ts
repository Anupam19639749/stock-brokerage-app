import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { WalletBalanceDto, WalletTransactionDto } from "../../types/walletTypes";
import { fetchWalletBalance, fetchWalletHistory } from "./walletThunks";

// 1. Define the state shape
interface WalletState {
  balance: number | null;
  transactions: WalletTransactionDto[];
  status: "idle" | "pending" | "succeeded" | "failed";
  error: string | null;
}

// 2. Define the initial state
const initialState: WalletState = {
  balance: null,
  transactions: [],
  status: "idle",
  error: null,
};

// 3. Create the slice
export const walletSlice = createSlice({
  name: "wallet",
  initialState,
  reducers: {
    // This is correctly used by portfolioSlice
    setWalletBalance: (state, action: PayloadAction<WalletBalanceDto>) => {
      state.balance = action.payload.balance;
      state.status = "succeeded";
    },
    // This is correctly used by the Navbar
    clearWallet: (state) => {
      state.balance = null;
      state.transactions = []; 
      state.status = "idle";
    },
  },
  extraReducers: (builder) => {
    builder
      // fetchWalletBalance
      .addCase(fetchWalletBalance.pending, (state) => {
        state.status = "pending";
        state.error = null;
      })
      .addCase(fetchWalletBalance.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.balance = action.payload.balance;
      })
      .addCase(fetchWalletBalance.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      })
      // fetchWalletHistory
      .addCase(fetchWalletHistory.pending, (state) => {
        state.status = "pending";
      })
      .addCase(fetchWalletHistory.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.transactions = action.payload;
      })
      .addCase(fetchWalletHistory.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      });
  },
});

export const { setWalletBalance, clearWallet } = walletSlice.actions;
export default walletSlice.reducer;