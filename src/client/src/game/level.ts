import Phaser from "phaser"
import {
    LAYER_BASE,
    LEVEL_HEIGHT,
    LEVEL_WIDTH,
    TILESET_TEXTURE_KEY,
    TILE_HEIGHT,
    TILE_WIDTH, TILE_GRASS, TILE_WALL, TILE_BOX
} from "./constants.ts";
import {Box} from "./box.ts";
import {Bomb} from "./bomb.ts";
import {Figure} from "./figure.ts";
import {BinaryReader} from "../instrumentation/networking/binary-reader.ts";

export class Level {
    private readonly scene: Phaser.Scene;

    private readonly offsetContainer: Phaser.GameObjects.Container;
    private readonly baseLayer: Array<Phaser.GameObjects.Sprite> = [];
    private readonly boxLayer: Array<Box> = [];

    private bombs: Array<Bomb> = [];


    constructor(scene: Phaser.Scene) {
        this.scene = scene;
        this.offsetContainer = scene.add.container(0, TILE_HEIGHT);

        for(let y = 0; y < LEVEL_HEIGHT; y++) {
            for(let x = 0; x < LEVEL_WIDTH; x++) {
                const sprite = scene.add.sprite(
                    x * TILE_WIDTH,
                    y * TILE_HEIGHT, TILESET_TEXTURE_KEY, 0);
                sprite.setOrigin(0, 0);
                sprite.setDepth(LAYER_BASE);
                this.offsetContainer.add(sprite);

                this.baseLayer.push(sprite);

                const box = new Box(scene, this.offsetContainer, x, y);
                box.hide();
                this.boxLayer.push(box);
            }
        }
    }

    update(elapsedSeconds: number) {
        let anyExploded = false;
        this.bombs.forEach(b => {
            b.update(elapsedSeconds);
            if (b.isExploded) {
                b.destroy();
                anyExploded = true;
            }
        });
        if (anyExploded) {
            this.bombs = this.bombs.filter(b => !b.isExploded);
        }
    }

    get container(): Phaser.GameObjects.Container {
        return this.offsetContainer;
    }

    placeBomb(figure: Figure): void {
        this.bombs.push(new Bomb(this.scene, this, figure.tilePositionX, figure.tilePositionY));
    }

    sync(message: ArrayBuffer) {
        const binaryReader = new BinaryReader(new Uint8Array(message));

        binaryReader.readInt32();

        for(let y = 0; y < LEVEL_HEIGHT; y++) {
            for(let x = 0; x < LEVEL_WIDTH; x++) {
                const tile = binaryReader.readUint8();
                if (tile === TILE_GRASS) {
                    this.baseLayer[y * LEVEL_WIDTH + x].setFrame(0);
                    this.boxLayer[y * LEVEL_WIDTH + x].hide();
                }
                else if (tile === TILE_WALL) {
                    this.baseLayer[y * LEVEL_WIDTH + x].setFrame(2);
                    this.boxLayer[y * LEVEL_WIDTH + x].hide();
                }
                else if (tile === TILE_BOX) {
                    this.boxLayer[y * LEVEL_WIDTH + x].show();
                }
            }
        }
    }
}