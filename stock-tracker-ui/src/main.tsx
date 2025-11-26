import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.tsx';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { theme } from './theme.ts';
import 'react-toastify/dist/ReactToastify.css';
import { Provider } from 'react-redux';
import { store } from './app/store.ts';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { BrowserRouter } from 'react-router-dom';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    {/* --- WRAP YOUR APP IN THE PROVIDER --- */}
    <Provider store={store}>
      <LocalizationProvider dateAdapter={AdapterDateFns}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
            <BrowserRouter>
              <App />
            </BrowserRouter>
        </ThemeProvider>
      </LocalizationProvider>
    </Provider>
  </React.StrictMode>,
);