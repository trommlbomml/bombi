import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { LoginPage } from "./login/login-page"

@customElement('app-element')
export class AppElement extends LitElement {
    render() {
        return html`
        <login-page></login-page>
        <lobby-page></lobby-page>
        <session-page></session-page>
        <game-page></game-page>
        `; 
  }

  protected createRenderRoot() {
    return this;
  }
}