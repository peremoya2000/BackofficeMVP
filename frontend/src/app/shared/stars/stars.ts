import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-stars',
  template: '{{ symbols() }}',
  styles: `
    :host {
      color: #e8a400;
      letter-spacing: 2px;
      white-space: nowrap;
    }
  `,
  host: { role: 'img', '[attr.aria-label]': 'label()' },
})
export class Stars {
  readonly count = input.required<number>();

  protected readonly symbols = computed(() => '★'.repeat(this.count()));
  protected readonly label = computed(() =>
    this.count() === 1 ? '1 estrella' : `${this.count()} estrellas`,
  );
}
