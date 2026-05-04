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
            postMessage('socket-open-failed');
            cleanup();
            return;
        }

        webSocket.onopen = () => {
            postMessage('socket-open');
        };
        webSocket.onclose = () => {
            postMessage('socket-closed');
            cleanup();
        };
        webSocket.onmessage = (e) => {
            postMessage(e.data,  [e.data ]);
        };
    } else if (message.type === 'send') {
        webSocket.send(message.data);
    }
});
