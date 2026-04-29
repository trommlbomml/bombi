export type SocketState = 'Disconnected' | 'Connected';

export class WebSocketClient {
    private readonly _socketWorker: Worker;
    private readonly _messages: Array<string | ArrayBuffer | any> = [];

    private _connectionResolver?: (value: boolean | PromiseLike<boolean>) => void;
    private _state: SocketState = 'Disconnected';

    constructor() {
        this._socketWorker = new Worker(new URL('web-socket-worker.js', import.meta.url));
        this._socketWorker.onerror = (e) => this.onWorkerError(e);
        this._socketWorker.addEventListener('message', e => this.onWorkerMessage(e));
    }

    async connectAsync(url: string, token: string): Promise<void> {
        const successOpenPromise = new Promise<boolean>(
            (resolve: (value: boolean | PromiseLike<boolean>) => void) => this._connectionResolver = resolve
        );
        this._socketWorker.postMessage({
            type: 'connect',
            url: `${url}/ws?token=${token}`,
        });

        const successOpen = await successOpenPromise;

        if (!successOpen) {
            throw new Error('Cannot open websocket.');
        } else {
            this._state = 'Connected'
        }
    }

    get state(): SocketState {
        return this._state;
    }

    popQueuedMessageData(): string|ArrayBuffer | any {
        return this._messages.shift();
    }

    sendSocketMessage(data: any) {
        if (data instanceof ArrayBuffer) {
            this._socketWorker.postMessage({
                type: 'send',
                data,
            }, [data]);
        } else {
            this._socketWorker.postMessage({
                type: 'send',
                data,
            });
        }
    }

    private onWorkerError(e: ErrorEvent) {
        throw new Error(`WebSocket Error: ${e}`);
    }

    private onWorkerMessage(e: MessageEvent){
        if (e.data.type === 'socket-open') {
            this._state = 'Connected';
            return this._connectionResolver?.(true);
        } else if (e.data.type === 'socket-open-failed') {
            this._state = 'Disconnected';
            return this._connectionResolver?.(false);
        } else if (e.data.type === 'socket-closed') {
            this._state = 'Disconnected';
            return this._connectionResolver?.(false);
        } else if (e.data.type === 'socket-message') {
            this._messages.push(e.data.data);
        }
    }
}

