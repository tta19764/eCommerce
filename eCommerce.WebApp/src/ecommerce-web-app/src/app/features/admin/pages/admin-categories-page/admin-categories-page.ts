import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductsApiClient } from '../../../../core/api/products-api.client';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ProductCategory, CreateCategoryRequest } from '../../../../core/models/product.models';
import { flattenCategories } from '../../../../shared/utils/category-utils';

@Component({
  selector: 'app-admin-categories-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-categories-page.html',
  styleUrl: './admin-categories-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminCategoriesPage {
  private readonly api = inject(ProductsApiClient);

  protected readonly categories = signal<ProductCategory[]>([]);
  protected readonly flatCategories = computed(() => flattenCategories(this.categories()));
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly success = signal('');
  protected searchQuery = '';

  // Category addition form state
  protected readonly showAddForm = signal(false);
  protected readonly creating = signal(false);
  protected newCategoryName = '';
  protected newCategorySlug = '';
  protected selectedParentId: string | null = null;

  constructor() {
    this.loadCategories();
  }

  protected loadCategories(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('[AdminCategories loadCategories error]:', err);
        this.error.set(apiErrorMessage(err) || 'Failed to load product categories.');
        this.loading.set(false);
      },
    });
  }

  protected toggleAddForm(): void {
    this.showAddForm.update((v) => !v);
    this.resetAddForm();
  }

  protected onNameChange(): void {
    if (!this.newCategorySlug || this.newCategorySlug === this.slugify(this.newCategoryName.slice(0, -1))) {
      this.newCategorySlug = this.slugify(this.newCategoryName);
    }
  }

  protected createCategory(): void {
    if (!this.newCategoryName.trim()) {
      return;
    }

    this.creating.set(true);
    this.error.set('');
    this.success.set('');

    const request: CreateCategoryRequest = {
      name: this.newCategoryName.trim(),
      slug: this.newCategorySlug.trim() || undefined,
      parentCategoryId: this.selectedParentId || null,
    };

    this.api.createCategory(request).subscribe({
      next: () => {
        this.success.set(`Category "${request.name}" created successfully.`);
        this.creating.set(false);
        this.showAddForm.set(false);
        this.resetAddForm();
        this.loadCategories();
      },
      error: (err) => {
        console.error('[AdminCategories createCategory error]:', err);
        this.error.set(apiErrorMessage(err) || 'Failed to create category.');
        this.creating.set(false);
      },
    });
  }

  protected get filteredCategories(): ProductCategory[] {
    const q = this.searchQuery.toLowerCase().trim();
    if (!q) return this.categories();

    const matchCategory = (cat: ProductCategory): boolean => {
      if (cat.name.toLowerCase().includes(q) || cat.slug.toLowerCase().includes(q)) {
        return true;
      }
      return (cat.subcategories || []).some((sub) => matchCategory(sub));
    };

    return this.categories().filter((cat) => matchCategory(cat));
  }

  private resetAddForm(): void {
    this.newCategoryName = '';
    this.newCategorySlug = '';
    this.selectedParentId = null;
  }

  private slugify(text: string): string {
    return text
      .toLowerCase()
      .trim()
      .replace(/[^\w\s-]/g, '')
      .replace(/[\s_-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }
}
