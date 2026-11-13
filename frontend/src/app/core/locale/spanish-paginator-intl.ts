import { formatNumber } from '@angular/common';
import { Injectable, LOCALE_ID, inject } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';

@Injectable()
export class SpanishPaginatorIntl extends MatPaginatorIntl {
  private readonly locale = inject(LOCALE_ID);

  override itemsPerPageLabel = 'Elementos por página';
  override nextPageLabel = 'Página siguiente';
  override previousPageLabel = 'Página anterior';
  override firstPageLabel = 'Primera página';
  override lastPageLabel = 'Última página';

  override getRangeLabel = (page: number, pageSize: number, length: number): string => {
    if (length === 0) return '0 de 0';

    const start = page * pageSize + 1;
    const end = Math.min(start + pageSize - 1, length);
    return `${this.format(start)} – ${this.format(end)} de ${this.format(length)}`;
  };

  private format(value: number): string {
    return formatNumber(value, this.locale);
  }
}
