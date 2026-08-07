import { computed, inject, Injectable, signal, effect } from '@angular/core';
import { UsersApiClient } from '../api/users-api';
import { AuthStore } from '../auth/auth-store';
import { UserProfile } from '../models/user-model';

@Injectable({ providedIn: 'root' })
export class UserStore {
  private readonly usersApi = inject(UsersApiClient);
  private readonly auth = inject(AuthStore);

  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal<boolean>(false);

  readonly userId = computed(() => this.profile()?.id ?? '');
  readonly firstName = computed(() => this.profile()?.firstName ?? '');
  readonly lastName = computed(() => this.profile()?.lastName ?? '');
  readonly fullName = computed(() => this.profile()?.fullName ?? '');
  readonly email = computed(() => this.profile()?.email ?? '');

  constructor() {
    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.loadProfile();
      } else {
        this.profile.set(null);
      }
    });
  }

  loadProfile(): void {
    if (!this.auth.isAuthenticated()) return;

    this.loading.set(true);
    this.usersApi.getOwn().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  getFormattedReviewerName(): string {
    const p = this.profile();
    if (!p) return 'Verified Customer';

    const first = p.firstName ? p.firstName.trim() : '';
    const last = p.lastName ? p.lastName.trim() : '';

    if (first && last) {
      const formattedFirst = first.charAt(0).toUpperCase() + first.slice(1).toLowerCase();
      const lastInitial = last.charAt(0).toUpperCase();
      return `${formattedFirst} ${lastInitial}.`;
    }

    if (first) {
      const formattedFirst = first.charAt(0).toUpperCase() + first.slice(1).toLowerCase();
      return `${formattedFirst} D.`;
    }

    if (p.email) {
      const username = p.email.split('@')[0];
      const parts = username.split(/[._\s-]/).filter((part) => part.length > 0);
      if (parts.length >= 2) {
        const f = parts[0].charAt(0).toUpperCase() + parts[0].slice(1).toLowerCase();
        const l = parts[1].charAt(0).toUpperCase();
        return `${f} ${l}.`;
      }
      const f = parts[0].charAt(0).toUpperCase() + parts[0].slice(1).toLowerCase();
      return `${f} D.`;
    }

    return 'Verified Customer';
  }

  clear(): void {
    this.profile.set(null);
  }
}
