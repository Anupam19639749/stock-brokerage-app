import { useState, useEffect, useMemo } from "react";
import { Typography, Box, Grid, CircularProgress, Button, TextField, Autocomplete, Container, Paper } from "@mui/material";
import http from "../api/axiosInstance";
import type { StockQuoteCardDto, StockSearchResultDto } from "../types/stockTypes";
import StockCard from "../components/ui/StockCard";
import StockDetailModal from "../components/ui/StockDetailModal";
import { toast } from "react-toastify";
import { SymbolOverview } from "react-ts-tradingview-widgets";
import SearchIcon from '@mui/icons-material/Search';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';

const HomePage = () => {
  const [stocks, setStocks] = useState<StockQuoteCardDto[]>([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);

  // --- Search State ---
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<StockSearchResultDto[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [selectedStock, setSelectedStock] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Memoize charts to prevent reloading
  const spyChart = useMemo(() => (
    <Paper sx={{ height: 270, p: 1 }}> 
      <SymbolOverview
        colorTheme="dark"
        width="100%"
        height="100%"
        symbols={[["S&P 500", "SPY|1D"]]}
        chartOnly={false} 
        dateFormat={"dd MMM 'yy"}  
      />
    </Paper>
  ), []);

  const diaChart = useMemo(() => (
    <Paper sx={{ height: 270, p: 1 }}>
      <SymbolOverview
        colorTheme="dark"
        width="100%"
        height="100%"
        symbols={[["Dow Jones", "DIA|1D"]]}
        chartOnly={false} 
        dateFormat={"dd MMM 'yy"}          
      />
    </Paper>
  ), []);

  // Fetch initial data (Page 1 of stocks)
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const stockRes = await http.get<{ data: StockQuoteCardDto[] }>(`/stock/market-overview?page=1&limit=6`);
        setStocks(stockRes.data.data);
      } catch (error) {
        toast.error("Failed to fetch market data.");
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  // Fetch new search results when user stops typing
  useEffect(() => {
    if (searchQuery.length < 1) {
      setSearchResults([]);
      return;
    }

    setSearchLoading(true);
    const delayDebounceFn = setTimeout(async () => {
      try {
        const response = await http.get<{ data: StockSearchResultDto[] }>(`/stock/search?query=${searchQuery}`);
        setSearchResults(response.data.data);
      } catch (error) {
        console.error("Search failed", error);
      }
      setSearchLoading(false);
    }, 500);

    return () => clearTimeout(delayDebounceFn);
  }, [searchQuery]);

  // Handle "Show More" button click
  const handleShowMore = async () => {
    setLoadingMore(true);
    const nextPage = page + 1;
    try {
      const stockRes = await http.get<{ data: StockQuoteCardDto[] }>(`/stock/market-overview?page=${nextPage}&limit=6`);
      setStocks((prevStocks) => [...prevStocks, ...stockRes.data.data]);
      setPage(nextPage);
    } catch (error) {
      toast.error("Failed to fetch more stocks.");
    } finally {
      setLoadingMore(false);
    }
  };

  // Handle what happens when a user clicks a stock
  const handleStockClick = (ticker: string) => {
    setSelectedStock(ticker);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedStock(null);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Container maxWidth="lg">
      {/* --- 1. Market Overview Section --- */}
      <Box sx={{ mb: 6 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <TrendingUpIcon sx={{ mr: 1.5, color: '#5c6bc0', fontSize: 28 }} />
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Market Overview
          </Typography>
        </Box>
        <Grid container spacing={3}>
          <Grid size={{xs:12, sm:6}}>
            {spyChart}
          </Grid>
          <Grid size={{xs:12, sm:6}}>
            {diaChart}
          </Grid>
        </Grid>
      </Box>

      {/* --- 2. SEARCH BAR (Autocomplete) --- */}
      <Box sx={{ mb: 6, display: 'flex', justifyContent: 'center' }}>
        <Autocomplete
          sx={{ 
            width: '100%', 
            maxWidth: '700px',
          }}
          options={searchResults}
          loading={searchLoading}
          getOptionLabel={(option) => `${option.ticker} - ${option.description}`}
          onChange={(_event, value) => {
            if (value) handleStockClick(value.ticker);
          }}
          onInputChange={(_event, newInputValue) => {
            setSearchQuery(newInputValue);
          }}
          renderInput={(params) => (
            <TextField
              {...params}
              placeholder="Search stocks (e.g., 'AAPL', 'Tesla')"
              sx={{
                '& .MuiOutlinedInput-root': {
                  backgroundColor: 'rgba(30, 30, 30, 0.6)',
                  backdropFilter: 'blur(10px)',
                  borderRadius: 3,
                  paddingLeft: 1,
                  transition: 'all 0.3s ease',
                  '& fieldset': { 
                    borderColor: 'rgba(92, 107, 192, 0.2)',
                  },
                  '&:hover fieldset': { 
                    borderColor: 'rgba(92, 107, 192, 0.4)',
                  },
                  '&.Mui-focused fieldset': { 
                    borderColor: '#5c6bc0',
                  },
                },
                '& .MuiInputLabel-root': { 
                  color: 'rgba(255,255,255,0.6)',
                },
              }}
              InputProps={{
                ...params.InputProps,
                startAdornment: (
                  <SearchIcon sx={{ color: 'rgba(255,255,255,0.5)', mr: 1, ml: 1 }} />
                ),
                endAdornment: (
                  <>
                    {searchLoading ? <CircularProgress color="inherit" size={20} /> : null}
                    {params.InputProps.endAdornment}
                  </>
                ),
              }}
            />
          )}
        />
      </Box>
      
      {/* Explore the Market Section */}
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Typography variant="h5" sx={{ fontWeight: 600 }}>
            Explore the Market
          </Typography>
          <Typography 
            variant="body2" 
            sx={{ 
              color: 'rgba(255,255,255,0.6)',
              fontWeight: 500,
            }}
          >
            {stocks.length} stocks
          </Typography>
        </Box>
        <Grid container spacing={2.5}>
          {stocks.map((stock) => (
            <Grid size={{xs:12, sm:6, md:4}} key={stock.ticker}>
              <StockCard stock={stock} onClick={handleStockClick} />
            </Grid>
          ))}
        </Grid>

        {/* Show More Button */}
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
          <Button 
            variant="outlined"
            onClick={handleShowMore}
            disabled={loadingMore}
            sx={{
              px: 4,
              py: 1.2,
              borderRadius: 2,
              borderColor: 'rgba(92, 107, 192, 0.4)',
              color: '#5c6bc0',
              fontWeight: 600,
              textTransform: 'none',
              fontSize: '0.95rem',
              transition: 'all 0.3s ease',
              '&:hover': {
                borderColor: '#5c6bc0',
                backgroundColor: 'rgba(92, 107, 192, 0.08)',
                transform: 'translateY(-2px)',
              },
              '&:disabled': {
                borderColor: 'rgba(92, 107, 192, 0.2)',
                color: 'rgba(92, 107, 192, 0.5)',
              }
            }}
          >
            {loadingMore ? <CircularProgress size={24} sx={{ color: '#5c6bc0' }} /> : "Show More Stocks"}
          </Button>
        </Box>
      </Box>

      {/* --- 5. MODAL --- */}
      <StockDetailModal 
        ticker={selectedStock} 
        open={isModalOpen} 
        onClose={handleCloseModal} 
      />
    </Container>
  );
};

export default HomePage;