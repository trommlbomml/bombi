import Phaser from "phaser"
import {
    BOX_SPAWN_PROBABILITY,
    LAYER_BASE,
    LEVEL_HEIGHT,
    LEVEL_WIDTH,
    TILESET_TEXTURE_KEY,
    TILE_HEIGHT,
    TILE_WIDTH
} from "./constants.ts";
import {Box} from "./box.ts";

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
    private readonly offsetContainer: Phaser.GameObjects.Container;
    private readonly baseLayer: Array<Phaser.GameObjects.Sprite> = [];
    private readonly boxLayer: Array<Box|null> = [];

    constructor(scene: Phaser.Scene) {
        this.offsetContainer = scene.add.container(0, TILE_HEIGHT);
        this.boxLayer = Array(LEVEL_WIDTH * LEVEL_HEIGHT).fill(null);

        for(let y = 0; y < LEVEL_HEIGHT; y++) {
            for(let x = 0; x < LEVEL_WIDTH; x++) {
                const tileId = BASE_LAYER[y * LEVEL_WIDTH + x];
                const sprite = scene.add.sprite(
                    x * TILE_WIDTH,
                    y * TILE_HEIGHT, TILESET_TEXTURE_KEY,
                    tileId === 2 ? 2 : 0);
                sprite.setOrigin(0, 0);
                sprite.setDepth(LAYER_BASE);
                this.offsetContainer.add(sprite);

                this.baseLayer.push(sprite);

                if (tileId === 1 && Phaser.Math.FloatBetween(0, 1) > 1 - BOX_SPAWN_PROBABILITY) {
                    const box = new Box(scene, this.offsetContainer, x, y);
                    this.boxLayer[y * LEVEL_WIDTH + x] = box;
                }
            }
        }
    }

    get container(): Phaser.GameObjects.Container {
        return this.offsetContainer;
    }

}