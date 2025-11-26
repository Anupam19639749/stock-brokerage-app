import { createSlice, createAsyncThunk, type PayloadAction } from "@reduxjs/toolkit";
import http from "../../api/axiosInstance";
import type { OrderRequestDto, OrderDetailsDto, PortfolioHoldingDto } from "../../types/tradeTypes";
import type { WalletBalanceDto } from "../../types/walletTypes";
import { toast } from "react-toastify";
import { setWalletBalance } from "../wallet/walletSlice"; // This import is now safe

// --- THUNKS ARE NOW IN THIS FILE ---

export const fetchPortfolio = createAsyncThunk(
  "portfolio/fetchPortfolio",
  async (_, { rejectWithValue }) => {
    try {
      const response = await http.get<{ data: PortfolioHoldingDto[] }>("/portfolio");
      return response.data.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || "Failed to fetch portfolio");
    }
  }
);

export const fetchOrders = createAsyncThunk(
  "portfolio/fetchOrders",
  async (_, { rejectWithValue }) => {
    try {
      const response = await http.get<{ data: OrderDetailsDto[] }>("/portfolio/orders");
      return response.data.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || "Failed to fetch orders");
    }
  }
);

export const placeOrder = createAsyncThunk(
  "portfolio/placeOrder",
  async (orderRequest: OrderRequestDto, { dispatch, rejectWithValue }) => {
    try {
      const formData = new FormData();
      formData.append("ticker", orderRequest.ticker);
      formData.append("quantity", orderRequest.quantity.toString());
      formData.append("type", orderRequest.type);

      const response = await http.post<{
        data: OrderDetailsDto;
        success: boolean;
        message: string;
      }>("/trade/order", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      // Re-fetch the wallet balance (this is correct)
      const walletResponse = await http.get<{ data: WalletBalanceDto }>("/wallet/balance");
      dispatch(setWalletBalance(walletResponse.data.data));

      toast.success(response.data.message);
      return response.data.data;
      
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || "Order failed";
      toast.error(errorMsg);
      return rejectWithValue(errorMsg);
    }
  }
);

// --- SLICE DEFINITION ---

interface PortfolioState {
  holdings: PortfolioHoldingDto[];
  orders: OrderDetailsDto[];
  status: "idle" | "pending" | "succeeded" | "failed";
  error: string | null;
}

const initialState: PortfolioState = {
  holdings: [],
  orders: [],
  status: "idle",
  error: null,
};

export const portfolioSlice = createSlice({
  name: "portfolio",
  initialState,
  reducers: {
    clearPortfolio: (state) => {
      state.holdings = [];
      state.orders = [];
      state.status = "idle";
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchPortfolio.pending, (state) => {
        state.status = "pending";
      })
      .addCase(fetchPortfolio.fulfilled, (state, action: PayloadAction<PortfolioHoldingDto[]>) => {
        state.status = "succeeded";
        state.holdings = action.payload;
      })
      .addCase(fetchPortfolio.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      })
      .addCase(fetchOrders.pending, (state) => {
        state.status = "pending";
      })
      .addCase(fetchOrders.fulfilled, (state, action: PayloadAction<OrderDetailsDto[]>) => {
        state.status = "succeeded";
        state.orders = action.payload;
      })
      .addCase(fetchOrders.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      })
      .addCase(placeOrder.pending, (state) => {
        state.status = "pending";
      })
      .addCase(placeOrder.fulfilled, (state, action: PayloadAction<OrderDetailsDto>) => {
        state.status = "succeeded";
        state.orders.unshift(action.payload);
      })
      .addCase(placeOrder.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      });
  },
});

export const { clearPortfolio } = portfolioSlice.actions;
export default portfolioSlice.reducer;