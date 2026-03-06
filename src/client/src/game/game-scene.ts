import Phaser from "phaser"
import {SKIN_TEXTURE_KEY, TILE_HEIGHT, TILE_WIDTH} from "./constants.ts";
import {Level} from "./level.ts";

export default class GameScene extends Phaser.Scene {
  private _level?: Level;

  constructor() {
    super("GameScene")
  }

  preload() {
    this.load.spritesheet(SKIN_TEXTURE_KEY, "/assets/defaultskin.png", {
      frameWidth: TILE_WIDTH,
      frameHeight: TILE_HEIGHT
    });
  }

  create() {
    this._level = new Level(this);
  }
}
