import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from '../models/api.models';

// JSON endpoints wrap successful data and backend errors in the same response envelope.
export const apiData = <T>(response: ApiResponse<T>): T => {
  if (response.data !== null) {
    return response.data;
  }

  throw new Error(response.error?.name ?? 'The server returned an empty response.');
};

export const apiErrorMessage = (error: unknown): string => {
  if (error instanceof HttpErrorResponse) {
    // Prefer the backend's stable error name over the browser's generic HTTP message.
    return error.error?.error?.name ?? error.message ?? 'The request could not be completed.';
  }

  return error instanceof Error ? error.message : 'Something went wrong. Please try again.';
};
