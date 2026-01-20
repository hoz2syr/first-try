# Docker Quick Reference

This guide provides quick commands for working with the Docker setup.

## Production Deployment

### Start All Services
```bash
docker compose up --build
```

### Start in Background
```bash
docker compose up -d --build
```

### Stop All Services
```bash
docker compose down
```

### View Logs
```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f api
docker compose logs -f web
docker compose logs -f db
```

### Rebuild Specific Service
```bash
docker compose up --build api
docker compose up --build web
```

## Development Mode

### Start Development Services
```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up
```

### Start Specific Dev Services
```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up web api
```

## Database Management

### Access Database Shell
```bash
docker compose exec db psql -U postgres -d analytics_db
```

### Run Prisma Studio
```bash
docker compose exec api npx prisma studio
```

### Reset Database (WARNING: Deletes all data)
```bash
docker compose down -v
docker compose up -d
```

### Backup Database
```bash
docker compose exec db pg_dump -U postgres analytics_db > backup.sql
```

### Restore Database
```bash
cat backup.sql | docker compose exec -T db psql -U postgres -d analytics_db
```

## Troubleshooting

### Rebuild from Scratch
```bash
docker compose down -v
docker compose build --no-cache
docker compose up
```

### Check Service Health
```bash
docker compose ps
```

### Access Container Shell
```bash
# API container
docker compose exec api sh

# Web container
docker compose exec web sh

# Database container
docker compose exec db sh
```

### Clean Up Everything
```bash
# Stop and remove containers, networks, volumes
docker compose down -v

# Remove all unused Docker resources
docker system prune -a --volumes
```

## Environment Variables

### Default Values (Production)
- **API_PORT**: 3001
- **WEB_PORT**: 3000
- **DB_PORT**: 5432
- **POSTGRES_USER**: postgres
- **POSTGRES_PASSWORD**: postgres
- **POSTGRES_DB**: analytics_db

### Custom Environment Variables
Create a `.env` file in the root directory:
```env
POSTGRES_PASSWORD=your_secure_password
API_PORT=4000
WEB_PORT=8080
```

Then start with:
```bash
docker compose --env-file .env up
```

## Port Mappings

| Service | Container Port | Host Port | URL |
|---------|---------------|-----------|-----|
| Web     | 3000          | 3000      | http://localhost:3000 |
| API     | 3001          | 3001      | http://localhost:3001 |
| Database| 5432          | 5432      | localhost:5432 |

## Common Issues

### Port Already in Use
```bash
# Find process using port 3000
lsof -i :3000

# Or use netstat
netstat -tulpn | grep 3000

# Kill the process or change port in docker-compose.yml
```

### Permission Denied
```bash
# On Linux, you might need to add your user to docker group
sudo usermod -aG docker $USER

# Then logout and login again
```

### Database Connection Failed
1. Wait for database health check to pass
2. Check logs: `docker compose logs db`
3. Verify DATABASE_URL in API service

## Performance Tips

1. **Use Build Cache**: Don't use `--no-cache` unless necessary
2. **Limit Resources**: Use `docker compose up --scale api=2` for load balancing
3. **Clean Unused Images**: Run `docker image prune -a` periodically
4. **Monitor Resources**: Use `docker stats` to see resource usage

## Security Best Practices

1. **Change Default Passwords**: Update POSTGRES_PASSWORD in production
2. **Use Secrets**: For production, use Docker secrets or environment variables
3. **Network Isolation**: Containers communicate via internal Docker network
4. **Non-Root Users**: All services run as non-root users inside containers
5. **Regular Updates**: Keep base images updated

## Production Deployment

For production deployment, consider:

1. **Use a reverse proxy** (nginx, traefik) for SSL termination
2. **Set up health checks** for monitoring
3. **Use Docker Swarm or Kubernetes** for orchestration
4. **Implement backup strategy** for database
5. **Configure resource limits** in docker-compose.yml
6. **Use environment-specific configs** (.env.production, .env.staging)

Example resource limits:
```yaml
api:
  deploy:
    resources:
      limits:
        cpus: '0.5'
        memory: 512M
      reservations:
        cpus: '0.25'
        memory: 256M
```
