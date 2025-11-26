import { useEffect, useState } from 'react';
import { Container, Typography, Paper, CircularProgress, Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Button, Stack, Grid, Card, CardContent, useMediaQuery, useTheme } from '@mui/material';
import { useAppSelector, useAppDispatch } from '../hooks/redux-hooks';
import { fetchPortfolio } from '../features/portfolio/portfolioSlice';
import AlertsModal from '../components/ui/AlertsModal';
import StockDetailModal from '../components/ui/StockDetailModal';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';

const LivePriceCell = ({ price }: { price: number }) => {
  const [flash, setFlash] = useState("");

  useEffect(() => {
    setFlash("flash");
    const timer = setTimeout(() => setFlash(""), 500);
    return () => clearTimeout(timer);
  }, [price]);

  return (
    <Typography 
      variant="body2" 
      sx={{ 
        fontWeight: 'bold',
        bgcolor: flash === 'flash' ? 'action.selected' : 'transparent',
        transition: 'background-color 0.1s ease-in-out',
      }}
    >
      ${price.toFixed(2)}
    </Typography>
  );
};

const PnlCell = ({ pnl }: { pnl: number }) => {
  const [flash, setFlash] = useState("");
  const isPositive = pnl >= 0;
  const color = isPositive ? 'success.main' : 'error.main';

  useEffect(() => {
    setFlash("flash");
    const timer = setTimeout(() => setFlash(""), 500);
    return () => clearTimeout(timer);
  }, [pnl]);

  return (
    <Typography 
      variant="body2" 
      sx={{ 
        color, 
        fontWeight: 'bold',
        bgcolor: flash === 'flash' ? 'action.selected' : 'transparent',
        transition: 'background-color 0.1s ease-in-out',
      }}
    >
      {isPositive ? '+' : ''}${pnl.toFixed(2)}
    </Typography>
  );
};

const PortfolioPage = () => {
  const dispatch = useAppDispatch();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { holdings, status } = useAppSelector((state) => state.portfolio);
  const { user } = useAppSelector((state) => state.auth);
  const { prices } = useAppSelector((state) => state.livePrice);
  const [isAlertModalOpen, setIsAlertModalOpen] = useState(false);
  const [selectedTicker, setSelectedTicker] = useState<string | null>(null);
  const [isStockModalOpen, setIsStockModalOpen] = useState(false);

  useEffect(() => {
    if (user && status === 'idle') {
      dispatch(fetchPortfolio());
    }
  }, [status, dispatch, user]);

  const handleOpenAlertModal = (ticker: string) => {
    setSelectedTicker(ticker);
    setIsAlertModalOpen(true);
  };

  const handleCloseAlertModal = () => {
    setIsAlertModalOpen(false);
    setSelectedTicker(null);
  };

  const handleStockClick = (ticker: string) => {
    setSelectedTicker(ticker);
    setIsStockModalOpen(true);
  };

  const handleCloseStockModal = () => {
    setIsStockModalOpen(false);
    setSelectedTicker(null);
  };

  let totalCost = 0;
  let currentTotalValue = 0;

  holdings.forEach(holding => {
    const livePrice = prices[holding.ticker] ?? holding.averageCostPrice;
    totalCost += holding.totalCost;
    currentTotalValue += livePrice * holding.quantity;
  });

  const totalPnl = currentTotalValue - totalCost;
  const totalPnlPercent = totalCost === 0 ? 0 : (totalPnl / totalCost) * 100;
  const pnlColor = totalPnl >= 0 ? 'success.main' : 'error.main';

  if (status === 'pending') {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <TrendingUpIcon sx={{ mr: 1.5, color: '#5c6bc0', fontSize: 28 }} />
        <Typography variant="h5" sx={{ fontWeight: 600 }}>
          My Portfolio
        </Typography>
      </Box>

      {/* Summary Cards */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid size={{xs:12, sm:4}}>
          <Paper sx={{ p: 2.5, textAlign: 'center', backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
            <Typography variant="body2" color="text.secondary">Total Invested</Typography>
            <Typography variant="h5" sx={{ fontWeight: 'bold', mt: 1 }}>
              ${totalCost.toLocaleString('en-US', { minimumFractionDigits: 2 })}
            </Typography>
          </Paper>
        </Grid>
        <Grid size={{xs:12, sm:4}}>
          <Paper sx={{ p: 2.5, textAlign: 'center', backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
            <Typography variant="body2" color="text.secondary">Current Value</Typography>
            <Typography variant="h5" sx={{ fontWeight: 'bold', mt: 1 }}>
              ${currentTotalValue.toLocaleString('en-US', { minimumFractionDigits: 2 })}
            </Typography>
          </Paper>
        </Grid>
        <Grid size={{xs:12, sm:4}}>
          <Paper sx={{ p: 2.5, textAlign: 'center', backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
            <Typography variant="body2" color="text.secondary">Total P&L</Typography>
            <Typography variant="h5" sx={{ fontWeight: 'bold', color: pnlColor, mt: 1 }}>
              {totalPnl >= 0 ? '+' : ''}${totalPnl.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              <Typography component="span" variant="body2" sx={{ ml: 1 }}>
                ({totalPnlPercent.toFixed(2)}%)
              </Typography>
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      {/* Desktop Table View */}
      {!isMobile ? (
        <TableContainer component={Paper} sx={{ backgroundColor: 'rgba(30, 30, 30, 0.6)', border: '1px solid rgba(92, 107, 192, 0.15)' }}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Stock</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Quantity</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Avg Cost</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Total Cost</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Live Price</TableCell>
                <TableCell align="right" sx={{ fontWeight: 600 }}>Total P&L</TableCell>
                <TableCell align="center" sx={{ fontWeight: 600 }}>Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {holdings.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                    You do not own any stocks.
                  </TableCell>
                </TableRow>
              ) : (
                holdings.map((holding) => {
                  const livePrice = prices[holding.ticker] ?? holding.averageCostPrice;
                  const rowPnl = (livePrice - holding.averageCostPrice) * holding.quantity;

                  return (
                    <TableRow key={holding.id} sx={{ '&:hover': { backgroundColor: 'rgba(92, 107, 192, 0.05)' } }}>
                      <TableCell>
                        <Typography 
                          variant="body1" 
                          sx={{ 
                            fontWeight: 'bold',
                            color: '#5c6bc0',
                            cursor: 'pointer',
                            '&:hover': {
                              textDecoration: 'underline',
                            }
                          }}
                          onClick={() => handleStockClick(holding.ticker)}
                        >
                          {holding.ticker}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">{holding.quantity}</TableCell>
                      <TableCell align="right">${holding.averageCostPrice.toFixed(2)}</TableCell>
                      <TableCell align="right">${holding.totalCost.toFixed(2)}</TableCell>
                      <TableCell align="right">
                        <LivePriceCell price={livePrice} />
                      </TableCell>
                      <TableCell align="right">
                        <PnlCell pnl={rowPnl} />
                      </TableCell>
                      <TableCell align="center">
                        <Stack direction="row" spacing={1} justifyContent="center">
                          <Button size="small" variant="outlined" onClick={() => handleOpenAlertModal(holding.ticker)}>
                            Alert
                          </Button>
                          <Button size="small" variant="outlined" color="success">Buy</Button>
                          <Button size="small" variant="outlined" color="error">Sell</Button>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </TableContainer>
      ) : (
        /* Mobile Card View */
        <Box>
          {holdings.length === 0 ? (
            <Paper sx={{ p: 3, textAlign: 'center' }}>
              You do not own any stocks.
            </Paper>
          ) : (
            holdings.map((holding) => {
              const livePrice = prices[holding.ticker] ?? holding.averageCostPrice;
              const rowPnl = (livePrice - holding.averageCostPrice) * holding.quantity;
              const isPnlPositive = rowPnl >= 0;

              return (
                <Card 
                  key={holding.id} 
                  sx={{ 
                    mb: 2, 
                    backgroundColor: 'rgba(30, 30, 30, 0.6)', 
                    border: '1px solid rgba(92, 107, 192, 0.15)',
                    cursor: 'pointer',
                    transition: 'all 0.3s ease',
                    '&:hover': {
                      border: '1px solid rgba(92, 107, 192, 0.3)',
                      transform: 'translateY(-2px)',
                    }
                  }}
                  onClick={() => handleStockClick(holding.ticker)}
                >
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography variant="h6" sx={{ fontWeight: 'bold', color: '#5c6bc0' }}>
                        {holding.ticker}
                      </Typography>
                      <Typography variant="body2" sx={{ color: isPnlPositive ? 'success.main' : 'error.main', fontWeight: 'bold' }}>
                        {isPnlPositive ? '+' : ''}${rowPnl.toFixed(2)}
                      </Typography>
                    </Box>
                    <Grid container spacing={1.5} sx={{ mb: 2 }}>
                      <Grid size={{xs:6}}>
                        <Typography variant="caption" color="text.secondary">Quantity</Typography>
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>{holding.quantity}</Typography>
                      </Grid>
                      <Grid size={{xs:6}} sx={{ textAlign: 'right' }}>
                        <Typography variant="caption" color="text.secondary">Avg Cost</Typography>
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>${holding.averageCostPrice.toFixed(2)}</Typography>
                      </Grid>
                      <Grid size={{xs:6}}>
                        <Typography variant="caption" color="text.secondary">Live Price</Typography>
                        <LivePriceCell price={livePrice} />
                      </Grid>
                      <Grid size={{xs:6}} sx={{ textAlign: 'right' }}>
                        <Typography variant="caption" color="text.secondary">Total Cost</Typography>
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>${holding.totalCost.toFixed(2)}</Typography>
                      </Grid>
                    </Grid>
                    <Stack 
                      direction="row" 
                      spacing={1}
                      onClick={(e) => e.stopPropagation()} // Prevent card click when clicking buttons
                    >
                      <Button size="small" variant="outlined" fullWidth onClick={() => handleOpenAlertModal(holding.ticker)}>
                        Alert
                      </Button>
                      <Button size="small" variant="outlined" color="success" fullWidth>Buy</Button>
                      <Button size="small" variant="outlined" color="error" fullWidth>Sell</Button>
                    </Stack>
                  </CardContent>
                </Card>
              );
            })
          )}
        </Box>
      )}

      {/* Modals */}
      {selectedTicker && (
        <>
          <AlertsModal 
            open={isAlertModalOpen}
            onClose={handleCloseAlertModal}
            ticker={selectedTicker}
          />
          <StockDetailModal 
            ticker={selectedTicker}
            open={isStockModalOpen}
            onClose={handleCloseStockModal}
          />
        </>
      )}
    </Container>
  );
};

export default PortfolioPage;