import Phaser from "phaser"
import {tileXToWorldXCentered, tileYoWorldYCentered} from "./functions.ts";
import {FIGURE_MOVEMENT_SPEED, FIGURES_TEXTURE_KEY, LAYER_FRINGE} from "./constants.ts";

export type FigureColor = 'white' | 'red' | 'blue' | 'black';
export type FaceDirection = 'up' | 'down' | 'left' | 'right';

const FRAMES_PER_ROW = 6;

export class Figure {
    private readonly sprite: Phaser.GameObjects.Sprite;
    private readonly color: FigureColor;
    private readonly cursor: Phaser.Types.Input.Keyboard.CursorKeys;

    private frameIndex = 0;
    private faceDirection: FaceDirection;

    constructor(scene: Phaser.Scene, container: Phaser.GameObjects.Container, color: FigureColor) {
        this.sprite = scene.add.sprite(
            0, 0,
            FIGURES_TEXTURE_KEY,
            0);
        this.sprite.setDepth(LAYER_FRINGE);
        container.add(this.sprite);

        this.color = color;
        this.faceDirection = 'down';
        this.frameIndex = 1;
        this.updateCurrentFrame();

        this.cursor = scene.input.keyboard!.createCursorKeys();
    }

    setTilePosition(x: number, y: number) {
        this.sprite.setPosition(tileXToWorldXCentered(x), tileYoWorldYCentered(y))
    }

    update(elapsedSeconds: number) {
        let moveX = 0;
        let moveY = 0;
        if (this.cursor.left.isDown) {
            this.faceDirection = 'left';
            moveX = -1;
        } else if (this.cursor.right.isDown) {
            this.faceDirection = 'right';
            moveX = 1;
        }else if (this.cursor.up.isDown) {
            this.faceDirection = 'up';
            moveY = -1;
        } else if (this.cursor.down.isDown) {
            this.faceDirection = 'down';
            moveY = 1;
        }
        if (moveX !== 0 || moveY !== 0) {
            this.sprite.x += moveX * FIGURE_MOVEMENT_SPEED * elapsedSeconds;
            this.sprite.y += moveY * FIGURE_MOVEMENT_SPEED * elapsedSeconds;
        }

        this.updateCurrentFrame();
    }

    private updateCurrentFrame(): void {
        let x = 0;
        let y = 0;

        switch (this.color) {
            case "red":
                x = 3;
                break;
            case "blue":
                y = 4;
                break;
            case "black":
                x = 3;
                y = 4;
                break;
        }

        x += this.frameIndex;

        switch (this.faceDirection) {
            case "down":
                y += 2;
                break;
            case "left":
                y += 3;
                break;
            case "right":
                y += 1;
                break;

        }

        this.sprite.setFrame(y * FRAMES_PER_ROW + x);
    }
}