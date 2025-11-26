import { useState, useEffect } from 'react';
import { Container, Typography, Button, Box, Paper, CircularProgress, TableContainer, Table, TableHead, TableRow, TableCell, TableBody } from '@mui/material';
import { useAppSelector, useAppDispatch } from '../hooks/redux-hooks';
import AddMoneyModal from '../components/ui/AddMoneyModal';
import { fetchWalletHistory } from '../features/wallet/walletThunks';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';

const WalletPage = () => {
  const dispatch = useAppDispatch();
  const { balance, transactions = [], status } = useAppSelector((state) => state.wallet);
  const [modalOpen, setModalOpen] = useState(false);

  useEffect(() => {
    dispatch(fetchWalletHistory());
  }, [dispatch]);

  const getAmountColor = (type: string) => {
    switch (type) {
      case "DEPOSIT":
      case "TRADE_CREDIT":
        return "success.main";
      case "TRADE_DEBIT":
        return "error.main";
      default:
        return "text.primary";
    }
  };

  const isLoadingHistory = status === 'pending' && transactions.length === 0;

  return (
    <Container maxWidth="md">
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <AccountBalanceWalletIcon sx={{ mr: 1.5, color: '#5c6bc0', fontSize: 28 }} />
        <Typography variant="h5" sx={{ fontWeight: 600 }}>
          My Wallet
        </Typography>
      </Box>
      
      <Paper sx={{ 
        p: 4, 
        display: 'flex', 
        flexDirection: { xs: 'column', sm: 'row' },
        alignItems: 'center', 
        justifyContent: 'space-between', 
        mb: 4,
        gap: 2,
        backgroundColor: 'rgba(30, 30, 30, 0.6)',
        border: '1px solid rgba(92, 107, 192, 0.15)'
      }}>
        <Box sx={{ textAlign: { xs: 'center', sm: 'left' } }}>
          <Typography variant="body2" color="text.secondary">Current Balance</Typography>
          {status === 'pending' && !balance ? (
             <CircularProgress size={20} />
          ) : (
            <Typography variant="h3" sx={{ fontWeight: 'bold', mt: 1 }}>
              ${balance?.toLocaleString('en-US', { minimumFractionDigits: 2 }) ?? '0.00'}
            </Typography>
          )}
        </Box>
        <Button 
          variant="contained" 
          size="large" 
          onClick={() => setModalOpen(true)}
          sx={{ 
            px: 4,
            backgroundColor: '#5c6bc0',
            '&:hover': {
              backgroundColor: '#4a5bb5',
            }
          }}
        >
          Add Money
        </Button>
      </Paper>

      <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
        Transaction History
      </Typography>
      <TableContainer 
        component={Paper} 
        sx={{ 
          backgroundColor: 'rgba(30, 30, 30, 0.6)', 
          border: '1px solid rgba(92, 107, 192, 0.15)',
          overflowX: 'auto'
        }}
      >
        <Table>
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 600 }}>Date</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
              <TableCell align="right" sx={{ fontWeight: 600 }}>Amount</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoadingHistory ? (
              <TableRow>
                <TableCell colSpan={3} align="center" sx={{ py: 4 }}>
                  <CircularProgress size={24} />
                </TableCell>
              </TableRow>
            ) : transactions.length === 0 ? (
              <TableRow>
                <TableCell colSpan={3} align="center" sx={{ py: 4 }}>
                  No transactions found.
                </TableCell>
              </TableRow>
            ) : (
              transactions.map((tx) => (
                <TableRow key={tx.id} sx={{ '&:hover': { backgroundColor: 'rgba(92, 107, 192, 0.05)' } }}>
                  <TableCell>{new Date(tx.timestamp).toLocaleString()}</TableCell>
                  <TableCell>{tx.type.replace("_", " ")}</TableCell>
                  <TableCell align="right" sx={{ color: getAmountColor(tx.type), fontWeight: 'bold' }}>
                    {tx.amount >= 0 ? '+' : ''}${tx.amount.toLocaleString('en-US')}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      
      <AddMoneyModal open={modalOpen} onClose={() => setModalOpen(false)} />
    </Container>
  );
};

export default WalletPage;