// frontend/src/app/features/contracts/contracts-page/contracts-page.ts
import { Component, OnInit, signal } from '@angular/core';
import { ContractListComponent } from '../contract-list/contract-list';
import { UploadZoneComponent } from '../upload-zone/upload-zone';
import { PreavisConfigComponent } from '../preavis-config/preavis-config';
import { ContractsService } from '../../../core/services/contracts.service';
import { Contract } from '../../../core/models/contract.model';

@Component({
  selector: 'app-contracts-page',
  standalone: true,
  imports: [ContractListComponent, UploadZoneComponent, PreavisConfigComponent],
  templateUrl: './contracts-page.html',
})
export class ContractsPageComponent implements OnInit {
  contracts = signal<Contract[]>([]);
  preavis = signal<number>(3);

  constructor(private service: ContractsService) {}

  ngOnInit(): void {
    this.loadContracts();
  }

  loadContracts(): void {
    this.service.getAll().subscribe(data => this.contracts.set(data));
  }
}
