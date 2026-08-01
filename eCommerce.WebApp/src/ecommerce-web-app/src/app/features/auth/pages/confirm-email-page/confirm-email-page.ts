import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-confirm-email-page',
  imports: [RouterLink],
  template: `<section class="mx-auto max-w-2xl px-4 py-16 text-center">
    <div class="rounded-xl border border-blue-200 bg-blue-50 p-8 text-blue-900">
      <h1 class="text-2xl font-bold">Thank you, {{ email }}.</h1>
      <p class="mt-3">
        Your confirmation link reached the eCommerce application. The backend confirmation endpoint
        is not yet exposed, so no further action is required here.
      </p>
      <a
        class="mt-6 inline-block rounded-lg bg-brand-600 px-4 py-2 font-semibold text-white"
        routerLink="/login"
      >
        Continue to sign in
      </a>
    </div>
  </section>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmEmailPage {
  protected readonly email = inject(ActivatedRoute).snapshot.queryParamMap.get('email') ?? 'friend';
}
