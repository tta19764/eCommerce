import { Injectable, signal } from '@angular/core';
import { Order } from '../models/order-model';

const STORAGE_KEY = 'ecommerce.recently_paid_order_ids';

@Injectable({ providedIn: 'root' })
/**
 * Tracks orders returning from Stripe confirmation until webhook-authoritative order state catches up.
 * This is presentation state only: it never asserts payment to OrderApi or PaymentApi.
 */
export class PaymentStateService {
  private readonly recentlyPaidIds = signal<Set<string>>(this.restore());

  markAsPaid(orderId: string): void {
    if (!orderId) return;
    const current = new Set(this.recentlyPaidIds());
    current.add(orderId);
    this.save(current);
  }

  isRecentlyPaid(orderId: string): boolean {
    if (!orderId) return false;
    return this.recentlyPaidIds().has(orderId);
  }

  clearPaid(orderId: string): void {
    if (!orderId) return;
    const current = new Set(this.recentlyPaidIds());
    if (current.delete(orderId)) {
      this.save(current);
    }
  }

  reconcile(orders: Order[]): void {
    const current = new Set(this.recentlyPaidIds());
    let changed = false;

    // Remove the temporary marker once a refreshed OrderApi projection reaches any terminal/post-paid state.
    for (const order of orders) {
      if (
        order.paidOnUtc ||
        order.status === 'Paid' ||
        order.status === 'Shipped' ||
        order.status === 'Completed' ||
        order.status === 'Cancelled'
      ) {
        if (current.delete(order.id)) {
          changed = true;
        }
      }
    }

    if (changed) {
      this.save(current);
    }
  }

  private save(set: Set<string>): void {
    this.recentlyPaidIds.set(set);
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(Array.from(set)));
    } catch {
      // Ignore storage write errors in restricted environments
    }
  }

  private restore(): Set<string> {
    try {
      const value = sessionStorage.getItem(STORAGE_KEY);
      if (value) {
        const arr = JSON.parse(value) as string[];
        return new Set(arr);
      }
    } catch {
      // Fallback on parse failure
    }
    return new Set<string>();
  }
}
