# Documentação da API - Bufunfa

## Base URL
```
http://localhost:5000/api
```

## Autenticação

### Google OAuth2 Login
```http
GET /Auth/google-login
```
Redireciona para o Google OAuth2 para autenticação.

### Callback do Google
```http
GET /Auth/google-callback
```
Endpoint de callback após autenticação no Google.

## Endpoints de Contas

### Listar Contas
```http
GET /Contas
Authorization: Bearer {token}
```

**Resposta:**
```json
[
  {
    "id": 1,
    "nome": "Conta Corrente",
    "tipo": 1,
    "saldoInicial": 1000.00,
    "dataFechamento": null,
    "dataVencimento": null
  }
]
```

### Obter Conta por ID
```http
GET /Contas/{id}
Authorization: Bearer {token}
```

### Criar Nova Conta
```http
POST /Contas
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Nova Conta",
  "tipo": 1,
  "saldoInicial": 500.00
}
```

### Atualizar Conta
```http
PUT /Contas/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Conta Atualizada",
  "tipo": 1,
  "saldoInicial": 750.00
}
```

### Deletar Conta
```http
DELETE /Contas/{id}
Authorization: Bearer {token}
```

## Endpoints de Categorias

### Listar Categorias
```http
GET /Categorias
Authorization: Bearer {token}
```

### Criar Nova Categoria
```http
POST /Categorias
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Alimentação",
  "descricao": "Gastos com alimentação"
}
```

## Endpoints de Lançamentos

### Listar Lançamentos
```http
GET /Lancamentos
Authorization: Bearer {token}
```

### Criar Novo Lançamento
```http
POST /Lancamentos
Authorization: Bearer {token}
Content-Type: application/json

{
  "descricao": "Salário",
  "valor": 3000.00,
  "data": "2025-08-03T00:00:00",
  "tipo": 1,
  "tipoRecorrencia": 2,
  "contaId": 1,
  "categoriaId": 1
}
```

### Consolidar Fatura do Cartão
```http
POST /Lancamentos/consolidar-fatura/{contaId}
Authorization: Bearer {token}
```

## Endpoints de Contas Conjuntas

### Listar Contas Conjuntas
```http
GET /ContasConjuntas
Authorization: Bearer {token}
```

### Criar Conta Conjunta
```http
POST /ContasConjuntas
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Casa Compartilhada",
  "manterSaldoPositivo": true
}
```

### Adicionar Usuário à Conta Conjunta
```http
POST /ContasConjuntas/{id}/adicionar-usuario
Authorization: Bearer {token}
Content-Type: application/json

"usuario@email.com"
```

### Atualizar Rateio
```http
PUT /ContasConjuntas/{contaId}/atualizar-rateio/{rateioId}
Authorization: Bearer {token}
Content-Type: application/json

50.0
```

### Apurar Conta Conjunta
```http
POST /ContasConjuntas/{id}/apurar
Authorization: Bearer {token}
```

## Códigos de Status

- `200 OK` - Sucesso
- `201 Created` - Recurso criado com sucesso
- `400 Bad Request` - Dados inválidos
- `401 Unauthorized` - Token inválido ou ausente
- `404 Not Found` - Recurso não encontrado
- `500 Internal Server Error` - Erro interno do servidor

## Tipos de Dados

### TipoConta
- `1` - Principal (Conta corrente, poupança, etc.)
- `2` - Cartão de Crédito

### TipoLancamento
- `1` - Receita
- `2` - Despesa

### TipoRecorrencia
- `1` - Único
- `2` - Mensal
- `3` - Semanal
- `4` - Anual

## Autenticação JWT

Todos os endpoints (exceto os de autenticação) requerem um token JWT válido no header:

```
Authorization: Bearer {token}
```

O token é obtido após o login bem-sucedido via Google OAuth2.

