import { createSlice, createAsyncThunk, type PayloadAction } from "@reduxjs/toolkit";
import type { UserDetailsDto } from "../../types/userTypes";
import type { LoginDto, RegisterUserDto } from "../../types/authTypes";
// import type { WalletBalanceDto } from "../../types/walletTypes";
import http from "../../api/axiosInstance";
import { toast } from "react-toastify";
// import { setWalletBalance, clearWallet } from "../wallet/walletSlice";

// --- THUNKS ARE NOW IN THIS FILE ---

interface AuthResponse {
  data: {
    token: string;
    user: UserDetailsDto;
  };
  success: boolean;
  message: string;
}

export const loginUser = createAsyncThunk(
  "auth/loginUser",
  async (loginDto: LoginDto, { dispatch, rejectWithValue }) => {
    try {
      const formData = new FormData();
      formData.append("email", loginDto.email);
      formData.append("password", loginDto.password);

      const response = await http.post<AuthResponse>("/auth/login", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      const { user } = response.data.data;
      dispatch(setLogin(user)); 
      return response.data; // Return full response for toast-then-navigate
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || "Login failed";
      toast.error(errorMsg);
      return rejectWithValue(errorMsg);
    }
  }
);

export const registerUser = createAsyncThunk(
  "auth/registerUser",
  async (registerDto: RegisterUserDto, { rejectWithValue }) => {
    try {
      const formData = new FormData();
      formData.append("firstName", registerDto.firstName);
      formData.append("lastName", registerDto.lastName);
      formData.append("email", registerDto.email);
      formData.append("password", registerDto.password);
      if (registerDto.phoneNumber) {
        formData.append("phoneNumber", registerDto.phoneNumber);
      }

      const response = await http.post<{ message: string, success: boolean }>( // Added success
        "/auth/register",
        formData,
        { headers: { "Content-Type": "multipart/form-data" } }
      );
      
      return response.data; // Return for toast-then-navigate
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || "Registration failed";
      toast.error(errorMsg);
      return rejectWithValue(errorMsg);
    }
  }
);

export const checkAuthStatus = createAsyncThunk(
  "auth/checkAuthStatus",
  async (_, { dispatch, rejectWithValue }) => {
    try {
      const response = await http.get<{ data: UserDetailsDto, success: boolean, message: string }>("/users/me");
      const user = response.data.data;
      dispatch(setLogin(user));
      // await fetchUserWallet(dispatch);
      return user;
    } catch (error: any) {
      return rejectWithValue("No active session");
    }
  }
);

// --- SLICE DEFINITION ---

interface AuthState {
  user: UserDetailsDto | null;
  isAuthenticated: boolean;
  status: "idle" | "pending" | "succeeded" | "failed";
  error: string | null;
}

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  status: "idle",
  error: null,
};

export const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setLogin: (state, action: PayloadAction<UserDetailsDto>) => {
      state.user = action.payload;
      state.isAuthenticated = true;
    },
    setLogout: (state) => {
      state.user = null;
      state.isAuthenticated = false;
    },
  },
  extraReducers: (builder) => {
    // This now correctly references the thunks in this file
    builder
      .addCase(loginUser.pending, (state) => {
        state.status = "pending";
        state.error = null;
      })
      .addCase(registerUser.pending, (state) => {
        state.status = "pending";
        state.error = null;
      })
      .addCase(loginUser.fulfilled, (state) => {
        state.status = "succeeded";
      })
      .addCase(registerUser.fulfilled, (state) => {
        state.status = "succeeded";
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      })
      .addCase(registerUser.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload as string;
      })
      .addCase(checkAuthStatus.pending, (state) => {
        state.status = "pending";
      })
      .addCase(checkAuthStatus.fulfilled, (state) => {
        state.status = "succeeded";
      })
      .addCase(checkAuthStatus.rejected, (state) => {
        state.status = "succeeded";
      });
  },
});

export const { setLogin, setLogout } = authSlice.actions;
export default authSlice.reducer;