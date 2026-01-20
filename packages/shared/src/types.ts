// Analytics Types
export interface AnalyticsSummary {
  totalOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
  periodStart: string;
  periodEnd: string;
}

export interface Order {
  id: string;
  status: string;
  total: number;
  createdAt: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

// Utility functions
export function formatCurrency(amount: number): string {
  return `$${amount.toLocaleString(undefined, { 
    minimumFractionDigits: 2, 
    maximumFractionDigits: 2 
  })}`;
}
