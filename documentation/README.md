# Bufunfa - Aplicativo de Controle Financeiro Pessoal

## Visão Geral

O Bufunfa é um aplicativo web de controle financeiro pessoal que permite aos usuários gerenciar suas finanças de forma eficiente e intuitiva. O sistema oferece funcionalidades completas para controle de receitas, despesas, contas bancárias, cartões de crédito e contas conjuntas.

## Arquitetura do Sistema

### Backend
- **Tecnologia**: .NET 8.0 Web API
- **Banco de Dados**: PostgreSQL
- **Autenticação**: Google OAuth2 + JWT
- **ORM**: Entity Framework Core

### Frontend
- **Tecnologia**: Angular 20.1.4
- **UI Framework**: Angular Material
- **Estilo**: CSS3 + SCSS

### Website
- **Tecnologia**: React + Vite
- **Estilo**: Tailwind CSS
- **Deploy**: Vercel/Netlify

## Funcionalidades Principais

### 1. Autenticação
- Login via Google OAuth2
- Gerenciamento de sessão com JWT
- Controle de acesso baseado em usuário

### 2. Gestão de Contas
- **Contas Principais**: Conta corrente, poupança, investimentos
- **Cartões de Crédito**: Controle de faturas e vencimentos
- **Saldo Inicial**: Configuração do saldo inicial de cada conta

### 3. Gestão de Lançamentos
- **Receitas e Despesas**: Registro completo de transações
- **Categorização**: Sistema de categorias personalizáveis
- **Recorrência**: Lançamentos mensais, semanais, etc.
- **Parcelamento**: Controle de compras parceladas
- **Consolidação de Faturas**: Fechamento automático de cartões

### 4. Contas Conjuntas
- **Compartilhamento**: Contas compartilhadas entre usuários
- **Rateio**: Sistema de divisão proporcional de gastos
- **Apuração**: Cálculo automático de valores devidos

### 5. Dashboard e Relatórios
- **Visão Geral**: Resumo financeiro mensal
- **Gráficos**: Visualização de receitas vs despesas
- **Extratos**: Histórico detalhado de transações

## Estrutura do Repositório

```
bufunfa-app/
├── backend/           # API .NET 8.0
│   └── Bufunfa.Api/
├── frontend/          # Aplicação Angular
├── website/           # Site institucional React
├── slides/            # Apresentação do projeto
├── documentation/     # Documentação técnica
└── README.md
```

## Configuração e Execução

### Pré-requisitos
- .NET SDK 8.0+
- Node.js 18+
- PostgreSQL 14+
- Angular CLI

### Backend
```bash
cd backend/Bufunfa.Api
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend
```bash
cd frontend
npm install
ng serve
```

### Website
```bash
cd website
npm install
npm run dev
```

## Configuração do Banco de Dados

1. Instalar PostgreSQL
2. Criar banco de dados `bufunfa_db`
3. Configurar string de conexão no `appsettings.json`
4. Executar migrações com `dotnet ef database update`

## Configuração OAuth2

1. Criar projeto no Google Cloud Console
2. Configurar OAuth 2.0 credentials
3. Adicionar URLs de redirecionamento autorizadas
4. Configurar Client ID e Client Secret no `appsettings.json`

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## Contato

Para dúvidas ou sugestões, entre em contato através do GitHub Issues.

