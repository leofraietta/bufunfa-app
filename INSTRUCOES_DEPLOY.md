# 🚀 Instruções de Deploy - Projeto Bufunfa

**Versão:** 3.0  
**Data:** Agosto 2025  
**Status:** Pronto para Produção

## 📋 Pré-requisitos

### **Ambiente de Desenvolvimento**
- .NET SDK 8.0+
- Node.js 18+
- PostgreSQL 13+
- Git

### **Ambiente de Produção**
- Servidor Linux/Windows
- PostgreSQL configurado
- Domínio configurado
- SSL/TLS configurado

## 🗄️ Configuração do Banco de Dados

### **1. Instalar PostgreSQL**
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install postgresql postgresql-contrib

# Configurar usuário
sudo -u postgres psql
CREATE DATABASE bufunfa_db;
CREATE USER bufunfa_user WITH PASSWORD 'sua_senha_segura';
GRANT ALL PRIVILEGES ON DATABASE bufunfa_db TO bufunfa_user;
\q
```

### **2. Configurar String de Conexão**
```json
// backend/Bufunfa.Api/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bufunfa_db;Username=bufunfa_user;Password=sua_senha_segura"
  }
}
```

### **3. Aplicar Migrações**
```bash
cd backend/Bufunfa.Api
dotnet ef database update
```

## 🔐 Configuração de Autenticação

### **1. Google OAuth Setup**
1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione existente
3. Ative a API Google+ 
4. Configure OAuth 2.0:
   - **Authorized redirect URIs:** `https://seudominio.com/signin-google`
   - **Authorized JavaScript origins:** `https://seudominio.com`

### **2. Configurar Credenciais**
```json
// backend/Bufunfa.Api/appsettings.json
{
  "Authentication": {
    "Google": {
      "ClientId": "seu_google_client_id",
      "ClientSecret": "seu_google_client_secret"
    }
  },
  "Jwt": {
    "Key": "sua_chave_jwt_super_secreta_com_pelo_menos_32_caracteres",
    "Issuer": "https://seudominio.com",
    "Audience": "https://seudominio.com"
  }
}
```

## 🏗️ Deploy do Backend

### **Opção 1: Deploy Manual**
```bash
# 1. Publicar aplicação
cd backend/Bufunfa.Api
dotnet publish -c Release -o ./publish

# 2. Copiar arquivos para servidor
scp -r ./publish/* usuario@servidor:/var/www/bufunfa-api/

# 3. Configurar serviço systemd
sudo nano /etc/systemd/system/bufunfa-api.service
```

**Arquivo de serviço:**
```ini
[Unit]
Description=Bufunfa API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/bufunfa-api/Bufunfa.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=bufunfa-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

```bash
# 4. Ativar serviço
sudo systemctl enable bufunfa-api
sudo systemctl start bufunfa-api
sudo systemctl status bufunfa-api
```

### **Opção 2: Deploy com Docker**
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Bufunfa.Api.csproj", "."]
RUN dotnet restore "Bufunfa.Api.csproj"
COPY . .
RUN dotnet build "Bufunfa.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bufunfa.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bufunfa.Api.dll"]
```

```bash
# Build e run
docker build -t bufunfa-api .
docker run -d -p 5000:80 --name bufunfa-api bufunfa-api
```

## 🎨 Deploy do Frontend

### **1. Build da Aplicação**
```bash
cd frontend
npm install
npm run build
```

### **2. Configurar URL da API**
```typescript
// frontend/src/app/services/api.ts
private baseUrl = 'https://api.seudominio.com/api';
```

### **Opção 1: Nginx**
```nginx
# /etc/nginx/sites-available/bufunfa
server {
    listen 80;
    server_name seudominio.com;
    root /var/www/bufunfa-frontend/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Ativar site
sudo ln -s /etc/nginx/sites-available/bufunfa /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### **Opção 2: Apache**
```apache
# /etc/apache2/sites-available/bufunfa.conf
<VirtualHost *:80>
    ServerName seudominio.com
    DocumentRoot /var/www/bufunfa-frontend/dist
    
    <Directory /var/www/bufunfa-frontend/dist>
        AllowOverride All
        Require all granted
    </Directory>
    
    ProxyPass /api/ http://localhost:5000/api/
    ProxyPassReverse /api/ http://localhost:5000/api/
</VirtualHost>
```

## 🔒 Configuração SSL

### **Usando Certbot (Let's Encrypt)**
```bash
# Instalar certbot
sudo apt install certbot python3-certbot-nginx

# Obter certificado
sudo certbot --nginx -d seudominio.com

# Renovação automática
sudo crontab -e
# Adicionar: 0 12 * * * /usr/bin/certbot renew --quiet
```

## 🔧 Configurações de Produção

### **1. Variáveis de Ambiente**
```bash
# /etc/environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5000
```

### **2. Configurações de Segurança**
```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "seudominio.com"
}
```

## 📊 Monitoramento

### **1. Logs da Aplicação**
```bash
# Ver logs do backend
sudo journalctl -u bufunfa-api -f

# Ver logs do nginx
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### **2. Health Checks**
```bash
# Verificar status da API
curl https://api.seudominio.com/api/health

# Verificar status do frontend
curl https://seudominio.com
```

## 🔄 Backup e Manutenção

### **1. Backup do Banco**
```bash
# Script de backup
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
pg_dump -h localhost -U bufunfa_user bufunfa_db > /backups/bufunfa_$DATE.sql
```

### **2. Atualizações**
```bash
# Atualizar backend
cd backend/Bufunfa.Api
git pull
dotnet publish -c Release -o ./publish
sudo systemctl restart bufunfa-api

# Atualizar frontend
cd frontend
git pull
npm run build
sudo cp -r dist/* /var/www/bufunfa-frontend/dist/
```

## 🚨 Troubleshooting

### **Problemas Comuns**

**1. Erro de CORS**
- Verificar configuração no `Program.cs`
- Confirmar URLs permitidas

**2. Erro de Autenticação**
- Verificar credenciais do Google OAuth
- Confirmar redirect URIs

**3. Erro de Banco**
- Verificar string de conexão
- Confirmar se migrações foram aplicadas

**4. Erro 404 no Frontend**
- Verificar configuração do servidor web
- Confirmar fallback para `index.html`

### **Comandos Úteis**
```bash
# Verificar status dos serviços
sudo systemctl status bufunfa-api
sudo systemctl status nginx
sudo systemctl status postgresql

# Verificar logs
sudo journalctl -u bufunfa-api --since "1 hour ago"
sudo tail -f /var/log/nginx/error.log

# Verificar conectividade
curl -I https://seudominio.com
curl -I https://api.seudominio.com/api/health
```

## ✅ Checklist de Deploy

### **Pré-Deploy**
- [ ] Banco de dados configurado
- [ ] Credenciais do Google OAuth configuradas
- [ ] Variáveis de ambiente definidas
- [ ] SSL/TLS configurado
- [ ] Backup do banco atual (se aplicável)

### **Deploy Backend**
- [ ] Código atualizado no servidor
- [ ] Migrações aplicadas
- [ ] Serviço configurado e ativo
- [ ] Health check funcionando

### **Deploy Frontend**
- [ ] Build gerado com URL correta da API
- [ ] Arquivos copiados para servidor web
- [ ] Servidor web configurado
- [ ] Redirecionamentos funcionando

### **Pós-Deploy**
- [ ] Testes de funcionalidade básica
- [ ] Login com Google funcionando
- [ ] CRUD de contas funcionando
- [ ] CRUD de lançamentos funcionando
- [ ] Folhas mensais funcionando
- [ ] Monitoramento ativo

## 🎯 Conclusão

Seguindo estas instruções, o projeto Bufunfa estará **pronto para produção** com:

- ✅ Backend robusto e escalável
- ✅ Frontend responsivo e moderno
- ✅ Autenticação segura
- ✅ Banco de dados otimizado
- ✅ Monitoramento ativo
- ✅ Backup automatizado

**Suporte:** Para dúvidas ou problemas, consulte a documentação técnica ou entre em contato com a equipe de desenvolvimento.

