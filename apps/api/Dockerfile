# Stage 1: Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Copy workspace configuration
COPY package*.json ./
COPY tsconfig.json ./

# Copy shared package
COPY packages/shared ./packages/shared

# Copy API app
COPY apps/api ./apps/api

# Install dependencies
RUN npm ci

# Generate Prisma client
RUN cd apps/api && npx prisma generate

# Build the API
RUN npm run build:api

# Stage 2: Production stage
FROM node:18-alpine AS runner

WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 apiuser

# Copy built artifacts and dependencies
COPY --from=builder /app/package*.json ./
COPY --from=builder /app/apps/api/package.json ./apps/api/
COPY --from=builder /app/packages/shared/package.json ./packages/shared/
COPY --from=builder /app/apps/api/dist ./apps/api/dist
COPY --from=builder /app/apps/api/node_modules ./apps/api/node_modules
COPY --from=builder /app/apps/api/prisma ./apps/api/prisma
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/packages/shared ./packages/shared

# Change ownership to non-root user
RUN chown -R apiuser:nodejs /app

USER apiuser

EXPOSE 3001

ENV NODE_ENV=production

# Run database migrations and start the server
CMD ["sh", "-c", "cd apps/api && npx prisma migrate deploy && npm start"]
