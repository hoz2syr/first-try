'use client';

import { useEffect, useState } from 'react';
import type { AnalyticsSummary, ApiResponse } from '@first-try/shared';
import SummaryCard from '@/components/SummaryCard';
import ChartPlaceholder from '@/components/ChartPlaceholder';
import styles from './page.module.css';

export default function Home() {
  const [analytics, setAnalytics] = useState<AnalyticsSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001';
        const response = await fetch(`${apiUrl}/api/analytics/summary`);
        const data: ApiResponse<AnalyticsSummary> = await response.json();

        if (data.success && data.data) {
          setAnalytics(data.data);
        } else {
          setError(data.error || 'Failed to fetch analytics');
        }
      } catch (err) {
        setError('Failed to connect to API server');
        console.error('Error fetching analytics:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  if (loading) {
    return (
      <main className={styles.main}>
        <div className={styles.container}>
          <h1 className={styles.title}>Analytics Dashboard</h1>
          <p className={styles.loading}>Loading analytics...</p>
        </div>
      </main>
    );
  }

  if (error) {
    return (
      <main className={styles.main}>
        <div className={styles.container}>
          <h1 className={styles.title}>Analytics Dashboard</h1>
          <p className={styles.error}>{error}</p>
          <p className={styles.errorHint}>
            Make sure the API server is running on port 3001
          </p>
        </div>
      </main>
    );
  }

  return (
    <main className={styles.main}>
      <div className={styles.container}>
        <h1 className={styles.title}>Analytics Dashboard</h1>

        <div className={styles.grid}>
          <SummaryCard
            title="Total Orders"
            value={analytics?.totalOrders.toLocaleString() || '0'}
            icon="📦"
          />
          <SummaryCard
            title="Total Revenue"
            value={`$${analytics?.totalRevenue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || '0.00'}`}
            icon="💰"
          />
          <SummaryCard
            title="Average Order Value"
            value={`$${analytics?.averageOrderValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || '0.00'}`}
            icon="📊"
          />
          <SummaryCard
            title="Period"
            value={analytics ? `${new Date(analytics.periodStart).toLocaleDateString()} - ${new Date(analytics.periodEnd).toLocaleDateString()}` : 'N/A'}
            icon="📅"
          />
        </div>

        <ChartPlaceholder />
      </div>
    </main>
  );
}
