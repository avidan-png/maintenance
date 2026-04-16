// frontend/src/app/features/contracts/alert-settings/alert-settings.ts
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlertsService } from '../../../core/services/alerts.service';
import { AlertSettings } from '../../../core/models/alert.model';

@Component({
  selector: 'app-alert-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './alert-settings.html',
})
export class AlertSettingsComponent implements OnInit {
  settings = signal<AlertSettings>({
    alert6Mois: true,
    alert3Mois: true,
    alert1Mois: true,
    alertDepasse: true,
    email: '',
    copieCLient: false,
    resumeHebdo: false,
  });
  saved = signal(false);

  constructor(private alerts: AlertsService) {}

  ngOnInit(): void {
    this.alerts.getSettings().subscribe(s => this.settings.set(s));
  }

  save(): void {
    this.alerts.updateSettings(this.settings()).subscribe(() => {
      this.saved.set(true);
      setTimeout(() => this.saved.set(false), 2000);
    });
  }

  toggle(key: keyof AlertSettings): void {
    const current = this.settings();
    this.settings.set({ ...current, [key]: !current[key] });
  }
}
