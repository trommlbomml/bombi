import { LitElement, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import {appStore} from "../instrumentation/app-state.ts";
@customElement('game-element')
export class GameElement extends LitElement {
    @property({ type: Boolean })
    isVisible;

    private unsubscribe?: () => void;

    constructor() {
        super();
        this.isVisible = false;
    }


    connectedCallback() {
        super.connectedCallback();
        this.unsubscribe = appStore.subscribe(state => {
            this.isVisible = state.state === 'InGame';
        });
    }

    disconnectedCallback() {
        this.unsubscribe?.();
        super.disconnectedCallback();
    }

    render() {
        if (this.isVisible) {
            return html`
                <div class="card">
                    blubb
                </div>
    `;
        }
    }
}