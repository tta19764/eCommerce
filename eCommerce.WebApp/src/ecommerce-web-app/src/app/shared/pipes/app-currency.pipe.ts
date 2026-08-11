import { Pipe, PipeTransform } from '@angular/core';

export const CURRENCY_SYMBOLS: Record<string, string> = {
  USD: '$',
  EUR: '€',
  UAH: '₴',
};

/** Formats current supported decimal display amounts without making them authoritative money values. */
export function formatCurrencyWithSymbol(value: number | null | undefined, currencyCode: string = 'USD'): string {
  if (value === null || value === undefined || isNaN(value)) {
    return '';
  }

  const code = (currencyCode || 'USD').toUpperCase();
  const symbol = CURRENCY_SYMBOLS[code] ?? code;
  const formattedNumber = value.toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  return `${symbol}${formattedNumber}`;
}

@Pipe({
  name: 'appCurrency',
  standalone: true,
})
export class AppCurrencyPipe implements PipeTransform {
  /** Converts a display amount to the application's compact symbol-first representation. */
  transform(value: number | null | undefined, currencyCode: string = 'USD'): string {
    return formatCurrencyWithSymbol(value, currencyCode);
  }
}
