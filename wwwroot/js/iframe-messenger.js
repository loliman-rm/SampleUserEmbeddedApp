//// Usage in parent
//const parentMessenger = new IFrameMessenger(iframeWindow, iframeOrigin);
//parentMessenger.startAndSignalReady(() => {
//    console.log("Both sides are ready, sending data...");
//    parentMessenger.sendMessage(IFrameMessenger.TYPES.DATA, { key: "value" });
//});

//// Usage in iframe
//const iframeMessenger = new IFrameMessenger(parentWindow, parentOrigin);
//iframeMessenger.startAndSignalReady();
//iframeMessenger.onData((data) => {
//    console.log("Received data:", data);
//});

class IFrameMessenger {
    static TYPES = {
        READY: "ready",
        DATA: "data",
        READYCHECK: "readycheck",
    };

    /**
     * Creates an instance of IFrameMessenger
     * @param {Window} targetWindow - The target window (iframe or parent) to communicate with
     * @param {string} targetOrigin - The expected origin of the target window for security
     */
    constructor(targetWindow, targetOrigin) {
        this.targetWindow = targetWindow;
        this.targetOrigin = targetOrigin;
        this.listeners = new Map();
        this.isListening = false; // Ensures that we only add the message event listener once
        this.isReady = false;
        this.peerReady = false;
        this.onBothReadyCallback = null;
        this._handleMessage = this._handleMessage.bind(this);
    }

    /**
     * Sends a message to the target window
     * @param {string} type - The message type (must be one of IFrameMessenger.TYPES)
     * @param {any} data - The data payload to send
     */
    sendMessage(type, data) {
        if (!this.targetWindow) {
            console.warn("Message not sent, target window is not set");
            return;
        }
        this.targetWindow.postMessage({ type, data }, this.targetOrigin);
    }

    /**
     * Starts listening for messages from the target window
     */
    startListening() {
        if (!this.isListening) {
            window.addEventListener("message", this._handleMessage);
            this.isListening = true;
        }
    }

    /**
     * Stops listening for messages
     */
    stopListening() {
        if (this.isListening) {
            window.removeEventListener("message", this._handleMessage);
            this.isListening = false;
        }
    }

    /**
     * Registers a callback for a specific message type
     * @param {string} type - The message type to listen for
     * @param {Function} callback - The function to call when the message type is received
     */
    on(type, callback) {
        this.listeners.set(type, callback);
    }

    /**
     * Convenience method to register a callback for 'data' messages
     * @param {Function} callback - The function to call when a 'data' message is received
     */
    onData(callback) {
        this.on(IFrameMessenger.TYPES.DATA, callback);
    }

    /**
     * Starts listening, sends a `ready` signal, and acknowledges the peer's readiness
     * If an `onBothReady` callback is provided, it executes it once both are ready
     * @param {Function} [onBothReady] - Callback executed when both sides are ready
     */
    startAndSignalReady(onBothReady = null) {
        this.startListening();
        this.sendMessage(IFrameMessenger.TYPES.READY);
        this.isReady = true;

        if (onBothReady) {
            this.onBothReadyCallback = onBothReady;
        }

        this.on(IFrameMessenger.TYPES.READY, () => {
            console.log("Peer is ready");
            this.peerReady = true;
            this._checkBothReady();
        });

        // Check if peer is already ready
        this.sendMessage(IFrameMessenger.TYPES.READYCHECK);
    }

    /**
     * Internal message handler for postMessage events
     * @param {MessageEvent} event - The received message event
     */
    _handleMessage(event) {
        if (event.origin !== this.targetOrigin) return; // Ignore messages from unexpected origins
        const { type, data } = event.data;

        if (type === IFrameMessenger.TYPES.READY) {
            console.log("Received ready message");
            this.peerReady = true;
            this._checkBothReady();
            return;
        }

        if (type === IFrameMessenger.TYPES.READYCHECK) {
            if (this.isReady) {
                this.sendMessage(IFrameMessenger.TYPES.READY);
            }
            return;
        }

        this.listeners.get(type)?.(data, event); // Call registered callback if it exists
    }

    /**
     * Checks if both sides are ready and calls `onBothReadyCallback` if set.
     * This ensures readiness is acknowledged **no matter who sends `ready` first**.
     */
    _checkBothReady() {
        if (this.isReady && this.peerReady && this.onBothReadyCallback) {
            console.log("Both sides are ready");
            this.onBothReadyCallback();
            this.onBothReadyCallback = null; // Prevent multiple executions
        }
    }
}