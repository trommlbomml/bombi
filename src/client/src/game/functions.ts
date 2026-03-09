import {TILE_HEIGHT, TILE_WIDTH} from "./constants.ts";

export function tileXToWorldXCentered(value: number): number {
    return value * TILE_WIDTH + TILE_WIDTH/2;
}

export function tileYoWorldYCentered(value: number): number {
    return value * TILE_HEIGHT + TILE_HEIGHT/2;
}

export function worldXToTileX(value: number): number {
    return Math.floor(value / TILE_WIDTH);
}

export function worldYToTileY(value: number): number {
    return Math.floor(value / TILE_HEIGHT);
}