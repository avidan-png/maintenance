// frontend/src/app/features/contracts/contracts-page/contracts-page.ts
import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContractListComponent } from '../contract-list/contract-list';
import { UploadZoneComponent } from '../upload-zone/upload-zone';
import { PreavisConfigComponent } from '../preavis-config/preavis-config';
import { TimelineComponent } from '../timeline/timeline';
import { AlertsPanelComponent } from '../alerts-panel/alerts-panel';
import { AlertSettingsComponent } from '../alert-settings/alert-settings';
import { ContractsService } from '../../../core/services/contracts.service';
import { Contract } from '../../../core/models/contract.model';

type Tab = 'tableau' | 'timeline' | 'alertes' | 'parametres';

@Component({
  selector: 'app-contracts-page',
  standalone: true,
  imports: [
    CommonModule,
    ContractListComponent,
    UploadZoneComponent,
    PreavisConfigComponent,
    TimelineComponent,
    AlertsPanelComponent,
    AlertSettingsComponent,
  ],
  templateUrl: './contracts-page.html',
})
export class ContractsPageComponent implements OnInit {
  contracts = signal<Contract[]>([]);
  activeTab = signal<Tab>('tableau');

  constructor(private service: ContractsService) {}

  ngOnInit(): void {
    this.loadContracts();
  }

  loadContracts(): void {
    this.service.getAll().subscribe(data => this.contracts.set(data));
  }

  setTab(tab: Tab): void {
    this.activeTab.set(tab);
  }

  get urgentsCount(): number {
    return this.contracts().filter(c => c.statutDenonciation === 'depasse').length;
  }

  get bientotCount(): number {
    return this.contracts().filter(c => c.statutDenonciation === 'bientot').length;
  }
}
