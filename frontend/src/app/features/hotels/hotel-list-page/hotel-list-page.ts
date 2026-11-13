import { Component, computed, effect, inject, linkedSignal } from '@angular/core';
import { rxResource, takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule, Sort, SortDirection } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { debounceTime, map } from 'rxjs';
import { DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '../../../core/http/paged-request';
import { PagedResponse } from '../../../core/http/paged-response';
import { CountryNamePipe } from '../../../shared/countries/country-name-pipe';
import { LoadError } from '../../../shared/load-error/load-error';
import { PageHeader } from '../../../shared/page-header/page-header';
import { Stars } from '../../../shared/stars/stars';
import { HotelListItemResponse } from '../data/hotel-list-item-response';
import { HotelSearchRequest } from '../data/hotel-search-request';
import { HotelsApi } from '../data/hotels-api';

const SEARCH_DEBOUNCE_MS = 300;
const DEFAULT_SORT = 'name';
const DESCENDING_PREFIX = '-';

type HotelsPage = PagedResponse<HotelListItemResponse>;

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    CountryNamePipe,
    LoadError,
    PageHeader,
    Stars,
  ],
  selector: 'app-hotel-list-page',
  styleUrl: './hotel-list-page.scss',
  templateUrl: './hotel-list-page.html',
})
export class HotelListPage {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly hotelsApi = inject(HotelsApi);

  protected readonly displayedColumns = ['name', 'stars', 'city', 'country'];
  protected readonly pageSizeOptions = PAGE_SIZE_OPTIONS;
  protected readonly starOptions = [1, 2, 3, 4, 5];

  // The URL is the source of truth, so Back, refresh and shared links keep the search.
  protected readonly request = toSignal(this.route.queryParamMap.pipe(map(readSearchRequest)), {
    requireSync: true,
  });

  protected readonly hotels = rxResource({
    params: this.request,
    stream: ({ params }) => this.hotelsApi.search(params),
  });

  // Keeps the previous page on screen while the next one loads, so the table doesn't flash empty.
  protected readonly shownPage = linkedSignal<HotelsPage | undefined, HotelsPage | undefined>({
    source: () => (this.hotels.hasValue() ? this.hotels.value() : undefined),
    computation: (current, previous) => current ?? previous?.value,
  });

  protected readonly sortActive = computed(() =>
    (this.request().sort ?? DEFAULT_SORT).replace(DESCENDING_PREFIX, ''),
  );
  protected readonly sortDirection = computed<SortDirection>(() =>
    this.request().sort?.startsWith(DESCENDING_PREFIX) ? 'desc' : 'asc',
  );

  protected readonly filters = inject(FormBuilder).nonNullable.group({
    search: this.request().search ?? '',
    countryCode: this.request().countryCode ?? '',
    stars: [this.request().stars ?? null],
  });

  constructor() {
    effect(() => this.showRequestInFilters(this.request()));

    this.filters.valueChanges
      .pipe(debounceTime(SEARCH_DEBOUNCE_MS), takeUntilDestroyed())
      .subscribe(() => {
        const filters = this.filters.getRawValue();
        this.navigate({
          search: filters.search.trim() || undefined,
          countryCode: filters.countryCode.trim() || undefined,
          stars: filters.stars ?? undefined,
          page: 1,
        });
      });
  }

  protected onSortChange(sort: Sort): void {
    const descending = sort.direction === 'desc';
    this.navigate({ sort: descending ? DESCENDING_PREFIX + sort.active : sort.active, page: 1 });
  }

  protected onPageChange(event: PageEvent): void {
    this.navigate({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }

  // Only fields that disagree with the URL are rewritten, so the one being typed in keeps its trailing space.
  private showRequestInFilters(request: HotelSearchRequest): void {
    const { search, countryCode, stars } = this.filters.controls;
    if (search.value.trim() !== (request.search ?? '')) {
      search.setValue(request.search ?? '', { emitEvent: false });
    }
    if (countryCode.value.trim() !== (request.countryCode ?? '')) {
      countryCode.setValue(request.countryCode ?? '', { emitEvent: false });
    }
    if (stars.value !== (request.stars ?? null)) {
      stars.setValue(request.stars ?? null, { emitEvent: false });
    }
  }

  private navigate(changes: Partial<HotelSearchRequest>): void {
    const queryParams = { ...this.request(), ...changes };
    this.router.navigate([], { relativeTo: this.route, queryParams, replaceUrl: true });
  }
}

function readSearchRequest(params: ParamMap): HotelSearchRequest {
  const stars = params.get('stars');
  return {
    page: Number(params.get('page') ?? 1),
    pageSize: Number(params.get('pageSize') ?? DEFAULT_PAGE_SIZE),
    sort: params.get('sort') ?? undefined,
    search: params.get('search') ?? undefined,
    countryCode: params.get('countryCode') ?? undefined,
    stars: stars ? Number(stars) : undefined,
  };
}
