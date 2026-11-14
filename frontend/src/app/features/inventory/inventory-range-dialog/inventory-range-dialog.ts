import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { startOfToday } from 'date-fns';
import { toIsoDate } from '../../../shared/dates/iso-date';
import { InventoryApi } from '../data/inventory-api';

export interface InventoryRangeDialogData {
  roomTypes: { id: string; name: string }[];
  roomTypeId: string;
  from: Date | null;
  to: Date | null;
  currency: string;
}

@Component({
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  selector: 'app-inventory-range-dialog',
  styleUrl: './inventory-range-dialog.scss',
  templateUrl: './inventory-range-dialog.html',
})
export class InventoryRangeDialog {
  private readonly dialogRef = inject<MatDialogRef<InventoryRangeDialog, boolean>>(MatDialogRef);
  private readonly inventoryApi = inject(InventoryApi);

  protected readonly data = inject<InventoryRangeDialogData>(MAT_DIALOG_DATA);
  protected readonly today = startOfToday();
  protected readonly saving = signal(false);

  protected readonly form = new FormGroup(
    {
      roomTypeId: new FormControl(this.data.roomTypeId, {
        nonNullable: true,
        validators: Validators.required,
      }),
      from: new FormControl(this.data.from, Validators.required),
      to: new FormControl(this.data.to, Validators.required),
      totalRooms: new FormControl<number | null>(null),
      price: new FormControl<number | null>(null),
      salesClosed: new FormControl<boolean | null>(null),
    },
    { validators: atLeastOneChange },
  );

  protected save(): void {
    const value = this.form.getRawValue();
    if (this.form.invalid || !value.from || !value.to) return;

    this.saving.set(true);
    this.dialogRef.disableClose = true;
    this.inventoryApi
      .updateRange(value.roomTypeId, {
        from: toIsoDate(value.from),
        to: toIsoDate(value.to),
        totalRooms: value.totalRooms,
        price: value.price,
        salesClosed: value.salesClosed,
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: () => {
          this.saving.set(false);
          this.dialogRef.disableClose = false;
        },
      });
  }
}

function atLeastOneChange(form: AbstractControl): ValidationErrors | null {
  const { totalRooms, price, salesClosed } = form.value;
  return totalRooms == null && price == null && salesClosed == null
    ? { nothingToChange: true }
    : null;
}
