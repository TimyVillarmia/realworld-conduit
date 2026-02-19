# RealWorld Conduit: Full-Stack .NET & React Monorepo

This repository is a unified monorepo implementation of the [RealWorld](https://github.com/gothinkster/realworld) specification. It combines a robust **ASP.NET Core** backend with a modern **React + Vite** frontend.

## 📂 Project Structure

This project is organized into two main services. Each contains its own specialized documentation:

- **[Backend (Dotnet 8)](./backend/README.md)**: A Clean Architecture implementation using Entity Framework Core, SQLite, and PascalCase routing.
- **[Frontend (React + Vite)](./frontend/README.md)**: A functional React application utilizing React Query for state management and Axios for API communication.

## 🚀 Quick Start (Monorepo)

The root folder is configured to manage both services simultaneously using `concurrently` and `wait-on`.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Bun](https://bun.sh) (or Node.js/NPM)

### Installation & Execution

1. Install all dependencies:
   ```bash
   bun install
   ```
2. Start the full-stack environment:
   ```bash
   bun start
   ```
   _The frontend will wait for the backend port (5000) to be active before launching._

## 🛠️ Key Monorepo Features

- **Unified Scripts**: Manage the entire lifecycle from the root `package.json`.
- **API Synchronization**: Automatic path normalization via Axios interceptors to bridge JS lowercase and .NET PascalCase.
- **Shared Environment**: Designed for local development with a pre-configured SQLite database.

## 📖 Specifications

Both projects adhere to the official RealWorld API endpoints and frontend functionality, including JWT Auth, Article CRUD, and social features. For detailed breakdowns of specific layers (Core, Data, Infrastructure) or Page breakdowns (Home, Editor, Profile), please refer to the respective sub-folder READMEs.
