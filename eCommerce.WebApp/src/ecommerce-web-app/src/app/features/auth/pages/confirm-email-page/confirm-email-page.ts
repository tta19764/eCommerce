import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthApiClient } from '../../../../core/api/auth-api';

type ConfirmationState = 'loading' | 'success' | 'invalid';

@Component({
  selector: 'app-confirm-email-page',
  imports: [RouterLink],
  templateUrl: './confirm-email-page.html',
  styleUrl: './confirm-email-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmEmailPage {
  private readonly api = inject(AuthApiClient);
  private readonly route = inject(ActivatedRoute);

  protected readonly state = signal<ConfirmationState>('loading');
  protected readonly message = signal('');

  constructor() {
    const accountId = this.route.snapshot.queryParamMap.get('accountId');
    const email = this.route.snapshot.queryParamMap.get('email');

    if (!accountId || !email) {
      this.state.set('invalid');
      this.message.set('The confirmation link is missing the required account information.');
      return;
    }

    this.api.confirmEmail(accountId, email).subscribe({
      next: () => this.state.set('success'),
      error: (error) => {
        this.state.set('invalid');
        this.message.set(this.confirmationErrorMessage(error));
      },
    });
  }

  private confirmationErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'The account for this confirmation link was not found.';
    }

    if (error instanceof HttpErrorResponse && error.status === 400) {
      return 'This confirmation link is invalid or has expired.';
    }

    return 'Email confirmation is temporarily unavailable. Please try again later.';
  }
}
