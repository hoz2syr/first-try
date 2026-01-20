# Analytics Dashboard Monorepo

A full-stack monorepo project featuring an analytics dashboard with Next.js frontend and Express backend, powered by PostgreSQL and Prisma ORM.

## 🏗️ Project Structure

```
first-try/
├── apps/
│   ├── web/                 # Next.js frontend (Dashboard)
│   │   ├── src/
│   │   │   ├── app/        # Next.js 14 App Router pages
│   │   │   └── components/ # React components
│   │   ├── Dockerfile      # Production-ready web container
│   │   ├── package.json
│   │   └── tsconfig.json
│   └── api/                 # Express backend
│       ├── src/
│       │   ├── routes/     # API routes
│       │   ├── db.ts       # Prisma client
│       │   └── index.ts    # Server entry point
│       ├── prisma/
│       │   └── schema.prisma
│       ├── Dockerfile      # Production-ready API container
│       ├── package.json
│       └── tsconfig.json
├── packages/
│   └── shared/              # Shared types and utilities
│       ├── src/
│       │   ├── types.ts    # Shared TypeScript types
│       │   └── index.ts
│       ├── package.json
│       └── tsconfig.json
├── docker-compose.yml       # Docker orchestration
├── package.json             # Root workspace configuration
└── tsconfig.json           # Base TypeScript config
```

## 🚀 Features

- **Monorepo Setup**: npm workspaces for efficient dependency management
- **Frontend**: Next.js 14 with App Router, TypeScript, and React
- **Backend**: Express.js REST API with TypeScript
- **Database**: PostgreSQL with Prisma ORM
- **Docker Support**: Full containerization with multi-stage builds
- **Shared Code**: Common types and utilities across apps
- **Analytics Dashboard**: 
  - Summary cards displaying key metrics
  - Chart placeholder for data visualization
  - Real-time data fetching from backend API

## 📋 Prerequisites

### For Docker Setup (Recommended)
- **Docker** >= 20.10
- **Docker Compose** >= 2.0

### For Local Development

- **Node.js** >= 18.0.0
- **npm** >= 9.0.0
- **PostgreSQL** >= 14.0

## 🐳 Quick Start with Docker (Recommended)

The easiest way to run the entire stack is using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/hoz2syr/first-try.git
cd first-try

# Start all services (database, API, and web)
docker compose up --build
```

That's it! The services will be available at:
- **Frontend Dashboard**: http://localhost:3000
- **Backend API**: http://localhost:3001
- **API Health Check**: http://localhost:3001/health
- **Analytics Endpoint**: http://localhost:3001/api/analytics/summary
- **PostgreSQL**: localhost:5432

### Docker Commands

```bash
# Start all services in detached mode
docker compose up -d

# View logs
docker compose logs -f

# Stop all services
docker compose down

# Stop and remove volumes (deletes database data)
docker compose down -v

# Rebuild and restart a specific service
docker compose up --build api
docker compose up --build web

# Access database shell
docker compose exec db psql -U postgres -d analytics_db

# Run Prisma Studio (database GUI)
docker compose exec api npx prisma studio
```

### Docker Architecture

The Docker setup includes:

1. **PostgreSQL Container** (`db`):
   - Image: `postgres:16-alpine`
   - Port: `5432`
   - Data persisted in Docker volume `postgres_data`
   - Health checks ensure database is ready before API starts

2. **API Container** (`api`):
   - Multi-stage build for optimized image size
   - Runs as non-root user for security
   - Automatic database migrations on startup
   - Port: `3001`
   - Environment variables:
     - `DATABASE_URL`: `postgresql://postgres:postgres@db:5432/analytics_db?schema=public`
     - `PORT`: `3001`
     - `NODE_ENV`: `production`

3. **Web Container** (`web`):
   - Next.js standalone output for optimal performance
   - Runs as non-root user for security
   - Port: `3000`
   - Environment variables:
     - `NEXT_PUBLIC_API_URL`: `http://localhost:3001` (for browser access to API)

### Port Configuration

The following ports are exposed:

| Service    | Internal Port | External Port | Description           |
|------------|---------------|---------------|-----------------------|
| Web (Next.js) | 3000       | 3000          | Frontend dashboard    |
| API (Express) | 3001       | 3001          | Backend REST API      |
| Database (PostgreSQL) | 5432 | 5432     | PostgreSQL database   |

**Note**: The web app uses `NEXT_PUBLIC_API_URL=http://localhost:3001` so that client-side JavaScript in the browser can access the API through the exposed port.


- **Node.js** >= 18.0.0
- **npm** >= 9.0.0
- **PostgreSQL** >= 14.0

## ⚙️ Setup Instructions

### 1. Clone and Install Dependencies

```bash
# Clone the repository
git clone https://github.com/hoz2syr/first-try.git
cd first-try

# Install all dependencies
npm install
```

### 2. Database Setup

#### Create PostgreSQL Database

```bash
# Using psql
psql -U postgres
CREATE DATABASE analytics_db;
\q
```

Or use your preferred PostgreSQL client.

#### Configure Environment Variables

**Backend (apps/api/.env)**:
```bash
cp apps/api/.env.example apps/api/.env
```

Edit `apps/api/.env`:
```env
DATABASE_URL="postgresql://user:password@localhost:5432/analytics_db?schema=public"
PORT=3001
NODE_ENV=development
```

**Frontend (apps/web/.env.local)**:
```bash
cp apps/web/.env.example apps/web/.env.local
```

Edit `apps/web/.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:3001
```

#### Run Database Migrations

```bash
# Generate Prisma client
npm run prisma:generate

# Create database tables
npm run prisma:migrate
# When prompted, give the migration a name like "init"
```

### 3. Start Development Servers

You can start all services at once or individually:

#### Option 1: Start All Services
```bash
npm run dev
```

#### Option 2: Start Services Individually

**Terminal 1 - Backend API**:
```bash
npm run dev:api
# Runs on http://localhost:3001
```

**Terminal 2 - Frontend**:
```bash
npm run dev:web
# Runs on http://localhost:3000
```

### 4. Access the Application

- **Frontend Dashboard**: http://localhost:3000
- **Backend API**: http://localhost:3001
- **API Health Check**: http://localhost:3001/health
- **Analytics Endpoint**: http://localhost:3001/api/analytics/summary

## 📦 Available Scripts

### Root Level Scripts

```bash
# Development
npm run dev              # Start all apps in development mode
npm run dev:api          # Start only the API server
npm run dev:web          # Start only the web app

# Build
npm run build            # Build all apps
npm run build:api        # Build only the API
npm run build:web        # Build only the web app

# Linting
npm run lint             # Lint all workspaces

# Prisma (Database)
npm run prisma:generate  # Generate Prisma client
npm run prisma:migrate   # Run database migrations
npm run prisma:studio    # Open Prisma Studio (Database GUI)
```

## 🗄️ Database Schema

The project includes an `Order` model as an example:

```prisma
model Order {
  id        String   @id @default(uuid())
  status    String   // e.g., "pending", "completed", "cancelled"
  total     Float    // Total order amount
  createdAt DateTime @default(now())
  updatedAt DateTime @updatedAt
}
```

### Adding Sample Data

You can use Prisma Studio to add sample orders:

```bash
npm run prisma:studio
```

Or create a seed script to populate sample data.

## 🧪 API Endpoints

### Analytics

#### GET `/api/analytics/summary`

Returns analytics summary with metrics:

**Response**:
```json
{
  "success": true,
  "data": {
    "totalOrders": 1247,
    "totalRevenue": 52348.76,
    "averageOrderValue": 41.98,
    "periodStart": "2024-01-01T00:00:00.000Z",
    "periodEnd": "2024-01-31T23:59:59.999Z"
  }
}
```

## 🔧 Development Tips

### Working with Prisma

```bash
# After modifying schema.prisma, always run:
npm run prisma:generate

# To create a new migration:
npm run prisma:migrate

# To reset the database (careful - deletes all data):
cd apps/api
npx prisma migrate reset
```

### Adding New Shared Types

1. Add types to `packages/shared/src/types.ts`
2. Export them in `packages/shared/src/index.ts`
3. Build the shared package: `npm run build -w packages/shared`
4. Use in apps: `import { YourType } from '@first-try/shared'`

### Hot Reload

- The API uses `ts-node-dev` for automatic restart on file changes
- Next.js has built-in Fast Refresh for React components
- Shared package changes require a rebuild

## 📝 Project Configuration

- **TypeScript**: Strict mode enabled with shared base configuration
- **npm Workspaces**: Efficient dependency management and linking
- **Prisma**: Type-safe database access with auto-generated client
- **Next.js**: App Router with React Server Components
- **Express**: RESTful API with CORS enabled

## 🛠️ Tech Stack

- **Frontend**: Next.js 14, React 18, TypeScript
- **Backend**: Node.js, Express, TypeScript
- **Database**: PostgreSQL, Prisma ORM
- **Package Manager**: npm with workspaces
- **Dev Tools**: ts-node-dev, eslint, prettier-ready

## 📚 Additional Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [Prisma Documentation](https://www.prisma.io/docs)
- [Express Documentation](https://expressjs.com/)
- [npm Workspaces](https://docs.npmjs.com/cli/v8/using-npm/workspaces)

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Submit a pull request

## 📄 License

MIT