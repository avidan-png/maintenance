// frontend/src/app/features/contracts/upload-zone/upload-zone.ts
import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContractsService } from '../../../core/services/contracts.service';

@Component({
  selector: 'app-upload-zone',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-zone.html',
})
export class UploadZoneComponent {
  readonly preavisDefault = input<number>(3);
  readonly contractsUpdated = output<void>();
  loading = false;
  lastResult: string | null = null;

  constructor(private service: ContractsService) {}

  onExcelSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.loading = true;
    this.lastResult = null;
    this.service.importFromExcel(file, this.preavisDefault()).subscribe({
      next: ({ imported }) => {
        this.loading = false;
        this.lastResult = `${imported} contrats importés avec succès`;
        this.contractsUpdated.emit();
      },
      error: () => {
        this.loading = false;
        this.lastResult = 'Erreur lors de l\'import';
      },
    });
  }
}
