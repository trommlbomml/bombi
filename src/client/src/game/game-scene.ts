import Phaser from "phaser"
import {
  TILESET_TEXTURE_KEY,
  TILE_HEIGHT,
  TILE_WIDTH,
  FIGURE_WIDTH,
  FIGURE_HEIGHT,
  FIGURES_TEXTURE_KEY
} from "./constants.ts";
import {Level} from "./level.ts";
import {createFigureAnimations, Figure} from "./figure.ts";

export default class GameScene extends Phaser.Scene {
  private level!: Level;
  private figure!: Figure;

  constructor() {
    super("GameScene")
  }

  preload() {
    this.load.spritesheet(TILESET_TEXTURE_KEY, "/assets/tileset.png", {
      frameWidth: TILE_WIDTH,
      frameHeight: TILE_HEIGHT
    });
    this.load.spritesheet(FIGURES_TEXTURE_KEY, "/assets/figures24x32.png", {
      frameWidth: FIGURE_WIDTH,
      frameHeight: FIGURE_HEIGHT
    });
  }

  create() {
    createFigureAnimations(this);

    this.level = new Level(this);
    this.figure = new Figure(this, this.level, 'white');
    this.figure.setTilePosition(1,1);

  }

  update(_time: number, delta: number) {
    const elapsedSeconds = delta / 1000;
    this.level.update(elapsedSeconds);
    this.figure.update(elapsedSeconds);
  }
}
