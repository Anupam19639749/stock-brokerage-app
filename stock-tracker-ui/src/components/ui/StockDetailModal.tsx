import { useState, useEffect } from 'react';
import { Modal, Box, Typography, CircularProgress, Grid, Button, Paper, Stack, IconButton, Avatar, Chip } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import { AdvancedRealTimeChart } from 'react-ts-tradingview-widgets';
import http from '../../api/axiosInstance';
import type { CompanyProfileDto } from '../../types/stockTypes';
import type { FinnhubQuoteDto, OrderRequestDto } from '../../types/tradeTypes';
import { useAppDispatch } from '../../hooks/redux-hooks';
import { placeOrder } from '../../features/portfolio/portfolioSlice';
import BuySellModal from './BuySellModal';

const modalStyle = {
  position: 'absolute' as 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: { xs: '95%', md: '80%' },
  maxWidth: '1000px',
  maxHeight: '95vh',
  overflow: 'auto',
  bgcolor: 'background.paper',
  boxShadow: 24,
  p: { xs: 3, md: 4 },
  borderRadius: 3,
};

interface StockDetailModalProps {
  ticker: string | null;
  open: boolean;
  onClose: () => void;
}

const StockDetailModal = ({ ticker, open, onClose }: StockDetailModalProps) => {
  const dispatch = useAppDispatch();
  const [profile, setProfile] = useState<CompanyProfileDto | null>(null);
  const [quote, setQuote] = useState<FinnhubQuoteDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [isBuySellModalOpen, setIsBuySellModalOpen] = useState(false);
  const [orderType, setOrderType] = useState<"BUY" | "SELL">("BUY");

  useEffect(() => {
    if (!ticker || !open) {
      setLoading(false); 
      return;
    }
    setLoading(true);
    const fetchData = async () => {
      setProfile(null);
      setQuote(null);
      try {
        const [profileRes, quoteRes] = await Promise.all([
          http.get<{ data: CompanyProfileDto }>(`/stock/profile/${ticker}`),
          http.get<{ data: FinnhubQuoteDto }>(`/stock/quote/${ticker}`)
        ]);
        setProfile(profileRes.data.data);
        setQuote(quoteRes.data.data);
      } catch (error) {
        console.error("Failed to fetch stock details", error);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [ticker, open]);

  const handleOpenBuyModal = () => {
    setOrderType("BUY");
    setIsBuySellModalOpen(true);
  };

  const handleOpenSellModal = () => {
    setOrderType("SELL");
    setIsBuySellModalOpen(true);
  };

  const handleCloseBuySellModal = () => {
    setIsBuySellModalOpen(false);
  };

  const handleConfirmOrder = (order: OrderRequestDto) => {
    dispatch(placeOrder(order)).then(() => {
      handleCloseBuySellModal();
      onClose();
    });
  };

  // Calculate price change
  const priceChange = quote ? quote.c - quote.pc : 0;
  const priceChangePercent = quote && quote.pc !== 0 ? (priceChange / quote.pc) * 100 : 0;
  const isPositive = priceChange >= 0;

  const renderQuoteData = () => {
    if (!quote) return null;
    
    const quoteItems = [
      { label: 'Open', value: quote.o },
      { label: 'High', value: quote.h },
      { label: 'Low', value: quote.l },
      { label: 'Prev. Close', value: quote.pc },
    ];

    return (
      <Grid container spacing={2} sx={{ mt: 1.5 }}>
        {quoteItems.map((item, index) => (
          <Grid size={{xs:6, sm:3}} key={index}>
            <Paper 
              sx={{ 
                p: 2, 
                textAlign: 'center',
                backgroundColor: 'rgba(30, 30, 30, 0.4)',
                border: '1px solid rgba(92, 107, 192, 0.15)',
              }}
            >
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
                {item.label}
              </Typography>
              <Typography variant="body1" sx={{ fontWeight: 600 }}>
                ${item.value?.toFixed(2) ?? '---'}
              </Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>
    );
  };

  return (
    <>
      <Modal open={open} onClose={onClose}>
        <Box sx={modalStyle}>
          <IconButton 
            onClick={onClose} 
            sx={{ 
              position: 'absolute', 
              top: 8, 
              right: 8,
              zIndex: 1,
              backgroundColor: 'rgba(0,0,0,0.3)',
              '&:hover': {
                backgroundColor: 'rgba(0,0,0,0.5)',
              }
            }}
          >
            <CloseIcon />
          </IconButton>

          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              {/* Header Section */}
              <Box sx={{ mb: 3 }}>
                <Box sx={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'space-between',
                  flexWrap: { xs: 'wrap', md: 'nowrap' },
                  gap: 2,
                }}>
                  {/* Left Side - Company Info */}
                  <Box sx={{ display: 'flex', alignItems: 'center', minWidth: 0 }}>
                    {profile?.logoUrl && (
                      <Avatar 
                        src={profile.logoUrl} 
                        sx={{ 
                          width: 48, 
                          height: 48, 
                          mr: 2,
                          border: '2px solid rgba(92, 107, 192, 0.3)',
                        }} 
                      />
                    )}
                    <Box sx={{ minWidth: 0 }}>
                      <Typography variant="h6" sx={{ fontWeight: 700 }}>
                        {ticker}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" noWrap>
                        {profile?.name || 'Loading...'}
                      </Typography>
                      {profile?.industry && (
                        <Chip 
                          label={profile.industry} 
                          size="small" 
                          sx={{ 
                            mt: 0.5,
                            backgroundColor: 'rgba(92, 107, 192, 0.15)',
                            color: '#5c6bc0',
                            fontSize: '0.7rem',
                            height: '20px',
                          }} 
                        />
                      )}
                    </Box>
                  </Box>

                  {/* Right Side - Price Info */}
                  <Paper 
                    sx={{ 
                      p: 2, 
                      backgroundColor: 'rgba(30, 30, 30, 0.4)',
                      border: '1px solid rgba(92, 107, 192, 0.15)',
                      minWidth: { xs: '100%', md: 'auto' },
                    }}
                  >
                    <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1.5 }}>
                      <Typography variant="h4" sx={{ fontWeight: 700 }}>
                        ${quote?.c?.toFixed(2) ?? '---'}
                      </Typography>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        {isPositive ? (
                          <TrendingUpIcon sx={{ color: 'success.main', fontSize: 20 }} />
                        ) : (
                          <TrendingDownIcon sx={{ color: 'error.main', fontSize: 20 }} />
                        )}
                        <Box>
                          <Typography 
                            variant="body1" 
                            sx={{ 
                              color: isPositive ? 'success.main' : 'error.main',
                              fontWeight: 600,
                            }}
                          >
                            {isPositive ? '+' : ''}{priceChange.toFixed(2)}
                          </Typography>
                          <Typography 
                            variant="caption" 
                            sx={{ 
                              color: isPositive ? 'success.main' : 'error.main',
                              display: 'block',
                            }}
                          >
                            ({isPositive ? '+' : ''}{priceChangePercent.toFixed(2)}%)
                          </Typography>
                        </Box>
                      </Box>
                    </Box>
                  </Paper>
                </Box>
              </Box>

              {/* Chart Section */}
              <Paper 
                sx={{ 
                  height: '320px', 
                  mb: 3,
                  overflow: 'hidden',
                  backgroundColor: 'rgba(30, 30, 30, 0.4)',
                  border: '1px solid rgba(92, 107, 192, 0.15)',
                }}
              >
                {ticker && (
                  <AdvancedRealTimeChart symbol={ticker} theme="dark" autosize />
                )}
              </Paper>
              
              {/* Quote Data Cards */}
              {renderQuoteData()}

              {/* Action Buttons */}
              <Stack direction="row" spacing={2} sx={{ mt: 3 }}>
                <Button 
                  variant="contained" 
                  color="success" 
                  fullWidth 
                  onClick={handleOpenBuyModal}
                  sx={{ 
                    py: 1.5,
                    fontWeight: 600,
                    fontSize: '1rem',
                  }}
                >
                  Buy {ticker}
                </Button>
                <Button 
                  variant="contained" 
                  color="error" 
                  fullWidth 
                  onClick={handleOpenSellModal}
                  sx={{ 
                    py: 1.5,
                    fontWeight: 600,
                    fontSize: '1rem',
                  }}
                >
                  Sell {ticker}
                </Button>
              </Stack>
            </>
          )}
        </Box>
      </Modal>

      {ticker && quote && (
        <BuySellModal
          open={isBuySellModalOpen}
          onClose={handleCloseBuySellModal}
          orderType={orderType}
          ticker={ticker}
          livePrice={quote.c}
          onSubmit={handleConfirmOrder}
        />
      )}
    </>
  );
};

export default StockDetailModal;