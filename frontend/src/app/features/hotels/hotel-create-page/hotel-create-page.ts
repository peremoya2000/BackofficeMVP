import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { CountryNamePipe } from '../../../shared/countries/country-name-pipe';
import { knownCountryValidator } from '../../../shared/countries/country-names';
import { PageHeader } from '../../../shared/page-header/page-header';
import { Stars } from '../../../shared/stars/stars';
import { HotelsApi } from '../data/hotels-api';

const CREATED_SNACK_BAR_DURATION_MS = 4000;

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    CountryNamePipe,
    PageHeader,
    Stars,
  ],
  selector: 'app-hotel-create-page',
  styleUrl: './hotel-create-page.scss',
  templateUrl: './hotel-create-page.html',
})
export class HotelCreatePage {
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly hotelsApi = inject(HotelsApi);

  protected readonly starOptions = [1, 2, 3, 4, 5];
  protected readonly saving = signal(false);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    stars: new FormControl<number | null>(null, Validators.required),
    address: ['', Validators.required],
    city: ['', Validators.required],
    countryCode: ['', [Validators.required, knownCountryValidator]],
    contactEmail: ['', Validators.email],
    contactPhone: [''],
  });

  protected save(): void {
    const value = this.form.getRawValue();
    if (this.form.invalid || value.stars === null) return;

    this.saving.set(true);
    this.hotelsApi
      .create({
        name: value.name,
        stars: value.stars,
        address: value.address,
        city: value.city,
        countryCode: value.countryCode,
        contactEmail: value.contactEmail,
        contactPhone: value.contactPhone,
      })
      .subscribe({
        next: (hotel) => {
          this.snackBar.open(`Hotel «${hotel.name}» creado.`, 'Cerrar', {
            duration: CREATED_SNACK_BAR_DURATION_MS,
          });
          this.router.navigate(['/hotels', hotel.id], { replaceUrl: true });
        },
        error: () => this.saving.set(false),
      });
  }
}
