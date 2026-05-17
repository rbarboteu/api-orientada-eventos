# Highway Monitor 

> Um sistema de monitoramento rodoviário de alta disponibilidade construído com Arquitetura Orientada a Eventos (EDA) e Microsserviços — projetado para escala real e resiliência operacional.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600?style=flat-square&logo=rabbitmq)](https://www.rabbitmq.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat-square&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker)](https://www.docker.com/)
[![Liquibase](https://img.shields.io/badge/Liquibase-4.27-2962FF?style=flat-square)](https://www.liquibase.org/)

---

## Visão Geral

O Highway Monitor é um sistema de backend para recebimento e processamento em tempo real de relatórios de inspeção e acidentes rodoviários enviados por agentes de campo. O sistema foi projetado em torno do **processamento assíncrono de eventos**: a API de ingestão responde em milissegundos, enquanto toda a lógica de negócios roda em segundo plano através de serviços de worker dedicados.

O projeto demonstra padrões de nível de produção comumente exigidos em vagas internacionais de engenharia de backend:

- **Arquitetura Orientada a Eventos (EDA)** com produtores e consumidores desacoplados
- **Processamento assíncrono de mensagens** via RabbitMQ e MassTransit
- **Infraestrutura conteinerizada** orquestrada com Docker Compose
- **Versionamento de banco de dados** via arquivos de migração do Liquibase
- **Sanitização de dados no backend** — os dados brutos de campo são limpos e validados no worker, nunca no cliente

---

## Arquitetura

```
???????????????????????????????????????????????????????
?                  Docker Compose                      ?
?                                                      ?
?  ???????????????     ????????????????????????????   ?
?  ?   API de    ???????        RabbitMQ           ?   ?
?  ?  Ingestão   ?     ?  ???????????????????????? ?   ?
?  ?  (Minimal   ?     ?  ?  inspecoes-queue      ? ?   ?
?  ?   API)      ?     ?  ?  acidentes-queue      ? ?   ?
?  ?             ?     ?  ???????????????????????? ?   ?
?  ? POST /api/  ?     ????????????????????????????   ?
?  ?  inspecoes  ?                  ?                  ?
?  ? POST /api/  ?     ????????????????????????????   ?
?  ?  acidentes  ?     ?      Worker Services       ?   ?
?  ?             ?     ?  ???????????????????????? ?   ?
?  ? Retorna     ?     ?  ? Inspections Worker   ? ?   ?
?  ? HTTP 202    ?     ?  ? · Valida a ordem dos ? ?   ?
?  ???????????????     ?  ?   passos do checklist? ?   ?
?                      ?  ???????????????????????? ?   ?
?                      ?  ? Accidents Worker     ? ?   ?
?                      ?  ? · Sanitiza CPF/docs  ? ?   ?
?                      ?  ? · Aplica regras de   ? ?   ?
?                      ?  ?   negócio da ARTESP   ? ?   ?
?                      ?  ???????????????????????? ?   ?
?                      ????????????????????????????   ?
?                                   ?                  ?
?  ?????????????????????????????????????????????????  ?
?  ?  SQL Server 2022 (schema gerenciado por Liquibase)?  ?
?  ?  Inspecoes · ItensChecklist · Acidentes · Vitimas? ?
?  ??????????????????????????????????????????????????  ?
???????????????????????????????????????????????????????
```

### Por que Orientado a Eventos?

A API de ingestão e os workers de processamento são **completamente desacoplados**. A API não sabe quantos workers existem, o que eles fazem ou se estão rodando. Um pico nos relatórios de acidentes não bloqueia o processamento das inspeções — eles vivem em filas separadas. Adicionar um novo worker (ex: notificações, relatórios) exige zero alterações na API.

---

## Stack Tecnológica

| Camada | Tecnologia | Por quê? |
|---|---|---|
| API de Ingestão | .NET 9 Minimal API | Baixo overhead, resposta rápida com HTTP 202 |
| Message broker | RabbitMQ 3.13 | Filas duráveis, canais separados por domínio |
| Biblioteca de Mensageria | MassTransit 8.3 | Abstrai retry, DLQ, ACK/NACK e serialização |
| Serviço Worker | .NET 9 Background Service | Processo de longa execução, sem overhead de HTTP |
| ORM de Banco de Dados | Dapper | SQL explícito, performance previsível |
| Banco de Dados | SQL Server 2022 | Integridade referencial, restrições de FK |
| Versionamento de DB | Liquibase 4.27 | Schema como código, reproduzível entre ambientes |
| Infraestrutura | Docker + Docker Compose | Um único comando sobe o ambiente completo |

---

## Estrutura do Projeto

```
highway-monitor/
??? docker-compose.yml              # Definição completa da infraestrutura
??? infra/
?   ??? liquibase/
?       ??? liquibase.properties    # Configuração de conexão do DB
?       ??? changelog/
?           ??? db.changelog-master.xml
?           ??? 002-create-table-inspecoes.sql
?           ??? 003-create-table-checklist.sql
?           ??? 004-create-table-acidentes.sql
?           ??? 005-create-table-vitimas.sql
??? src/
    ??? HighwayMonitor.sln
    ??? HighwayMonitor.Contracts/   # Contratos de eventos compartilhados (records)
    ?   ??? InspecaoRecebidaEvent.cs
    ?   ??? AcidenteRecebidoEvent.cs
    ??? HighwayMonitor.Api/         # API de Ingestão
    ?   ??? Program.cs
    ?   ??? Dockerfile
    ??? HighwayMonitor.Worker/      # Processadores de background
        ??? Program.cs
        ??? Consumers/
        ?   ??? InspecaoConsumer.cs
        ?   ??? AcidenteConsumer.cs
        ??? Repositories/
        ?   ??? InspecaoRepository.cs
        ?   ??? AcidenteRepository.cs
        ??? Dockerfile
```

---

## Como Começar

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (com containers Linux habilitados)

Apenas isso. Não é necessário instalar o .NET SDK, SQL Server ou RabbitMQ diretamente na sua máquina.

### Executando o sistema completo

```bash
git clone https://github.com/seu-usuario/highway-monitor.git
cd highway-monitor
docker compose up --build
```

Este único comando irá:

1. Iniciar o **SQL Server 2022** e aguardar até que esteja saudável
2. Criar o banco de dados `HighwayMonitorDb` automaticamente
3. Executar as migrações do **Liquibase** para criar todas as tabelas
4. Iniciar o **RabbitMQ** com o painel de gerenciamento (Management UI)
5. Buildar e iniciar a **API de Ingestão** na porta `8080`
6. Buildar e iniciar o **Worker Service** conectado a ambas as filas

Aguarde cerca de 45 a 60 segundos na primeira execução para que o SQL Server inicialize completamente.

### Verificar se tudo está rodando

```bash
docker compose ps
```

Todos os serviços devem aparecer como `running` (rodando) ou `healthy` (saudável).

| Serviço | URL |
|---|---|
| API de Ingestão | http://localhost:8080 |
| RabbitMQ Management | http://localhost:15672 (usuário: `highway_user` / senha: `Highway@123456`) |
| SQL Server | `localhost,1433` (usuário: `sa` / senha: `Highway@123456`) |

---

## Referência da API

### POST `/api/inspecoes`

Recebe um relatório de inspeção rodoviária enviado por um agente de campo.

**Corpo da requisição (Request body):**
```json
{
  "rodovia": "SP-280",
  "kmInicial": "45",
  "kmFinal": "46",
  "fiscalId": "F-001",
  "checklist": [
    { "ordem": 1, "descricao": "Verificar sinalização", "conforme": true },
    { "ordem": 2, "descricao": "Verificar iluminação", "conforme": false },
    { "ordem": 3, "descricao": "Verificar asfalto", "conforme": true }
  ]
}
```

**Resposta:** `HTTP 202 Accepted`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "mensagem": "Inspeção recebida e em processamento."
}
```

**Regra de negócio:** O worker valida se os itens do checklist são processados na sequência estrita da sua `ordem`. Se qualquer passo estiver fora de ordem, a mensagem é rejeitada e enviada para a fila de erros / dead letter queue (`inspecoes-queue_error`).

---

### POST `/api/acidentes`

Recebe um relatório de acidente com dados brutos de identificação das vítimas.

**Corpo da requisição (Request body):**
```json
{
  "rodovia": "SP-280",
  "km": "45",
  "tipoAcidente": "Colisão",
  "vitimas": [
    {
      "nomeRaw": "  João da Silva  ",
      "documentoRaw": "123.456.789-09",
      "tipoDocumento": "CPF"
    }
  ]
}
```

**Resposta:** `HTTP 202 Accepted`
```json
{
  "id": "8b3c1a77-4321-4abc-9def-1c2d3e4f5a6b",
  "mensagem": "Acidente recebido e em processamento."
}
```

**Regra de negócio:** Toda a sanitização de dados acontece no worker — nunca no cliente. O worker remove espaços em branco dos nomes, elimina caracteres não numéricos dos documentos e formata os CPFs para o padrão `XXX.XXX.XXX-XX` antes de persistir.

---

## Esquema do Banco de Dados (Schema)

```sql
Inspecoes
  Id            UNIQUEIDENTIFIER  PK
  Rodovia       NVARCHAR(100)
  KmInicial     NVARCHAR(20)
  KmFinal       NVARCHAR(20)
  FiscalId      NVARCHAR(50)
  Status        NVARCHAR(20)      DEFAULT 'PROCESSANDO'
  RecebidoEm   DATETIME2
  ProcessadoEm DATETIME2

ItensChecklist
  Id          UNIQUEIDENTIFIER  PK
  InspecaoId  UNIQUEIDENTIFIER  FK ? Inspecoes(Id)
  Ordem       INT               NOT NULL
  Descricao   NVARCHAR(255)
  Conforme    BIT
  INDEX IX_ItensChecklist_InspecaoId_Ordem (InspecaoId, Ordem)

Acidentes
  Id           UNIQUEIDENTIFIER  PK
  Rodovia      NVARCHAR(100)
  Km           NVARCHAR(20)
  TipoAcidente NVARCHAR(100)
  RecebidoEm  DATETIME2
  ProcessadoEm DATETIME2

Vitimas
  Id              UNIQUEIDENTIFIER  PK
  AcidenteId      UNIQUEIDENTIFIER  FK ? Acidentes(Id)
  NomeSanitizado  NVARCHAR(200)
  Documento       NVARCHAR(20)
  TipoDocumento   NVARCHAR(10)
```

A coluna `Ordem` em `ItensChecklist` é `NOT NULL` por design do schema — a regra de negócio é reforçada a nível de banco de dados, não apenas no código. O índice composto em `(InspecaoId, Ordem)` garante que a query de validação de sequência seja performática em larga escala.

---

## Decisões Chave de Arquitetura

### 1. API retorna HTTP 202, nunca 200

A API não salva nada diretamente no banco de dados. Ela publica um evento e retorna `202 Accepted`, comunicando que a requisição foi recebida e será processada. Essa é a semântica correta para operações assíncronas — um `200 OK` implicaria que o trabalho já foi concluído.

### 2. Filas separadas por domínio

A `inspecoes-queue` e a `acidentes-queue` são intencionalmente separadas. Um pico de tráfego em relatórios de acidentes (ex: um grande engavetamento) não atrasa o processamento das inspeções de rotina. O isolamento de filas é um padrão fundamental de resiliência.

### 3. Contratos em um projeto dedicado

O `HighwayMonitor.Contracts` é uma classlib. A API e o Worker não dependem diretamente um do outro, mas sim desse projeto de contratos. Essa é a arquitetura ideal para deploys independentes: você pode implantar uma nova versão do Worker sem precisar reinstalar ou reabrir a API.

### 4. Sanitização centralizada no Worker

Os dados brutos das vítimas (CPFs desformatados, nomes com espaços extras, letras maiúsculas/minúsculas inconsistentes) chegam direto dos dispositivos móveis. Não há `cleanVal()` ou pré-processamento no frontend. O Worker é a única fonte de verdade para a qualidade dos dados — tornando as regras auditáveis, testáveis e centralizadas.

### 5. Liquibase em vez de EF Migrations

As migrações do Entity Framework são fortemente acopladas ao runtime da aplicação. O Liquibase é independente de tecnologia: ele roda em um container separado, possui suporte nativo a rollback e expõe as alterações de schema como SQL puro no controle de versão — permitindo a revisão de qualquer pessoa do time, incluindo DBAs.

---

## Fluxo de Trabalho de Desenvolvimento

Para rodar a API e o Worker localmente (fora do Docker) visando uma iteração mais rápida:

```bash
# Inicia apenas a infraestrutura básica
docker compose up sqlserver rabbitmq liquibase -d

# Executa a API localmente
cd src/HighwayMonitor.Api
dotnet run

# Executa o Worker localmente (em outro terminal)
cd src/HighwayMonitor.Worker
dotnet run
```

Os arquivos `appsettings.json` locais já estão pré-configurados para se conectar ao `localhost` de ambos os serviços.