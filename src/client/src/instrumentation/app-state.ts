import { createStore } from 'zustand/vanilla';

export type GameState = 'LoggedOut' | 'PreLobby' | 'Lobby' |  'InGame';

export interface AppState {
    readonly state: GameState;
    login: () => void;
    joinedLobby: () => void;
    startGame: () => void;
    logout: () => void;
}

export const appStore = createStore<AppState>(set => ({
    state: 'LoggedOut',
    login: () => set({state: 'PreLobby'}),
    joinedLobby: () => set({state: 'Lobby'}),
    startGame: () => set({state: 'InGame'}),
    logout: () => set({ state: 'LoggedOut' })
}));