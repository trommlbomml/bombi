import { LitElement, html } from "lit";
import {customElement, property, state} from "lit/decorators.js";
import {appStore} from "../instrumentation/app-state.ts";
import type {Session} from "./models/session.ts";
import {querySessions} from "./services/query-sessions.ts";
import {gameClient} from "../instrumentation/networking/game-client.ts";

@customElement('session-page')
export class SessionPage extends LitElement {
    @property({ type: Boolean })
    isVisible;

    @state()
    sessions: Array<Session> = [];

    private unsubscribe?: () => void;

    constructor() {
        super();
        this.isVisible = false;
    }


    connectedCallback() {
        super.connectedCallback();
        this.unsubscribe = appStore.subscribe(state => {
            this.isVisible = state.state === 'PreLobby';
        });

        querySessions()
            .then(sessions => {
                this.sessions = sessions;
            });
    }

    disconnectedCallback() {
        this.unsubscribe?.();
        super.disconnectedCallback();
    }

    private onCreateSession() {
        gameClient.createLobby()
            .then(() => gameClient.startGame())
            .catch(err => {
                console.log(err);
            });
    }

    render() {
        if (this.isVisible) {
            return html`
                <div class="card">
                    <div class="card-content">
                        <div class="content">
                            <h2>
                                Sessions
                            </h2>
                            <button class="button is-primary" @click=${this.onCreateSession}>
                                Create Own Session
                            </button>
                            ${this.renderSessions()}
                        </div>
                    </div>
                </div>
    `;
        }
    }

    renderSessions() {
        if (this.sessions.length === 0) {
            return html`
                <div class="notification mt-4">
                   There is no session started yet.
                </div>
            `;
        }

        return html`
            <div class="scrollable-content-y-80">
                <table class="table ">
                    <tr>
                        <th>
                            Name
                        </th>
                        <th>
                            Players
                        </th>
                        <th>
                        </th>
                    </tr>
                    ${this.sessions.map((session) => this.renderSession(session))}
                </table>
            </div>
        `
    }

    renderNoSessionsAvailable() {
        if (this.sessions.length === 0) {
            return html`
                <div class="notification">
                   There is no session started yet.
                </div>
            `
        }
        return html``;
    }

    renderSession(session: Session) {
        return html`
        <tr>
            <td style="min-width: 15em">
                ${session.name}
            </td>
            <td>
                ${session.playerCount} / ${session.maxPlayerCount}
            </td>
            <td>
                <button class="button is-primary" ?disabled=${session.playerCount >= session.maxPlayerCount}>
                    Join
                </button>
            </td>
        </tr>`
    }

    protected createRenderRoot() {
        return this;
    }
}