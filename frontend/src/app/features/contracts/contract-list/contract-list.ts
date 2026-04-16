// frontend/src/app/features/contracts/contract-list/contract-list.ts
import { Component, input, computed } from '@angular/core';
import { CommonModule, DatePipe, CurrencyPipe } from '@angular/common';
import { Contract } from '../../../core/models/contract.model';

@Component({
  selector: 'app-contract-list',
  standalone: true,
  imports: [CommonModule, DatePipe, CurrencyPipe],
  templateUrl: './contract-list.html',
})
export class ContractListComponent {
  readonly contracts = input<Contract[]>([]);
  filterUrgents = false;

  readonly displayed = computed(() =>
    this.filterUrgents
      ? this.contracts().filter(c => c.statutDenonciation === 'depasse')
      : this.contracts()
  );

  readonly urgentsCount = computed(() =>
    this.contracts().filter(c => c.statutDenonciation === 'depasse').length
  );

  readonly columns = [
    'source', 'adresse', 'prestation', 'prestataire',
    'montantHtAnnuel', 'dateFin', 'dateDenonciation', 'statutDenonciation'
  ];

  badgeClass(statut: string): string {
    if (statut === 'depasse') return 'statut statut--depasse';
    if (statut === 'bientot') return 'statut statut--bientot';
    return 'statut statut--ok';
  }

  badgeLabel(statut: string): string {
    if (statut === 'depasse') return 'Reconduit';
    if (statut === 'bientot') return 'Bientôt';
    return 'OK';
  }
}
