import type {Session} from "../models/session.ts";

export function querySessions(): Promise<Array<Session>> {
    return new Promise<Array<Session>>((resolve) => {
        resolve([
        ])
    })
}