import { LitElement, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import {appStore} from "../instrumentation/app-state.ts";
import BootScene from "./boot-scene.ts";

@customElement('game-page')
export class GamePage extends LitElement {
    @property({ type: Boolean })
    isVisible;

    private unsubscribe?: () => void;
    private game?: Phaser.Game;

    constructor() {
        super();
        this.isVisible = false;
    }

    firstUpdated(): void {
        this.unsubscribe = appStore.subscribe(state => {
            const newIsVisible = state.state === 'InGame';
            if (this.isVisible !== newIsVisible) {
                this.isVisible = newIsVisible;
                if (this.isVisible) {
                    const config: Phaser.Types.Core.GameConfig = {
                        type: Phaser.AUTO,
                        parent: "game",
                        width: 800,
                        height: 600,
                        backgroundColor: "#1d1d1d",
                        scene: [BootScene]
                    }

                    this.game = new Phaser.Game(config);
                } else {
                    this.game?.destroy(false);
                    this.game = undefined;
                }
            }
        });
    }

    disconnectedCallback() {
        this.unsubscribe?.();
        super.disconnectedCallback();
        this.game?.destroy(false);
        this.game = undefined;
    }

    render() {
        return html`
            <div id="game"></div>
        `;
    }
    protected createRenderRoot() {
        return this;
    }
}