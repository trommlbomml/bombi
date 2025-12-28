import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";
import { LoginPage } from "./login/login-page"

@customElement('app-element')
export class AppElement extends LitElement {
    render() {
        return html`
        <login-page></login-page>
        `; 
  }

  protected createRenderRoot() {
    return this;
  }
}