import {appStore} from "../app-state.ts";
import {WebSocketClient} from "./web-socket-client.ts";

export class GameClient {
    private readonly baseUrl;
    private currentToken: string = "";
    private webSocketClient: WebSocketClient;

    constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
        this.webSocketClient = new WebSocketClient();
    }

    async login(_name: string): Promise<void> {
        return new Promise((resolve) => {
            appStore.getState().login();
            resolve()
        });
    }

    async createLobby(): Promise<void> {
        const response = await fetch(`${this.baseUrl}/game-instance`, {
            method: 'POST',
            body: JSON.stringify({
                name: "peter"
            })
        });

        if (response.ok) {
            const bodyJson = await response.json();
            this.currentToken = bodyJson.token;

            await this.webSocketClient.connectAsync(this.baseUrl, this.currentToken);
            appStore.getState().joinedLobby();
        } else {
            throw new Error("Could not login");
        }
    }
}

export const gameClient = new GameClient('http://localhost:3000');