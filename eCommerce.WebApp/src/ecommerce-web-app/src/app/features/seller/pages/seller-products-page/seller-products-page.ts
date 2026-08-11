import { AppCurrencyPipe } from '../../../../shared/pipes/app-currency.pipe';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin, Observable } from 'rxjs';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { OrdersApiClient } from '../../../../core/api/orders-api';
import { ProductsApiClient } from '../../../../core/api/products-api';
import { UsersApiClient } from '../../../../core/api/users-api';
import { OrderStatus, SellerOrder } from '../../../../core/models/order-model';
import {
  CreateCategoryRequest,
  CreateProductRequest,
  Product,
  ProductCategory,
  ProductType,
  ProductTypeOption,
} from '../../../../core/models/product-model';
import { UserProfile } from '../../../../core/models/user-model';
import { flattenCategories } from '../../../../shared/utils/category-utils';

@Component({
  selector: 'app-seller-products-page',
  standalone: true,
  imports: [AppCurrencyPipe, FormsModule, ReactiveFormsModule],
  templateUrl: './seller-products-page.html',
  styleUrl: './seller-products-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerProductsPage {
  protected readonly maxProductImages = 8;

  private readonly productsApi = inject(ProductsApiClient);
  private readonly imagesApi = inject(ImagesApiClient);
  private readonly usersApi = inject(UsersApiClient);
  private readonly ordersApi = inject(OrdersApiClient);

  // Tab State
  protected readonly activeTab = signal<'store' | 'products' | 'orders'>('products');

  // Seller Data
  protected readonly profile = signal<UserProfile | null>(null);
  protected readonly sellerId = signal('');
  protected readonly categories = signal<ProductCategory[]>([]);
  protected readonly flatCategories = computed(() => flattenCategories(this.categories()));
  protected readonly productTypes = signal<ProductTypeOption[]>([]);
  protected readonly products = signal<Product[]>([]);
  protected readonly sellerOrders = signal<SellerOrder[]>([]);

  // UI Loaders & Form States
  protected readonly loadingOptions = signal(true);
  protected readonly loadingProducts = signal(false);
  protected readonly loadingOrders = signal(false);
  protected readonly saving = signal(false);
  protected readonly uploadingImages = signal(false);
  protected readonly showProductForm = signal(false);
  protected readonly editingProduct = signal<Product | null>(null);
  protected readonly productSearchQuery = signal('');

  // Messages
  protected readonly error = signal('');
  protected readonly success = signal('');

  // Image Upload State
  protected readonly imageIds = signal<string[]>([]);
  protected readonly draggedImageIndex = signal<number | null>(null);

  // Quick Category Addition State
  protected readonly showCategoryModal = signal(false);
  protected readonly addingCategory = signal(false);
  protected newCategoryName = '';
  protected selectedParentCategoryId: string | null = null;

  // Product Form
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

  protected setTab(tab: 'store' | 'products' | 'orders'): void {
    this.activeTab.set(tab);
    this.clearMessages();
    if (tab === 'products') {
      this.loadSellerProducts();
    } else if (tab === 'orders') {
      this.loadSellerOrders();
    }
  }

  protected filteredProducts(): Product[] {
    const q = this.productSearchQuery().toLowerCase().trim();
    if (!q) return this.products();
    return this.products().filter(
      (p) => p.name.toLowerCase().includes(q) || p.description.toLowerCase().includes(q),
    );
  }

  protected categoryName(categoryId: string): string {
    const flat = this.flatCategories();
    const found = flat.find((c) => c.id === categoryId);
    return found ? found.fullPath : 'General';
  }

  protected openCreateForm(): void {
    this.editingProduct.set(null);
    this.resetForm();
    this.imageIds.set([]);
    this.showProductForm.set(true);
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
    this.imageIds.set(this.orderedImageIds(product));
    this.showProductForm.set(true);
    this.clearMessages();
  }

  protected closeProductForm(): void {
    this.showProductForm.set(false);
    this.editingProduct.set(null);
    this.resetForm();
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

  protected productMainImageUrl(product: Product): string | null {
    const id = product.displayImageId ?? product.imageIds[0];
    return id ? this.imagesApi.contentUrl(id) : null;
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

  protected saveProduct(): void {
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

    const editing = this.editingProduct();
    const save$: Observable<unknown> = editing
      ? this.productsApi.update(editing.id, request)
      : this.productsApi.create(request);

    save$.subscribe({
      next: () => {
        this.success.set(
          editing ? 'Product listing updated successfully.' : 'Product published to marketplace.',
        );
        this.saving.set(false);
        this.closeProductForm();
        this.loadSellerProducts();
      },
      error: (error) => {
        console.error('[Seller saveProduct error]:', error);
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }

  protected deleteProduct(product: Product): void {
    if (!confirm(`Delete product listing "${product.name}"?`)) {
      return;
    }

    this.productsApi.delete(product.id).subscribe({
      next: () => {
        this.success.set('Product deleted.');
        this.loadSellerProducts();
      },
      error: (err) => {
        console.error('[Seller deleteProduct error]:', err);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  // Category Quick Addition
  protected openCategoryModal(): void {
    this.newCategoryName = '';
    this.selectedParentCategoryId = null;
    this.showCategoryModal.set(true);
  }

  protected closeCategoryModal(): void {
    this.showCategoryModal.set(false);
  }

  protected saveCategory(): void {
    if (!this.newCategoryName.trim()) return;

    this.addingCategory.set(true);
    const request: CreateCategoryRequest = {
      name: this.newCategoryName.trim(),
      parentCategoryId: this.selectedParentCategoryId || null,
    };

    this.productsApi.createCategory(request).subscribe({
      next: () => {
        this.addingCategory.set(false);
        this.closeCategoryModal();
        this.success.set(`Category "${request.name}" created!`);
        // Refresh categories
        this.productsApi.getCategories().subscribe({
          next: (cats) => {
            this.categories.set(cats);
            const flat = this.flatCategories();
            if (flat.length) {
              this.form.patchValue({ categoryId: flat[flat.length - 1].id });
            }
          },
        });
      },
      error: (err) => {
        console.error('[Seller saveCategory error]:', err);
        this.error.set(apiErrorMessage(err));
        this.addingCategory.set(false);
      },
    });
  }

  // Client Orders Status Update
  protected updateOrderStatus(sellerOrderId: string, status: OrderStatus): void {
    this.ordersApi.updateSellerOrderStatus(sellerOrderId, status).subscribe({
      next: () => {
        this.success.set(`Order status updated to ${status}.`);
        this.loadSellerOrders();
      },
      error: (err) => {
        console.error('[Seller updateOrderStatus error]:', err);
        this.error.set(apiErrorMessage(err));
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
        this.profile.set(profile);
        this.sellerId.set(profile.id);
        this.categories.set(categories);
        this.productTypes.set(productTypes);
        const flat = this.flatCategories();
        this.form.patchValue({
          categoryId: flat[0]?.id ?? '',
          productType: productTypes[0]?.value ?? 'Physical',
        });
        this.loadingOptions.set(false);
        this.loadSellerProducts();
      },
      error: (error) => {
        console.error('[Seller loadFormOptions error]:', error);
        this.error.set(apiErrorMessage(error));
        this.loadingOptions.set(false);
      },
    });
  }

  private loadSellerProducts(): void {
    const sId = this.sellerId();
    if (!sId) return;

    this.loadingProducts.set(true);
    this.productsApi.getPage({ sellerId: sId, page: 1, pageSize: 50 }).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.loadingProducts.set(false);
      },
      error: (err) => {
        console.error('[Seller loadSellerProducts error]:', err);
        this.error.set(apiErrorMessage(err));
        this.loadingProducts.set(false);
      },
    });
  }

  private loadSellerOrders(): void {
    this.loadingOrders.set(true);
    this.ordersApi.getSellerOrders({ page: 1, pageSize: 20 }).subscribe({
      next: (res) => {
        this.sellerOrders.set(res.items);
        this.loadingOrders.set(false);
      },
      error: (err) => {
        console.error('[Seller loadSellerOrders error]:', err);
        this.error.set(apiErrorMessage(err));
        this.loadingOrders.set(false);
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
        console.error('[Seller uploadNext error]:', error);
        this.error.set(apiErrorMessage(error));
        this.uploadingImages.set(false);
      },
    });
  }

  private resetForm(): void {
    const flat = this.flatCategories();
    this.form.reset({
      name: '',
      description: '',
      price: 0,
      currencyCode: 'USD',
      quantity: 0,
      categoryId: flat[0]?.id ?? '',
      productType: this.productTypes()[0]?.value ?? 'Physical',
    });
    this.imageIds.set([]);
  }

  private orderedImageIds(product: Product): string[] {
    const displayImageId = product.displayImageId;
    if (!displayImageId) {
      return [...product.imageIds];
    }
    return [displayImageId, ...product.imageIds.filter((id) => id !== displayImageId)];
  }

  private isSupportedImage(file: File): boolean {
    return ['image/jpeg', 'image/png', 'image/webp', 'image/gif'].includes(file.type);
  }

  private clearMessages(): void {
    this.error.set('');
    this.success.set('');
  }
}
