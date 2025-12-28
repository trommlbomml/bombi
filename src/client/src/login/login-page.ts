import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('login-page')
export class LoginPage extends LitElement {
render() {
    return html`
      <div class=login-page>
        <h2>Login to Bombi</h2>
        <form>
          <div class="form-row">
            <label>
                Username
            </label>
            <input type="text">
          </div>
          <div class="form-row">
              <label>
                Password
            </label>
            <input type="text">
          </div>
        <button>
          Login
        </button>
        </form>
      </div>
    `; 
  }

  protected createRenderRoot() {
    return this;
  }
}