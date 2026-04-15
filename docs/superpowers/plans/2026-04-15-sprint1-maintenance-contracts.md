# Sprint 1 — Contrats de maintenance : Import Excel + Date de dénonciation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permettre l'import d'un tableau récap Excel multi-feuilles, calculer automatiquement la date de dénonciation (date_fin − délai préavis), et afficher deux nouvelles colonnes dans le tableau existant des contrats.

**Architecture:** Extension de l'app Angular 20 + Taiga UI existante. Le parsing Excel se fait côté backend (NestJS). Deux nouvelles colonnes (`date_denonciation`, `statut_denonciation`) sont calculées à la réception et stockées en base. Le frontend reçoit ces champs via l'API existante étendue.

**Tech Stack:** Angular 20, Taiga UI, NestJS (backend), SheetJS/xlsx (parsing Excel), TypeScript, Jest (tests)

> ⚠️ **Note préalable :** Les chemins de fichiers ci-dessous suivent les conventions Angular observées via les noms de composants (`app-upload-zone`, `app-contract-list`, etc.). Vérifier les chemins exacts dans le repo avant de commencer — adapter si la structure diffère.

---

## Cartographie des fichiers

### Backend (NestJS)
| Fichier | Action | Rôle |
|---|---|---|
| `src/maintenance-contracts/dto/import-excel.dto.ts` | Créer | DTO de validation pour l'import Excel |
| `src/maintenance-contracts/maintenance-contracts.service.ts` | Modifier | Méthode `importFromExcel()` + calcul `date_denonciation` |
| `src/maintenance-contracts/maintenance-contracts.controller.ts` | Modifier | Endpoint `POST /maintenance-contracts/import/excel` |
| `src/maintenance-contracts/entities/contract.entity.ts` | Modifier | Ajout champs `date_denonciation`, `statut_denonciation`, `delai_preavis_mois` |
| `src/maintenance-contracts/utils/excel-parser.ts` | Créer | Parsing SheetJS + mapping colonnes |
| `src/maintenance-contracts/utils/denonciation.ts` | Créer | Calcul date et statut dénonciation |
| `src/settings/settings.service.ts` | Modifier | Lecture/écriture `preavis_default_mois` |
| `src/settings/settings.controller.ts` | Modifier | `PATCH /settings/preavis-default` |
| `test/excel-parser.spec.ts` | Créer | Tests unitaires parser Excel |
| `test/denonciation.spec.ts` | Créer | Tests unitaires calcul dénonciation |

### Frontend (Angular)
| Fichier | Action | Rôle |
|---|---|---|
| `src/app/shared/models/contract.model.ts` | Modifier | Ajout `dateDenonciation`, `statutDenonciation`, `delaiPreavisMois` |
| `src/app/modules/maintenance-contracts/components/upload-zone/upload-zone.component.ts` | Modifier | Accepter `.xlsx`/`.xls` + appel import Excel |
| `src/app/modules/maintenance-contracts/components/upload-zone/upload-zone.component.html` | Modifier | Carte Excel avec pill "Nouveau" |
| `src/app/modules/maintenance-contracts/components/contract-list/contract-list.component.ts` | Modifier | Colonnes `date_denonciation` + `statut_denonciation` + filtre Urgents |
| `src/app/modules/maintenance-contracts/components/contract-list/contract-list.component.html` | Modifier | 2 nouvelles colonnes + chip filtre |
| `src/app/modules/maintenance-contracts/components/preavis-config/preavis-config.component.ts` | Créer | Composant sélecteur délai préavis |
| `src/app/modules/maintenance-contracts/components/preavis-config/preavis-config.component.html` | Créer | Template Taiga UI select |
| `src/app/modules/maintenance-contracts/maintenance-contracts.service.ts` | Modifier | Méthode `importExcel()` + `updatePreavisDefault()` |

---

## Task 1 : Utilitaire backend — calcul de dénonciation

**Files:**
- Créer : `src/maintenance-contracts/utils/denonciation.ts`
- Créer : `test/denonciation.spec.ts`

- [ ] **Step 1.1 : Écrire les tests unitaires**

```typescript
// test/denonciation.spec.ts
import { computeDenonciation, StatutDenonciation } from '../src/maintenance-contracts/utils/denonciation';

describe('computeDenonciation', () => {
  const today = new Date('2026-04-15');

  it('calcule la date de dénonciation = date_fin - delai_mois', () => {
    const result = computeDenonciation(new Date('2026-12-31'), 3, today);
    expect(result.dateDenonciation).toEqual(new Date('2026-09-30'));
  });

  it('statut = "depasse" si date_denonciation < aujourd\'hui', () => {
    const result = computeDenonciation(new Date('2025-12-31'), 3, today);
    expect(result.statut).toBe(StatutDenonciation.DEPASSE);
  });

  it('statut = "bientot" si date_denonciation dans moins de 6 mois', () => {
    const result = computeDenonciation(new Date('2026-09-30'), 3, today);
    expect(result.statut).toBe(StatutDenonciation.BIENTOT);
  });

  it('statut = "ok" si date_denonciation dans plus de 6 mois', () => {
    const result = computeDenonciation(new Date('2027-06-30'), 3, today);
    expect(result.statut).toBe(StatutDenonciation.OK);
  });

  it('gère les fins de mois correctement (jan → 31 oct)', () => {
    const result = computeDenonciation(new Date('2027-01-31'), 3, today);
    expect(result.dateDenonciation).toEqual(new Date('2026-10-31'));
  });
});
```

- [ ] **Step 1.2 : Lancer les tests — vérifier qu'ils échouent**

```bash
cd backend && npx jest test/denonciation.spec.ts --no-coverage
```
Attendu : `FAIL — Cannot find module`

- [ ] **Step 1.3 : Implémenter l'utilitaire**

```typescript
// src/maintenance-contracts/utils/denonciation.ts

export enum StatutDenonciation {
  DEPASSE = 'depasse',
  BIENTOT = 'bientot',
  OK = 'ok',
}

export interface DenonciationResult {
  dateDenonciation: Date;
  statut: StatutDenonciation;
}

export function computeDenonciation(
  dateFin: Date,
  delaiMois: number,
  today: Date = new Date(),
): DenonciationResult {
  const dateDenonciation = new Date(dateFin);
  dateDenonciation.setMonth(dateDenonciation.getMonth() - delaiMois);

  const sixMonthsFromNow = new Date(today);
  sixMonthsFromNow.setMonth(sixMonthsFromNow.getMonth() + 6);

  let statut: StatutDenonciation;
  if (dateDenonciation < today) {
    statut = StatutDenonciation.DEPASSE;
  } else if (dateDenonciation <= sixMonthsFromNow) {
    statut = StatutDenonciation.BIENTOT;
  } else {
    statut = StatutDenonciation.OK;
  }

  return { dateDenonciation, statut };
}
```

- [ ] **Step 1.4 : Lancer les tests — vérifier qu'ils passent**

```bash
cd backend && npx jest test/denonciation.spec.ts --no-coverage
```
Attendu : `PASS — 5 tests`

- [ ] **Step 1.5 : Commit**

```bash
git add src/maintenance-contracts/utils/denonciation.ts test/denonciation.spec.ts
git commit -m "feat(contracts): utilitaire calcul date et statut dénonciation"
```

---

## Task 2 : Utilitaire backend — parsing Excel

**Files:**
- Créer : `src/maintenance-contracts/utils/excel-parser.ts`
- Créer : `test/excel-parser.spec.ts`

- [ ] **Step 2.1 : Installer SheetJS**

```bash
cd backend && npm install xlsx
```
Attendu : `added 1 package`

- [ ] **Step 2.2 : Écrire les tests**

```typescript
// test/excel-parser.spec.ts
import { parseContractsExcel } from '../src/maintenance-contracts/utils/excel-parser';
import * as path from 'path';

const FIXTURE = path.join(__dirname, 'fixtures/contrats-test.xlsx');

describe('parseContractsExcel', () => {
  it('parse toutes les feuilles et retourne un tableau de contrats', () => {
    const contracts = parseContractsExcel(Buffer.from([])); // remplacé par fixture
    expect(Array.isArray(contracts)).toBe(true);
  });

  it('mappe correctement les colonnes Excel vers le modèle', () => {
    // Utiliser les données de fixture
    const mockRow = {
      ' Propriété': '00000076',
      'Nature des locaux': 'MIXTES',
      'Adresse 1': '285 AV DU PERE LEONID CHROL',
      'Adresse 2': 0,
      'CP': 82000,
      'Ville': 'MONTAUBAN',
      ' Libellé contrat': 'CONTRAT ENTRETIEN TOITURE',
      ' Libellé fournisseur': 'MIDI AQUITAINE ETANCHEITE MAE',
      ' Montant HT / an': 1200,
      ' Date début renouv.': new Date('2025-01-01'),
      ' Date fin renouv.': new Date('2025-12-31'),
    };
    const result = mapRowToContract(mockRow);
    expect(result.idPropriete).toBe('00000076');
    expect(result.typeBien).toBe('MIXTES');
    expect(result.adresse).toBe('285 AV DU PERE LEONID CHROL, 82000 MONTAUBAN');
    expect(result.prestataire).toBe('MIDI AQUITAINE ETANCHEITE MAE');
    expect(result.montantHtAnnuel).toBe(1200);
  });

  it('ignore les lignes d\'en-tête et les lignes vides', () => {
    const contracts = parseContractsExcel(readFileSync(FIXTURE));
    const hasEmpty = contracts.some(c => !c.idPropriete);
    expect(hasEmpty).toBe(false);
  });

  it('concatène adresse 2 si non nulle/zéro', () => {
    const mockRow = {
      'Adresse 1': 'ZAC DU TUBE',
      'Adresse 2': 'AVENUE CLEMENT ADER',
      'CP': 13800,
      'Ville': 'ISTRES',
    };
    const result = mapRowToContract(mockRow as any);
    expect(result.adresse).toBe('ZAC DU TUBE, AVENUE CLEMENT ADER, 13800 ISTRES');
  });
});
```

- [ ] **Step 2.3 : Créer le fichier fixture de test**

Copier le fichier Excel client dans `test/fixtures/contrats-test.xlsx` (version anonymisée si nécessaire).

```bash
cp "/path/to/contrats pour MONGA au 11 06 25 (2).XLSX" backend/test/fixtures/contrats-test.xlsx
```

- [ ] **Step 2.4 : Lancer les tests — vérifier qu'ils échouent**

```bash
cd backend && npx jest test/excel-parser.spec.ts --no-coverage
```
Attendu : `FAIL — Cannot find module`

- [ ] **Step 2.5 : Implémenter le parser**

```typescript
// src/maintenance-contracts/utils/excel-parser.ts
import * as XLSX from 'xlsx';

export interface RawContractRow {
  idPropriete: string;
  typeBien: string;
  adresse: string;
  prestation: string;
  prestataire: string;
  montantHtAnnuel: number;
  dateDebut: Date | null;
  dateFin: Date | null;
}

// Colonnes attendues (avec les espaces tels quels dans le fichier client)
const COL_MAP = {
  propriete: [' Propriété', 'Propriété', 'Propriete'],
  typeBien: ['Nature des locaux'],
  adresse1: ['Adresse 1'],
  adresse2: ['Adresse 2'],
  cp: ['CP'],
  ville: ['Ville'],
  prestation: [' Libellé contrat', 'Libellé contrat', 'Libelle contrat'],
  prestataire: [' Libellé fournisseur', 'Libellé fournisseur'],
  montant: [' Montant HT / an', 'Montant HT / an'],
  dateDebut: [' Date début renouv.', 'Date début renouv.', 'Date debut renouv.'],
  dateFin: [' Date fin renouv.', 'Date fin renouv.', 'Date fin renouv.'],
};

function findCol(row: Record<string, unknown>, candidates: string[]): unknown {
  for (const key of candidates) {
    if (key in row) return row[key];
  }
  return undefined;
}

export function mapRowToContract(row: Record<string, unknown>): RawContractRow {
  const adresse1 = String(findCol(row, COL_MAP.adresse1) ?? '').trim();
  const adresse2 = findCol(row, COL_MAP.adresse2);
  const cp = findCol(row, COL_MAP.cp);
  const ville = String(findCol(row, COL_MAP.ville) ?? '').trim();

  const adresseParts = [adresse1];
  if (adresse2 && adresse2 !== 0 && String(adresse2).trim() !== '') {
    adresseParts.push(String(adresse2).trim());
  }
  if (cp) adresseParts.push(String(cp).trim());
  adresseParts.push(ville);

  const rawDateDebut = findCol(row, COL_MAP.dateDebut);
  const rawDateFin = findCol(row, COL_MAP.dateFin);

  return {
    idPropriete: String(findCol(row, COL_MAP.propriete) ?? '').trim(),
    typeBien: String(findCol(row, COL_MAP.typeBien) ?? '').trim(),
    adresse: adresseParts.filter(Boolean).join(', '),
    prestation: String(findCol(row, COL_MAP.prestation) ?? '').trim(),
    prestataire: String(findCol(row, COL_MAP.prestataire) ?? '').trim(),
    montantHtAnnuel: Number(findCol(row, COL_MAP.montant) ?? 0),
    dateDebut: rawDateDebut instanceof Date ? rawDateDebut : null,
    dateFin: rawDateFin instanceof Date ? rawDateFin : null,
  };
}

export function parseContractsExcel(buffer: Buffer): RawContractRow[] {
  const workbook = XLSX.read(buffer, { type: 'buffer', cellDates: true });
  const results: RawContractRow[] = [];

  for (const sheetName of workbook.SheetNames) {
    const sheet = workbook.Sheets[sheetName];
    const rows: Record<string, unknown>[] = XLSX.utils.sheet_to_json(sheet, {
      defval: null,
      raw: false,
    });

    for (const row of rows) {
      const mapped = mapRowToContract(row);
      // Ignorer les lignes d'en-tête répétées et les lignes sans propriété
      if (!mapped.idPropriete || mapped.idPropriete === 'Propriété') continue;
      results.push(mapped);
    }
  }

  return results;
}
```

- [ ] **Step 2.6 : Exporter `mapRowToContract` pour les tests**

Ajouter `export` devant `mapRowToContract` dans le fichier (déjà fait ci-dessus).

- [ ] **Step 2.7 : Lancer les tests — vérifier qu'ils passent**

```bash
cd backend && npx jest test/excel-parser.spec.ts --no-coverage
```
Attendu : `PASS — 4 tests`

- [ ] **Step 2.8 : Commit**

```bash
git add src/maintenance-contracts/utils/excel-parser.ts test/excel-parser.spec.ts test/fixtures/
git commit -m "feat(contracts): parser Excel multi-feuilles (SheetJS)"
```

---

## Task 3 : Backend — Endpoint import Excel

**Files:**
- Modifier : `src/maintenance-contracts/entities/contract.entity.ts`
- Modifier : `src/maintenance-contracts/maintenance-contracts.service.ts`
- Modifier : `src/maintenance-contracts/maintenance-contracts.controller.ts`

- [ ] **Step 3.1 : Ajouter les champs au modèle/entité**

Dans `contract.entity.ts`, ajouter (adapter selon ORM utilisé — TypeORM ou Prisma) :

```typescript
// TypeORM
@Column({ type: 'date', nullable: true })
dateDenonciation: Date | null;

@Column({ type: 'varchar', length: 20, default: 'ok' })
statutDenonciation: 'ok' | 'bientot' | 'depasse';

@Column({ type: 'int', default: 3 })
delaiPreavisMois: number;
```

Si Prisma — ajouter dans `schema.prisma` :
```prisma
dateDenonciation   DateTime?
statutDenonciation String    @default("ok")
delaiPreavisMois   Int       @default(3)
```
Puis : `npx prisma migrate dev --name add_denonciation_fields`

- [ ] **Step 3.2 : Écrire le test du service**

```typescript
// test/maintenance-contracts.service.spec.ts (ajouter)
describe('importFromExcel', () => {
  it('calcule date_denonciation pour chaque contrat importé', async () => {
    const buffer = readFileSync('test/fixtures/contrats-test.xlsx');
    const contracts = await service.importFromExcel(buffer, 3);
    const withDateFin = contracts.filter(c => c.dateFin);
    expect(withDateFin.every(c => c.dateDenonciation !== null)).toBe(true);
  });

  it('applique le délai de préavis passé en paramètre', async () => {
    const buffer = readFileSync('test/fixtures/contrats-test.xlsx');
    const contracts = await service.importFromExcel(buffer, 1);
    const contract = contracts.find(c => 
      c.dateFin?.toISOString().startsWith('2025-12-31')
    );
    // 1 mois de préavis → date = 2025-11-30
    expect(contract?.dateDenonciation?.getMonth()).toBe(10); // novembre = index 10
  });
});
```

- [ ] **Step 3.3 : Lancer — vérifier que le test échoue**

```bash
cd backend && npx jest test/maintenance-contracts.service.spec.ts --no-coverage
```
Attendu : `FAIL`

- [ ] **Step 3.4 : Implémenter `importFromExcel` dans le service**

```typescript
// Dans maintenance-contracts.service.ts
import { parseContractsExcel } from './utils/excel-parser';
import { computeDenonciation } from './utils/denonciation';

async importFromExcel(
  buffer: Buffer,
  delaiPreavisMois: number = 3,
): Promise<Contract[]> {
  const rows = parseContractsExcel(buffer);
  const contracts: Contract[] = [];

  for (const row of rows) {
    const { dateDenonciation, statut } = row.dateFin
      ? computeDenonciation(row.dateFin, delaiPreavisMois)
      : { dateDenonciation: null, statut: 'ok' as const };

    const contract = this.contractRepository.create({
      idPropriete: row.idPropriete,
      typeBien: row.typeBien,
      adresse: row.adresse,
      prestation: row.prestation,
      prestataire: row.prestataire,
      montantHtAnnuel: row.montantHtAnnuel,
      dateDebut: row.dateDebut,
      dateFin: row.dateFin,
      delaiPreavisMois,
      dateDenonciation,
      statutDenonciation: statut,
      source: 'externe',
      statutValidation: 'valide',
    });

    contracts.push(await this.contractRepository.save(contract));
  }

  return contracts;
}
```

- [ ] **Step 3.5 : Ajouter l'endpoint dans le contrôleur**

```typescript
// Dans maintenance-contracts.controller.ts
import { FileInterceptor } from '@nestjs/platform-express';
import { UploadedFile, UseInterceptors, BadRequestException } from '@nestjs/common';

@Post('import/excel')
@UseInterceptors(FileInterceptor('file'))
async importExcel(
  @UploadedFile() file: Express.Multer.File,
  @Body('delaiPreavisMois') delaiPreavisMois?: string,
) {
  if (!file) throw new BadRequestException('Fichier manquant');
  const delai = delaiPreavisMois ? parseInt(delaiPreavisMois, 10) : 3;
  const contracts = await this.maintenanceContractsService.importFromExcel(
    file.buffer,
    delai,
  );
  return { imported: contracts.length, contracts };
}
```

- [ ] **Step 3.6 : Lancer les tests — vérifier qu'ils passent**

```bash
cd backend && npx jest test/maintenance-contracts.service.spec.ts --no-coverage
```
Attendu : `PASS`

- [ ] **Step 3.7 : Tester l'endpoint manuellement**

```bash
curl -X POST http://localhost:3000/maintenance-contracts/import/excel \
  -F "file=@test/fixtures/contrats-test.xlsx" \
  -F "delaiPreavisMois=3"
```
Attendu : `{ "imported": N, "contracts": [...] }`

- [ ] **Step 3.8 : Commit**

```bash
git add src/maintenance-contracts/ test/maintenance-contracts.service.spec.ts
git commit -m "feat(contracts): endpoint POST /import/excel avec calcul dénonciation"
```

---

## Task 4 : Backend — Config délai de préavis global

**Files:**
- Modifier : `src/settings/settings.service.ts`
- Modifier : `src/settings/settings.controller.ts`

- [ ] **Step 4.1 : Ajouter la méthode dans le service settings**

```typescript
// Dans settings.service.ts
async getPreavisDefault(): Promise<number> {
  const setting = await this.settingRepository.findOne({ 
    where: { key: 'preavis_default_mois' } 
  });
  return setting ? parseInt(setting.value, 10) : 3;
}

async setPreavisDefault(mois: number): Promise<void> {
  await this.settingRepository.upsert(
    { key: 'preavis_default_mois', value: String(mois) },
    ['key'],
  );
}
```

- [ ] **Step 4.2 : Ajouter les endpoints**

```typescript
// Dans settings.controller.ts
@Get('preavis-default')
async getPreavisDefault() {
  const mois = await this.settingsService.getPreavisDefault();
  return { delaiPreavisMois: mois };
}

@Patch('preavis-default')
async setPreavisDefault(@Body('delaiPreavisMois') mois: number) {
  await this.settingsService.setPreavisDefault(mois);
  return { delaiPreavisMois: mois };
}
```

- [ ] **Step 4.3 : Tester manuellement**

```bash
# Lire la valeur actuelle
curl http://localhost:3000/settings/preavis-default
# Attendu : { "delaiPreavisMois": 3 }

# Modifier
curl -X PATCH http://localhost:3000/settings/preavis-default \
  -H "Content-Type: application/json" \
  -d '{"delaiPreavisMois": 6}'
# Attendu : { "delaiPreavisMois": 6 }
```

- [ ] **Step 4.4 : Commit**

```bash
git add src/settings/
git commit -m "feat(settings): endpoint GET/PATCH délai de préavis par défaut"
```

---

## Task 5 : Frontend — Modèle et service Angular

**Files:**
- Modifier : `src/app/shared/models/contract.model.ts`
- Modifier : `src/app/modules/maintenance-contracts/maintenance-contracts.service.ts`

- [ ] **Step 5.1 : Étendre le modèle**

```typescript
// Dans contract.model.ts — ajouter aux champs existants :
export interface Contract {
  // ... champs existants ...
  dateDenonciation: string | null;       // ISO date string
  statutDenonciation: 'ok' | 'bientot' | 'depasse';
  delaiPreavisMois: number;
}

export type StatutDenonciation = Contract['statutDenonciation'];
```

- [ ] **Step 5.2 : Ajouter les méthodes dans le service Angular**

```typescript
// Dans maintenance-contracts.service.ts — ajouter :

importFromExcel(file: File, delaiPreavisMois: number): Observable<{ imported: number }> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('delaiPreavisMois', String(delaiPreavisMois));
  return this.http.post<{ imported: number }>(
    `${this.apiUrl}/maintenance-contracts/import/excel`,
    formData,
  );
}

getPreavisDefault(): Observable<{ delaiPreavisMois: number }> {
  return this.http.get<{ delaiPreavisMois: number }>(
    `${this.apiUrl}/settings/preavis-default`,
  );
}

updatePreavisDefault(mois: number): Observable<{ delaiPreavisMois: number }> {
  return this.http.patch<{ delaiPreavisMois: number }>(
    `${this.apiUrl}/settings/preavis-default`,
    { delaiPreavisMois: mois },
  );
}
```

- [ ] **Step 5.3 : Commit**

```bash
git add src/app/shared/models/contract.model.ts \
        src/app/modules/maintenance-contracts/maintenance-contracts.service.ts
git commit -m "feat(frontend): modèle Contract étendu + méthodes service import Excel"
```

---

## Task 6 : Frontend — Composant `preavis-config`

**Files:**
- Créer : `src/app/modules/maintenance-contracts/components/preavis-config/preavis-config.component.ts`
- Créer : `src/app/modules/maintenance-contracts/components/preavis-config/preavis-config.component.html`

- [ ] **Step 6.1 : Créer le composant**

```typescript
// preavis-config.component.ts
import { Component, OnInit } from '@angular/core';
import { MaintenanceContractsService } from '../../maintenance-contracts.service';

@Component({
  selector: 'app-preavis-config',
  templateUrl: './preavis-config.component.html',
})
export class PreavisConfigComponent implements OnInit {
  readonly options = [1, 2, 3, 6];
  selected = 3;

  constructor(private service: MaintenanceContractsService) {}

  ngOnInit(): void {
    this.service.getPreavisDefault().subscribe(({ delaiPreavisMois }) => {
      this.selected = delaiPreavisMois;
    });
  }

  onChange(mois: number): void {
    this.service.updatePreavisDefault(mois).subscribe();
  }
}
```

- [ ] **Step 6.2 : Créer le template (Taiga UI)**

```html
<!-- preavis-config.component.html -->
<div class="preavis-bar">
  <span class="label">Délai de préavis par défaut</span>

  <tui-select
    [ngModel]="selected"
    (ngModelChange)="onChange($event)"
    [tuiTextfieldSize]="'s'"
    style="width: 120px;"
  >
    <ng-container *ngFor="let opt of options">
      <tui-data-list-option [value]="opt">{{ opt }} mois</tui-data-list-option>
    </ng-container>
  </tui-select>

  <span class="note">Modifiable par contrat · Appliqué à l'import</span>
</div>
```

- [ ] **Step 6.3 : Déclarer le composant dans le module**

Ajouter `PreavisConfigComponent` dans les `declarations` (ou `imports` si standalone) du module `MaintenanceContractsModule`.

- [ ] **Step 6.4 : Insérer dans la page principale**

Dans `app-maintenance-contracts-page` template, ajouter sous la zone d'import :

```html
<app-preavis-config></app-preavis-config>
```

- [ ] **Step 6.5 : Vérifier visuellement dans le navigateur**

Ouvrir `http://192.168.1.79:8081/maintenance-contracts` — le sélecteur de préavis doit apparaître sous la zone d'import. Tester le changement de valeur.

- [ ] **Step 6.6 : Commit**

```bash
git add src/app/modules/maintenance-contracts/components/preavis-config/
git commit -m "feat(frontend): composant sélecteur délai de préavis"
```

---

## Task 7 : Frontend — Zone d'import Excel dans `app-upload-zone`

**Files:**
- Modifier : `src/app/modules/maintenance-contracts/components/upload-zone/upload-zone.component.ts`
- Modifier : `src/app/modules/maintenance-contracts/components/upload-zone/upload-zone.component.html`

- [ ] **Step 7.1 : Étendre le composant pour accepter Excel**

```typescript
// Dans upload-zone.component.ts — ajouter :

importExcel(event: Event): void {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  this.loading = true;
  this.service.importFromExcel(file, this.preavisDefault).subscribe({
    next: ({ imported }) => {
      this.loading = false;
      this.alerts.open(`${imported} contrats importés avec succès`, {
        status: 'success',
      }).subscribe();
      this.contractsUpdated.emit();
    },
    error: () => {
      this.loading = false;
      this.alerts.open('Erreur lors de l\'import Excel', {
        status: 'error',
      }).subscribe();
    },
  });
}
```

Ajouter `@Input() preavisDefault = 3;` et `@Output() contractsUpdated = new EventEmitter<void>();`.

- [ ] **Step 7.2 : Ajouter la carte Excel dans le template**

Dans `upload-zone.component.html`, à côté de la carte PDF existante :

```html
<!-- Carte Excel (NOUVELLE) -->
<label class="import-card import-card--excel" for="excel-input">
  <span class="import-card__icon">📊</span>
  <div class="import-card__body">
    <h4>Importer un tableau récap Excel</h4>
    <p>Multi-feuilles .xlsx · Toutes propriétés en un import</p>
    <tui-badge status="success" value="Nouveau"></tui-badge>
  </div>
  <input
    id="excel-input"
    type="file"
    accept=".xlsx,.xls"
    style="display:none"
    (change)="importExcel($event)"
  />
</label>
```

- [ ] **Step 7.3 : Tester l'import avec le fichier client**

1. Ouvrir `http://192.168.1.79:8081/maintenance-contracts`
2. Cliquer "Importer un tableau récap Excel"
3. Sélectionner `contrats pour MONGA au 11 06 25 (2).XLSX`
4. Vérifier le toast de confirmation et que les contrats apparaissent dans le tableau

- [ ] **Step 7.4 : Commit**

```bash
git add src/app/modules/maintenance-contracts/components/upload-zone/
git commit -m "feat(frontend): import Excel dans la zone d'upload"
```

---

## Task 8 : Frontend — Colonnes dénonciation dans `app-contract-list`

**Files:**
- Modifier : `src/app/modules/maintenance-contracts/components/contract-list/contract-list.component.ts`
- Modifier : `src/app/modules/maintenance-contracts/components/contract-list/contract-list.component.html`

- [ ] **Step 8.1 : Ajouter le filtre "Urgents" dans le composant**

```typescript
// Dans contract-list.component.ts
filterUrgents = false;

get filteredContracts(): Contract[] {
  if (!this.filterUrgents) return this.contracts;
  return this.contracts.filter(c => c.statutDenonciation === 'depasse');
}

get urgentsCount(): number {
  return this.contracts.filter(c => c.statutDenonciation === 'depasse').length;
}

toggleUrgents(): void {
  this.filterUrgents = !this.filterUrgents;
}
```

- [ ] **Step 8.2 : Ajouter les colonnes dans le template**

Dans `contract-list.component.html`, après la colonne `DATE DE FIN` existante, ajouter :

```html
<!-- Colonne DATE DÉNONCIATION -->
<ng-template tuiCell="dateDenonciation" let-contract>
  <span
    [class.text-danger]="contract.statutDenonciation === 'depasse'"
    [class.text-warning]="contract.statutDenonciation === 'bientot'"
    [class.text-success]="contract.statutDenonciation === 'ok'"
    class="date-num"
  >
    {{ contract.dateDenonciation | date:'dd/MM/yyyy' }}
  </span>
</ng-template>

<!-- Colonne STATUT DÉNONCIATION -->
<ng-template tuiCell="statutDenonciation" let-contract>
  <tui-badge
    [status]="contract.statutDenonciation === 'depasse' ? 'error'
            : contract.statutDenonciation === 'bientot' ? 'warning'
            : 'success'"
    [value]="contract.statutDenonciation === 'depasse' ? 'Reconduit'
           : contract.statutDenonciation === 'bientot' ? 'Dans ' + contract.moisAvantDenonciation + ' mois'
           : 'OK'"
  ></tui-badge>
</ng-template>
```

Ajouter `'dateDenonciation'` et `'statutDenonciation'` dans le tableau `columns` du composant.

- [ ] **Step 8.3 : Ajouter le chip filtre "Urgents"**

Dans la toolbar des filtres existante, ajouter :

```html
<button
  tuiButton
  [appearance]="filterUrgents ? 'destructive' : 'outline'"
  size="s"
  (click)="toggleUrgents()"
>
  🔴 Urgents ({{ urgentsCount }})
</button>
```

- [ ] **Step 8.4 : Ajouter la propriété `moisAvantDenonciation` dans le modèle**

Dans `contract.model.ts` :
```typescript
// Champ calculé côté frontend (pour l'affichage)
get moisAvantDenonciation(): number {
  if (!this.dateDenonciation) return 0;
  const diff = new Date(this.dateDenonciation).getTime() - Date.now();
  return Math.ceil(diff / (1000 * 60 * 60 * 24 * 30));
}
```
Ou calculer dans le composant si Contract est une interface (pas une classe) :

```typescript
// Dans contract-list.component.ts
getMonthsUntilDenonciation(contract: Contract): number {
  if (!contract.dateDenonciation) return 0;
  const diff = new Date(contract.dateDenonciation).getTime() - Date.now();
  return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24 * 30)));
}
```

- [ ] **Step 8.5 : Vérifier visuellement**

Ouvrir `http://192.168.1.79:8081/maintenance-contracts` :
- Les colonnes "DATE DÉNONCIATION" et "STATUT DÉNONCIATION" sont visibles
- Les badges sont correctement colorés (rouge/orange/vert)
- Le chip "🔴 Urgents (N)" filtre le tableau

- [ ] **Step 8.6 : Commit**

```bash
git add src/app/modules/maintenance-contracts/components/contract-list/
git commit -m "feat(frontend): colonnes dénonciation + filtre urgents dans le tableau contrats"
```

---

## Récapitulatif des commits Sprint 1

```
feat(contracts): utilitaire calcul date et statut dénonciation
feat(contracts): parser Excel multi-feuilles (SheetJS)
feat(contracts): endpoint POST /import/excel avec calcul dénonciation
feat(settings): endpoint GET/PATCH délai de préavis par défaut
feat(frontend): modèle Contract étendu + méthodes service import Excel
feat(frontend): composant sélecteur délai de préavis
feat(frontend): import Excel dans la zone d'upload
feat(frontend): colonnes dénonciation + filtre urgents dans le tableau contrats
```

---

## Hors scope Sprint 1

- Sprint 2 : timeline, alertes email
- Sprint 3 : enrichissement adresse Pappers, génération lettres
- Sprint 4 : envoi LRE/AR24, postal Maileva
