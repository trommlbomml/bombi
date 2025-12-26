import { LitElement, html } from "lit";
import { customElement } from "lit/decorators.js";

@customElement('app-element')
export class AppElement extends LitElement {
    render() {
    return html`
      <login-element></login-element>
    `; 
  }
}