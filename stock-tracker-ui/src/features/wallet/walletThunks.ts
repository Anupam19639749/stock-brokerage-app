import { createAsyncThunk } from "@reduxjs/toolkit";
import http from "../../api/axiosInstance";
import type { WalletBalanceDto, WalletTransactionDto } from "../../types/walletTypes";

export const fetchWalletBalance = createAsyncThunk(
  "wallet/fetchBalance",
  async (_, { rejectWithValue }) => {
    try {
      const response = await http.get<{ data: WalletBalanceDto }>("/wallet/balance");
      return response.data.data;
    } catch (error: any) {
      // Don't show a toast here, it's a silent fetch
      return rejectWithValue(error.response?.data?.message || "Failed to fetch wallet");
    }
  }
);

export const fetchWalletHistory = createAsyncThunk(
  "wallet/fetchHistory",
  async (_, { rejectWithValue }) => {
    try {
      const response = await http.get<WalletTransactionDto[]>("/wallet/history");
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || "Failed to fetch history");
    }
  }
);