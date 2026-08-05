import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { apiErrorMessage } from '../../../../core/api/api-base';
import { ImagesApiClient } from '../../../../core/api/images-api';
import { UsersApiClient } from '../../../../core/api/users-api';
import { UserProfile } from '../../../../core/models/user-model';

@Component({
  selector: 'app-profile-page',
  imports: [ReactiveFormsModule],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePage {
  private readonly usersApi = inject(UsersApiClient);
  private readonly imagesApi = inject(ImagesApiClient);

  protected readonly profile = signal<UserProfile | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly editing = signal(false);
  protected readonly uploadingImage = signal(false);
  protected readonly error = signal('');
  protected readonly success = signal('');

  protected readonly form = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  constructor() {
    this.loadProfile();
  }

  protected imageUrl(): string | null {
    const imageId = this.profile()?.imageId;

    return imageId ? this.imagesApi.contentUrl(imageId) : null;
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.clearMessages();

    this.usersApi.updateOwn(this.form.getRawValue()).subscribe({
      next: () => {
        this.success.set('Your profile was updated successfully.');
        this.editing.set(false);
        this.saving.set(false);
        this.loadProfile(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }

  protected startEditing(): void {
    const profile = this.profile();

    if (!profile) {
      return;
    }

    this.form.setValue({
      firstName: profile.firstName,
      lastName: profile.lastName,
    });
    this.editing.set(true);
    this.clearMessages();
  }

  protected cancelEditing(): void {
    this.editing.set(false);
    this.clearMessages();
  }

  protected updateImage(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';

    if (!file) {
      return;
    }

    if (!this.isSupportedImage(file) || file.size > 10 * 1024 * 1024) {
      this.error.set('Choose a JPEG, PNG, WebP, or GIF image no larger than 10 MB.');
      return;
    }

    this.uploadingImage.set(true);
    this.clearMessages();

    this.imagesApi.upload(file).subscribe({
      next: (image) => {
        this.usersApi.updateOwn({ imageId: image.id }).subscribe({
          next: () => {
            this.success.set('Your profile picture was updated.');
            this.uploadingImage.set(false);
            this.loadProfile(false);
          },
          error: (error) => {
            this.error.set(apiErrorMessage(error));
            this.uploadingImage.set(false);
          },
        });
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.uploadingImage.set(false);
      },
    });
  }

  protected removeImage(): void {
    this.saving.set(true);
    this.clearMessages();

    this.usersApi.updateOwn({ imageId: null }).subscribe({
      next: () => {
        this.success.set('Your profile image was removed.');
        this.saving.set(false);
        this.loadProfile(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.saving.set(false);
      },
    });
  }

  private loadProfile(showLoading = true): void {
    if (showLoading) {
      this.loading.set(true);
    }

    this.usersApi.getOwn().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.form.setValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
        });
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  private clearMessages(): void {
    this.error.set('');
    this.success.set('');
  }

  private isSupportedImage(file: File): boolean {
    return ['image/jpeg', 'image/png', 'image/webp', 'image/gif'].includes(file.type);
  }
}
