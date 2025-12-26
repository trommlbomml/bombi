import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('login-element')
export class LoginElement extends LitElement {
render() {
    return html`
      <div>Hello from LoginElement!</div>
    `; 
  }
}