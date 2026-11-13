import { registerLocaleData } from '@angular/common';
import localeEs from '@angular/common/locales/es';
import {
  DEFAULT_CURRENCY_CODE,
  EnvironmentProviders,
  LOCALE_ID,
  makeEnvironmentProviders,
} from '@angular/core';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { es } from 'date-fns/locale';
import { SpanishPaginatorIntl } from './spanish-paginator-intl';

export function provideSpanishLocale(): EnvironmentProviders {
  registerLocaleData(localeEs);

  return makeEnvironmentProviders([
    { provide: LOCALE_ID, useValue: 'es-ES' },
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'EUR' },
    { provide: MAT_DATE_LOCALE, useValue: es },
    provideDateFnsAdapter(),
    { provide: MatPaginatorIntl, useClass: SpanishPaginatorIntl },
  ]);
}
