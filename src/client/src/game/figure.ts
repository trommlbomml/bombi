import Phaser from "phaser"
import {tileXToWorldXCentered, tileYoWorldYCentered} from "./functions.ts";
import {FIGURE_ANINMATION_SPEED, FIGURE_MOVEMENT_SPEED, FIGURES_TEXTURE_KEY, LAYER_FRINGE} from "./constants.ts";

export type FigureColor = 'white' | 'red' | 'blue' | 'black';
export type FaceDirection = 'up' | 'down' | 'left' | 'right';

const FRAMES_PER_ROW = 6;

export function toAnimationFrame(x: number, y: number): number {
    return y * FRAMES_PER_ROW + x;
}

function createFigureAnimation(scene: Phaser.Scene, name: string, startX: number, startY: number) {
    scene.anims.create({
        key: name,
        frames: scene.anims.generateFrameNumbers(FIGURES_TEXTURE_KEY, { start: toAnimationFrame(startX, startY), end: toAnimationFrame(startX+2, startY) }),
        frameRate: FIGURE_ANINMATION_SPEED,
        repeat: -1
    });
}

export function createFigureAnimations(scene: Phaser.Scene) {
    createFigureAnimation(scene, 'white_up', 0, 0);
    createFigureAnimation(scene, 'white_right', 0, 1);
    createFigureAnimation(scene, 'white_down', 0, 2);
    createFigureAnimation(scene, 'white_left', 0, 3);
    createFigureAnimation(scene, 'blue_up', 0, 4);
    createFigureAnimation(scene, 'blue_right', 0, 5);
    createFigureAnimation(scene, 'blue_down', 0, 6);
    createFigureAnimation(scene, 'blue_left', 0, 7);
    createFigureAnimation(scene, 'red_up', 3, 0);
    createFigureAnimation(scene, 'red_right', 3, 1);
    createFigureAnimation(scene, 'red_down', 3, 2);
    createFigureAnimation(scene, 'red_left', 3, 3);
    createFigureAnimation(scene, 'black_up', 3, 4);
    createFigureAnimation(scene, 'black_right', 3, 5);
    createFigureAnimation(scene, 'black_down', 3, 6);
    createFigureAnimation(scene, 'black_left', 3, 7);
}

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

            this.sprite.anims.play(`${this.color}_${this.faceDirection}`, true);
        } else {
            this.updateCurrentFrame();
        }
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

        this.sprite.setFrame(toAnimationFrame(x,y));
    }
}