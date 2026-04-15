# Presentation Skill — Design Spec
**Date :** 2026-04-15  
**Statut :** Approuvé  
**Auteur :** Brainstorming session

---

## 1. Objectif

Créer un skill Claude (`/présentation`) capable de produire des présentations de niveau cabinet marketing premium — design professionnel, photos contextuelles, infographies, graphiques, logos de marques auto-sourcés — exportées en PDF et PPTX depuis une seule commande.

Le design doit être systématiquement **ultra clean, désirable et adaptatif** selon le type de deck et la cible.

---

## 2. Architecture

```
Utilisateur (prompt /présentation)
    │
    ▼
Claude Skill
    │  • Analyse le type de deck et la cible
    │  • Auto-détecte ou applique le thème demandé
    │  • Structure le contenu en JSON de slides
    │  • Identifie les marques à logoter
    │  • Sélectionne les layouts adaptés à chaque slide
    │  • Détermine les données à visualiser
    │
    ▼
HTML Renderer (Node.js)
    │  • Injecte le JSON dans les templates HTML/CSS
    │  • Brandfetch API → logos des marques citées
    │  • Unsplash API → photos contextuelles haute qualité
    │  • Chart.js → graphiques (bar, line, donut, area)
    │  • Applique le thème sélectionné (accent color)
    │
    ▼
Export Pipeline
    ├── Puppeteer (headless Chrome) → PDF A4 landscape, print-perfect
    └── html-to-pptx → PPTX éditable dans PowerPoint / Keynote
                       (best-effort : les gradients et blobs CSS complexes
                        sont simplifiés — le fond, la typo et les données
                        sont fidèlement reproduits)
```

### Pourquoi cette approche

HTML/CSS offre une liberté de design totale, impossible à atteindre avec python-pptx. Puppeteer produit un PDF indiscernable d'un fichier natif. Le PPTX est un bonus d'éditabilité pour les cas où l'utilisateur doit modifier après export.

---

## 3. Système de thèmes adaptatifs

Le fond bleu Monga (`#0d1f4e`) est **invariant** — identité commune à tous les thèmes.

| Thème | Accent principal | Accent secondaire | Déclencheurs typiques |
|---|---|---|---|
| **Commercial** | `#ff7832` Orange | `#d4af37` Or | pitch client, proposal, deal |
| **Tech / Fintech** | `#818cf8` Indigo | `#60a5fa` Bleu ciel | SaaS, produit, startup, VC |
| **Prestige** | `#d4af37` Or | `#ff7832` Orange | conseil haut de gamme, luxe, rapport annuel |
| **Neutre** | `#e2e8f0` Blanc | `#94a3b8` Gris | usage interne, non défini |

### Auto-détection du thème

Claude analyse les mots-clés du prompt pour sélectionner le thème automatiquement. L'utilisateur peut toujours forcer un thème :

```
/présentation                          → auto-detect
/présentation --theme tech             → force Tech/Fintech
/présentation --theme prestige         → force Prestige
/présentation --theme commercial       → force Commercial
```

### Typographie

- **Titres et corps** : Inter (Google Fonts, system fallback: sans-serif)
- **Citations et édito** : Georgia (serif)
- **Données et labels** : Inter Mono

---

## 4. Layouts disponibles (9 types)

| Layout | Description | Déclencheur auto |
|---|---|---|
| **Cover** | Titre, sous-titre, logo client, date | Première slide toujours |
| **KPIs / Métriques** | 3–4 chiffres clés en cards colorées | Données chiffrées détectées |
| **Chart** | Bar, line, donut, area — depuis données | Séries de données détectées |
| **Visuel + Texte** | Photo Unsplash + points clés en split | Slide narrative / contexte |
| **Comparaison + Logos** | Benchmark concurrentiel, logos auto-sourcés | Marques citées en comparaison |
| **Timeline** | Jalons horizontaux avec progression colorée | Dates, phases, roadmap |
| **Process** | Étapes séquentielles avec avancement visuel | Méthodologie, approche |
| **Organigramme** | Hiérarchie générée depuis texte structuré | Équipes, gouvernance |
| **Citation / Closing** | Impact émotionnel, conclusion mémorable | Fin de deck, slide impactante |

---

## 5. Intégrations API

### Logos — pipeline multi-stratégies (`logo-sourcer.js`)

Le sourcing de logos suit une cascade de 4 stratégies dans l'ordre suivant. Jamais une seule source.

| Priorité | Stratégie | Condition | Résultat |
|---|---|---|---|
| 1 | **Cache local** | Toujours | Instantané si déjà sourcé |
| 2 | **Brandfetch API** | Si `BRANDFETCH_KEY` configurée | SVG/PNG HD officiel |
| 3 | **Playwright multi-domaines** | Toujours | SVG extrait du site officiel |
| 4 | **Fallback texte premium** | Dernier recours | Nom typographié, jamais un carré vide |

#### Playwright multi-domaines
- Pour chaque entreprise, génère automatiquement les variantes de domaine :
  - TLDs : `.com`, `.fr`, `.eu`, `.co.uk`, `.net`, `.org`, `.io`
  - Préfixes : `www.`, `` (vide), `groupe.`, `group.`
  - Slugs : `groupeduval`, `groupe-duval`, `duval` (decomposé)
- Valide que le SVG trouvé est un logo réel (viewBox, ratio, nb de paths)
- Résout les CSS variables dynamiquement (ex : `--logo-fill-a → #fff`)
- Auto-détecte la couleur dominante → applique le traitement inline :
  - Logo sombre (luma < 140) → fills convertis en `#ffffff`
  - Couleurs vives (saturation > 0.4) → conservées (rouge de SGL, etc.)
  - Logo déjà blanc → aucun filtre

#### Brandfetch (logos HD, production)
- Endpoint : `https://api.brandfetch.io/v2/brands/{domain}`
- Format préféré : SVG > PNG transparent
- Utilisé en priorité si `BRANDFETCH_KEY` est définie dans `.env`

### Unsplash (photos)
- Endpoint : `https://api.unsplash.com/search/photos`
- Déclencheur : slides narratives (Visuel + Texte)
- Requête : mots-clés extraits du titre + contexte de la slide
- Résolution : 1600×900 minimum pour qualité impression

### Chart.js (graphiques)
- Rendu : canvas HTML, capturé par Puppeteer au moment de l'export
- Types supportés : bar (vertical/horizontal), line, donut, area
- Palette : couleurs du thème actif
- Données : injectées depuis le JSON de slides

---

## 6. Interface du skill

### Syntaxe

```
/présentation [--theme commercial|tech|prestige|neutre] [--slides N]
<contenu libre en langage naturel>
```

### Exemples d'usage

```
/présentation
Pitch pour Renault — réduction des coûts supply chain de 23%,
ROI projeté ×3.2, comparaison avec Toyota et BMW, plan en 4 étapes

/présentation --theme tech
Investor deck Série B — ARR €4M, croissance +180% YoY, roadmap 18 mois,
équipe de 12 personnes, marché adressable €2.4Md

/présentation --theme prestige --slides 12
Rapport annuel McKinsey — bilan 2024, résultats clients, perspectives 2025
```

### Processus interne du skill

1. **Parse** le prompt → type de deck, thème, nombre de slides souhaité
2. **Structure** le contenu en JSON : `{ slides: [{ layout, title, content, data, brands, imageQuery }] }`
3. **Enrichit** : appels Brandfetch + Unsplash en parallèle
4. **Rend** le HTML avec le thème appliqué
5. **Exporte** PDF via Puppeteer + PPTX via html-to-pptx
6. **Livre** les deux fichiers avec un aperçu de chaque slide

---

## 7. Structure des fichiers

```
presentation-skill/
├── skill.md                    # Définition du skill Claude
├── renderer/
│   ├── index.js                # Point d'entrée Node.js
│   ├── templates/
│   │   ├── base.html           # Template de base (fond, typographie)
│   │   ├── layouts/
│   │   │   ├── cover.html
│   │   │   ├── metrics.html
│   │   │   ├── chart.html
│   │   │   ├── visual-text.html
│   │   │   ├── comparison.html
│   │   │   ├── timeline.html
│   │   │   ├── process.html
│   │   │   ├── orgchart.html
│   │   │   └── quote.html
│   │   └── themes/
│   │       ├── commercial.css
│   │       ├── tech.css
│   │       ├── prestige.css
│   │       └── neutral.css
│   ├── services/
│   │   ├── brandfetch.js       # Sourcing logos
│   │   ├── unsplash.js         # Sourcing photos
│   │   └── chartjs.js          # Génération graphiques
│   └── export/
│       ├── pdf.js              # Puppeteer → PDF
│       └── pptx.js             # html-to-pptx → PPTX
├── package.json
└── .env.example                # BRANDFETCH_KEY, UNSPLASH_KEY
```

---

## 8. Gestion des cas limites

| Situation | Comportement |
|---|---|
| Logo Brandfetch introuvable | Placeholder stylisé avec initiales de la marque |
| Photo Unsplash non pertinente | Fond dégradé abstrait contextuel |
| Données insuffisantes pour un chart | Layout texte simple à la place |
| Trop peu de contenu pour N slides | Adaptation au nombre de slides nécessaires |
| Nom de marque ambigu | Claude demande confirmation avant sourcing |

---

## 9. Hors scope (v1)

- Animations et transitions (PDF ne les supporte pas)
- Édition interactive des slides dans le navigateur
- Synchronisation Google Slides
- Thèmes personnalisés par client (v2)
- Templates sectoriels spécifiques (v2)
