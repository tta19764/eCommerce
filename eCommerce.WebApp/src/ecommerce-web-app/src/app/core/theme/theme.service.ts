import { Injectable, signal } from '@angular/core';

export type ThemeMode = 'dark' | 'light';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'apex_theme';

  readonly currentTheme = signal<ThemeMode>(this.getInitialTheme());

  constructor() {
    this.applyTheme(this.currentTheme());
  }

  toggleTheme(): void {
    const nextTheme: ThemeMode = this.currentTheme() === 'dark' ? 'light' : 'dark';
    this.setTheme(nextTheme);
  }

  setTheme(theme: ThemeMode): void {
    this.currentTheme.set(theme);
    try {
      localStorage.setItem(this.storageKey, theme);
    } catch {
      // Ignore storage errors in restricted contexts
    }
    this.applyTheme(theme);
  }

  private getInitialTheme(): ThemeMode {
    try {
      const saved = localStorage.getItem(this.storageKey) as ThemeMode | null;
      if (saved === 'dark' || saved === 'light') {
        return saved;
      }
    } catch {
      // Fallback if localStorage is unreadable
    }
    return 'dark';
  }

  private applyTheme(theme: ThemeMode): void {
    const root = document.documentElement;
    if (theme === 'light') {
      root.classList.add('light');
      root.classList.remove('dark');
      root.setAttribute('data-theme', 'light');
    } else {
      root.classList.add('dark');
      root.classList.remove('light');
      root.setAttribute('data-theme', 'dark');
    }
  }
}
