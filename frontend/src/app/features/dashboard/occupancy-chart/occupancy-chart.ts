import { formatDate } from '@angular/common';
import {
  Component,
  ElementRef,
  LOCALE_ID,
  afterRenderEffect,
  inject,
  input,
  viewChild,
} from '@angular/core';
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  ChartConfiguration,
  LinearScale,
  Tooltip,
} from 'chart.js';
import { OccupancyDayResponse } from '../data/occupancy-day-response';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip);
Chart.defaults.font.family = 'Roboto, sans-serif';

@Component({
  selector: 'app-occupancy-chart',
  template: `
    <canvas #canvas role="img" aria-label="Gráfico de barras: ocupación de cada noche"></canvas>
  `,
  styles: `
    :host {
      display: block;
      position: relative;
      height: 280px;
      color: var(--mat-sys-primary);
    }
  `,
})
export class OccupancyChart {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly locale = inject(LOCALE_ID);
  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');

  readonly nights = input.required<OccupancyDayResponse[]>();

  constructor() {
    afterRenderEffect((onCleanup) => {
      const chart = new Chart(this.canvas().nativeElement, this.buildConfig(this.nights()));
      onCleanup(() => chart.destroy());
    });
  }

  private buildConfig(nights: OccupancyDayResponse[]): ChartConfiguration<'bar'> {
    // A canvas can't resolve CSS variables, so the theme colour is read from the host's computed `color`.
    const barColor = getComputedStyle(this.host.nativeElement).color;

    return {
      type: 'bar',
      data: {
        labels: nights.map((night) => [
          formatDate(night.date, 'EEE', this.locale),
          formatDate(night.date, 'd', this.locale),
        ]),
        datasets: [
          {
            data: nights.map(occupancyPercent),
            backgroundColor: barColor,
            borderRadius: 4,
            maxBarThickness: 24,
          },
        ],
      },
      options: {
        maintainAspectRatio: false,
        animation: false,
        interaction: { mode: 'index', intersect: false },
        scales: {
          x: { grid: { display: false }, ticks: { maxRotation: 0 } },
          y: {
            min: 0,
            max: 100,
            border: { display: false },
            ticks: { stepSize: 25, callback: (value) => `${value} %` },
          },
        },
        plugins: {
          tooltip: {
            displayColors: false,
            callbacks: {
              title: ([item]) =>
                formatDate(nights[item.dataIndex].date, 'EEEE d MMMM', this.locale),
              label: (item) => describeNight(nights[item.dataIndex]),
            },
          },
        },
      },
    };
  }
}

function occupancyPercent(night: OccupancyDayResponse): number {
  return night.totalRooms === 0 ? 0 : Math.round((100 * night.bookedRooms) / night.totalRooms);
}

function describeNight(night: OccupancyDayResponse): string {
  return `${occupancyPercent(night)} % · ${night.bookedRooms} de ${night.totalRooms} habitaciones`;
}
