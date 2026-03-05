import Phaser from "phaser"
import {SCREEN_HEIGHT, SCREEN_WIDTH} from "./constants.ts";

export default class GameScene extends Phaser.Scene {
  constructor() {
    super("GameScene")
  }

  preload() {
    this.load.image("skin", "/assets/defaultskin.png")
  }

  create() {
    this.add.image(SCREEN_WIDTH/2, SCREEN_HEIGHT/2, "skinn")
  }
}
