import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api.client';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import {
  Product,
  CreateProductRequest,
  ProductCategory,
  ProductType,
  ProductTypeOption,
} from '../../../../core/models/product.models';

@Component({
  selector: 'app-admin-products-page',
  imports: [CurrencyPipe, ReactiveFormsModule],
  templateUrl: './admin-products-page.html',
  styleUrl: './admin-products-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminProductsPage {
  protected readonly maxProductImages = 8;

  private readonly api = inject(ProductsApiClient);
  private readonly images = inject(ImagesApiClient);

  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<ProductCategory[]>([]);
  protected readonly productTypes = signal<ProductTypeOption[]>([]);
  protected readonly editingProduct = signal<Product | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly formOpen = signal(false);
  protected readonly error = signal('');
  protected readonly success = signal('');
  protected readonly productImageIds = signal<string[]>([]);
  protected readonly uploadingImages = signal(false);
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
    sellerId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    productType: new FormControl<ProductType>('Physical', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor() {
    this.loadCategories();
    this.loadProductTypes();
    this.load();
  }

  protected imageUrl(product: Product): string | null {
    const imageId = product.displayImageId ?? product.imageIds[0];

    return imageId ? this.images.contentUrl(imageId) : null;
  }

  protected openCreateForm(): void {
    this.editingProduct.set(null);
    this.resetForm();
    this.productImageIds.set([]);
    this.formOpen.set(true);
    this.clearMessages();
  }

  protected openEditForm(product: Product): void {
    this.editingProduct.set(product);
    this.form.setValue({
      name: product.name,
      description: product.description,
      price: product.price,
      currencyCode: product.currency,
      quantity: product.quantity,
      sellerId: product.sellerId,
      categoryId: product.categoryId,
      productType: product.productType,
    });
    this.productImageIds.set(this.orderedImageIds(product));
    this.formOpen.set(true);
    this.clearMessages();
  }

  protected closeForm(): void {
    this.formOpen.set(false);
    this.editingProduct.set(null);
    this.resetForm();
    this.productImageIds.set([]);
  }

  protected uploadImages(event: Event): void {
    const input = event.target as HTMLInputElement;
    const availableSlots = this.maxProductImages - this.productImageIds().length;
    const selectedFiles = Array.from(input.files ?? []);
    const files = selectedFiles.slice(0, availableSlots);
    input.value = '';

    if (!files.length) {
      return;
    }

    if (selectedFiles.length > availableSlots) {
      this.error.set(`Only ${availableSlots} more image(s) can be added.`);
      return;
    }

    const invalidFile = files.find(
      (file) => !this.isSupportedImage(file) || file.size > 10 * 1024 * 1024,
    );

    if (invalidFile) {
      this.error.set('Each image must be a JPEG, PNG, WebP, or GIF no larger than 10 MB.');
      return;
    }

    this.uploadingImages.set(true);
    this.clearMessages();
    this.uploadNext(files, 0);
  }

  protected removeImage(index: number): void {
    this.productImageIds.update((ids) => ids.filter((_, currentIndex) => currentIndex !== index));
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

    this.productImageIds.update((ids) => {
      const reordered = [...ids];
      const [moved] = reordered.splice(sourceIndex, 1);
      reordered.splice(targetIndex, 0, moved);
      return reordered;
    });
    this.draggedImageIndex.set(null);
  }

  protected imageContentUrl(imageId: string): string {
    return this.images.contentUrl(imageId);
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

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const editingProduct = this.editingProduct();
    const request = this.productRequest();
    const saveRequest: Observable<unknown> = editingProduct
      ? this.api.update(editingProduct.id, request)
      : this.api.create(request);

    saveRequest.subscribe({
      next: () => {
        this.success.set(
          editingProduct ? 'Product updated successfully.' : 'Product created successfully.',
        );
        this.saving.set(false);
        this.closeForm();
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

  private productRequest(): CreateProductRequest {
    const formValue = this.form.getRawValue();

    return {
      ...formValue,
      currencyCode: formValue.currencyCode.toUpperCase(),
      productType: formValue.productType,
      imageIds: this.productImageIds(),
      displayImageId: this.productImageIds()[0] ?? null,
    };
  }

  private resetForm(): void {
    this.form.reset({
      name: '',
      description: '',
      price: 0,
      currencyCode: 'USD',
      quantity: 0,
      sellerId: '',
      categoryId: this.categories()[0]?.id ?? '',
      productType: 'Physical',
    });
  }

  private loadCategories(): void {
    this.api.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        if (!this.form.controls.categoryId.value && categories.length) {
          this.form.controls.categoryId.setValue(categories[0].id);
        }
      },
      error: (error) => this.error.set(apiErrorMessage(error)),
    });
  }

  private loadProductTypes(): void {
    this.api.getTypes().subscribe({
      next: (types) => this.productTypes.set(types),
      error: (error) => this.error.set(apiErrorMessage(error)),
    });
  }

  private orderedImageIds(product: Product): string[] {
    const displayImageId = product.displayImageId;

    if (!displayImageId) {
      return [...product.imageIds];
    }

    return [displayImageId, ...product.imageIds.filter((id) => id !== displayImageId)];
  }

  private uploadNext(files: File[], index: number): void {
    if (index >= files.length) {
      this.uploadingImages.set(false);
      return;
    }

    this.images.upload(files[index]).subscribe({
      next: (image) => {
        this.productImageIds.update((ids) => [...ids, image.id]);
        this.uploadNext(files, index + 1);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.uploadingImages.set(false);
      },
    });
  }

  private isSupportedImage(file: File): boolean {
    return ['image/jpeg', 'image/png', 'image/webp', 'image/gif'].includes(file.type);
  }

  private clearMessages(): void {
    this.error.set('');
    this.success.set('');
  }
}
