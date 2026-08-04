import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api.client';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import { UsersApiClient } from '../../../../core/api/users-api.client';
import {
  CreateProductRequest,
  ProductCategory,
  ProductType,
  ProductTypeOption,
} from '../../../../core/models/product.models';

@Component({
  selector: 'app-seller-products-page',
  imports: [ReactiveFormsModule],
  templateUrl: './seller-products-page.html',
  styleUrl: './seller-products-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerProductsPage {
  protected readonly maxProductImages = 8;

  private readonly productsApi = inject(ProductsApiClient);
  private readonly imagesApi = inject(ImagesApiClient);
  private readonly usersApi = inject(UsersApiClient);

  protected readonly categories = signal<ProductCategory[]>([]);
  protected readonly productTypes = signal<ProductTypeOption[]>([]);
  protected readonly sellerId = signal('');
  protected readonly loadingOptions = signal(true);
  protected readonly saving = signal(false);
  protected readonly uploadingImages = signal(false);
  protected readonly error = signal('');
  protected readonly success = signal('');
  protected readonly imageIds = signal<string[]>([]);
  protected readonly draggedImageIndex = signal<number | null>(null);

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
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    productType: new FormControl<ProductType>('Physical', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor() {
    this.loadFormOptions();
  }

  protected uploadImages(event: Event): void {
    const input = event.target as HTMLInputElement;
    const availableSlots = this.maxProductImages - this.imageIds().length;
    const files = Array.from(input.files ?? []);
    input.value = '';

    if (!files.length) {
      return;
    }

    if (files.length > availableSlots) {
      this.error.set(`You can add only ${availableSlots} more image(s).`);
      return;
    }

    if (files.some((file) => !this.isSupportedImage(file) || file.size > 10 * 1024 * 1024)) {
      this.error.set('Each image must be a JPEG, PNG, WebP, or GIF no larger than 10 MB.');
      return;
    }

    this.uploadingImages.set(true);
    this.clearMessages();
    this.uploadNext(files, 0);
  }

  protected imageUrl(imageId: string): string {
    return this.imagesApi.contentUrl(imageId);
  }

  protected removeImage(index: number): void {
    this.imageIds.update((ids) => ids.filter((_, currentIndex) => currentIndex !== index));
  }

  protected startDragging(index: number): void {
    this.draggedImageIndex.set(index);
  }

  protected dropImage(targetIndex: number): void {
    const sourceIndex = this.draggedImageIndex();

    if (sourceIndex === null || sourceIndex === targetIndex) {
      this.draggedImageIndex.set(null);
      return;
    }

    this.imageIds.update((ids) => {
      const reordered = [...ids];
      const [moved] = reordered.splice(sourceIndex, 1);
      reordered.splice(targetIndex, 0, moved);
      return reordered;
    });
    this.draggedImageIndex.set(null);
  }

  protected save(): void {
    if (this.form.invalid || !this.sellerId()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.clearMessages();

    const value = this.form.getRawValue();
    const request: CreateProductRequest = {
      ...value,
      currencyCode: value.currencyCode.toUpperCase(),
      sellerId: this.sellerId(),
      imageIds: this.imageIds(),
      displayImageId: this.imageIds()[0] ?? null,
    };

    this.productsApi.create(request).subscribe({
      next: () => {
        this.success.set('Your product was added to the marketplace.');
        this.saving.set(false);
        this.resetForm();
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }

  private loadFormOptions(): void {
    forkJoin({
      profile: this.usersApi.getOwn(),
      categories: this.productsApi.getCategories(),
      productTypes: this.productsApi.getTypes(),
    }).subscribe({
      next: ({ profile, categories, productTypes }) => {
        this.sellerId.set(profile.id);
        this.categories.set(categories);
        this.productTypes.set(productTypes);
        this.form.patchValue({
          categoryId: categories[0]?.id ?? '',
          productType: productTypes[0]?.value ?? 'Physical',
        });
        this.loadingOptions.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loadingOptions.set(false);
      },
    });
  }

  private uploadNext(files: File[], index: number): void {
    if (index >= files.length) {
      this.uploadingImages.set(false);
      return;
    }

    this.imagesApi.upload(files[index]).subscribe({
      next: (image) => {
        this.imageIds.update((ids) => [...ids, image.id]);
        this.uploadNext(files, index + 1);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.uploadingImages.set(false);
      },
    });
  }

  private resetForm(): void {
    this.form.reset({
      name: '',
      description: '',
      price: 0,
      currencyCode: 'USD',
      quantity: 0,
      categoryId: this.categories()[0]?.id ?? '',
      productType: this.productTypes()[0]?.value ?? 'Physical',
    });
    this.imageIds.set([]);
  }

  private isSupportedImage(file: File): boolean {
    return ['image/jpeg', 'image/png', 'image/webp', 'image/gif'].includes(file.type);
  }

  private clearMessages(): void {
    this.error.set('');
    this.success.set('');
  }
}
