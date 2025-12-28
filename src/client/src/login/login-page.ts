import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('login-page')
export class LoginPage extends LitElement {
render() {
    return html`
    <div class="container is-max-desktop mt-6">
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
              <button class="button is-primary">
                Login
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
    `; 
  }

  protected createRenderRoot() {
    return this;
  }
}