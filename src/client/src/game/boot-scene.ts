import Phaser from "phaser"

export default class BootScene extends Phaser.Scene {
  constructor() {
    super("BootScene")
  }

  preload() {
    this.load.image("logo", "/assets/defaultskin.png")
  }

  create() {
    this.add.image(400, 300, "logo")
  }
}
