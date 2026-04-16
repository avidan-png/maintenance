// frontend/src/app/features/contracts/timeline/timeline.ts
import { Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Contract } from '../../../core/models/contract.model';

interface TimelineRow {
  label: string;
  barLeft: number;    // % from year start
  barWidth: number;   // % width
  markerLeft: number; // % position of denonciation date
  barClass: string;   // 'bar-ok' | 'bar-warn' | 'bar-danger'
  markerClass: string; // 'dm-ok' | 'dm-warn' | 'dm-danger'
  statusLabel: string;
}

@Component({
  selector: 'app-timeline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './timeline.html',
})
export class TimelineComponent {
  readonly contracts = input<Contract[]>([]);

  readonly MONTHS = ['Jan','Fév','Mar','Avr','Mai','Juin','Juil','Août','Sep','Oct','Nov','Déc'];
  readonly currentMonthIndex = new Date().getMonth(); // 0-based
  readonly currentYear = new Date().getFullYear();

  readonly rows = computed<TimelineRow[]>(() => {
    const year = new Date().getFullYear();
    const yearStart = new Date(year, 0, 1).getTime();
    const yearEnd   = new Date(year, 11, 31, 23, 59, 59).getTime();
    const yearMs    = yearEnd - yearStart;

    return this.contracts()
      .filter(c => c.dateFin != null)
      .map(c => {
        const debut  = c.dateDebut ? new Date(c.dateDebut).getTime() : yearStart;
        const fin    = new Date(c.dateFin!).getTime();
        const denonc = c.dateDenonciation ? new Date(c.dateDenonciation).getTime() : null;

        const clamp = (v: number) => Math.min(100, Math.max(0, v));

        const barLeft  = clamp((debut - yearStart) / yearMs * 100);
        const barRight = clamp((fin   - yearStart) / yearMs * 100);
        const barWidth = Math.max(barRight - barLeft, 0.5);
        const markerLeft = denonc != null
          ? clamp((denonc - yearStart) / yearMs * 100)
          : barRight;

        const barClass    = c.statutDenonciation === 'depasse' ? 'bar-danger'
                          : c.statutDenonciation === 'bientot' ? 'bar-warn'
                          : 'bar-ok';
        const markerClass = c.statutDenonciation === 'depasse' ? 'dm-danger'
                          : c.statutDenonciation === 'bientot' ? 'dm-warn'
                          : 'dm-ok';

        const monthsLeft = denonc != null
          ? Math.round((denonc - Date.now()) / (1000 * 60 * 60 * 24 * 30))
          : 0;
        const statusLabel = c.statutDenonciation === 'depasse' ? 'Reconduit tacitement'
                          : `Dans ${monthsLeft} mois`;

        const label = `${c.prestation} · ${c.adresse.split(',')[0]}`;

        return { label, barLeft, barWidth, markerLeft, barClass, markerClass, statusLabel };
      });
  });

  readonly todayLeft = computed<number>(() => {
    const year = new Date().getFullYear();
    const yearStart = new Date(year, 0, 1).getTime();
    const yearEnd   = new Date(year, 11, 31, 23, 59, 59).getTime();
    const yearMs    = yearEnd - yearStart;
    return (Date.now() - yearStart) / yearMs * 100;
  });
}
