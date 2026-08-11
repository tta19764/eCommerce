import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from '../models/api-model';

// JSON endpoints wrap successful data and backend errors in the same response envelope.
export const apiData = <T>(response: ApiResponse<T>): T => {
  if (response.data !== null) {
    return response.data;
  }

  throw new Error(response.error?.name ?? 'The server returned an empty response.');
};

/**
 * Log technical API error details to console and return user-friendly messages for UI display.
 */
export const apiErrorMessage = (error: unknown): string => {
  if (error instanceof HttpErrorResponse) {
    console.error(`[API Error Detail] ${error.status} ${error.statusText} (${error.url}):`, error);

    // Check if backend returned a clean domain-specific error payload
    const backendMessage =
      error.error?.error?.name ||
      error.error?.detail ||
      error.error?.message ||
      error.error?.title;

    if (backendMessage && typeof backendMessage === 'string' && !isTechnicalErrorMessage(backendMessage)) {
      return backendMessage;
    }

    // Map status codes to clean, user-friendly messages
    return getStatusErrorMessage(error.status);
  }

  if (error instanceof Error) {
    console.error('[API Error Detail]:', error);
    if (!isTechnicalErrorMessage(error.message)) {
      return error.message;
    }
  } else if (error) {
    console.error('[API Error Detail]:', error);
  }

  return 'Something went wrong. Please try again.';
};

const isTechnicalErrorMessage = (msg: string): boolean => {
  return (
    /http:\/\//i.test(msg) ||
    /https:\/\//i.test(msg) ||
    /Http failure response/i.test(msg) ||
    /\b503\b/i.test(msg) ||
    /\b500\b/i.test(msg)
  );
};

const getStatusErrorMessage = (status: number): string => {
  switch (status) {
    case 0:
      return 'Unable to connect to server. Please check your network connection.';
    case 400:
      return 'Invalid request parameters. Please check your input.';
    case 401:
      return 'Authentication failed or session expired. Please sign in again.';
    case 403:
      return 'Access denied. You do not have permission to perform this action.';
    case 404:
      return 'The requested item or service could not be found.';
    case 503:
      return 'Service is temporarily unavailable. Please try again shortly.';
    case 500:
    case 502:
    case 504:
      return 'An unexpected server error occurred. Please try again later.';
    default:
      return 'The request could not be completed at this time.';
  }
};
