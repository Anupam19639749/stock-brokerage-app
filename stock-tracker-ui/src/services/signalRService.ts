import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { store } from "../app/store";
import { updatePrice } from "../features/livePrice/livePriceSlice";

let connection: HubConnection | null = null;

const signalRService = {
  startConnection: () => {
    // Get the base URL from our environment variables
    const hubUrl = `${import.meta.env.VITE_API_BASE_URL}/pricehub`;
    
    // Build the connection
    connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        // This is how we send our HttpOnly cookie for authentication
        withCredentials: true,
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect() // Automatically try to reconnect
      .build();

    // --- This is the most important part ---
    // Listen for the "ReceivePriceUpdate" message from the backend
    connection.on("ReceivePriceUpdate", (ticker: string, price: number) => {
      // When we get a new price, dispatch it to our Redux store
      store.dispatch(updatePrice({ ticker, price }));
    });
    // --- End of important part ---

    // Start the connection
    connection
      .start()
      .then(() => console.log("SignalR Connected."))
      .catch((err) => console.error("SignalR Connection Error: ", err));
  },

  stopConnection: () => {
    connection
      ?.stop()
      .then(() => console.log("SignalR Disconnected."))
      .catch((err) => console.error("SignalR Disconnection Error: ", err));
  },
};

export default signalRService;