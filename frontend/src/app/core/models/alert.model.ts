export interface AlertContract {
  id: number;
  prestation: string;
  prestataire: string;
  adresse: string;
  dateDenonciation: string | null;
  dateFin: string | null;
}

export interface AlertSummary {
  depasse: AlertContract[];
  bientot: AlertContract[];
}

export interface AlertSettings {
  alert6Mois: boolean;
  alert3Mois: boolean;
  alert1Mois: boolean;
  alertDepasse: boolean;
  email: string;
  copieCLient: boolean;
  resumeHebdo: boolean;
}
