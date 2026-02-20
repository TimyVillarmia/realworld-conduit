# RealWorld Conduit: Full-Stack .NET & React Monorepo

This repository represents a full-stack implementation of the [RealWorld](https://github.com/gothinkster/realworld) specification. It integrates a high-performance **.NET** backend with a modern **React** frontend to create a complete social blogging platform.

## 📂 Project Structure

This project is built by integrating and extending the following open-source repositories:

- **[Backend (Dotnet 8)](https://github.com/Erikvdv/realworlddotnet)**: A Clean Architecture implementation using ASP.NET, Entity Framework Core, SQLite.
- **[Frontend (React + Vite)](https://github.com/romansndlr/react-vite-realworld-example-app)**: A functional React application utilizing React Query for state management and Axios for API communication.

## 🚀 Quick Start (Monorepo)

The root folder is configured to manage both Frontend & Backend simultaneously using `concurrently` and `wait-on`.

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

