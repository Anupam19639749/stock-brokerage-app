import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

// This defines the dictionary: { "AAPL": 150.50, "MSFT": 300.75 }
interface PriceMap {
  [ticker: string]: number;
}

interface LivePriceState {
  prices: PriceMap;
}

const initialState: LivePriceState = {
  prices: {},
};

export const livePriceSlice = createSlice({
  name: "livePrice",
  initialState,
  reducers: {
    // This action is called by SignalR to update a single stock's price
    updatePrice: (state, action: PayloadAction<{ ticker: string; price: number }>) => {
      const { ticker, price } = action.payload;
      state.prices[ticker] = price;
    },
    // This action can be called on logout to clear the data
    clearPrices: (state) => {
      state.prices = {};
    },
  },
});

export const { updatePrice, clearPrices } = livePriceSlice.actions;
export default livePriceSlice.reducer;