import { createStore } from 'zustand/vanilla';

export type GameState = 'LoggedOut' | 'PreLobby' | 'Lobby' |  'InGame';

export interface AppState {
    readonly state: GameState;
    login: () => void;
    logout: () => void;
}

export const appStore = createStore<AppState>(set => ({
    state: 'LoggedOut',
    login: () => set({state: 'PreLobby'}),
    logout: () => set({ state: 'LoggedOut' })
}));