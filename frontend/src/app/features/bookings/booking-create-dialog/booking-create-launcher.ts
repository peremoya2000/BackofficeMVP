import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { BookingResponse } from '../data/booking-response';
import { BookingCreateDialog, BookingCreateDialogData } from './booking-create-dialog';

const CONFIRMED_SNACK_BAR_DURATION_MS = 3000;

@Injectable({ providedIn: 'root' })
export class BookingCreateLauncher {
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  open(data: BookingCreateDialogData): void {
    this.dialog
      .open<BookingCreateDialog, BookingCreateDialogData, BookingResponse>(BookingCreateDialog, {
        data,
      })
      .afterClosed()
      .subscribe((booking) => {
        if (!booking) return;
        this.snackBar.open(`Reserva ${booking.reference} confirmada.`, 'Cerrar', {
          duration: CONFIRMED_SNACK_BAR_DURATION_MS,
        });
        this.router.navigate(['/bookings', booking.id]);
      });
  }
}
