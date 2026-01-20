import type { Metadata } from 'next'
import './globals.css'

export const metadata: Metadata = {
  title: 'Analytics Dashboard',
  description: 'Dashboard for viewing analytics and metrics',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  )
}
