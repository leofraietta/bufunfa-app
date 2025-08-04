# 🚀 Instruções de Setup - Projeto Bufunfa

Este guia mostra como configurar e executar o projeto Bufunfa (backend e frontend) em sua máquina local.

## 📋 Pré-requisitos

### Backend (.NET 8.0)
- **.NET SDK 8.0** ou superior
- **PostgreSQL** (versão 14 ou superior)
- **Git** para clonar o repositório

### Frontend (Angular)
- **Node.js** (versão 18 ou superior)
- **npm** (incluído com Node.js)
- **Angular CLI** (será instalado automaticamente)

## 🔧 Configuração Inicial

### 1. Clone o Repositório
```bash
git clone https://github.com/leofraietta/bufunfa-app.git
cd bufunfa-app
```

### 2. Configuração do PostgreSQL

#### Instalação do PostgreSQL (Ubuntu/Debian):
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
```

#### Configuração do Banco de Dados:
```bash
# Iniciar o serviço PostgreSQL
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Configurar usuário e banco
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'postgres';"
sudo -u postgres psql -c "CREATE DATABASE bufunfa_db;"
```

#### Verificar se o PostgreSQL está rodando:
```bash
sudo systemctl status postgresql
```

### 3. Configuração do Google OAuth2 (Opcional)

Para usar o login com Google, você precisa:

1. Acesse o [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Ative a API do Google+ 
4. Crie credenciais OAuth 2.0
5. Configure as URLs de redirecionamento:
   - `http://localhost:5000/api/Auth/google-callback`
   - `https://localhost:7000/api/Auth/google-callback`

6. Atualize o arquivo `backend/Bufunfa.Api/appsettings.json`:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "SEU_CLIENT_ID_AQUI",
      "ClientSecret": "SEU_CLIENT_SECRET_AQUI"
    }
  }
}
```

## 🚀 Executando o Backend

### 1. Navegar para o diretório do backend:
```bash
cd backend/Bufunfa.Api
```

### 2. Restaurar dependências:
```bash
dotnet restore
```

### 3. Aplicar migrações do banco de dados:
```bash
dotnet ef database update
```

### 4. Executar o backend:
```bash
dotnet run
```

O backend estará disponível em:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:7000`
- **Swagger UI:** `http://localhost:5000/swagger` ou `https://localhost:7000/swagger`

**Nota:** As configurações de porta foram corrigidas nos arquivos `launchSettings.json` e `api.ts` para usar as portas 5000 (HTTP) e 7000 (HTTPS).

## 🎯 Executando o Frontend

### 1. Abrir um novo terminal e navegar para o diretório do frontend:
```bash
cd frontend
```

### 2. Instalar dependências:
```bash
npm install
```

### 3. Executar o frontend:
```bash
npm start
# ou
ng serve
```

O frontend estará disponível em:
- **URL:** `http://localhost:4200`

## 🔍 Verificação do Setup

### Backend
1. Acesse `https://localhost:7000/swagger` para ver a documentação da API
2. Teste o endpoint de saúde: `GET https://localhost:7000/api/health`

### Frontend
1. Acesse `http://localhost:4200` no navegador
2. Você deve ver a tela de login do Bufunfa

## 🐛 Solução de Problemas

### Erro de Conexão com PostgreSQL
```bash
# Verificar se o PostgreSQL está rodando
sudo systemctl status postgresql

# Reiniciar o PostgreSQL se necessário
sudo systemctl restart postgresql
```

### Erro de Porta em Uso
```bash
# Verificar processos usando as portas
sudo lsof -i :5000  # Backend HTTP
sudo lsof -i :7000  # Backend HTTPS
sudo lsof -i :4200  # Frontend

# Matar processo se necessário
sudo kill -9 <PID>
```

### Erro de Dependências do Frontend
```bash
# Limpar cache e reinstalar
cd frontend
rm -rf node_modules package-lock.json
npm install
```

### Erro de Migrações do Entity Framework
```bash
cd backend/Bufunfa.Api

# Remover migrações existentes (se necessário)
rm -rf Migrations/

# Criar nova migração
dotnet ef migrations add InitialCreate

# Aplicar migração
dotnet ef database update
```

## 📱 Testando a Aplicação

1. **Inicie o backend** primeiro (porta 5000/7000)
2. **Inicie o frontend** (porta 4200)
3. **Acesse** `http://localhost:4200`
4. **Teste o login** com Google (se configurado)
5. **Explore** as funcionalidades:
   - Dashboard
   - Gestão de Contas
   - Lançamentos
   - Contas Conjuntas

## 🔧 Comandos Úteis

### Backend
```bash
# Executar em modo de desenvolvimento
dotnet run --environment Development

# Executar testes
dotnet test

# Gerar nova migração
dotnet ef migrations add <NomeDaMigracao>
```

### Frontend
```bash
# Executar em modo de desenvolvimento
ng serve --open

# Executar testes
ng test

# Build para produção
ng build --prod
```

## 📚 Recursos Adicionais

- **Documentação da API:** `https://localhost:7000/swagger`
- **Repositório GitHub:** `https://github.com/leofraietta/bufunfa-app`
- **Documentação do Projeto:** `./documentation/README.md`

---

**Nota:** Certifique-se de que o backend esteja rodando antes de iniciar o frontend para que a comunicação entre eles funcione corretamente.

