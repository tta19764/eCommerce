import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { SellerApiClient } from '../../../../core/api/seller-api';
import { PagedList } from '../../../../core/models/api-model';
import { PendingSellerApplicationResponse } from '../../../../core/models/seller-model';
import { ConfirmModal, ConfirmModalDetail } from '../../../../shared/ui/confirm-modal/confirm-modal';

@Component({
  selector: 'app-admin-sellers-page',
  standalone: true,
  imports: [FormsModule, DatePipe, ConfirmModal],
  templateUrl: './admin-sellers-page.html',
  styleUrl: './admin-sellers-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminSellersPage {
  private readonly sellerApi = inject(SellerApiClient);

  protected readonly pagedResponse = signal<PagedList<PendingSellerApplicationResponse> | null>(null);
  protected readonly pendingSellers = computed(() => this.pagedResponse()?.items ?? []);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly totalCount = computed(() => this.pagedResponse()?.totalCount ?? 0);
  protected readonly totalPages = computed(() =>
    Math.ceil(this.totalCount() / this.pageSize()) || 1
  );

  protected readonly loading = signal(true);
  protected readonly approvingSeller = signal<PendingSellerApplicationResponse | null>(null);
  protected readonly approvingId = signal<string | null>(null);

  protected readonly approveDetails = computed<ConfirmModalDetail[] | null>(() => {
    const app = this.approvingSeller();
    if (!app) return null;
    return [
      { label: 'Applicant Name', value: app.applicant.fullName || app.applicant.email || app.applicant.userId },
      { label: 'Applicant Email', value: app.applicant.email || 'N/A' },
      { label: 'Proposed Store', value: app.store.name },
      { label: 'Store Slug', value: `/store/${app.store.slug}` },
      { label: 'Country & Currency', value: `${app.store.countryCode} (${app.store.defaultCurrency})` },
    ];
  });

  protected readonly rejectingSeller = signal<PendingSellerApplicationResponse | null>(null);
  protected readonly rejectionReason = signal('');
  protected readonly submittingReject = signal(false);

  protected readonly error = signal('');
  protected readonly success = signal('');

  constructor() {
    this.loadPending();
  }

  protected loadPending(targetPage?: number): void {
    const p = targetPage ?? this.page();
    this.loading.set(true);
    this.clearMessages();

    this.sellerApi.getPendingSellers({ page: p, pageSize: this.pageSize() }).subscribe({
      next: (res) => {
        this.pagedResponse.set(res);
        this.page.set(res.page);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('[AdminSellers loadPending error]:', err);
        this.error.set(apiErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  protected setPage(targetPage: number): void {
    if (targetPage < 1 || targetPage > this.totalPages() || targetPage === this.page()) {
      return;
    }
    this.loadPending(targetPage);
  }

  protected openApproveModal(application: PendingSellerApplicationResponse): void {
    this.approvingSeller.set(application);
    this.clearMessages();
  }

  protected closeApproveModal(): void {
    this.approvingSeller.set(null);
  }

  protected confirmApprove(): void {
    const application = this.approvingSeller();
    if (!application) return;

    const storeName = application.store.name;
    this.approvingId.set(application.sellerId);
    this.clearMessages();

    this.sellerApi.approveSeller(application.sellerId).subscribe({
      next: () => {
        this.approvingId.set(null);
        this.closeApproveModal();
        this.success.set(`Seller application for "${storeName}" approved successfully.`);
        this.reloadAfterAction();
      },
      error: (err) => {
        console.error('[AdminSellers confirmApprove error]:', err);
        this.approvingId.set(null);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  protected openRejectModal(application: PendingSellerApplicationResponse): void {
    this.rejectingSeller.set(application);
    this.rejectionReason.set('');
    this.clearMessages();
  }

  protected closeRejectModal(): void {
    this.rejectingSeller.set(null);
    this.rejectionReason.set('');
  }

  protected confirmReject(): void {
    const application = this.rejectingSeller();
    const reason = this.rejectionReason().trim();

    if (!application) return;
    if (!reason) {
      this.error.set('A non-empty rejection reason is required.');
      return;
    }

    this.submittingReject.set(true);
    this.clearMessages();

    this.sellerApi.rejectSeller(application.sellerId, { reason }).subscribe({
      next: () => {
        this.submittingReject.set(false);
        const storeName = application.store.name;
        this.closeRejectModal();
        this.success.set(`Seller application for "${storeName}" rejected.`);
        this.reloadAfterAction();
      },
      error: (err) => {
        console.error('[AdminSellers confirmReject error]:', err);
        this.submittingReject.set(false);
        this.error.set(apiErrorMessage(err));
      },
    });
  }

  private reloadAfterAction(): void {
    const currentPage = this.page();
    const currentItemCount = this.pendingSellers().length;
    const targetPage = currentItemCount === 1 && currentPage > 1 ? currentPage - 1 : currentPage;
    this.loadPending(targetPage);
  }

  private clearMessages(): void {
    this.error.set('');
    this.success.set('');
  }
}
