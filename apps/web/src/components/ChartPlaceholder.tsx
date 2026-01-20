import styles from './ChartPlaceholder.module.css';

export default function ChartPlaceholder() {
  return (
    <div className={styles.chartContainer}>
      <h2 className={styles.chartTitle}>Revenue Over Time</h2>
      <div className={styles.chartPlaceholder}>
        <div className={styles.chartBars}>
          <div className={styles.bar} style={{ height: '60%' }}></div>
          <div className={styles.bar} style={{ height: '75%' }}></div>
          <div className={styles.bar} style={{ height: '45%' }}></div>
          <div className={styles.bar} style={{ height: '90%' }}></div>
          <div className={styles.bar} style={{ height: '70%' }}></div>
          <div className={styles.bar} style={{ height: '85%' }}></div>
          <div className={styles.bar} style={{ height: '65%' }}></div>
        </div>
        <p className={styles.chartLabel}>Chart visualization placeholder</p>
      </div>
    </div>
  );
}
