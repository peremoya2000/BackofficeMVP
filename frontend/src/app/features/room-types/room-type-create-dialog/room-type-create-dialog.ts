import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ROOM_CATEGORY_LABELS, RoomCategory } from '../data/room-category';
import { RoomTypeResponse } from '../data/room-type-response';
import { RoomTypesApi } from '../data/room-types-api';

export interface RoomTypeCreateDialogData {
  hotelId: string;
}

@Component({
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  selector: 'app-room-type-create-dialog',
  styleUrl: './room-type-create-dialog.scss',
  templateUrl: './room-type-create-dialog.html',
})
export class RoomTypeCreateDialog {
  private readonly dialogRef =
    inject<MatDialogRef<RoomTypeCreateDialog, RoomTypeResponse>>(MatDialogRef);
  private readonly roomTypesApi = inject(RoomTypesApi);

  protected readonly data = inject<RoomTypeCreateDialogData>(MAT_DIALOG_DATA);
  protected readonly categories = Object.entries(ROOM_CATEGORY_LABELS) as [RoomCategory, string][];
  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    category: new FormControl<RoomCategory>('Double', { nonNullable: true }),
    maxOccupancy: new FormControl(2, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1)],
    }),
  });

  protected save(): void {
    if (this.form.invalid) return;

    this.saving.set(true);
    this.dialogRef.disableClose = true;
    this.roomTypesApi.create(this.data.hotelId, this.form.getRawValue()).subscribe({
      next: (roomType) => this.dialogRef.close(roomType),
      error: () => {
        this.saving.set(false);
        this.dialogRef.disableClose = false;
      },
    });
  }
}
