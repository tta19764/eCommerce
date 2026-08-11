import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { loadStripe, Stripe, StripeElements } from '@stripe/stripe-js';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { PaymentsApiClient } from '../../../../core/api/payments-api';
import { PaymentStateService } from '../../../../core/services/payment-state.service';

@Component({
  selector: 'app-payment-page',
  imports: [AppCurrencyPipe, RouterLink],
  templateUrl: './payment-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaymentPage implements AfterViewInit {
  private readonly payments = inject(PaymentsApiClient);
  private readonly paymentState = inject(PaymentStateService);
  private readonly route = inject(ActivatedRoute);
  private readonly paymentElement = viewChild.required<ElementRef<HTMLDivElement>>('paymentElement');
  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private orderId: string | null = null;

  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly error = signal('');
  protected readonly amountMinor = signal(0);
  protected readonly currency = signal('USD');

  async ngAfterViewInit(): Promise<void> {
    try {
      this.orderId = this.route.snapshot.paramMap.get('orderId');
      if (!this.orderId) throw new Error('Order identifier is missing.');

      // The browser supplies only the order ID. PaymentApi resolves the frozen amount/currency and
      // returns the short-lived client secret while the publishable key is safe frontend configuration.
      const [config, payment] = await Promise.all([
        firstValueFrom(this.payments.getConfig()),
        firstValueFrom(this.payments.create(this.orderId)),
      ]);

      this.amountMinor.set(payment.amountMinor);
      this.currency.set(payment.currency);
      this.stripe = await loadStripe(config.publishableKey);
      if (!this.stripe) throw new Error('Stripe could not be initialized.');

      this.elements = this.stripe.elements({ clientSecret: payment.clientSecret });
      this.elements.create('payment').mount(this.paymentElement().nativeElement);
      this.loading.set(false);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
      this.loading.set(false);
    }
  }

  protected async submit(): Promise<void> {
    if (!this.stripe || !this.elements) return;
    this.submitting.set(true);
    this.error.set('');

    // This marker supports redirect UX only. The backend remains unpaid until the signed Stripe
    // webhook is committed and projected into OrderApi.
    if (this.orderId) {
      this.paymentState.markAsPaid(this.orderId);
    }

    const result = await this.stripe.confirmPayment({
      elements: this.elements,
      confirmParams: { return_url: `${window.location.origin}/orders` },
    });

    if (result.error) {
      this.error.set(result.error.message ?? 'Payment could not be completed.');
      this.submitting.set(false);
    }
  }
}
