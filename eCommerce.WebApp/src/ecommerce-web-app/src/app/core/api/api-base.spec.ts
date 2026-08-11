import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage } from './api-base';

describe('apiErrorMessage', () => {
  let consoleSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    consoleSpy.mockRestore();
  });

  it('logs technical 503 HttpErrorResponse to console and returns user friendly message', () => {
    const error = new HttpErrorResponse({
      status: 503,
      statusText: 'OK',
      url: 'http://localhost:7013/api/authentication',
      error: 'Http failure response for http://localhost:7013/api/authentication: 503 OK',
    });

    const result = apiErrorMessage(error);

    expect(consoleSpy).toHaveBeenCalledWith(
      expect.stringContaining('[API Error Detail] 503 OK (http://localhost:7013/api/authentication):'),
      error,
    );
    expect(result).toBe('Service is temporarily unavailable. Please try again shortly.');
  });

  it('logs technical 500 HttpErrorResponse to console and returns server error message', () => {
    const error = new HttpErrorResponse({
      status: 500,
      statusText: 'Internal Server Error',
      url: 'http://localhost:7013/api/products',
    });

    const result = apiErrorMessage(error);

    expect(consoleSpy).toHaveBeenCalled();
    expect(result).toBe('An unexpected server error occurred. Please try again later.');
  });

  it('returns clean domain error message when backend provides one', () => {
    const error = new HttpErrorResponse({
      status: 400,
      url: 'http://localhost:7013/api/auth/login',
      error: { error: { name: 'InvalidCredentials' } },
    });

    const result = apiErrorMessage(error);

    expect(consoleSpy).toHaveBeenCalled();
    expect(result).toBe('InvalidCredentials');
  });

  it('handles standard network error (status 0)', () => {
    const error = new HttpErrorResponse({
      status: 0,
      statusText: 'Unknown Error',
      url: 'http://localhost:7013/api/authentication',
    });

    const result = apiErrorMessage(error);

    expect(consoleSpy).toHaveBeenCalled();
    expect(result).toBe('Unable to connect to server. Please check your network connection.');
  });
});
