import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import { ChangeDetectionStrategy, Component, computed, HostListener, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { ProductsApiClient } from '../../../../core/api/products-api';
import {
  CreateProductRequest,
  Product,
  ProductCategory,
  ProductType,
  ProductTypeOption,
  UpdateProductRequest,
} from '../../../../core/models/product-model';
import { flattenCategories } from '../../../../shared/utils/category-utils';

@Component({
  selector: 'app-admin-products-page',
  standalone: true,
  imports: [AppCurrencyPipe, ReactiveFormsModule],
  templateUrl: './admin-products-page.html',
  styleUrl: './admin-products-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
/** Administrative catalog editor; product money remains validated and persisted by ProductApi. */
export class AdminProductsPage {
  protected readonly maxProductImages = 8;

  private readonly api = inject(ProductsApiClient);
  private readonly images = inject(ImagesApiClient);

  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<ProductCategory[]>([]);
  protected readonly flatCategories = computed(() => flattenCategories(this.categories()));
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
  protected readonly deletingProduct = signal<Product | null>(null);
  protected readonly deleting = signal(false);

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
        console.error('[AdminProducts load error]:', error);
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
    const formValue = this.form.getRawValue();

    if (editingProduct) {
      const request: UpdateProductRequest = {
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        currencyCode: formValue.currencyCode.toUpperCase(),
        quantity: formValue.quantity,
        categoryId: formValue.categoryId,
        productType: formValue.productType,
        imageIds: this.productImageIds(),
        displayImageId: this.productImageIds()[0] ?? null,
      };

      this.api.update(editingProduct.id, request).subscribe({
        next: () => {
          this.success.set('Product updated successfully.');
          this.saving.set(false);
          this.closeForm();
          this.load();
        },
        error: (error) => {
          console.error('[AdminProducts save error]:', error);
          this.error.set(apiErrorMessage(error));
          this.saving.set(false);
        },
      });
    } else {
      const request: CreateProductRequest = {
        name: formValue.name,
        description: formValue.description,
        price: formValue.price,
        currencyCode: formValue.currencyCode.toUpperCase(),
        quantity: formValue.quantity,
        categoryId: formValue.categoryId,
        productType: formValue.productType,
        imageIds: this.productImageIds(),
        displayImageId: this.productImageIds()[0] ?? null,
      };

      this.api.create(request).subscribe({
        next: () => {
          this.success.set('Product created successfully.');
          this.saving.set(false);
          this.closeForm();
          this.load();
        },
        error: (error) => {
          console.error('[AdminProducts save error]:', error);
          this.error.set(apiErrorMessage(error));
          this.saving.set(false);
        },
      });
    }
  }

  @HostListener('window:keydown.escape')
  protected handleEscape(): void {
    if (this.deletingProduct() && !this.deleting()) {
      this.cancelDelete();
    }
  }

  protected promptDelete(product: Product): void {
    this.clearMessages();
    this.deletingProduct.set(product);
  }

  protected cancelDelete(): void {
    if (this.deleting()) {
      return;
    }
    this.deletingProduct.set(null);
  }

  protected confirmDelete(): void {
    const product = this.deletingProduct();
    if (!product || this.deleting()) {
      return;
    }

    this.deleting.set(true);
    this.clearMessages();

    this.api.delete(product.id).subscribe({
      next: () => {
        this.success.set(`Product "${product.name}" deleted successfully.`);
        this.deleting.set(false);
        this.deletingProduct.set(null);
        this.load();
      },
      error: (error) => {
        console.error('[AdminProducts delete error]:', error);
        this.error.set(apiErrorMessage(error));
        this.deleting.set(false);
      },
    });
  }

  protected remove(product: Product): void {
    this.promptDelete(product);
  }

  private resetForm(): void {
    const defaultCatId = this.flatCategories()[0]?.id ?? '';
    this.form.reset({
      name: '',
      description: '',
      price: 0,
      currencyCode: 'USD',
      quantity: 0,
      categoryId: defaultCatId,
      productType: 'Physical',
    });
  }

  private loadCategories(): void {
    this.api.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        const flat = this.flatCategories();
        if (!this.form.controls.categoryId.value && flat.length) {
          this.form.controls.categoryId.setValue(flat[0].id);
        }
      },
      error: (error) => {
        console.error('[AdminProducts loadCategories error]:', error);
        this.error.set(apiErrorMessage(error));
      },
    });
  }

  private loadProductTypes(): void {
    this.api.getTypes().subscribe({
      next: (types) => this.productTypes.set(types),
      error: (error) => {
        console.error('[AdminProducts loadProductTypes error]:', error);
        this.error.set(apiErrorMessage(error));
      },
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
