import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStore } from '../../auth/auth.store';
import { CartStore } from '../../../features/cart/data-access/cart.store';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShell {
  protected readonly auth = inject(AuthStore);
  protected readonly cart = inject(CartStore);
  protected readonly menuOpen = signal(false);
  private readonly router = inject(Router);

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
