import { ChangeDetectionStrategy, Component, input, output, computed } from '@angular/core';
import { ProductCategory } from '../../../core/models/product-model';

@Component({
  selector: 'app-category-picker',
  standalone: true,
  templateUrl: './category-picker.html',
  styleUrl: './category-picker.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryPicker {
  categories = input.required<ProductCategory[]>();
  selectedId = input<string | null>(null);
  categorySelected = output<string | null>();

  protected rootCategories = computed(() => this.categories());

  protected activePath = computed(() => {
    const currentId = this.selectedId();
    if (!currentId) return [];

    const getPath = (cats: ProductCategory[], targetId: string): string[] | null => {
      for (const cat of cats) {
        if (cat.id === targetId) return [cat.id];
        if (cat.subcategories && cat.subcategories.length > 0) {
          const subPath = getPath(cat.subcategories, targetId);
          if (subPath) return [cat.id, ...subPath];
        }
      }
      return null;
    };

    return getPath(this.categories(), currentId) || [];
  });

  protected childLevels = computed(() => {
    const levels: ProductCategory[][] = [];
    const path = this.activePath();

    let currentLevel = this.categories();
    for (const id of path) {
      const found = currentLevel.find((c) => c.id === id);
      if (found && found.subcategories && found.subcategories.length > 0) {
        levels.push(found.subcategories);
        currentLevel = found.subcategories;
      } else {
        break;
      }
    }
    return levels;
  });

  protected selectCategory(category: ProductCategory | null): void {
    this.categorySelected.emit(category?.id ?? null);
  }

  protected isPathActive(categoryId: string): boolean {
    return this.activePath().includes(categoryId);
  }
}
