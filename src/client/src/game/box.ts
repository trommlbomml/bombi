import Phaser from "phaser"
import {LAYER_FRINGE, TILESET_TEXTURE_KEY, TILE_HEIGHT, TILE_WIDTH} from "./constants.ts";

export class Box {
    private readonly sprite: Phaser.GameObjects.Sprite;
    constructor(scene: Phaser.Scene, container: Phaser.GameObjects.Container, tileX: number, tileY: number) {
        this.sprite = scene.add.sprite(
            tileX * TILE_WIDTH,
            tileY * TILE_HEIGHT,
            TILESET_TEXTURE_KEY,
            1);
        this.sprite.setOrigin(0, 0);
        this.sprite.setDepth(LAYER_FRINGE);
        container.add(this.sprite);
    }
    
    get visible(): boolean {
        return this.sprite.visible;
    }

    show() {
        this.sprite.visible = false;
    }

    hide() {
        this.sprite.visible = false;
    }
}