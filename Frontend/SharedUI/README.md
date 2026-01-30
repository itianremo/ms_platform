# Shared UI Library

A shared React component library used across all frontend applications (Admin Dashboards and App Portal).

## 📦 content
- **Components**: Reusable UI elements (Buttons, Inputs, Modals, etc.).
- **Hooks**: Common React hooks (e.g., `useAuth`, `useTheme`).
- **Utils**: Helper functions (date formatting, validation).
- **Styles**: Base TailwindCSS configuration.

## 🔨 Usage
Import components directly into the consumer apps:
```tsx
import { Button } from '@shared/ui';
```
*(assuming alias configuration)*

## 🚀 Development
Changes here reflect immediately across all consuming apps in the monorepo structure.
