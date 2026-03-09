import {BOMB_ANIMATION_SPEED, BOMB_BURNTIME, LAYER_FRINGE, TILESET_TEXTURE_KEY} from "./constants.ts";
import {tileXToWorldXCentered, tileYoWorldYCentered} from "./functions.ts";
import type {Level} from "./level.ts";

export class Bomb {
    private readonly sprite: Phaser.GameObjects.Sprite;
    private currentBurnTime = 0;
    constructor(scene: Phaser.Scene, level: Level, tileX: number, tileY: number) {
        this.sprite = scene.add.sprite(tileXToWorldXCentered(tileX), tileYoWorldYCentered(tileY), TILESET_TEXTURE_KEY, 3);
        this.sprite.setDepth(LAYER_FRINGE);

        level.container.add(this.sprite);
    }

    get isExploded(): boolean {
        return this.currentBurnTime >= BOMB_BURNTIME;
    }

    destroy() {
        this.sprite.destroy();
    }

    update(elapsedSeconds: number): void {

        this.currentBurnTime += elapsedSeconds;
        const scale = 0.75 + Math.cos(this.currentBurnTime / BOMB_BURNTIME * Math.PI * BOMB_ANIMATION_SPEED) * 0.05;
        this.sprite.setScale(scale);
    }
}