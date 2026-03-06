import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { LoginPage } from "./login/login-page"
import { SessionPage} from "./lobby/session-page.ts";
import { GamePage} from "./game/game-page.ts";

@customElement('app-element')
export class AppElement extends LitElement {

    render() {
        return html`
            <game-page class="page"></game-page>
        <login-page class="page"></login-page>
        <lobby-page class="page"></lobby-page>
        <session-page class="page"></session-page>
        `; 
  }

  protected createRenderRoot() {
    return this;
  }
}