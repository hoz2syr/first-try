import styles from './SummaryCard.module.css';

interface SummaryCardProps {
  title: string;
  value: string;
  icon: string;
}

export default function SummaryCard({ title, value, icon }: SummaryCardProps) {
  return (
    <div className={styles.card}>
      <div className={styles.icon}>{icon}</div>
      <div className={styles.content}>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.value}>{value}</p>
      </div>
    </div>
  );
}
