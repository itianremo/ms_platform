import axios from 'axios';

// Create an Axios instance with default configuration
const api = axios.create({
    baseURL: '/api', // Proxied via Nginx to Gateway
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor to add JWT token
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('fitit_admin_token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor to handle errors (e.g., 401 Unauthorized)
let isRefreshing = false;
let failedQueue: any[] = [];

const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            console.log("Retrying queued request with new token");
            prom.resolve(token);
        }
    });
    failedQueue = [];
};

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
            // Prevent loop if refresh endpoint itself fails
            if (originalRequest.url.includes('/refresh') || originalRequest.url.includes('/login')) {
                return Promise.reject(error);
            }

            if (isRefreshing) {
                return new Promise(function (resolve, reject) {
                    failedQueue.push({ resolve, reject })
                }).then(token => {
                    originalRequest.headers['Authorization'] = 'Bearer ' + token;
                    return api(originalRequest);
                }).catch(err => {
                    return Promise.reject(err);
                })
            }

            originalRequest._retry = true;
            isRefreshing = true;

            try {
                // Call Refresh Endpoint directly via axios to avoid interceptor loop
                // Using relative path '/api/auth/api/auth/refresh' to match proxy.
                const rs = await axios.post('/api/auth/api/auth/refresh', {}, {
                    withCredentials: true // Ensure cookies are sent
                });

                const { accessToken } = rs.data;

                localStorage.setItem('fitit_admin_token', accessToken);
                api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;

                processQueue(null, accessToken);

                originalRequest.headers['Authorization'] = `Bearer ${accessToken}`;
                return api(originalRequest);

            } catch (_error) {
                processQueue(_error, null);

                // Clear state and redirect (Kick Out)
                localStorage.removeItem('fitit_admin_token');

                // Avoid toast flood?
                // For now, just redirect
                if (!window.location.pathname.includes('/login')) {
                    window.location.href = '/login';
                }

                return Promise.reject(_error);
            } finally {
                isRefreshing = false;
            }
        }

        return Promise.reject(error);
    }
);

export default api;
