import { Card, CardContent, Typography, Box } from "@mui/material";
import type { MarketIndexDto } from "../../types/stockTypes";

interface IndexCardProps {
  index: MarketIndexDto;
}

const IndexCard = ({ index }: IndexCardProps) => {
  const isPositive = index.change >= 0;
  const color = isPositive ? "success.main" : "error.main";

  return (
    <Card sx={{ minWidth: 275 }}>
      <CardContent>
        <Typography variant="h6" component="div">
          {index.name}
        </Typography>
        <Typography variant="h5" sx={{ mt: 1, fontWeight: 'bold' }}>
          {index.currentPrice.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', color }}>
          <Typography color="inherit" sx={{ fontSize: '1rem' }}>
            {isPositive ? '+' : ''}{index.change.toFixed(2)}
          </Typography>
          <Typography color="inherit" sx={{ fontSize: '0.9rem' }}>
            ({isPositive ? '+' : ''}{index.percentChange.toFixed(2)}%)
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};

export default IndexCard;