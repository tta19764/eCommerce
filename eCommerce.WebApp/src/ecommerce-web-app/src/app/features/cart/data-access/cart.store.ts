import { computed, Injectable, signal } from '@angular/core';
import { Product } from '../../../core/models/product.models';

export interface CartItem {
  product: Product;
  quantity: number;
}
const CART_KEY = 'ecommerce.cart';
@Injectable({ providedIn: 'root' })
export class CartStore {
  private readonly state = signal<CartItem[]>(this.restore());
  readonly items = this.state.asReadonly();
  readonly count = computed(() => this.state().reduce((sum, item) => sum + item.quantity, 0));
  readonly total = computed(() =>
    this.state().reduce((sum, item) => sum + item.product.price * item.quantity, 0),
  );
  readonly currency = computed(() => this.state()[0]?.product.currency ?? 'USD');

  add(product: Product, quantity = 1) {
    const items = [...this.state()];
    const existing = items.find((item) => item.product.id === product.id);
    if (existing) existing.quantity = Math.min(existing.quantity + quantity, product.quantity);
    else items.push({ product, quantity: Math.min(quantity, product.quantity) });
    this.save(items);
  }

  update(productId: string, quantity: number) {
    const items = this.state().map((item) =>
      item.product.id === productId
        ? { ...item, quantity: Math.max(1, Math.min(quantity, item.product.quantity)) }
        : item,
    );

    this.save(items);
  }

  remove(productId: string) {
    this.save(this.state().filter((item) => item.product.id !== productId));
  }

  clear() {
    this.save([]);
  }

  private save(items: CartItem[]) {
    this.state.set(items);
    localStorage.setItem(CART_KEY, JSON.stringify(items));
  }

  private restore(): CartItem[] {
    try {
      const value = localStorage.getItem(CART_KEY);

      return value ? JSON.parse(value) : [];
    } catch {
      return [];
    }
  }
}
