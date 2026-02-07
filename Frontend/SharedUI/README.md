# Shared UI Library

## Overview
A shared component library used across all frontend applications (Global Admin, FitIT Admin, Wissler Admin) to ensure design consistency and code reusability.

## Tech Stack
- **Framework**: React 19
- **Build Tool**: Vite (Library Mode)
- **Styling**: TailwindCSS
- **Components**: Shadcn UI (Radix Primitives)

## Key Components
- **UI Primitives**: Buttons, Cards, Inputs, Dialogs, etc.
- **Layouts**: Dashboard Layout, Sidebar, Navbar.
- **Hooks**: Shared React hooks for common functionality.
- **Utils**: Helper functions (CN, formatters).

## Usage
Import components directly into the consumer applications:
\`\`\`typescript
import { Button } from 'sharedui';
\`\`\`
