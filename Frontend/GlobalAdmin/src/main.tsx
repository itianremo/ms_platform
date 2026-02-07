import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { AuthProvider } from './context/AuthContext.tsx';
import { ThemeProvider } from './components/theme-provider.tsx';
import { UserPreferencesProvider } from './context/UserPreferencesContext.tsx';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <AuthProvider>
            <ThemeProvider>
                <UserPreferencesProvider>
                    <BrowserRouter>
                        <App />
                    </BrowserRouter>
                </UserPreferencesProvider>
            </ThemeProvider>
        </AuthProvider>
    </React.StrictMode>,
);
