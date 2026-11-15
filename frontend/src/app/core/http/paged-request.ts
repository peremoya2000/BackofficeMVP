import { ParamMap } from '@angular/router';

export const DEFAULT_PAGE_SIZE = 20;
export const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export interface PagedRequest {
  page: number;
  pageSize: number;
  sort?: string;
}

export function readPaging(params: ParamMap): Pick<PagedRequest, 'page' | 'pageSize'> {
  const page = Number(params.get('page'));
  const pageSize = Number(params.get('pageSize'));
  return {
    page: Number.isInteger(page) && page >= 1 ? page : 1,
    pageSize: PAGE_SIZE_OPTIONS.includes(pageSize) ? pageSize : DEFAULT_PAGE_SIZE,
  };
}
