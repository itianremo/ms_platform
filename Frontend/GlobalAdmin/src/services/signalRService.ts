import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { toast } from 'sonner';

class SignalRService {
    private connection: HubConnection | null = null;
    private static instance: SignalRService;
    private isConnected = false;

    private constructor() { }

    public static getInstance(): SignalRService {
        if (!SignalRService.instance) {
            SignalRService.instance = new SignalRService();
        }
        return SignalRService.instance;
    }

    public async startConnection(token: string) {
        if (this.connection && this.isConnected) return;

        // Base URL should come from config, defaulting to gateway for now
        const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:7032';
        // Note: Gateway proxies /hubs/notifications to Notifications.API
        // But SignalR client might need exact path. 
        // The Notifications.API maps to /hubs/notifications.
        // Gateway maps /hubs/notifications -> Notifications.API

        const hubUrl = `${BASE_URL}/hubs/notifications`.replace('/api', '');
        // Adjust logic: if VITE_API_URL is http://localhost:7032/api, we want http://localhost:7032/hubs/notifications
        // If VITE_API_URL is http://localhost:7032, we want http://localhost:7032/hubs/notifications

        this.connection = new HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.connection.on("ReceiveNotification", (title: string, message: string) => {
            toast.info(title, {
                description: message,
                duration: 5000,
            });
        });

        try {
            await this.connection.start();
            this.isConnected = true;
            console.log("SignalR Connected");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            // Retry logic usually handled by withAutomaticReconnect after initial connection
            setTimeout(() => this.startConnection(token), 5000);
        }

        this.connection.onclose(() => {
            this.isConnected = false;
        });
    }

    public stopConnection() {
        if (this.connection) {
            this.connection.stop();
            this.isConnected = false;
        }
    }
}

export const signalRService = SignalRService.getInstance();
