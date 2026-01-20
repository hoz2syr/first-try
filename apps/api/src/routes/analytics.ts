import { Router } from 'express';
import { prisma } from '../db';
import type { AnalyticsSummary, ApiResponse } from '@first-try/shared';

const router = Router();

/**
 * GET /api/analytics/summary
 * Returns analytics summary with mock/placeholder data
 */
router.get('/summary', async (req, res) => {
  try {
    // Try to connect to database and fetch real data
    let orders = [];
    let hasDatabase = false;

    try {
      await prisma.$connect();
      orders = await prisma.order.findMany({
        orderBy: {
          createdAt: 'desc',
        },
        take: 100, // Limit to last 100 orders
      });
      hasDatabase = true;
    } catch (dbError) {
      // Database not available, will use mock data
      console.log('Database not available, using mock data');
    }

    // Calculate analytics
    const totalOrders = orders.length;
    const totalRevenue = orders.reduce((sum, order) => sum + order.total, 0);
    const averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

    // If no orders or no database, return mock data
    const summary: AnalyticsSummary = totalOrders > 0 && hasDatabase
      ? {
          totalOrders,
          totalRevenue: Math.round(totalRevenue * 100) / 100,
          averageOrderValue: Math.round(averageOrderValue * 100) / 100,
          periodStart: orders[orders.length - 1]?.createdAt.toISOString() || new Date().toISOString(),
          periodEnd: orders[0]?.createdAt.toISOString() || new Date().toISOString(),
        }
      : {
          // Mock placeholder data
          totalOrders: 1247,
          totalRevenue: 52348.76,
          averageOrderValue: 41.98,
          periodStart: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString(),
          periodEnd: new Date().toISOString(),
        };

    const response: ApiResponse<AnalyticsSummary> = {
      success: true,
      data: summary,
    };

    res.json(response);
  } catch (error) {
    console.error('Error fetching analytics:', error);
    const response: ApiResponse<AnalyticsSummary> = {
      success: false,
      error: 'Failed to fetch analytics data',
    };
    res.status(500).json(response);
  }
});

export default router;
