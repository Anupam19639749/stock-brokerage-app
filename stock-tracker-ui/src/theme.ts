import { createTheme } from '@mui/material/styles';

// This is our central theme file.
// We're setting the default mode to 'dark'.
export const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#3f51b5', // You can change this to any blue/accent you like
    },
    secondary: {
      main: '#f50057',
    },
    background: {
      default: '#121212', // A standard dark theme background
      paper: '#1e1e1e', // The color for cards, modals, etc.
    },
  },
  typography: {
    fontFamily: [
      '-apple-system',
      'BlinkMacSystemFont',
      '"Segoe UI"',
      'Roboto',
      '"Helvetica Neue"',
      'Arial',
      'sans-serif',
    ].join(','),
  },
});