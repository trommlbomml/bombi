import Phaser from "phaser"
import {LEVEL_HEIGHT, LEVEL_WIDTH, SKIN_TEXTURE_KEY, TILE_HEIGHT, TILE_WIDTH} from "./constants.ts";

const BASE_LAYER: Array<number> = [
    2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,
    2,0,0,1,1,1,1,1,1,1,1,1,0,0,2,
    2,0,2,1,2,1,2,1,2,1,2,1,2,0,2,
    2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,
    2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,
    2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,
    2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,
    2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,
    2,1,2,1,2,1,2,1,2,1,2,1,2,1,2,
    2,1,1,1,1,1,1,1,1,1,1,1,1,1,2,
    2,0,2,1,2,1,2,1,2,1,2,1,2,0,2,
    2,0,0,1,1,1,1,1,1,1,1,1,0,0,2,
    2,2,2,2,2,2,2,2,2,2,2,2,2,2,2
];

export class Level {
    private readonly baseLayer: Array<Phaser.GameObjects.Sprite> = [];

    constructor(scene: Phaser.Scene) {
        for(let y = 0; y < LEVEL_HEIGHT; y++) {
            for(let x = 0; x < LEVEL_WIDTH; x++) {
                const tileId = BASE_LAYER[y * LEVEL_WIDTH + x];
                const sprite = scene.add.sprite(
                    x * TILE_WIDTH,
                    y * TILE_HEIGHT, SKIN_TEXTURE_KEY,
                    tileId === 2 ? 2 : 0);
                sprite.setOrigin(0, 0);

                this.baseLayer.push(sprite);
            }
        }
    }

}