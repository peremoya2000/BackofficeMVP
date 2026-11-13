import { AbstractControl, ValidationErrors } from '@angular/forms';
import { APP_LOCALE } from '../../core/locale/provide-spanish-locale';

const COUNTRY_CODE_PATTERN = /^[A-Z]{2}$/;
const REGION_NAMES = new Intl.DisplayNames([APP_LOCALE], { type: 'region', fallback: 'none' });

export function countryName(code: string | null | undefined): string | undefined {
  const normalizedCode = (code ?? '').trim().toUpperCase();
  // Intl throws a RangeError for anything that isn't shaped like a region code.
  return COUNTRY_CODE_PATTERN.test(normalizedCode) ? REGION_NAMES.of(normalizedCode) : undefined;
}

export function knownCountryValidator(control: AbstractControl<string>): ValidationErrors | null {
  if (!control.value) return null;
  return countryName(control.value) ? null : { unknownCountry: true };
}
