# Outil de pilotage des contrats de maintenance — Design Spec

**Date :** 2026-04-15  
**Statut :** Approuvé  
**Contexte :** Extension de l'app Monga existante (Angular) sur la route `/maintenance-contracts`

---

## 1. Contexte & objectif

Monga met à disposition de ses clients professionnels de l'immobilier un outil pour :
- Centraliser et visualiser leurs contrats de maintenance tiers (facility management)
- Être alertés avant les dates de reconduction tacite en prenant en compte le délai de préavis
- Générer et envoyer en batch les lettres de dénonciation aux prestataires
- Faciliter le passage vers les prestataires Monga au bon moment

**Utilisateurs :**
- **Monga SuperAdmin** — vue globale tous clients, tous contrats
- **Client pro** — vue de son propre portefeuille uniquement (multi-tenant)

**App existante :** Angular, hébergée sur `monga.io`, route `/maintenance-contracts` déjà en production avec import PDF, tableau de contrats et filtres.

---

## 2. Architecture

On **étend** la page Angular existante. Aucune migration de stack.

| Composant | Technologie |
|---|---|
| Frontend | Angular (existant) — nouveaux composants |
| Backend | API Monga existante — nouveaux endpoints |
| Enrichissement adresse | API Pappers (SIRENE/RCS) — recherche par nom société |
| Génération PDF | PDFKit ou Puppeteer côté backend |
| Envoi LRE | API AR24 |
| Envoi postal | API Maileva |
| Auth multi-tenant | Système existant Monga |

**Design system :** Fond blanc `#f7f7f8`, Inter font, style Revolut — épuré, minimaliste, fonctionnel.

---

## 3. Modèle de données

### Contrat (enrichi)

| Champ | Type | Source |
|---|---|---|
| `id_propriete` | string | Excel col. "Propriété" |
| `type_bien` | enum | Excel col. "Nature des locaux" |
| `adresse` | string | Concaténation Adresse1 + CP + Ville |
| `lot` | string | Excel col. "Adresse 2" |
| `prestation` | string | Excel col. "Libellé contrat" |
| `type_prestation` | enum | Auto-détecté (CVC, Toiture, Nettoyage…) |
| `prestataire` | string | Excel col. "Libellé fournisseur" |
| `adresse_prestataire` | string | Enrichi via Pappers · modifiable |
| `montant_ht_annuel` | number | Excel col. "Montant HT / an" |
| `date_debut` | date | Excel col. "Date début renouv." |
| `date_fin` | date | Excel col. "Date fin renouv." |
| `delai_preavis_mois` | number | Défaut : 3 · configurable par contrat |
| `date_denonciation` | date | **Calculé** : `date_fin − delai_preavis_mois` |
| `statut_denonciation` | enum | `ok` / `bientot` / `depasse` |
| `source` | enum | `monga` / `externe` |
| `statut_validation` | enum | `valide` / `en_attente` |
| `lettre_generee` | boolean | Sprint 3 |
| `lettre_envoyee_le` | date | Sprint 4 |
| `mode_envoi` | enum | `pdf` / `lre` / `postal` |

### Règles de calcul

```
date_denonciation = date_fin - delai_preavis_mois

statut_denonciation:
  - "depasse"  : date_denonciation < aujourd'hui
  - "bientot"  : date_denonciation dans les 6 mois
  - "ok"       : date_denonciation dans plus de 6 mois
```

---

## 4. Sprint 1 — Import Excel + Date de dénonciation

### 4.1 Import Excel

**Déclencheur :** Nouvelle carte "Importer un tableau récap Excel" dans la zone d'import existante.

**Format accepté :** `.xlsx`, `.xls` multi-feuilles (une feuille par type de bien).

**Colonnes attendues (mapping automatique) :**

| Colonne Excel | Champ Monga | Traitement |
|---|---|---|
| Propriété | `id_propriete` | Direct |
| Nature des locaux | `type_bien` | Normalisation enum |
| Adresse 1 + Adresse 2 + CP + Ville | `adresse` | Concaténation |
| Libellé contrat | `prestation` | + détection `type_prestation` |
| Libellé fournisseur | `prestataire` | Direct |
| Montant HT / an | `montant_ht_annuel` | Nettoyage numérique |
| Date début renouv. | `date_debut` | Parse datetime |
| Date fin renouv. | `date_fin` | Parse datetime |

**Gestion des erreurs :**
- Lignes avec colonnes manquantes → signalées dans un résumé d'import, ignorées ou importées partiellement selon le champ
- Colonnes non reconnues → mapping manuel proposé à l'utilisateur

### 4.2 Date de dénonciation

**Configuration globale :** Sélecteur "Délai de préavis par défaut" — 1 / 2 / 3 / 6 mois — défaut 3 mois.  
**Override par contrat :** modifiable dans la fiche contrat.

**Deux nouvelles colonnes dans le tableau existant :**
- `DATE DÉNONCIATION` — date calculée, format `JJ/MM/AAAA`, tabular-nums
- `STATUT DÉNONCIATION` — pill colorée :
  - 🔴 "Reconduit" — délai dépassé (`#fef2f2 / #dc2626`)
  - 🟡 "Dans N mois" — moins de 6 mois (`#fffbeb / #d97706`)
  - 🟢 "OK · N mois" — plus de 6 mois (`#f0fdf7 / #16a34a`)

**Filtre rapide :** Chip "🔴 Urgents (N)" dans la toolbar.

---

## 5. Sprint 2 — Dashboard timeline + Alertes

### 5.1 Vue Timeline

Nouvelle tab "Timeline" sur la page `/maintenance-contracts`.

**Composants :**
- Grille 12 colonnes (12 mois de l'année courante)
- Ligne verticale "Aujourd'hui" en violet (`#6366f1`)
- Une ligne par contrat :
  - Barre colorée sur toute la durée du contrat (rouge / orange / vert)
  - Marqueur circulaire à la date de dénonciation
- Légende en bas de page

### 5.2 Alertes

Nouvelle tab "Alertes" avec 3 niveaux :
1. **Urgence** (rouge) — délai dépassé, contrats reconduits tacitement — CTA "Générer les lettres quand même"
2. **À planifier** (orange) — dénonciation dans < 6 mois — CTA "Préparer les lettres"
3. **Rappel programmé** (info) — prochaine alerte email planifiée

**Configuration alertes** (panneau latéral) :
- Toggles par seuil : 6 mois / 3 mois / 1 mois / délai dépassé
- Destinataire email (gestionnaire Monga)
- Copie client : toggle
- Résumé hebdomadaire : toggle

---

## 6. Sprint 3 — Enrichissement prestataire + Génération lettres

### 6.1 Enrichissement adresse prestataire

**Déclencheur :** Sélection de contrats → bouton "Préparer les lettres".

**Flow :**
1. Pour chaque prestataire unique, recherche via **API Pappers** par nom de société
2. Résultat affiché dans un tableau de vérification :
   - ✅ Trouvé — adresse pré-remplie, modifiable
   - ⚠️ Partiel — suggestion à confirmer
   - ❌ Manquant — saisie manuelle obligatoire avant de continuer
3. Validation → passage à l'étape génération

**Persistance :** Une fois confirmée, l'adresse est sauvegardée sur le prestataire pour les prochains imports.

### 6.2 Génération des lettres

**Stepper 4 étapes :** Sélection → Enrichissement → Génération → Envoi

**Génération automatique :** Une lettre par contrat sélectionné, injectant :
- Identité du propriétaire (depuis le lot/propriété)
- Adresse du prestataire (enrichie)
- Date de fin de contrat
- Délai de préavis
- Référence contrat / propriété

**Contenu lettre type :**
> Résiliation du contrat [libellé], échéance [date_fin], conformément au préavis de [N] mois.

**Modifications :** Éditeur inline sur l'aperçu — modifications sauvegardées par lettre.

---

## 7. Sprint 4 — Envoi multi-CTA

### 7.1 Actions par lettre

Chaque lettre dispose de 3 CTAs indépendants :
- **📄 Télécharger PDF** — génération et download direct
- **⚡ LRE via AR24** — envoi dématérialisé, valeur légale, nécessite email destinataire
- **📮 Recommandé postal via Maileva** — impression + envoi physique, nécessite adresse postale

### 7.2 Actions batch

Barre de batch au-dessus de la liste (sélection par cases à cocher) :
- "Tout télécharger en PDF" — ZIP de toutes les lettres sélectionnées
- "Envoyer en LRE (AR24)" — envoi groupé, confirmation avant exécution
- "Envoyer en recommandé postal (Maileva)" — envoi groupé, confirmation avant exécution

### 7.3 Suivi envois

Colonne `STATUT ENVOI` dans le tableau principal :
- "Non envoyé" / "PDF téléchargé" / "LRE envoyée le JJ/MM" / "Postal envoyé le JJ/MM"

---

## 8. Découpage sprint

| Sprint | Périmètre | Livrable |
|---|---|---|
| **Sprint 1** | Import Excel + calcul date dénonciation + colonnes tableau | Tableau enrichi avec statuts |
| **Sprint 2** | Vue timeline + alertes + config notifications | Dashboard pilotage |
| **Sprint 3** | Enrichissement Pappers + génération lettres PDF | Lettres prêtes à envoyer |
| **Sprint 4** | Envoi multi-CTA (PDF / LRE AR24 / Postal Maileva) | Envoi batch automatisé |

---

## 9. Hors scope (pour l'instant)

- Application mobile
- Intégration directe avec les logiciels de gestion tiers (Yardi, Altaix…)
- Signature électronique des lettres
- Gestion des renouvellements (acceptation d'un nouveau contrat)
- Statistiques avancées / reporting financier

---

## 10. Maquettes de référence

Fichiers dans `.superpowers/brainstorm/59317-1776273998/content/` :
- `sprint1-design-v3.html` — Tableau enrichi Sprint 1
- `sprint2-design.html` — Timeline + alertes Sprint 2
- `sprint3-design.html` — Enrichissement + génération lettres Sprint 3
