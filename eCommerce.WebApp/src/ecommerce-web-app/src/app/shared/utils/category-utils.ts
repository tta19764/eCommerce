import { ProductCategory, FlatCategoryOption } from '../../core/models/product-model';

/**
 * Recursively flattens a category tree into a list of options with depth indicators and breadcrumb paths.
 */
export function flattenCategories(
  categories: ProductCategory[],
  parentPath: string = '',
  depth: number = 0
): FlatCategoryOption[] {
  const result: FlatCategoryOption[] = [];

  for (const cat of categories) {
    const fullPath = parentPath ? `${parentPath} > ${cat.name}` : cat.name;
    const prefix = depth === 0 ? '' : '\u00A0\u00A0'.repeat(depth) + '↳ ';
    const indentedName = `${prefix}${cat.name}`;

    result.push({
      id: cat.id,
      name: cat.name,
      slug: cat.slug,
      parentCategoryId: cat.parentCategoryId,
      depth,
      fullPath,
      indentedName,
    });

    if (cat.subcategories && cat.subcategories.length > 0) {
      result.push(...flattenCategories(cat.subcategories, fullPath, depth + 1));
    }
  }

  return result;
}
