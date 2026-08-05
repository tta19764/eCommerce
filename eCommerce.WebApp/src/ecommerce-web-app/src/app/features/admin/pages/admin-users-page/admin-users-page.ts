import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountsApiClient } from '../../../../core/api/accounts-api.client';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { AuthApiClient } from '../../../../core/api/auth-api.client';
import { Account } from '../../../../core/models/account.models';

@Component({
  selector: 'app-admin-users-page',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './admin-users-page.html',
  styleUrl: './admin-users-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUsersPage {
  private readonly api = inject(AccountsApiClient);
  private readonly authApi = inject(AuthApiClient);

  protected readonly accounts = signal<Account[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly formOpen = signal(false);
  protected readonly error = signal('');
  protected readonly success = signal('');
  protected readonly form = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
  });

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

  protected applicationRole(account: Account): 'Admin' | 'Seller' | 'Customer' {
    if (account.roles.some((role) => role.name.toLowerCase() === 'admin')) return 'Admin';
    if (account.roles.some((role) => role.name.toLowerCase() === 'seller')) return 'Seller';
    return 'Customer';
  }

  protected createAdmin(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.authApi.registerAdmin(this.form.getRawValue()).subscribe({
      next: () => {
        this.form.reset({
          firstName: '',
          lastName: '',
          email: '',
          password: '',
        });
        this.formOpen.set(false);
        this.success.set('Administrator account created successfully.');
        this.saving.set(false);
        this.load();
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
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
