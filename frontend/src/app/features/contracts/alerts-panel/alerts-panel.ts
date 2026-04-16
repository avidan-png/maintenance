// frontend/src/app/features/contracts/alerts-panel/alerts-panel.ts
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertsService } from '../../../core/services/alerts.service';
import { AlertContract, AlertSummary } from '../../../core/models/alert.model';

@Component({
  selector: 'app-alerts-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './alerts-panel.html',
})
export class AlertsPanelComponent implements OnInit {
  summary = signal<AlertSummary>({ depasse: [], bientot: [] });
  loading = signal(true);

  constructor(private alerts: AlertsService) {}

  ngOnInit(): void {
    this.alerts.getSummary().subscribe({
      next: data => { this.summary.set(data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  formatContracts(list: AlertContract[]): string {
    return list.map(c => c.adresse.split(',')[0] || c.prestataire).slice(0, 4).join(' · ');
  }
}
