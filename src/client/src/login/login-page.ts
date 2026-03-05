import { LitElement, html } from "lit";
import { customElement, property } from "lit/decorators.js";
import {appStore} from "../instrumentation/app-state.ts";

@customElement('login-page')
export class LoginPage extends LitElement {
    @property({ type: Boolean })
    isVisible;

    private unsubscribe?: () => void;

    constructor() {
        super();
        this.isVisible = true;
    }

    connectedCallback() {
        super.connectedCallback();
        this.unsubscribe = appStore.subscribe(state => {
            this.isVisible = state.state === 'LoggedOut';
        })
    }

    disconnectedCallback() {
        this.unsubscribe?.();
        super.disconnectedCallback();
    }

    onLogin() {
        appStore.getState().login();
    }

render() {
    if (this.isVisible) {
        return html`
        <div class="is-flex flex-direction-row">
          <div class="card">
            <div class="card-content">
              <div class="content">
                <h2>
                  Bombi Login
                </h2>
                <form>
                  <div class="field">
                    <label class="label">
                      Username
                    </label>
                    <input class="input" type="text">
                  </div>
                  <div class="field">
                    <label class="label">
                      Password
                    </label>
                    <input class="input" type="password">
                  </div>
                  <button class="button is-primary" @click=${this.onLogin}>
                    Login
                  </button>
                  <button class="button is-ghost">
                    Register
                  </button>
                </form>
              </div>
            </div>
          </div>
        </div>
        `;
    }
  }

  protected createRenderRoot() {
    return this;
  }
}