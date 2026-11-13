import { Pipe, PipeTransform } from '@angular/core';
import { countryName } from './country-names';

@Pipe({ name: 'countryName' })
export class CountryNamePipe implements PipeTransform {
  transform(code: string | null | undefined): string {
    return countryName(code) ?? '';
  }
}
