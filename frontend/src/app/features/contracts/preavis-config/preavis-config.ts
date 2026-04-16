// frontend/src/app/features/contracts/preavis-config/preavis-config.ts
import { Component, OnInit, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../core/services/settings.service';

@Component({
  selector: 'app-preavis-config',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './preavis-config.html',
})
export class PreavisConfigComponent implements OnInit {
  readonly preavisChanged = output<number>();
  readonly options = [1, 2, 3, 6];
  selected = 3;

  constructor(private settings: SettingsService) {}

  ngOnInit(): void {
    this.settings.getPreavisDefault().subscribe(({ mois }) => {
      this.selected = mois;
    });
  }

  onChange(mois: number): void {
    this.settings.updatePreavisDefault(mois).subscribe();
    this.preavisChanged.emit(mois);
  }
}
