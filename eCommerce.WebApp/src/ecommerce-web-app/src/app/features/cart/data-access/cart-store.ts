import { computed, Injectable, signal } from '@angular/core';
import { Product } from '../../../core/models/product-model';

export interface CartItem {
  product: Product;
  quantity: number;
}

const CART_KEY = 'ecommerce.cart';

@Injectable({ providedIn: 'root' })
export class CartStore {
  // Cart state is a convenience snapshot only. It deliberately contains no exchange-rate quote or
  // authoritative total; CartPage obtains those from OrderApi whenever this signal changes.
  private readonly state = signal<CartItem[]>(this.restore());

  readonly items = this.state.asReadonly();
  readonly count = computed(() => this.state().reduce((sum, item) => sum + item.quantity, 0));
  add(product: Product, quantity = 1): void {
    // Customer-facing controls and the store both reject products known to be unavailable.
    if (product.quantity < 1) {
      return;
    }

    const items = [...this.state()];
    const existing = items.find((item) => item.product.id === product.id);

    if (existing) {
      // Prevent the UI quantity from exceeding the last known product stock.
      existing.quantity = Math.min(existing.quantity + quantity, product.quantity);
    } else {
      items.push({
        product,
        quantity: Math.min(quantity, product.quantity),
      });
    }

    this.save(items);
  }

  update(productId: string, quantity: number): void {
    const items = this.state().map((item) =>
      item.product.id === productId
        ? { ...item, quantity: Math.max(1, Math.min(quantity, item.product.quantity)) }
        : item,
    );

    this.save(items);
  }

  remove(productId: string): void {
    this.save(this.state().filter((item) => item.product.id !== productId));
  }

  clear(): void {
    this.save([]);
  }

  private save(items: CartItem[]): void {
    this.state.set(items);
    // Persist only browsing continuity. ProductApi stock and OrderApi pricing are revalidated remotely.
    localStorage.setItem(CART_KEY, JSON.stringify(items));
  }

  private restore(): CartItem[] {
    try {
      const value = localStorage.getItem(CART_KEY);

      return value ? (JSON.parse(value) as CartItem[]) : [];
    } catch {
      return [];
    }
  }
}
