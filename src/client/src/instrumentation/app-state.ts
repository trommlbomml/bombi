import { createStore } from 'zustand/vanilla';

export interface AppState {
    readonly loggedIn: boolean;
    login: () => void;
    logout: () => void;
}

export const appStore = createStore<AppState>(set => ({
    loggedIn: false,
    login: () => set({loggedIn: true}),
    logout: () => set({ loggedIn: true })
}));