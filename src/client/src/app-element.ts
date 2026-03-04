import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { LoginPage } from "./login/login-page"
import { SessionPage} from "./lobby/session-page.ts";

@customElement('app-element')
export class AppElement extends LitElement {

    render() {
        return html`
        <login-page class="page"></login-page>
        <lobby-page class="page"></lobby-page>
        <session-page class="page"></session-page>
        <game-page class="page"></game-page>
        `; 
  }

  protected createRenderRoot() {
    return this;
  }
}