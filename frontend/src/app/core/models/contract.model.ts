// frontend/src/app/core/models/contract.model.ts
export type StatutDenonciation = 'ok' | 'bientot' | 'depasse';
export type SourceContrat = 'monga' | 'externe';
export type StatutValidation = 'valide' | 'enattente';

export interface Contract {
  id: number;
  idPropriete: string;
  typeBien: string;
  adresse: string;
  prestation: string;
  prestataire: string;
  montantHtAnnuel: number;
  dateDebut: string | null;
  dateFin: string | null;
  delaiPreavisMois: number;
  dateDenonciation: string | null;
  statutDenonciation: StatutDenonciation;
  source: SourceContrat;
  statutValidation: StatutValidation;
}

export interface ImportResult {
  imported: number;
}
