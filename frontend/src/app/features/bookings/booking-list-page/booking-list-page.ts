import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, linkedSignal } from '@angular/core';
import { rxResource, takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule, Sort, SortDirection } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { isValid, parseISO, startOfToday } from 'date-fns';
import { debounceTime, map } from 'rxjs';
import { DEFAULT_PAGE_SIZE, PAGE_SIZE_OPTIONS } from '../../../core/http/paged-request';
import { PagedResponse } from '../../../core/http/paged-response';
import { toIsoDate } from '../../../shared/dates/iso-date';
import { LoadError } from '../../../shared/load-error/load-error';
import { PageHeader } from '../../../shared/page-header/page-header';
import { BookingStatusChip } from '../booking-status-chip/booking-status-chip';
import { BookingListItemResponse } from '../data/booking-list-item-response';
import { BookingSearchRequest } from '../data/booking-search-request';
import { BOOKING_STATUS_LABELS, BookingStatus } from '../data/booking-status';
import { BookingsApi } from '../data/bookings-api';

const SEARCH_DEBOUNCE_MS = 300;
const DEFAULT_SORT = '-createdAtUtc';
const DESCENDING_PREFIX = '-';

type BookingsPage = PagedResponse<BookingListItemResponse>;

@Component({
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    LoadError,
    PageHeader,
    BookingStatusChip,
  ],
  selector: 'app-booking-list-page',
  styleUrl: './booking-list-page.scss',
  templateUrl: './booking-list-page.html',
})
export class BookingListPage {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly bookingsApi = inject(BookingsApi);

  protected readonly displayedColumns = [
    'reference',
    'status',
    'hotelName',
    'leadGuestName',
    'clientName',
    'checkIn',
    'checkOut',
    'createdAtUtc',
  ];
  protected readonly pageSizeOptions = PAGE_SIZE_OPTIONS;
  protected readonly statuses = Object.entries(BOOKING_STATUS_LABELS) as [BookingStatus, string][];

  // The URL is the source of truth, so Back, refresh and shared links keep the search.
  protected readonly request = toSignal(this.route.queryParamMap.pipe(map(readSearchRequest)), {
    requireSync: true,
  });

  protected readonly bookings = rxResource({
    params: this.request,
    stream: ({ params }) => this.bookingsApi.search(params),
  });

  // Keeps the previous page on screen while the next one loads, so the table doesn't flash empty.
  protected readonly shownPage = linkedSignal<BookingsPage | undefined, BookingsPage | undefined>({
    source: () => (this.bookings.hasValue() ? this.bookings.value() : undefined),
    computation: (current, previous) => current ?? previous?.value,
  });

  protected readonly sortActive = computed(() =>
    (this.request().sort ?? DEFAULT_SORT).replace(DESCENDING_PREFIX, ''),
  );
  protected readonly sortDirection = computed<SortDirection>(() =>
    (this.request().sort ?? DEFAULT_SORT).startsWith(DESCENDING_PREFIX) ? 'desc' : 'asc',
  );

  protected readonly filters = inject(FormBuilder).nonNullable.group({
    search: this.request().search ?? '',
    status: new FormControl<BookingStatus | null>(this.request().status ?? null),
    checkIn: new FormControl<Date | null>(toDate(this.request().checkIn)),
    checkOut: new FormControl<Date | null>(toDate(this.request().checkOut)),
  });

  constructor() {
    effect(() => this.showRequestInFilters(this.request()));

    this.filters.valueChanges
      .pipe(debounceTime(SEARCH_DEBOUNCE_MS), takeUntilDestroyed())
      .subscribe(() => {
        const filters = this.filters.getRawValue();
        this.navigate({
          search: filters.search.trim() || undefined,
          status: filters.status ?? undefined,
          checkIn: toOptionalIsoDate(filters.checkIn),
          checkOut: toOptionalIsoDate(filters.checkOut),
          page: 1,
        });
      });
  }

  protected showArrivalsToday(): void {
    this.navigate({ status: 'Confirmed', checkIn: toIsoDate(startOfToday()), checkOut: undefined, page: 1 });
  }

  protected showDeparturesToday(): void {
    this.navigate({ status: 'Confirmed', checkIn: undefined, checkOut: toIsoDate(startOfToday()), page: 1 });
  }

  protected clearFilters(): void {
    this.navigate({ search: undefined, status: undefined, checkIn: undefined, checkOut: undefined, page: 1 });
  }

  protected onSortChange(sort: Sort): void {
    const descending = sort.direction === 'desc';
    this.navigate({ sort: descending ? DESCENDING_PREFIX + sort.active : sort.active, page: 1 });
  }

  protected onPageChange(event: PageEvent): void {
    this.navigate({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }

  // Only fields that disagree with the URL are rewritten, so the one being typed in keeps its trailing space.
  private showRequestInFilters(request: BookingSearchRequest): void {
    const { search, status, checkIn, checkOut } = this.filters.controls;
    if (search.value.trim() !== (request.search ?? '')) {
      search.setValue(request.search ?? '', { emitEvent: false });
    }
    if (status.value !== (request.status ?? null)) {
      status.setValue(request.status ?? null, { emitEvent: false });
    }
    if (toOptionalIsoDate(checkIn.value) !== request.checkIn) {
      checkIn.setValue(toDate(request.checkIn), { emitEvent: false });
    }
    if (toOptionalIsoDate(checkOut.value) !== request.checkOut) {
      checkOut.setValue(toDate(request.checkOut), { emitEvent: false });
    }
  }

  private navigate(changes: Partial<BookingSearchRequest>): void {
    const queryParams = { ...this.request(), ...changes };
    this.router.navigate([], { relativeTo: this.route, queryParams, replaceUrl: true });
  }
}

function readSearchRequest(params: ParamMap): BookingSearchRequest {
  return {
    page: Number(params.get('page') ?? 1),
    pageSize: Number(params.get('pageSize') ?? DEFAULT_PAGE_SIZE),
    sort: params.get('sort') ?? undefined,
    search: params.get('search') ?? undefined,
    status: (params.get('status') as BookingStatus | null) ?? undefined,
    checkIn: params.get('checkIn') ?? undefined,
    checkOut: params.get('checkOut') ?? undefined,
  };
}

function toDate(isoDate: string | undefined): Date | null {
  const date = parseISO(isoDate ?? '');
  return isValid(date) ? date : null;
}

function toOptionalIsoDate(date: Date | null): string | undefined {
  return date ? toIsoDate(date) : undefined;
}
