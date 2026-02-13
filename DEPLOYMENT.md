# Deep Overflow - Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Environment Configuration](#environment-configuration)
3. [Local Development Setup](#local-development-setup)
4. [Docker Deployment](#docker-deployment)
5. [Kubernetes Production Deployment](#kubernetes-production-deployment)
6. [Database Setup](#database-setup)
7. [Security Configuration](#security-configuration)
8. [Monitoring & Maintenance](#monitoring--maintenance)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software
- **Docker** 24.0+ and Docker Compose 2.0+
- **Kubernetes** 1.28+ (for production)
- **.NET SDK** 8.0+
- **Node.js** 18+ and npm
- **PostgreSQL** 15+ (if not using Docker)
- **Redis** 7+ (if not using Docker)
- **ElasticSearch/OpenSearch** 8+/2+ (if not using Docker)

### Recommended Tools
- kubectl (Kubernetes CLI)
- helm (Kubernetes package manager)
- k9s (Kubernetes terminal UI)
- pgAdmin (PostgreSQL management)
- Redis Commander
- Postman/Insomnia (API testing)

---

## Environment Configuration

### 1. Create Environment Files

Copy the template and fill in your values:
```bash
cp .env.template .env
```

### 2. Required Environment Variables

**Critical Security Variables (MUST CHANGE):**
```bash
# Database
DB_PASSWORD=<generate-strong-password>

# JWT Authentication
JWT_SECRET=<generate-32-char-minimum-secret>

# Encryption
ENCRYPTION_KEY=<generate-256-bit-key>
```

**Generate secure values:**
```bash
# Generate random password
openssl rand -base64 32

# Generate JWT secret (32+ characters)
openssl rand -hex 32
```

### 3. Configuration Files

Update these files with your specific values:

**Backend:** `backend/src/DeepOverflow.API/appsettings.json`
- Database connection string
- Redis connection
- ElasticSearch URI
- JWT settings
- SSO configuration
- Email SMTP settings
- Storage settings

**Frontend:** `frontend/src/environments/environment.prod.ts`
- API URL
- SignalR URL
- Feature flags

---

## Local Development Setup

### Option 1: Docker Compose (Recommended)

**Start all services:**
```bash
docker-compose up -d
```

**Services will be available at:**
- Frontend: http://localhost:4200
- Backend API: http://localhost:5001
- Swagger UI: http://localhost:5001/swagger
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- ElasticSearch: http://localhost:9200

**View logs:**
```bash
docker-compose logs -f
docker-compose logs -f api  # Just API logs
```

**Stop services:**
```bash
docker-compose down
docker-compose down -v  # Also remove volumes
```

### Option 2: Manual Setup

#### 1. Database Setup
```bash
# Create database
createdb deepoverflow

# Run schema
psql -d deepoverflow -f database/schema.sql
```

#### 2. Start Backend
```bash
cd backend/src/DeepOverflow.API

# Restore dependencies
dotnet restore

# Apply migrations (if using EF migrations)
dotnet ef database update

# Run API
dotnet run
```

#### 3. Start Frontend
```bash
cd frontend

# Install dependencies
npm install

# Start dev server
npm start
```

---

## Docker Deployment

### Build Images

**Build backend:**
```bash
docker build -t deepoverflow-api:latest -f docker/Dockerfile.api .
```

**Build frontend:**
```bash
docker build -t deepoverflow-frontend:latest -f docker/Dockerfile.frontend .
```

### Push to Container Registry

```bash
# Tag images
docker tag deepoverflow-api:latest ghcr.io/rmes/deepoverflow-api:latest
docker tag deepoverflow-frontend:latest ghcr.io/rmes/deepoverflow-frontend:latest

# Login to registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Push images
docker push ghcr.io/rmes/deepoverflow-api:latest
docker push ghcr.io/rmes/deepoverflow-frontend:latest
```

---

## Kubernetes Production Deployment

### 1. Prepare Cluster

**Create namespace:**
```bash
kubectl create namespace deepoverflow
```

**Set default namespace:**
```bash
kubectl config set-context --current --namespace=deepoverflow
```

### 2. Create Secrets

Create `k8s/secrets.yaml`:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: deepoverflow-secrets
  namespace: deepoverflow
type: Opaque
stringData:
  db-connection-string: "Host=postgres;Database=deepoverflow;Username=postgres;Password=YOUR_PASSWORD"
  redis-connection-string: "redis:6379"
  jwt-secret: "YOUR_JWT_SECRET_HERE"
```

**Apply secrets:**
```bash
kubectl apply -f k8s/secrets.yaml
```

### 3. Deploy Infrastructure

```bash
# Deploy in order
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/configmaps.yaml

# Deploy databases
kubectl apply -f k8s/postgres.yaml
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/elasticsearch.yaml

# Wait for databases to be ready
kubectl wait --for=condition=ready pod -l app=postgres --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis --timeout=300s
kubectl wait --for=condition=ready pod -l app=elasticsearch --timeout=300s
```

### 4. Deploy Application

```bash
# Deploy API
kubectl apply -f k8s/api-deployment.yaml

# Deploy Frontend
kubectl apply -f k8s/frontend-deployment.yaml

# Deploy Ingress
kubectl apply -f k8s/ingress.yaml
```

### 5. Verify Deployment

```bash
# Check all pods
kubectl get pods -n deepoverflow

# Check services
kubectl get svc -n deepoverflow

# Check ingress
kubectl get ingress -n deepoverflow

# View API logs
kubectl logs -f deployment/deepoverflow-api

# View frontend logs
kubectl logs -f deployment/deepoverflow-frontend
```

### 6. Setup TLS Certificate

**Using cert-manager (recommended):**
```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create ClusterIssuer
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@rmes.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

The ingress is already configured to use cert-manager.

---

## Database Setup

### Initial Schema Setup

**Using Docker:**
```bash
docker exec -i deepoverflow-postgres psql -U postgres -d deepoverflow < database/schema.sql
```

**Direct PostgreSQL:**
```bash
psql -h localhost -U postgres -d deepoverflow -f database/schema.sql
```

### Database Migrations

If using Entity Framework migrations:

```bash
cd backend/src/DeepOverflow.API

# Create migration
dotnet ef migrations add InitialCreate --project ../DeepOverflow.Infrastructure

# Apply migration
dotnet ef database update
```

### Seed Data

Create initial admin user, badges, and sample data:

```sql
-- Create admin user
INSERT INTO users (email, username, display_name, role, is_active, is_email_verified)
VALUES ('admin@rmes.com', 'admin', 'System Admin', 'Admin', true, true);

-- Badge definitions are already created by schema.sql
```

### Database Backup

**Automated backup script:**
```bash
#!/bin/bash
BACKUP_DIR="/backups/deepoverflow"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/deepoverflow_$TIMESTAMP.sql"

# Create backup
docker exec deepoverflow-postgres pg_dump -U postgres deepoverflow > $BACKUP_FILE

# Compress
gzip $BACKUP_FILE

# Delete backups older than 30 days
find $BACKUP_DIR -name "*.sql.gz" -mtime +30 -delete
```

**Add to crontab:**
```bash
0 2 * * * /path/to/backup-script.sh
```

---

## Security Configuration

### 1. SSL/TLS Configuration

**Generate self-signed certificate (development):**
```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout docker/ssl/key.pem \
  -out docker/ssl/cert.pem \
  -subj "/CN=deepoverflow.rmes.com"
```

**Production:** Use Let's Encrypt with cert-manager (see above)

### 2. Firewall Rules

```bash
# Allow HTTPS
ufw allow 443/tcp

# Allow HTTP (for redirect to HTTPS)
ufw allow 80/tcp

# Allow SSH
ufw allow 22/tcp

# Enable firewall
ufw enable
```

### 3. API Rate Limiting

Already configured in `appsettings.json`:
- General: 100 requests/minute
- Auth endpoints: 10 requests/minute

Adjust as needed for your traffic patterns.

### 4. CORS Configuration

Update `appsettings.json` with your allowed origins:
```json
"CORS": {
  "AllowedOrigins": [
    "https://deepoverflow.rmes.com",
    "https://www.deepoverflow.rmes.com"
  ]
}
```

### 5. Database Security

```sql
-- Create read-only user for analytics
CREATE USER deepoverflow_readonly WITH PASSWORD 'secure_password';
GRANT CONNECT ON DATABASE deepoverflow TO deepoverflow_readonly;
GRANT USAGE ON SCHEMA public TO deepoverflow_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO deepoverflow_readonly;

-- Revoke unnecessary permissions
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
```

---

## Monitoring & Maintenance

### Health Checks

**API Health Check:**
```bash
curl https://deepoverflow.rmes.com/health
```

**Expected response:**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

### Logging

**View logs in Kubernetes:**
```bash
# API logs
kubectl logs -f -l app=deepoverflow-api --tail=100

# Frontend logs
kubectl logs -f -l app=deepoverflow-frontend --tail=100

# All logs from namespace
kubectl logs -f -n deepoverflow --all-containers=true
```

**Backend log files (if running locally):**
- Location: `backend/src/DeepOverflow.API/logs/`
- Format: `deepoverflow-YYYYMMDD.txt`
- Retention: Automatic daily rotation

### Monitoring Setup (Recommended)

**Install Prometheus & Grafana:**
```bash
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack -n monitoring --create-namespace
```

**Access Grafana:**
```bash
kubectl port-forward -n monitoring svc/prometheus-grafana 3000:80
# Default credentials: admin/prom-operator
```

### Performance Monitoring

**Database performance:**
```sql
-- Check slow queries
SELECT query, calls, total_time, mean_time
FROM pg_stat_statements
ORDER BY mean_time DESC
LIMIT 10;

-- Check index usage
SELECT schemaname, tablename, indexname, idx_scan, idx_tup_read, idx_tup_fetch
FROM pg_stat_user_indexes
ORDER BY idx_scan ASC;
```

**Redis monitoring:**
```bash
docker exec -it deepoverflow-redis redis-cli INFO stats
docker exec -it deepoverflow-redis redis-cli INFO memory
```

### Scaling

**Scale API horizontally:**
```bash
kubectl scale deployment deepoverflow-api --replicas=5
```

**Or use HPA (already configured):**
```bash
kubectl get hpa -n deepoverflow
```

### Updates & Maintenance

**Rolling update:**
```bash
# Update API
kubectl set image deployment/deepoverflow-api api=ghcr.io/rmes/deepoverflow-api:v2.0.0

# Monitor rollout
kubectl rollout status deployment/deepoverflow-api

# Rollback if needed
kubectl rollout undo deployment/deepoverflow-api
```

**Database maintenance:**
```sql
-- Vacuum and analyze
VACUUM ANALYZE;

-- Reindex
REINDEX DATABASE deepoverflow;

-- Update statistics
ANALYZE;
```

---

## Troubleshooting

### Common Issues

#### 1. API Can't Connect to Database

**Check:**
```bash
kubectl logs deployment/deepoverflow-api | grep -i "database\|postgres"
```

**Solution:**
- Verify database pod is running: `kubectl get pods -l app=postgres`
- Check connection string in secrets
- Verify network policies allow communication

#### 2. Frontend Can't Reach API

**Check:**
```bash
# From inside frontend pod
kubectl exec -it deployment/deepoverflow-frontend -- wget -O- http://deepoverflow-api-service/health
```

**Solution:**
- Verify API service is running: `kubectl get svc deepoverflow-api-service`
- Check CORS configuration
- Verify ingress rules

#### 3. SignalR Connection Fails

**Check:**
- WebSocket support in ingress: Look for `nginx.ingress.kubernetes.io/websocket-services` annotation
- Firewall rules allow WebSocket connections
- Client can establish secure WebSocket (wss://)

#### 4. High Memory Usage

**Check:**
```bash
kubectl top pods -n deepoverflow
```

**Solutions:**
- Increase memory limits in deployment manifests
- Check for memory leaks in application logs
- Enable Redis cache to reduce database load
- Optimize database queries

#### 5. Slow Search Performance

**Solutions:**
- Check ElasticSearch is running: `kubectl get pods -l app=elasticsearch`
- Rebuild search index
- Increase ElasticSearch resources
- Add more search replicas

### Debug Commands

```bash
# Get detailed pod information
kubectl describe pod <pod-name>

# Execute shell in pod
kubectl exec -it deployment/deepoverflow-api -- /bin/bash

# Port forward to local machine
kubectl port-forward deployment/deepoverflow-api 5001:80

# View resource usage
kubectl top pods
kubectl top nodes

# Check events
kubectl get events --sort-by='.lastTimestamp' -n deepoverflow
```

### Emergency Procedures

**Database Recovery:**
```bash
# Stop application
kubectl scale deployment deepoverflow-api --replicas=0

# Restore from backup
cat backup.sql | kubectl exec -i postgres-0 -- psql -U postgres -d deepoverflow

# Start application
kubectl scale deployment deepoverflow-api --replicas=3
```

**Complete System Reset (Development Only):**
```bash
kubectl delete namespace deepoverflow
# Then redeploy from scratch
```

---

## Performance Tuning

### Database Optimization

**PostgreSQL configuration** (`postgresql.conf`):
```ini
# Memory
shared_buffers = 256MB
effective_cache_size = 1GB
work_mem = 16MB

# Connections
max_connections = 200

# WAL
wal_buffers = 16MB
checkpoint_completion_target = 0.9

# Query Planner
random_page_cost = 1.1  # For SSD
effective_io_concurrency = 200
```

### Redis Configuration

```bash
# Set max memory
redis-cli CONFIG SET maxmemory 512mb
redis-cli CONFIG SET maxmemory-policy allkeys-lru
```

### API Performance

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=deepoverflow;Username=postgres;Password=pass;Maximum Pool Size=100;Connection Lifetime=0"
  }
}
```

---

## Support & Contacts

- **Technical Issues**: #deep-overflow-support (Slack)
- **Feature Requests**: deepoverflow-features@rmes.com
- **Security Issues**: security@rmes.com (Private)
- **Documentation**: https://docs.deepoverflow.rmes.internal

---

**Last Updated:** 2026-01-31
**Version:** 1.0.0
