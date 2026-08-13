import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { apiErrorMessage } from '../../../core/api/api-base';
import { SellerApiClient } from '../../../core/api/seller-api';

@Component({
  selector: 'app-store-review-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './store-review-form.html',
  styleUrl: './store-review-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoreReviewFormComponent {
  private readonly sellerApi = inject(SellerApiClient);

  // Inputs strictly passed from valid navigation context
  readonly storeId = input.required<string>();
  readonly sellerOrderId = input.required<string>();

  readonly reviewCreated = output<string>();

  protected readonly submitting = signal(false);
  protected readonly error = signal('');
  protected readonly success = signal('');

  protected readonly form = new FormGroup({
    rating: new FormControl(5, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1), Validators.max(5)],
    }),
    comment: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3)],
    }),
  });

  protected setRating(stars: number): void {
    this.form.controls.rating.setValue(stars);
  }

  protected submitReview(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set('');
    this.success.set('');

    const val = this.form.getRawValue();
    this.sellerApi
      .createStoreReview(this.storeId(), {
        sellerOrderId: this.sellerOrderId(),
        rating: val.rating,
        comment: val.comment.trim(),
      })
      .subscribe({
        next: (reviewId) => {
          this.submitting.set(false);
          this.success.set('Store review published successfully!');
          this.form.reset({ rating: 5, comment: '' });
          this.reviewCreated.emit(reviewId);
        },
        error: (err) => {
          console.error('[StoreReviewForm submitReview error]:', err);
          this.submitting.set(false);
          this.error.set(apiErrorMessage(err));
        },
      });
  }
}
