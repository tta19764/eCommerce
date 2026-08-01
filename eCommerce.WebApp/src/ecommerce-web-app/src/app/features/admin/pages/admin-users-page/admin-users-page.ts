import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AccountsApiClient } from '../../../../core/api/accounts-api.client';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { Account } from '../../../../core/models/account.models';

@Component({
  selector: 'app-admin-users-page',
  imports: [DatePipe],
  templateUrl: './admin-users-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUsersPage {
  private readonly api = inject(AccountsApiClient);

  protected readonly accounts = signal<Account[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly success = signal('');

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.api.getPage().subscribe({
      next: (result) => {
        this.accounts.set(result.items);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  protected applicationRole(account: Account): 'Admin' | 'Customer' {
    return account.roles.some((role) => role.name.toLowerCase() === 'admin') ? 'Admin' : 'Customer';
  }

  protected remove(account: Account): void {
    if (!confirm(`Deactivate the account for ${account.email}?`)) {
      return;
    }

    this.api.delete(account.id).subscribe({
      next: () => {
        this.success.set('Account deactivated successfully.');
        this.load();
      },
      error: (error) => this.error.set(apiErrorMessage(error)),
    });
  }
}
