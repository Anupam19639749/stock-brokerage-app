import { configureStore } from "@reduxjs/toolkit";
import authReducer from "../features/auth/authSlice";
import walletReducer from "../features/wallet/walletSlice";
import portfolioReducer from "../features/portfolio/portfolioSlice";
import livePriceReducer from "../features/livePrice/livePriceSlice";

export const store = configureStore({
  reducer: {
    auth: authReducer,
    wallet: walletReducer,
    portfolio: portfolioReducer,
    livePrice: livePriceReducer,
  },
});

// These are standard types for using Redux with TypeScript
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;