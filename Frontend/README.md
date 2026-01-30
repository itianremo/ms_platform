# Frontend Monorepo

Contains the React-based frontend applications for the MS Platform.

## 📂 Applications
| Application | Path | Port | Description |
| :--- | :--- | :--- | :--- |
| **Global Admin** | `/GlobalAdmin` | 3000 | Superadmin dashboard for platform management. |
| **FitIT Admin** | `/FitITAdmin` | 3001 | Tenant-specific dashboard for FitIT. |
| **Wissler Admin** | `/WisslerAdmin` | 3002 | Tenant-specific dashboard for Wissler. |
| **App Portal** | `/App` | 5173 | User-facing mobile web application. |
| **Shared UI** | `/SharedUI` | - | Shared components, hooks, and styles. |

## 🛠 Technology Stack
- **Framework**: React 18
- **Build Tool**: Vite
- **Language**: TypeScript
- **Styling**: TailwindCSS
- **State**: React Query / Context
- **Routing**: React Router DOM

## 🚀 Getting Started

### Prerequisites
- Node.js 18+
- npm or pnpm

### Running an App
Navigate to the specific app folder and run:
```bash
npm install
npm run dev
```
