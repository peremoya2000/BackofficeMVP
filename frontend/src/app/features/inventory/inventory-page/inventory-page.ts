import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, input, linkedSignal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import {
  addDays,
  eachDayOfInterval,
  isBefore,
  isValid,
  isWeekend,
  parseISO,
  startOfToday,
} from 'date-fns';
import { map } from 'rxjs';
import { toIsoDate } from '../../../shared/dates/iso-date';
import { PageHeader } from '../../../shared/page-header/page-header';
import { InventoryApi } from '../data/inventory-api';
import { InventoryCalendarResponse } from '../data/inventory-calendar-response';
import { InventoryDayResponse } from '../data/inventory-day-response';
import {
  InventoryRangeDialog,
  InventoryRangeDialogData,
} from '../inventory-range-dialog/inventory-range-dialog';

const VISIBLE_DAYS = 14;
const STEP_DAYS = 7;
const HIGH_OCCUPANCY = 0.7;
const SAVED_SNACK_BAR_DURATION_MS = 3000;

type CellStatus = 'none' | 'closed' | 'sold-out' | 'high' | 'open';

interface Column {
  date: Date;
  key: string;
  dayFormat: string;
  past: boolean;
  weekend: boolean;
}

interface Cell {
  column: Column;
  day: InventoryDayResponse | undefined;
  available: number;
  status: CellStatus;
  tooltip: string;
}

@Component({
  imports: [
    CurrencyPipe,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTooltipModule,
    PageHeader,
  ],
  selector: 'app-inventory-page',
  styleUrl: './inventory-page.scss',
  templateUrl: './inventory-page.html',
})
export class InventoryPage {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly inventoryApi = inject(InventoryApi);

  readonly id = input.required<string>();

  protected readonly stepDays = STEP_DAYS;

  // The first visible night lives in the URL, like the hotels list's search.
  protected readonly from = toSignal(this.route.queryParamMap.pipe(map(readFrom)), {
    requireSync: true,
  });

  protected readonly calendar = rxResource({
    params: () => ({ hotelId: this.id(), from: this.from() }),
    stream: ({ params }) =>
      this.inventoryApi.getCalendar(
        params.hotelId,
        toIsoDate(params.from),
        toIsoDate(addDays(params.from, VISIBLE_DAYS - 1)),
      ),
  });

  // Keeps the previous weeks on screen while the next ones load, so the grid doesn't flash empty.
  protected readonly shownCalendar = linkedSignal<
    InventoryCalendarResponse | undefined,
    InventoryCalendarResponse | undefined
  >({
    source: () => (this.calendar.hasValue() ? this.calendar.value() : undefined),
    computation: (current, previous) => current ?? previous?.value,
  });

  protected readonly columns = computed<Column[]>(() => {
    const calendar = this.shownCalendar();
    if (!calendar) return [];

    const today = startOfToday();
    return eachDayOfInterval({ start: parseISO(calendar.from), end: parseISO(calendar.to) }).map(
      (date, index) => ({
        date,
        key: toIsoDate(date),
        dayFormat: index === 0 || date.getDate() === 1 ? 'd MMM' : 'd',
        past: isBefore(date, today),
        weekend: isWeekend(date),
      }),
    );
  });

  protected readonly rows = computed(() =>
    (this.shownCalendar()?.roomTypes ?? []).map((roomType) => {
      const nights = new Map(roomType.days.map((day) => [day.date, day]));
      return {
        roomTypeId: roomType.roomTypeId,
        name: roomType.name,
        cells: this.columns().map((column) => toCell(column, nights.get(column.key))),
      };
    }),
  );

  protected shift(days: number): void {
    this.showFrom(addDays(this.from(), days));
  }

  protected showToday(): void {
    this.showFrom(startOfToday());
  }

  protected openEditor(roomTypeId?: string, date?: Date): void {
    const calendar = this.shownCalendar();
    if (!calendar) return;

    const data: InventoryRangeDialogData = {
      roomTypes: calendar.roomTypes.map((roomType) => ({
        id: roomType.roomTypeId,
        name: roomType.name,
      })),
      roomTypeId: roomTypeId ?? calendar.roomTypes[0].roomTypeId,
      from: date ?? null,
      to: date ?? null,
      currency: calendar.currency,
    };

    this.dialog
      .open<InventoryRangeDialog, InventoryRangeDialogData, boolean>(InventoryRangeDialog, { data })
      .afterClosed()
      .subscribe((saved) => {
        if (!saved) return;
        this.snackBar.open('Inventario actualizado.', 'Cerrar', {
          duration: SAVED_SNACK_BAR_DURATION_MS,
        });
        this.calendar.reload();
      });
  }

  private showFrom(from: Date): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { from: toIsoDate(from) },
      replaceUrl: true,
    });
  }
}

function readFrom(params: ParamMap): Date {
  const from = parseISO(params.get('from') ?? '');
  return isValid(from) ? from : startOfToday();
}

function toCell(column: Column, day: InventoryDayResponse | undefined): Cell {
  if (!day) return { column, day, available: 0, status: 'none', tooltip: 'Sin cupo' };

  const available = day.totalRooms - day.bookedRooms;
  const details = `${available} libres · ${day.bookedRooms} reservadas · cupo ${day.totalRooms}`;
  return {
    column,
    day,
    available,
    status: statusOf(day, available),
    tooltip: day.salesClosed ? `Venta cerrada · ${details}` : details,
  };
}

function statusOf(day: InventoryDayResponse, available: number): CellStatus {
  if (day.totalRooms === 0) return 'none';
  if (day.salesClosed) return 'closed';
  if (available === 0) return 'sold-out';
  return day.bookedRooms / day.totalRooms >= HIGH_OCCUPANCY ? 'high' : 'open';
}
