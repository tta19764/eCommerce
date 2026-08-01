import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import { Product } from '../../../../core/models/product.models';

@Component({
  selector: 'app-admin-products-page',
  imports: [CurrencyPipe, ReactiveFormsModule],
  templateUrl: './admin-products-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminProductsPage {
  private readonly api = inject(ProductsApiClient);

  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly formOpen = signal(false);
  protected readonly error = signal('');
  protected readonly success = signal('');

  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    price: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    currencyCode: new FormControl('USD', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(3)],
    }),
    quantity: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0)],
    }),
  });

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.api.getPage({ page: 1, pageSize: 50 }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  protected create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');
    this.api.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.form.reset({
          name: '',
          description: '',
          price: 0,
          currencyCode: 'USD',
          quantity: 0,
        });
        this.formOpen.set(false);
        this.success.set('Product created successfully.');
        this.saving.set(false);
        this.load();
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }

  protected remove(product: Product): void {
    if (!confirm(`Delete "${product.name}"?`)) {
      return;
    }

    this.api.delete(product.id).subscribe({
      next: () => {
        this.success.set('Product deleted successfully.');
        this.load();
      },
      error: (error) => this.error.set(apiErrorMessage(error)),
    });
  }
}
