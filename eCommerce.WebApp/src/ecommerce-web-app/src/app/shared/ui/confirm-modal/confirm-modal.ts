import { NgClass } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  effect,
  input,
  output,
} from '@angular/core';

export type ConfirmModalVariant = 'primary' | 'danger' | 'warning' | 'success';

export interface ConfirmModalDetail {
  label: string;
  value: string;
}

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [NgClass],
  templateUrl: './confirm-modal.html',
  styleUrl: './confirm-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[attr.title]': 'null',
  },
})
export class ConfirmModal {
  readonly isOpen = input<boolean>(false);
  readonly title = input<string>('Confirm Action');
  readonly description = input<string>('');
  readonly details = input<ConfirmModalDetail[] | null>(null);
  readonly confirmText = input<string>('Confirm');
  readonly cancelText = input<string>('Cancel');
  readonly variant = input<ConfirmModalVariant>('primary');
  readonly loading = input<boolean>(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  constructor() {
    effect((onCleanup) => {
      if (this.isOpen()) {
        const handleKeyDown = (event: KeyboardEvent) => {
          if (event.key === 'Escape' && !this.loading()) {
            this.cancelled.emit();
          }
        };

        window.addEventListener('keydown', handleKeyDown);
        onCleanup(() => {
          window.removeEventListener('keydown', handleKeyDown);
        });
      }
    });
  }

  protected onConfirm(): void {
    if (!this.loading()) {
      this.confirmed.emit();
    }
  }

  protected onCancel(): void {
    if (!this.loading()) {
      this.cancelled.emit();
    }
  }
}
