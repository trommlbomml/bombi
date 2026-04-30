let webSocket = null;
let sendInterval = null;

function cleanup() {
    if (webSocket) {
        webSocket.close();
        webSocket = null;
    }
    if (sendInterval) {
        clearInterval(sendInterval);
        sendInterval = null;
    }
}

addEventListener('message', (e) => {
    const message = e.data;
    if (message.type === 'connect') {
        cleanup();

        try {
            webSocket = new WebSocket(message.url);
            webSocket.binaryType = 'arraybuffer';
        } catch {
            postMessage({
                type: 'socket-open-failed',
            });
            cleanup();
            return;
        }

        webSocket.onopen = () => {
            postMessage({
                type: 'socket-open',
            });
        };
        webSocket.onclose = () => {
            postMessage({
                type: 'socket-closed',
            });
            cleanup();
        };
        webSocket.onmessage = (e) => {
            if (e.data instanceof ArrayBuffer) {
                postMessage({
                    type: 'socket-message',
                    data: e.data,
                }, [e.data]);
            } else {
                postMessage({
                    type: 'socket-message',
                    data: e.data,
                });
            }
        };
    } else if (message.type === 'send') {
        webSocket.send(message.data);
    }
});
