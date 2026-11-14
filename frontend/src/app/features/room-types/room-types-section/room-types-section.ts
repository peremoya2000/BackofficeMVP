import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { LoadError } from '../../../shared/load-error/load-error';
import { ROOM_CATEGORY_LABELS, RoomCategory } from '../data/room-category';
import { RoomTypeResponse } from '../data/room-type-response';
import { RoomTypesApi } from '../data/room-types-api';
import {
  RoomTypeCreateDialog,
  RoomTypeCreateDialogData,
} from '../room-type-create-dialog/room-type-create-dialog';

const CREATED_SNACK_BAR_DURATION_MS = 4000;

@Component({
  imports: [MatButtonModule, MatIconModule, MatProgressBarModule, MatTableModule, LoadError],
  selector: 'app-room-types-section',
  styleUrl: './room-types-section.scss',
  templateUrl: './room-types-section.html',
})
export class RoomTypesSection {
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly roomTypesApi = inject(RoomTypesApi);

  readonly hotelId = input.required<string>();

  protected readonly displayedColumns = ['name', 'category', 'maxOccupancy'];

  protected readonly roomTypes = rxResource({
    params: this.hotelId,
    stream: ({ params: hotelId }) => this.roomTypesApi.list(hotelId),
  });

  // mat-table rows are untyped in the template, so the lookup goes through a typed parameter.
  protected categoryLabel(category: RoomCategory): string {
    return ROOM_CATEGORY_LABELS[category];
  }

  protected openCreateDialog(): void {
    const data: RoomTypeCreateDialogData = { hotelId: this.hotelId() };

    this.dialog
      .open<RoomTypeCreateDialog, RoomTypeCreateDialogData, RoomTypeResponse>(RoomTypeCreateDialog, {
        data,
      })
      .afterClosed()
      .subscribe((roomType) => {
        if (!roomType) return;
        this.snackBar.open(`Tipo de habitación «${roomType.name}» creado.`, 'Cerrar', {
          duration: CREATED_SNACK_BAR_DURATION_MS,
        });
        this.roomTypes.reload();
      });
  }
}
