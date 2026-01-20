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
  createdAt: Date;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}
