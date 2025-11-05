# 🗂️ Taskly

O **Taskly** é um sistema de gerenciamento de tarefas de projetos desenvolvido com **.NET 8** e **MongoDB**.
O projeto foi criado como parte do meu aprendizado em **desenvolvimento back-end** e serve para entender melhor e colocar em prática conceitos de arquitetura, boas práticas e organização de código.

---

## 🧭 Objetivo do Projeto

O principal objetivo é **aprender na prática** como estruturar um sistema em camadas, aplicar padrões de retorno e começar a escrever testes.
Estou desenvolvendo o Taskly de forma progressiva, simulando como seria o back-end de um projeto real.

---

## ⚙️ Tecnologias e Conceitos Estudados

* **.NET 8 (C#)**
* **MongoDB** (driver 3.3.0)
* **Arquitetura em camadas** (Application, Domain, Infrastructure)
* **Controllers e DTOs**
* **Padrão de retorno `OperationResult`**
* **Injeção de dependência**
* **Testes unitários**
* **Cadastro, Login e controle de autenticação**

---

## 🧱 Estrutura do Projeto

```
Taskly/
 ├── Taskly.API/
 │   ├── Application/     → Lógica de aplicação e serviços  
 │   ├── Controllers/     → Endpoints da API  
 │   ├── Domain/          → Entidades e regras de negócio  
 │   └── Infrastructure/  → Integração com banco e serviços externos  
 └── Taskly.Tests/        → Testes unitários (em progresso)
```

## 🚧 Estado Atual do Projeto

| Funcionalidade                          | Status                |
| --------------------------------------- | --------------------- |
| CRUD de tarefas (`TodoTask`)            | ✅ Concluído           |
| Estrutura de camadas                    | ✅ Concluída           |
| Retorno padrão (`OperationResult`)      | ✅ Implementado        |
| DTOs e validações básicas               | ✅ Implementados       |
| Sistema de usuários, equipes e projetos | 🚧 Em desenvolvimento |
| Testes unitários                        | 🧪 Em desenvolvimento       |
| Autenticação e autorização              | 🚧 Em desenvolvimento  |

---

## 🧠 Próximos Passos

* Aprimorar os testes unitários
* Implementar autenticação e autorização
* Ter um funcionamento mínimo da API para criação de tarefas por um User
* Realizar o Deploy da aplicação

---

## 📚 O que estou aprendendo com este projeto

Esse projeto tem sido uma forma prática de estudar **.NET**, **MongoDB**, **Deploy de APIs** e **boas práticas de arquitetura**, entendendo melhor como um sistema real se organiza.
O foco é continuar evoluindo o código conforme aprendo mais sobre **padrões, testes e escalabilidade**.

---

## 💬 Contato

* **LinkedIn:** [linkedin.com/in/samuel-dantas-de-oliveira](https://www.linkedin.com/in/samuel-dantas-de-oliveira/)
* **GitHub:** [github.com/samueldantasoliveira](https://github.com/samueldantasoliveira)
