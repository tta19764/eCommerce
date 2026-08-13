import { ComponentRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { ConfirmModal } from './confirm-modal';

describe('ConfirmModal', () => {
  let component: ConfirmModal;
  let componentRef: ComponentRef<ConfirmModal>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ConfirmModal],
    });

    const fixture = TestBed.createComponent(ConfirmModal);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('renders title, description, and details when isOpen is true', () => {
    const fixture = TestBed.createComponent(ConfirmModal);
    fixture.componentRef.setInput('isOpen', true);
    fixture.componentRef.setInput('title', 'Approve Application');
    fixture.componentRef.setInput('description', 'Approve this seller');
    fixture.componentRef.setInput('details', [{ label: 'Store', value: 'Apex Store' }]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Approve Application');
    expect(compiled.textContent).toContain('Approve this seller');
    expect(compiled.textContent).toContain('Apex Store');
  });

  it('emits confirmed event when confirm button is clicked', () => {
    let emitted = false;
    component.confirmed.subscribe(() => {
      emitted = true;
    });

    (component as any).onConfirm();
    expect(emitted).toBe(true);
  });

  it('emits cancelled event on Escape keypress when not loading', () => {
    const fixture = TestBed.createComponent(ConfirmModal);
    fixture.componentRef.setInput('isOpen', true);
    fixture.detectChanges();

    let cancelledEmitted = false;
    fixture.componentInstance.cancelled.subscribe(() => {
      cancelledEmitted = true;
    });

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    expect(cancelledEmitted).toBe(true);
  });
});
