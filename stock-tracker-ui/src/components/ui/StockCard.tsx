import { Card, CardActionArea, CardContent, Typography, Box, Chip } from "@mui/material";
import type { StockQuoteCardDto } from "../../types/stockTypes";
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';

interface StockCardProps {
  stock: StockQuoteCardDto;
  onClick: (ticker: string) => void;
}

const StockCard = ({ stock, onClick }: StockCardProps) => {
  const isPositive = stock.change >= 0;

  return (
    <Card
      sx={{
        backgroundColor: 'rgba(30, 30, 30, 0.6)',
        backdropFilter: 'blur(10px)',
        border: '1px solid rgba(92, 107, 192, 0.15)',
        borderRadius: 2,
        transition: 'all 0.3s ease',
        '&:hover': {
          border: '1px solid rgba(92, 107, 192, 0.3)',
          transform: 'translateY(-4px)',
          boxShadow: '0 8px 24px rgba(92, 107, 192, 0.15)',
        }
      }}
    >
      <CardActionArea 
        onClick={() => onClick(stock.ticker)}
        sx={{ 
          p: 2.5,
          height: '100%',
        }}
      >
        <CardContent sx={{ p: 0 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
            <Typography 
              variant="h6" 
              component="div" 
              sx={{ 
                fontWeight: 700,
                color: 'white',
                letterSpacing: '0.5px',
              }}
            >
              {stock.ticker}
            </Typography>
            <Chip
              icon={isPositive ? <TrendingUpIcon sx={{ fontSize: 16 }} /> : <TrendingDownIcon sx={{ fontSize: 16 }} />}
              label={`${isPositive ? '+' : ''}${stock.percentChange.toFixed(2)}%`}
              size="small"
              sx={{
                backgroundColor: isPositive ? 'rgba(76, 175, 80, 0.15)' : 'rgba(244, 67, 54, 0.15)',
                color: isPositive ? '#4caf50' : '#f44336',
                fontWeight: 600,
                border: `1px solid ${isPositive ? 'rgba(76, 175, 80, 0.3)' : 'rgba(244, 67, 54, 0.3)'}`,
                '& .MuiChip-icon': {
                  color: isPositive ? '#4caf50' : '#f44336',
                }
              }}
            />
          </Box>
          
          <Typography 
            variant="h4" 
            sx={{ 
              fontWeight: 700,
              color: 'white',
              mb: 1.5,
              letterSpacing: '-0.5px',
            }}
          >
            ${stock.currentPrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
          </Typography>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography 
              sx={{ 
                fontSize: '0.95rem',
                fontWeight: 600,
                color: isPositive ? '#4caf50' : '#f44336',
              }}
            >
              {isPositive ? '+' : ''}{stock.change.toFixed(2)}
            </Typography>
            <Typography 
              sx={{ 
                fontSize: '0.85rem',
                color: 'rgba(255,255,255,0.6)',
              }}
            >
              today
            </Typography>
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
};

export default StockCard;