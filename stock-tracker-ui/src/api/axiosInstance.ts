import axios from "axios";

// Create the central axios instance
export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,

  // This is the magic line that tells axios to
  // automatically send our HttpOnly cookie with every request.
  withCredentials: true,
});

// You can also add interceptors here later if you need to
// (e.g., for global error handling)

export default http;