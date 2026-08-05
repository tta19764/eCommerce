import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { AuthStore } from '../../auth/auth.store';
import { CartStore } from '../../../features/cart/data-access/cart.store';
import { MessagingService } from '../../api/messaging.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShell {
  private readonly router = inject(Router);
  private readonly messaging = inject(MessagingService);

  protected readonly auth = inject(AuthStore);
  protected readonly cart = inject(CartStore);
  protected readonly menuOpen = signal(false);
  protected readonly profileMenuOpen = signal(false);

  constructor() {
    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.messaging.startConnection();
      } else {
        this.messaging.stopConnection();
      }
    });

    // Close mobile menu on navigation
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.menuOpen.set(false);
      this.profileMenuOpen.set(false);
    });
  }

  protected readonly isAdminPortal = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects.startsWith('/admin')),
    ),
    { initialValue: this.router.url.startsWith('/admin') },
  );

  protected readonly isSellerPortal = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects.startsWith('/seller')),
    ),
    { initialValue: this.router.url.startsWith('/seller') },
  );

  protected logout(): void {
    this.auth.logout();
    this.profileMenuOpen.set(false);
    this.router.navigate(['/']);
  }
}
