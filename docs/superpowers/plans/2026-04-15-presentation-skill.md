# Presentation Skill Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a `/présentation` Claude skill that generates premium marketing presentations (PDF + PPTX) from a single natural-language prompt.

**Architecture:** Claude parses the prompt into a JSON slide manifest, a Node.js renderer injects that JSON into HTML/CSS templates with adaptive theming, Brandfetch + Playwright source logos, Unsplash sources photos, Chart.js renders charts, and Puppeteer exports a print-perfect PDF plus pptxgenjs exports an editable PPTX.

**Tech Stack:** Node.js 20, Puppeteer, Playwright, pptxgenjs, Chart.js (CDN), Inter (Google Fonts), Jest for tests.

---

## File Map

| File | Responsibility |
|---|---|
| `presentation-skill/package.json` | Dependencies and test runner |
| `presentation-skill/.env.example` | BRANDFETCH_KEY, UNSPLASH_KEY |
| `presentation-skill/renderer/theme.js` | Theme detection, CSS variable generation |
| `presentation-skill/renderer/templates/base.html` | Shell: fonts, global CSS, slot for slides |
| `presentation-skill/renderer/templates/layouts/cover.js` | Cover slide HTML builder |
| `presentation-skill/renderer/templates/layouts/metrics.js` | KPI metrics slide HTML builder |
| `presentation-skill/renderer/templates/layouts/chart.js` | Chart slide HTML builder |
| `presentation-skill/renderer/templates/layouts/visual-text.js` | Photo + bullets slide HTML builder |
| `presentation-skill/renderer/templates/layouts/comparison.js` | Competitive comparison + logos slide HTML builder |
| `presentation-skill/renderer/templates/layouts/timeline.js` | Timeline/roadmap slide HTML builder |
| `presentation-skill/renderer/templates/layouts/process.js` | Process steps slide HTML builder |
| `presentation-skill/renderer/templates/layouts/orgchart.js` | Org chart slide HTML builder |
| `presentation-skill/renderer/templates/layouts/quote.js` | Citation/closing slide HTML builder |
| `presentation-skill/renderer/index.js` | Orchestrator: JSON manifest → full HTML document |
| `presentation-skill/renderer/services/logo-sourcer.js` | Already written — multi-strategy logo pipeline |
| `presentation-skill/renderer/services/unsplash.js` | Unsplash photo sourcing |
| `presentation-skill/renderer/services/chartjs.js` | Chart data serializer for Chart.js CDN |
| `presentation-skill/renderer/export/pdf.js` | Puppeteer → PDF A4 landscape |
| `presentation-skill/renderer/export/pptx.js` | pptxgenjs → PPTX editable |
| `presentation-skill/skill.md` | Claude skill definition for `/présentation` |
| `presentation-skill/tests/theme.test.js` | Theme unit tests |
| `presentation-skill/tests/layouts.test.js` | Layout HTML builder tests |
| `presentation-skill/tests/renderer.test.js` | Renderer integration tests |
| `presentation-skill/tests/unsplash.test.js` | Unsplash service tests |
| `presentation-skill/tests/logo-sourcer.test.js` | Logo sourcer tests |
| `presentation-skill/tests/pdf.test.js` | PDF export tests |
| `presentation-skill/tests/pptx.test.js` | PPTX export tests |
| `presentation-skill/tests/e2e.test.js` | End-to-end pipeline test |

---

## Task 1: Project Bootstrap

**Files:**
- Create: `presentation-skill/package.json`
- Create: `presentation-skill/.env.example`
- Create: `presentation-skill/jest.config.js`

- [ ] **Step 1: Create package.json**

```json
{
  "name": "presentation-skill",
  "version": "1.0.0",
  "type": "commonjs",
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "render": "node renderer/index.js"
  },
  "dependencies": {
    "puppeteer": "^22.0.0",
    "playwright": "^1.44.0",
    "pptxgenjs": "^3.12.0",
    "dotenv": "^16.4.0"
  },
  "devDependencies": {
    "jest": "^29.7.0"
  }
}
```

- [ ] **Step 2: Create .env.example**

```
BRANDFETCH_KEY=your_brandfetch_key_here
UNSPLASH_KEY=your_unsplash_access_key_here
```

- [ ] **Step 3: Create jest.config.js**

```js
module.exports = {
  testEnvironment: 'node',
  testMatch: ['**/tests/**/*.test.js'],
  testTimeout: 30000,
};
```

- [ ] **Step 4: Install dependencies**

Run: `cd presentation-skill && npm install`
Expected: `node_modules/` created, `package-lock.json` generated.

- [ ] **Step 5: Commit**

```bash
cd "presentation-skill"
git add package.json package-lock.json .env.example jest.config.js
git commit -m "feat(presentation): bootstrap project with dependencies"
```

---

## Task 2: Theme System

**Files:**
- Create: `presentation-skill/renderer/theme.js`
- Create: `presentation-skill/tests/theme.test.js`

- [ ] **Step 1: Write failing tests**

Create `presentation-skill/tests/theme.test.js`:

```js
const { getTheme, detectTheme, themeToCssVars } = require('../renderer/theme');

describe('getTheme', () => {
  test('returns commercial theme by name', () => {
    const t = getTheme('commercial');
    expect(t.accent).toBe('#ff7832');
    expect(t.name).toBe('commercial');
  });

  test('returns tech theme by name', () => {
    const t = getTheme('tech');
    expect(t.accent).toBe('#818cf8');
  });

  test('returns prestige theme by name', () => {
    const t = getTheme('prestige');
    expect(t.accent).toBe('#d4af37');
  });

  test('returns neutral theme by name', () => {
    const t = getTheme('neutral');
    expect(t.accent).toBe('#e2e8f0');
  });

  test('returns commercial theme for unknown name', () => {
    const t = getTheme('unknown');
    expect(t.name).toBe('commercial');
  });
});

describe('detectTheme', () => {
  test('detects tech theme from SaaS keyword', () => {
    expect(detectTheme('Investor deck SaaS startup Series B')).toBe('tech');
  });

  test('detects prestige theme from consulting keyword', () => {
    expect(detectTheme('Rapport annuel McKinsey conseil haut de gamme')).toBe('prestige');
  });

  test('detects commercial theme from pitch keyword', () => {
    expect(detectTheme('pitch client Renault réduction des coûts')).toBe('commercial');
  });

  test('returns commercial theme when no keyword matches', () => {
    expect(detectTheme('lorem ipsum dolor sit amet')).toBe('commercial');
  });
});

describe('themeToCssVars', () => {
  test('outputs CSS variables string for commercial theme', () => {
    const vars = themeToCssVars(getTheme('commercial'));
    expect(vars).toContain('--accent: #ff7832');
    expect(vars).toContain('--base: #0d1f4e');
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/theme.test.js --no-coverage`
Expected: FAIL — "Cannot find module '../renderer/theme'"

- [ ] **Step 3: Create renderer/theme.js**

```js
const THEMES = {
  commercial: {
    name: 'commercial',
    accent: '#ff7832',
    accentSecondary: '#d4af37',
    base: '#0d1f4e',
  },
  tech: {
    name: 'tech',
    accent: '#818cf8',
    accentSecondary: '#60a5fa',
    base: '#0d1f4e',
  },
  prestige: {
    name: 'prestige',
    accent: '#d4af37',
    accentSecondary: '#ff7832',
    base: '#0d1f4e',
  },
  neutral: {
    name: 'neutral',
    accent: '#e2e8f0',
    accentSecondary: '#94a3b8',
    base: '#0d1f4e',
  },
};

const TECH_KEYWORDS = ['saas', 'startup', 'fintech', 'product', 'produit', 'api', 'tech', 'investor', 'série', 'serie', 'vc', 'arr', 'mrr', 'software', 'platform', 'plateforme'];
const PRESTIGE_KEYWORDS = ['conseil', 'consulting', 'mckinseyy', 'mckinsey', 'rapport annuel', 'luxe', 'luxury', 'premium', 'prestige', 'haut de gamme'];

function getTheme(name) {
  return THEMES[name] || THEMES.commercial;
}

function detectTheme(prompt) {
  const lower = prompt.toLowerCase();
  if (TECH_KEYWORDS.some(k => lower.includes(k))) return 'tech';
  if (PRESTIGE_KEYWORDS.some(k => lower.includes(k))) return 'prestige';
  return 'commercial';
}

function themeToCssVars(theme) {
  return [
    `--base: ${theme.base}`,
    `--accent: ${theme.accent}`,
    `--accent2: ${theme.accentSecondary}`,
  ].join(';\n  ');
}

module.exports = { getTheme, detectTheme, themeToCssVars, THEMES };
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd presentation-skill && npx jest tests/theme.test.js --no-coverage`
Expected: PASS (all 9 tests green)

- [ ] **Step 5: Commit**

```bash
git add renderer/theme.js tests/theme.test.js
git commit -m "feat(presentation): theme system with auto-detection and CSS var generation"
```

---

## Task 3: Base HTML Template

**Files:**
- Create: `presentation-skill/renderer/templates/base.html`

- [ ] **Step 1: Create the base template**

Create `presentation-skill/renderer/templates/base.html`:

```html
<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>{{TITLE}}</title>
<link rel="preconnect" href="https://fonts.googleapis.com">
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&family=Inter+Mono:wght@400;500&display=swap" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.2/dist/chart.umd.min.js"></script>
<style>
  :root {
    {{CSS_VARS}};
  }
  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
  html, body { width: 100%; background: #111; }
  body { font-family: 'Inter', sans-serif; }

  .slide {
    width: 1280px;
    height: 720px;
    background: var(--base);
    color: #fff;
    position: relative;
    overflow: hidden;
    page-break-after: always;
    page-break-inside: avoid;
    display: flex;
    flex-direction: column;
  }

  /* Accent bar top */
  .slide::before {
    content: '';
    position: absolute;
    top: 0; left: 0; right: 0;
    height: 4px;
    background: linear-gradient(90deg, var(--accent), var(--accent2));
  }

  /* Slide number */
  .slide-number {
    position: absolute;
    bottom: 20px; right: 28px;
    font-size: 11px;
    color: rgba(255,255,255,0.3);
    font-family: 'Inter Mono', monospace;
  }

  /* Monga watermark */
  .slide-brand {
    position: absolute;
    bottom: 18px; left: 28px;
    font-size: 11px;
    color: rgba(255,255,255,0.2);
    letter-spacing: 0.08em;
    text-transform: uppercase;
  }

  @media print {
    html, body { background: transparent; }
    .slide { page-break-after: always; }
  }
</style>
</head>
<body>
{{SLIDES_HTML}}
</body>
</html>
```

- [ ] **Step 2: Verify file exists**

Run: `ls -la "presentation-skill/renderer/templates/base.html"`
Expected: file listed.

- [ ] **Step 3: Commit**

```bash
git add renderer/templates/base.html
git commit -m "feat(presentation): base HTML template with theme CSS variables slot"
```

---

## Task 4: Cover, Metrics, Quote Layout Builders

**Files:**
- Create: `presentation-skill/renderer/templates/layouts/cover.js`
- Create: `presentation-skill/renderer/templates/layouts/metrics.js`
- Create: `presentation-skill/renderer/templates/layouts/quote.js`
- Create: `presentation-skill/tests/layouts.test.js`

- [ ] **Step 1: Write failing tests**

Create `presentation-skill/tests/layouts.test.js`:

```js
const { buildCover } = require('../renderer/templates/layouts/cover');
const { buildMetrics } = require('../renderer/templates/layouts/metrics');
const { buildQuote } = require('../renderer/templates/layouts/quote');

describe('buildCover', () => {
  test('renders title and subtitle', () => {
    const html = buildCover({ title: 'Mon Deck', subtitle: 'Avril 2026', index: 1, total: 5 });
    expect(html).toContain('Mon Deck');
    expect(html).toContain('Avril 2026');
    expect(html).toContain('class="slide"');
  });

  test('renders logo src when provided', () => {
    const html = buildCover({
      title: 'T', subtitle: 'S', index: 1, total: 1,
      clientLogo: { src: 'data:image/svg+xml;base64,abc', type: 'svg', filter: 'none', label: 'Acme' }
    });
    expect(html).toContain('data:image/svg+xml;base64,abc');
  });

  test('renders text fallback when logo type is text', () => {
    const html = buildCover({
      title: 'T', subtitle: 'S', index: 1, total: 1,
      clientLogo: { src: null, type: 'text', filter: 'none', label: 'Acme Corp' }
    });
    expect(html).toContain('Acme Corp');
  });
});

describe('buildMetrics', () => {
  test('renders title and KPI cards', () => {
    const html = buildMetrics({
      title: 'Chiffres clés',
      metrics: [
        { value: '23%', label: 'Réduction coûts' },
        { value: '×3.2', label: 'ROI projeté' },
        { value: '18M€', label: 'Économies' },
      ],
      index: 2, total: 5
    });
    expect(html).toContain('23%');
    expect(html).toContain('Réduction coûts');
    expect(html).toContain('×3.2');
    expect(html).toContain('ROI projeté');
  });

  test('renders up to 4 metrics', () => {
    const metrics = Array.from({ length: 5 }, (_, i) => ({ value: `${i}`, label: `L${i}` }));
    const html = buildMetrics({ title: 'T', metrics, index: 1, total: 1 });
    const count = (html.match(/class="metric-card"/g) || []).length;
    expect(count).toBeLessThanOrEqual(4);
  });
});

describe('buildQuote', () => {
  test('renders quote text and attribution', () => {
    const html = buildQuote({
      quote: 'Le succès appartient à ceux qui osent.',
      attribution: '— Monga',
      index: 5, total: 5
    });
    expect(html).toContain('Le succès appartient');
    expect(html).toContain('— Monga');
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/layouts.test.js --no-coverage`
Expected: FAIL — "Cannot find module"

- [ ] **Step 3: Create cover.js**

Create `presentation-skill/renderer/templates/layouts/cover.js`:

```js
function logoHtml(logo) {
  if (!logo) return '';
  if (logo.type === 'text') {
    return `<div class="cover-logo-text">${logo.label}</div>`;
  }
  return `<img class="cover-logo-img" src="${logo.src}" alt="${logo.label}">`;
}

function buildCover({ title, subtitle, date, clientLogo, index, total }) {
  return `
<div class="slide slide-cover">
  <div class="cover-inner">
    <div class="cover-meta">
      ${clientLogo ? logoHtml(clientLogo) : ''}
      ${date ? `<span class="cover-date">${date}</span>` : ''}
    </div>
    <h1 class="cover-title">${title}</h1>
    ${subtitle ? `<p class="cover-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="cover-accent-blob"></div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-cover { justify-content: flex-end; padding: 60px 80px; }
.cover-inner { position: relative; z-index: 2; }
.cover-meta { display: flex; align-items: center; gap: 24px; margin-bottom: 32px; }
.cover-logo-img { max-height: 48px; max-width: 160px; object-fit: contain; }
.cover-logo-text { font-size: 18px; font-weight: 700; color: rgba(255,255,255,0.7); letter-spacing: 0.05em; }
.cover-date { font-size: 13px; color: rgba(255,255,255,0.4); font-family: 'Inter Mono', monospace; }
.cover-title { font-size: 56px; font-weight: 800; line-height: 1.1; max-width: 780px; margin-bottom: 20px; }
.cover-subtitle { font-size: 22px; color: rgba(255,255,255,0.6); font-weight: 300; max-width: 600px; }
.cover-accent-blob {
  position: absolute; top: -120px; right: -120px;
  width: 500px; height: 500px; border-radius: 50%;
  background: radial-gradient(circle, var(--accent) 0%, transparent 70%);
  opacity: 0.18; pointer-events: none;
}
</style>`;
}

module.exports = { buildCover };
```

- [ ] **Step 4: Create metrics.js**

Create `presentation-skill/renderer/templates/layouts/metrics.js`:

```js
function buildMetrics({ title, subtitle, metrics, index, total }) {
  const cards = (metrics || []).slice(0, 4).map(m => `
    <div class="metric-card">
      <div class="metric-value">${m.value}</div>
      <div class="metric-label">${m.label}</div>
      ${m.context ? `<div class="metric-context">${m.context}</div>` : ''}
    </div>
  `).join('');

  return `
<div class="slide slide-metrics">
  <div class="slide-header">
    <h2 class="slide-title">${title}</h2>
    ${subtitle ? `<p class="slide-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="metrics-grid metrics-${Math.min((metrics || []).length, 4)}">${cards}</div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-metrics { padding: 50px 80px; gap: 40px; }
.slide-header { flex-shrink: 0; }
.slide-title { font-size: 36px; font-weight: 700; }
.slide-subtitle { font-size: 16px; color: rgba(255,255,255,0.5); margin-top: 8px; }
.metrics-grid { display: grid; gap: 24px; flex: 1; align-items: center; }
.metrics-2 { grid-template-columns: repeat(2, 1fr); }
.metrics-3 { grid-template-columns: repeat(3, 1fr); }
.metrics-4 { grid-template-columns: repeat(4, 1fr); }
.metric-card {
  background: rgba(255,255,255,0.06);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 16px; padding: 32px 24px;
  border-top: 3px solid var(--accent);
}
.metric-value { font-size: 52px; font-weight: 800; color: var(--accent); line-height: 1; }
.metric-label { font-size: 15px; color: rgba(255,255,255,0.7); margin-top: 12px; font-weight: 500; }
.metric-context { font-size: 12px; color: rgba(255,255,255,0.35); margin-top: 6px; }
</style>`;
}

module.exports = { buildMetrics };
```

- [ ] **Step 5: Create quote.js**

Create `presentation-skill/renderer/templates/layouts/quote.js`:

```js
function buildQuote({ quote, attribution, index, total }) {
  return `
<div class="slide slide-quote">
  <div class="quote-inner">
    <div class="quote-mark">"</div>
    <blockquote class="quote-text">${quote}</blockquote>
    ${attribution ? `<cite class="quote-attribution">${attribution}</cite>` : ''}
  </div>
  <div class="quote-accent-line"></div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-quote { justify-content: center; align-items: center; padding: 80px; text-align: center; }
.quote-inner { position: relative; z-index: 2; max-width: 900px; }
.quote-mark { font-size: 140px; color: var(--accent); opacity: 0.25; line-height: 0.6; font-family: Georgia, serif; }
.quote-text { font-size: 36px; font-weight: 300; line-height: 1.5; font-family: Georgia, serif; color: #fff; margin-top: -20px; }
.quote-attribution { display: block; font-size: 16px; color: var(--accent); margin-top: 32px; font-style: normal; font-weight: 600; letter-spacing: 0.06em; }
.quote-accent-line {
  position: absolute; bottom: 0; left: 0; right: 0; height: 3px;
  background: linear-gradient(90deg, transparent, var(--accent), transparent);
}
</style>`;
}

module.exports = { buildQuote };
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `cd presentation-skill && npx jest tests/layouts.test.js --no-coverage`
Expected: PASS (all 6 tests green)

- [ ] **Step 7: Commit**

```bash
git add renderer/templates/layouts/cover.js renderer/templates/layouts/metrics.js renderer/templates/layouts/quote.js tests/layouts.test.js
git commit -m "feat(presentation): cover, metrics, quote layout builders with tests"
```

---

## Task 5: Chart, VisualText, Comparison Layout Builders

**Files:**
- Create: `presentation-skill/renderer/templates/layouts/chart.js`
- Create: `presentation-skill/renderer/templates/layouts/visual-text.js`
- Create: `presentation-skill/renderer/templates/layouts/comparison.js`
- Modify: `presentation-skill/tests/layouts.test.js`

- [ ] **Step 1: Add tests for new layouts**

Append to `presentation-skill/tests/layouts.test.js`:

```js
const { buildChart } = require('../renderer/templates/layouts/chart');
const { buildVisualText } = require('../renderer/templates/layouts/visual-text');
const { buildComparison } = require('../renderer/templates/layouts/comparison');

describe('buildChart', () => {
  test('renders canvas element with chart ID', () => {
    const html = buildChart({
      title: 'Croissance ARR',
      chartType: 'bar',
      chartData: {
        labels: ['Q1', 'Q2', 'Q3'],
        datasets: [{ label: 'ARR', data: [1, 2, 3] }]
      },
      index: 2, total: 5
    });
    expect(html).toContain('<canvas');
    expect(html).toContain('chartData');
  });

  test('supports line chart type', () => {
    const html = buildChart({
      title: 'T', chartType: 'line',
      chartData: { labels: ['A'], datasets: [{ label: 'D', data: [1] }] },
      index: 1, total: 1
    });
    expect(html).toContain('"type":"line"');
  });
});

describe('buildVisualText', () => {
  test('renders title and bullet points', () => {
    const html = buildVisualText({
      title: 'Notre approche',
      bullets: ['Audit', 'Analyse', 'Recommandations'],
      imageUrl: null,
      index: 3, total: 5
    });
    expect(html).toContain('Notre approche');
    expect(html).toContain('Audit');
    expect(html).toContain('Analyse');
  });

  test('renders image when imageUrl provided', () => {
    const html = buildVisualText({
      title: 'T', bullets: ['B1'], imageUrl: 'https://example.com/img.jpg',
      index: 1, total: 1
    });
    expect(html).toContain('https://example.com/img.jpg');
  });
});

describe('buildComparison', () => {
  test('renders company names and logos', () => {
    const html = buildComparison({
      title: 'Benchmark concurrentiel',
      companies: [
        { name: 'Acme', logo: { src: null, type: 'text', label: 'Acme' }, highlight: true },
        { name: 'Competitor', logo: { src: null, type: 'text', label: 'Competitor' }, highlight: false },
      ],
      criteria: [{ label: 'Prix', values: ['Élevé', 'Moyen'] }],
      index: 4, total: 5
    });
    expect(html).toContain('Acme');
    expect(html).toContain('Competitor');
    expect(html).toContain('Prix');
  });
});
```

- [ ] **Step 2: Run tests to verify new tests fail**

Run: `cd presentation-skill && npx jest tests/layouts.test.js --no-coverage`
Expected: FAIL on buildChart, buildVisualText, buildComparison — "Cannot find module"

- [ ] **Step 3: Create chart.js**

Create `presentation-skill/renderer/templates/layouts/chart.js`:

```js
let chartCounter = 0;

function buildChart({ title, subtitle, chartType, chartData, index, total }) {
  chartCounter++;
  const canvasId = `chart-${index}-${chartCounter}`;
  const chartConfig = JSON.stringify({
    type: chartType || 'bar',
    data: chartData,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { labels: { color: 'rgba(255,255,255,0.7)', font: { family: 'Inter', size: 13 } } },
      },
      scales: chartType !== 'doughnut' ? {
        x: { ticks: { color: 'rgba(255,255,255,0.6)' }, grid: { color: 'rgba(255,255,255,0.06)' } },
        y: { ticks: { color: 'rgba(255,255,255,0.6)' }, grid: { color: 'rgba(255,255,255,0.06)' } },
      } : {},
    }
  });

  return `
<div class="slide slide-chart">
  <div class="slide-header">
    <h2 class="slide-title">${title}</h2>
    ${subtitle ? `<p class="slide-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="chart-container">
    <canvas id="${canvasId}"></canvas>
  </div>
  <script>
    (function() {
      var chartData = ${chartConfig};
      // Apply theme accent colors to datasets if not already set
      var accentColor = getComputedStyle(document.documentElement).getPropertyValue('--accent').trim() || '#ff7832';
      var accent2 = getComputedStyle(document.documentElement).getPropertyValue('--accent2').trim() || '#d4af37';
      var palette = [accentColor, accent2, '#60a5fa', '#34d399', '#f472b6'];
      chartData.data.datasets.forEach(function(ds, i) {
        if (!ds.backgroundColor) ds.backgroundColor = palette[i % palette.length];
        if (!ds.borderColor) ds.borderColor = palette[i % palette.length];
      });
      var ctx = document.getElementById('${canvasId}').getContext('2d');
      new Chart(ctx, chartData);
    })();
  </script>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-chart { padding: 50px 80px; gap: 32px; }
.chart-container { flex: 1; position: relative; }
</style>`;
}

module.exports = { buildChart };
```

- [ ] **Step 4: Create visual-text.js**

Create `presentation-skill/renderer/templates/layouts/visual-text.js`:

```js
function buildVisualText({ title, subtitle, bullets, imageUrl, imageCredit, index, total }) {
  const bulletHtml = (bullets || []).map(b => `
    <li class="vt-bullet">
      <span class="vt-bullet-dot"></span>
      <span>${b}</span>
    </li>
  `).join('');

  const imageSection = imageUrl
    ? `<div class="vt-image" style="background-image: url('${imageUrl}')"></div>`
    : `<div class="vt-image vt-image-placeholder"></div>`;

  return `
<div class="slide slide-visual-text">
  ${imageSection}
  <div class="vt-content">
    <h2 class="vt-title">${title}</h2>
    ${subtitle ? `<p class="vt-subtitle">${subtitle}</p>` : ''}
    <ul class="vt-bullets">${bulletHtml}</ul>
    ${imageCredit ? `<span class="vt-credit">Photo: ${imageCredit}</span>` : ''}
  </div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-visual-text { flex-direction: row !important; }
.vt-image {
  width: 45%; flex-shrink: 0;
  background-size: cover; background-position: center;
  position: relative;
}
.vt-image::after {
  content: '';
  position: absolute; inset: 0;
  background: linear-gradient(90deg, transparent 60%, var(--base));
}
.vt-image-placeholder { background: linear-gradient(135deg, var(--accent) 0%, rgba(13,31,78,0.8) 100%); }
.vt-content { flex: 1; padding: 60px 60px 60px 48px; display: flex; flex-direction: column; justify-content: center; }
.vt-title { font-size: 36px; font-weight: 700; margin-bottom: 12px; }
.vt-subtitle { font-size: 15px; color: rgba(255,255,255,0.5); margin-bottom: 32px; }
.vt-bullets { list-style: none; display: flex; flex-direction: column; gap: 18px; }
.vt-bullet { display: flex; align-items: flex-start; gap: 16px; font-size: 18px; line-height: 1.4; }
.vt-bullet-dot { width: 8px; height: 8px; border-radius: 50%; background: var(--accent); margin-top: 7px; flex-shrink: 0; }
.vt-credit { font-size: 11px; color: rgba(255,255,255,0.2); margin-top: auto; padding-top: 16px; }
</style>`;
}

module.exports = { buildVisualText };
```

- [ ] **Step 5: Create comparison.js**

Create `presentation-skill/renderer/templates/layouts/comparison.js`:

```js
function logoHtml(logo, size = 40) {
  if (!logo) return '';
  if (logo.type === 'text') return `<span class="comp-logo-text">${logo.label}</span>`;
  return `<img class="comp-logo-img" src="${logo.src}" alt="${logo.label}" style="max-height:${size}px;">`;
}

function buildComparison({ title, subtitle, companies, criteria, index, total }) {
  const headerCells = (companies || []).map(c => `
    <th class="comp-th ${c.highlight ? 'comp-th-highlight' : ''}">
      <div class="comp-company-header">
        ${logoHtml(c.logo, 36)}
        <span class="comp-company-name">${c.name}</span>
      </div>
    </th>
  `).join('');

  const rows = (criteria || []).map(cr => {
    const cells = (companies || []).map((c, i) => `
      <td class="comp-td ${c.highlight ? 'comp-td-highlight' : ''}">${cr.values[i] || '—'}</td>
    `).join('');
    return `<tr><td class="comp-label">${cr.label}</td>${cells}</tr>`;
  }).join('');

  return `
<div class="slide slide-comparison">
  <div class="slide-header">
    <h2 class="slide-title">${title}</h2>
    ${subtitle ? `<p class="slide-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="comp-table-wrap">
    <table class="comp-table">
      <thead><tr><th class="comp-label-header"></th>${headerCells}</tr></thead>
      <tbody>${rows}</tbody>
    </table>
  </div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-comparison { padding: 50px 80px; gap: 32px; }
.comp-table-wrap { flex: 1; overflow: hidden; }
.comp-table { width: 100%; border-collapse: collapse; }
.comp-label-header { width: 200px; }
.comp-th, .comp-label-header { padding: 16px; text-align: center; font-size: 14px; color: rgba(255,255,255,0.5); border-bottom: 1px solid rgba(255,255,255,0.1); }
.comp-th-highlight { color: var(--accent); border-bottom: 2px solid var(--accent); }
.comp-company-header { display: flex; flex-direction: column; align-items: center; gap: 8px; }
.comp-logo-img { object-fit: contain; }
.comp-logo-text { font-size: 13px; font-weight: 700; color: rgba(255,255,255,0.6); }
.comp-company-name { font-size: 13px; font-weight: 600; }
.comp-label { font-size: 14px; color: rgba(255,255,255,0.6); padding: 14px 16px; border-bottom: 1px solid rgba(255,255,255,0.06); }
.comp-td { text-align: center; padding: 14px 16px; font-size: 14px; border-bottom: 1px solid rgba(255,255,255,0.06); }
.comp-td-highlight { color: var(--accent); font-weight: 600; }
</style>`;
}

module.exports = { buildComparison };
```

- [ ] **Step 6: Run all layout tests**

Run: `cd presentation-skill && npx jest tests/layouts.test.js --no-coverage`
Expected: PASS (all 15 tests green)

- [ ] **Step 7: Commit**

```bash
git add renderer/templates/layouts/chart.js renderer/templates/layouts/visual-text.js renderer/templates/layouts/comparison.js tests/layouts.test.js
git commit -m "feat(presentation): chart, visual-text, comparison layout builders with tests"
```

---

## Task 6: Timeline, Process, Orgchart Layout Builders

**Files:**
- Create: `presentation-skill/renderer/templates/layouts/timeline.js`
- Create: `presentation-skill/renderer/templates/layouts/process.js`
- Create: `presentation-skill/renderer/templates/layouts/orgchart.js`
- Modify: `presentation-skill/tests/layouts.test.js`

- [ ] **Step 1: Add tests for remaining layouts**

Append to `presentation-skill/tests/layouts.test.js`:

```js
const { buildTimeline } = require('../renderer/templates/layouts/timeline');
const { buildProcess } = require('../renderer/templates/layouts/process');
const { buildOrgchart } = require('../renderer/templates/layouts/orgchart');

describe('buildTimeline', () => {
  test('renders milestone labels', () => {
    const html = buildTimeline({
      title: 'Roadmap 18 mois',
      milestones: [
        { date: 'T1 2026', label: 'Audit' },
        { date: 'T2 2026', label: 'Déploiement' },
        { date: 'T3 2026', label: 'Go-live' },
      ],
      index: 3, total: 5
    });
    expect(html).toContain('Audit');
    expect(html).toContain('T2 2026');
    expect(html).toContain('Go-live');
  });
});

describe('buildProcess', () => {
  test('renders step titles', () => {
    const html = buildProcess({
      title: 'Notre approche',
      steps: [
        { number: 1, title: 'Diagnostic', description: 'Analyse de la situation' },
        { number: 2, title: 'Plan', description: 'Définition des objectifs' },
      ],
      index: 2, total: 5
    });
    expect(html).toContain('Diagnostic');
    expect(html).toContain('Analyse de la situation');
    expect(html).toContain('Plan');
  });
});

describe('buildOrgchart', () => {
  test('renders root node label', () => {
    const html = buildOrgchart({
      title: 'Équipe projet',
      root: {
        name: 'Jean Dupont', role: 'CEO',
        children: [
          { name: 'Marie Martin', role: 'CTO', children: [] },
          { name: 'Paul Bernard', role: 'CFO', children: [] },
        ]
      },
      index: 4, total: 5
    });
    expect(html).toContain('Jean Dupont');
    expect(html).toContain('CEO');
    expect(html).toContain('Marie Martin');
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/layouts.test.js --no-coverage`
Expected: FAIL on buildTimeline, buildProcess, buildOrgchart

- [ ] **Step 3: Create timeline.js**

Create `presentation-skill/renderer/templates/layouts/timeline.js`:

```js
function buildTimeline({ title, subtitle, milestones, index, total }) {
  const items = (milestones || []).map((m, i) => `
    <div class="tl-item">
      <div class="tl-dot ${m.current ? 'tl-dot-current' : ''}"></div>
      <div class="tl-date">${m.date || ''}</div>
      <div class="tl-label">${m.label}</div>
      ${m.description ? `<div class="tl-desc">${m.description}</div>` : ''}
    </div>
  `).join('');

  return `
<div class="slide slide-timeline">
  <div class="slide-header">
    <h2 class="slide-title">${title}</h2>
    ${subtitle ? `<p class="slide-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="tl-track">
    <div class="tl-line"></div>
    ${items}
  </div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-timeline { padding: 50px 80px; gap: 40px; justify-content: center; }
.tl-track { position: relative; display: flex; align-items: flex-start; gap: 0; flex: 1; }
.tl-line {
  position: absolute; top: 12px; left: 0; right: 0; height: 2px;
  background: linear-gradient(90deg, var(--accent), var(--accent2));
}
.tl-item { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 12px; position: relative; }
.tl-dot {
  width: 24px; height: 24px; border-radius: 50%;
  background: var(--base); border: 2px solid var(--accent);
  z-index: 1;
}
.tl-dot-current { background: var(--accent); box-shadow: 0 0 12px var(--accent); }
.tl-date { font-size: 12px; color: var(--accent); font-family: 'Inter Mono', monospace; font-weight: 600; }
.tl-label { font-size: 15px; font-weight: 600; text-align: center; }
.tl-desc { font-size: 12px; color: rgba(255,255,255,0.5); text-align: center; max-width: 140px; }
</style>`;
}

module.exports = { buildTimeline };
```

- [ ] **Step 4: Create process.js**

Create `presentation-skill/renderer/templates/layouts/process.js`:

```js
function buildProcess({ title, subtitle, steps, index, total }) {
  const stepHtml = (steps || []).map((s, i) => `
    <div class="proc-step">
      <div class="proc-number">${s.number || i + 1}</div>
      <div class="proc-connector ${i < (steps.length - 1) ? '' : 'proc-connector-last'}"></div>
      <div class="proc-body">
        <div class="proc-title">${s.title}</div>
        ${s.description ? `<div class="proc-desc">${s.description}</div>` : ''}
      </div>
    </div>
  `).join('');

  return `
<div class="slide slide-process">
  <div class="slide-header">
    <h2 class="slide-title">${title}</h2>
    ${subtitle ? `<p class="slide-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="proc-steps">${stepHtml}</div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-process { padding: 50px 80px; gap: 40px; }
.proc-steps { display: flex; gap: 0; align-items: flex-start; flex: 1; }
.proc-step { display: flex; flex-direction: column; align-items: center; flex: 1; gap: 16px; position: relative; }
.proc-number {
  width: 52px; height: 52px; border-radius: 50%;
  background: linear-gradient(135deg, var(--accent), var(--accent2));
  display: flex; align-items: center; justify-content: center;
  font-size: 22px; font-weight: 800; flex-shrink: 0; z-index: 1;
}
.proc-connector {
  position: absolute; top: 26px; left: 50%; right: -50%;
  height: 2px; background: rgba(255,255,255,0.15); z-index: 0;
}
.proc-connector-last { display: none; }
.proc-body { text-align: center; padding: 0 8px; }
.proc-title { font-size: 16px; font-weight: 700; margin-bottom: 8px; }
.proc-desc { font-size: 13px; color: rgba(255,255,255,0.55); line-height: 1.5; }
</style>`;
}

module.exports = { buildProcess };
```

- [ ] **Step 5: Create orgchart.js**

Create `presentation-skill/renderer/templates/layouts/orgchart.js`:

```js
function nodeHtml(node, depth = 0) {
  const children = node.children && node.children.length > 0
    ? `<div class="org-children">${node.children.map(c => nodeHtml(c, depth + 1)).join('')}</div>`
    : '';

  return `
    <div class="org-node-wrap">
      <div class="org-node">
        <div class="org-name">${node.name}</div>
        <div class="org-role">${node.role}</div>
      </div>
      ${children}
    </div>
  `;
}

function buildOrgchart({ title, subtitle, root, index, total }) {
  return `
<div class="slide slide-orgchart">
  <div class="slide-header">
    <h2 class="slide-title">${title}</h2>
    ${subtitle ? `<p class="slide-subtitle">${subtitle}</p>` : ''}
  </div>
  <div class="org-tree">${nodeHtml(root || { name: '', role: '', children: [] })}</div>
  <span class="slide-brand">Monga</span>
  <span class="slide-number">${index} / ${total}</span>
</div>
<style>
.slide-orgchart { padding: 50px 80px; gap: 32px; align-items: center; }
.org-tree { display: flex; justify-content: center; flex: 1; }
.org-node-wrap { display: flex; flex-direction: column; align-items: center; }
.org-node {
  background: rgba(255,255,255,0.07);
  border: 1px solid rgba(255,255,255,0.12);
  border-radius: 12px; padding: 14px 20px; text-align: center; min-width: 140px;
  border-top: 2px solid var(--accent);
}
.org-name { font-size: 14px; font-weight: 700; }
.org-role { font-size: 12px; color: var(--accent); margin-top: 4px; }
.org-children {
  display: flex; gap: 16px; margin-top: 0;
  border-top: 2px solid rgba(255,255,255,0.1);
  padding-top: 24px; margin-top: 24px;
  position: relative;
}
.org-children::before {
  content: '';
  position: absolute; top: 0; left: 50%;
  width: 2px; height: 24px;
  background: rgba(255,255,255,0.1);
  transform: translateX(-50%);
}
</style>`;
}

module.exports = { buildOrgchart };
```

- [ ] **Step 6: Run all layout tests**

Run: `cd presentation-skill && npx jest tests/layouts.test.js --no-coverage`
Expected: PASS (all 21 tests green)

- [ ] **Step 7: Commit**

```bash
git add renderer/templates/layouts/timeline.js renderer/templates/layouts/process.js renderer/templates/layouts/orgchart.js tests/layouts.test.js
git commit -m "feat(presentation): timeline, process, orgchart layout builders with tests"
```

---

## Task 7: Renderer Orchestrator

**Files:**
- Create: `presentation-skill/renderer/index.js`
- Create: `presentation-skill/tests/renderer.test.js`

- [ ] **Step 1: Write failing tests**

Create `presentation-skill/tests/renderer.test.js`:

```js
const { renderPresentation } = require('../renderer/index');
const fs = require('fs');
const path = require('path');

const SAMPLE_MANIFEST = {
  title: 'Test Deck',
  theme: 'commercial',
  slides: [
    {
      layout: 'cover',
      title: 'Test Presentation',
      subtitle: 'Avril 2026',
    },
    {
      layout: 'metrics',
      title: 'Chiffres clés',
      metrics: [
        { value: '23%', label: 'Réduction coûts' },
        { value: '×3.2', label: 'ROI' },
      ],
    },
    {
      layout: 'quote',
      quote: 'Excellence is not a skill, it is an attitude.',
      attribution: '— Ralph Marston',
    },
  ],
};

describe('renderPresentation', () => {
  test('returns a valid HTML document string', async () => {
    const html = await renderPresentation(SAMPLE_MANIFEST, { skipEnrich: true });
    expect(typeof html).toBe('string');
    expect(html).toContain('<!DOCTYPE html>');
    expect(html).toContain('Test Presentation');
  });

  test('includes all slide titles', async () => {
    const html = await renderPresentation(SAMPLE_MANIFEST, { skipEnrich: true });
    expect(html).toContain('Chiffres clés');
    expect(html).toContain('Excellence is not a skill');
  });

  test('applies commercial theme CSS variables', async () => {
    const html = await renderPresentation(SAMPLE_MANIFEST, { skipEnrich: true });
    expect(html).toContain('--accent: #ff7832');
  });

  test('applies tech theme when specified', async () => {
    const manifest = { ...SAMPLE_MANIFEST, theme: 'tech' };
    const html = await renderPresentation(manifest, { skipEnrich: true });
    expect(html).toContain('--accent: #818cf8');
  });

  test('throws for unknown layout type', async () => {
    const manifest = {
      ...SAMPLE_MANIFEST,
      slides: [{ layout: 'unknown-layout', title: 'Bad' }],
    };
    await expect(renderPresentation(manifest, { skipEnrich: true })).rejects.toThrow('Unknown layout');
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/renderer.test.js --no-coverage`
Expected: FAIL — "Cannot find module '../renderer/index'"

- [ ] **Step 3: Create renderer/index.js**

Create `presentation-skill/renderer/index.js`:

```js
require('dotenv').config();
const fs = require('fs');
const path = require('path');
const { getTheme, detectTheme, themeToCssVars } = require('./theme');
const { buildCover } = require('./templates/layouts/cover');
const { buildMetrics } = require('./templates/layouts/metrics');
const { buildChart } = require('./templates/layouts/chart');
const { buildVisualText } = require('./templates/layouts/visual-text');
const { buildComparison } = require('./templates/layouts/comparison');
const { buildTimeline } = require('./templates/layouts/timeline');
const { buildProcess } = require('./templates/layouts/process');
const { buildOrgchart } = require('./templates/layouts/orgchart');
const { buildQuote } = require('./templates/layouts/quote');

const LAYOUT_BUILDERS = {
  cover: buildCover,
  metrics: buildMetrics,
  chart: buildChart,
  'visual-text': buildVisualText,
  comparison: buildComparison,
  timeline: buildTimeline,
  process: buildProcess,
  orgchart: buildOrgchart,
  quote: buildQuote,
};

/**
 * @param {Object} manifest   - { title, theme, slides: [{ layout, ...slideData }] }
 * @param {Object} [opts]     - { skipEnrich: bool }
 * @returns {Promise<string>} - Full HTML document
 */
async function renderPresentation(manifest, opts = {}) {
  const themeName = manifest.theme || detectTheme(manifest.title || '');
  const theme = getTheme(themeName);
  const total = manifest.slides.length;

  // Enrich: logos + photos (skip in test mode)
  let enrichedSlides = manifest.slides;
  if (!opts.skipEnrich) {
    enrichedSlides = await enrichSlides(manifest.slides, theme);
  }

  // Build slides HTML
  const slidesHtml = enrichedSlides.map((slide, i) => {
    const builder = LAYOUT_BUILDERS[slide.layout];
    if (!builder) throw new Error(`Unknown layout: "${slide.layout}"`);
    return builder({ ...slide, index: i + 1, total });
  }).join('\n');

  // Inject into base template
  const baseTemplate = fs.readFileSync(
    path.join(__dirname, 'templates/base.html'),
    'utf8'
  );

  return baseTemplate
    .replace('{{TITLE}}', manifest.title || 'Présentation')
    .replace('{{CSS_VARS}}', themeToCssVars(theme))
    .replace('{{SLIDES_HTML}}', slidesHtml);
}

async function enrichSlides(slides, theme) {
  const { sourceLogo } = require('./services/logo-sourcer');
  const { fetchPhoto } = require('./services/unsplash');

  return Promise.all(slides.map(async (slide) => {
    const enriched = { ...slide };

    // Source logos for comparison slides
    if (slide.layout === 'comparison' && slide.companies) {
      enriched.companies = await Promise.all(
        slide.companies.map(async (c) => ({
          ...c,
          logo: await sourceLogo(c.name, c.domain || null),
        }))
      );
    }

    // Source client logo for cover slides
    if (slide.layout === 'cover' && slide.clientName) {
      enriched.clientLogo = await sourceLogo(slide.clientName, slide.clientDomain || null);
    }

    // Source photo for visual-text slides
    if (slide.layout === 'visual-text' && slide.imageQuery && !slide.imageUrl) {
      const photo = await fetchPhoto(slide.imageQuery);
      if (photo) {
        enriched.imageUrl = photo.url;
        enriched.imageCredit = photo.credit;
      }
    }

    return enriched;
  }));
}

module.exports = { renderPresentation, enrichSlides };
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd presentation-skill && npx jest tests/renderer.test.js --no-coverage`
Expected: PASS (all 5 tests green)

- [ ] **Step 5: Commit**

```bash
git add renderer/index.js tests/renderer.test.js
git commit -m "feat(presentation): renderer orchestrator - JSON manifest to full HTML"
```

---

## Task 8: Unsplash Service

**Files:**
- Create: `presentation-skill/renderer/services/unsplash.js`
- Create: `presentation-skill/tests/unsplash.test.js`

- [ ] **Step 1: Write failing tests**

Create `presentation-skill/tests/unsplash.test.js`:

```js
const { fetchPhoto, buildUnsplashQuery } = require('../renderer/services/unsplash');

describe('buildUnsplashQuery', () => {
  test('returns query string from keywords', () => {
    const q = buildUnsplashQuery('real estate luxury office');
    expect(typeof q).toBe('string');
    expect(q.length).toBeGreaterThan(0);
  });

  test('trims and lowercases keywords', () => {
    const q = buildUnsplashQuery('  Real Estate  ');
    expect(q).toBe('real estate');
  });
});

describe('fetchPhoto', () => {
  test('returns null when UNSPLASH_KEY is not set', async () => {
    const originalKey = process.env.UNSPLASH_KEY;
    delete process.env.UNSPLASH_KEY;
    const result = await fetchPhoto('office building');
    expect(result).toBeNull();
    if (originalKey) process.env.UNSPLASH_KEY = originalKey;
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/unsplash.test.js --no-coverage`
Expected: FAIL — "Cannot find module '../renderer/services/unsplash'"

- [ ] **Step 3: Create unsplash.js**

Create `presentation-skill/renderer/services/unsplash.js`:

```js
const https = require('https');

const UNSPLASH_KEY = process.env.UNSPLASH_KEY || null;

function buildUnsplashQuery(keywords) {
  return (keywords || '').trim().toLowerCase();
}

/**
 * Fetch a contextual photo from Unsplash.
 *
 * @param {string} query     - Search keywords
 * @returns {Promise<{url: string, credit: string}|null>}
 */
async function fetchPhoto(query) {
  if (!UNSPLASH_KEY) return null;

  const encodedQuery = encodeURIComponent(buildUnsplashQuery(query));
  return new Promise((resolve) => {
    const options = {
      hostname: 'api.unsplash.com',
      path: `/search/photos?query=${encodedQuery}&per_page=3&orientation=landscape&content_filter=high`,
      headers: {
        Authorization: `Client-ID ${UNSPLASH_KEY}`,
        'Accept-Version': 'v1',
      },
    };

    https.get(options, (res) => {
      let data = '';
      res.on('data', d => data += d);
      res.on('end', () => {
        try {
          const json = JSON.parse(data);
          const results = json.results || [];
          if (results.length === 0) { resolve(null); return; }
          const photo = results[0];
          resolve({
            url: photo.urls.regular,
            credit: `${photo.user.name} / Unsplash`,
          });
        } catch { resolve(null); }
      });
    }).on('error', () => resolve(null));
  });
}

module.exports = { fetchPhoto, buildUnsplashQuery };
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd presentation-skill && npx jest tests/unsplash.test.js --no-coverage`
Expected: PASS (all 3 tests green)

- [ ] **Step 5: Commit**

```bash
git add renderer/services/unsplash.js tests/unsplash.test.js
git commit -m "feat(presentation): Unsplash photo service with graceful no-key fallback"
```

---

## Task 9: Logo Sourcer Tests

**Files:**
- Create: `presentation-skill/tests/logo-sourcer.test.js`

The implementation (`logo-sourcer.js`) already exists. This task adds tests for the pure functions — no network calls in tests.

- [ ] **Step 1: Write tests for pure functions**

Create `presentation-skill/tests/logo-sourcer.test.js`:

```js
const {
  detectLogoFilter,
  convertDarkFillsToWhite,
  svgToDataUri,
} = require('../renderer/services/logo-sourcer');

describe('detectLogoFilter', () => {
  test('returns invert for dark fills', () => {
    const svg = '<svg><path fill="#000000"/><path fill="#1a1a2e"/></svg>';
    expect(detectLogoFilter(svg)).toBe('invert');
  });

  test('returns white for light fills', () => {
    const svg = '<svg><path fill="#ffffff"/><path fill="#f0f0f0"/></svg>';
    expect(detectLogoFilter(svg)).toBe('white');
  });

  test('returns invert for SVG with no fills (default dark assumption)', () => {
    const svg = '<svg><path d="M0 0h100v100H0z"/></svg>';
    expect(detectLogoFilter(svg)).toBe('invert');
  });
});

describe('convertDarkFillsToWhite', () => {
  test('converts dark fill to white', () => {
    const svg = '<svg><path fill="#000000"/></svg>';
    const result = convertDarkFillsToWhite(svg, false);
    expect(result).toContain('fill="#ffffff"');
    expect(result).not.toContain('fill="#000000"');
  });

  test('preserves vivid accent color when keepAccentColors is true', () => {
    const svg = '<svg><path fill="#d71031"/></svg>';  // vivid red
    const result = convertDarkFillsToWhite(svg, true);
    expect(result).toContain('fill="#d71031"');
  });

  test('converts vivid color when keepAccentColors is false', () => {
    const svg = '<svg><path fill="#d71031"/></svg>';
    const result = convertDarkFillsToWhite(svg, false);
    expect(result).not.toContain('fill="#d71031"');
  });

  test('leaves white fill untouched', () => {
    const svg = '<svg><path fill="#ffffff"/></svg>';
    const result = convertDarkFillsToWhite(svg, true);
    expect(result).toContain('fill="#ffffff"');
  });
});

describe('svgToDataUri', () => {
  test('returns a base64 data URI', () => {
    const svg = '<svg><rect/></svg>';
    const uri = svgToDataUri(svg);
    expect(uri.startsWith('data:image/svg+xml;base64,')).toBe(true);
  });

  test('is reversible', () => {
    const svg = '<svg><rect width="100" height="100"/></svg>';
    const uri = svgToDataUri(svg);
    const decoded = Buffer.from(uri.replace('data:image/svg+xml;base64,', ''), 'base64').toString('utf8');
    expect(decoded).toBe(svg);
  });
});
```

- [ ] **Step 2: Run tests**

Run: `cd presentation-skill && npx jest tests/logo-sourcer.test.js --no-coverage`
Expected: PASS (all 8 tests green)

- [ ] **Step 3: Commit**

```bash
git add tests/logo-sourcer.test.js
git commit -m "test(presentation): logo sourcer unit tests for pure color functions"
```

---

## Task 10: PDF Export

**Files:**
- Create: `presentation-skill/renderer/export/pdf.js`
- Create: `presentation-skill/tests/pdf.test.js`

- [ ] **Step 1: Write failing tests**

Create `presentation-skill/tests/pdf.test.js`:

```js
const fs = require('fs');
const path = require('path');
const os = require('os');
const { exportPdf } = require('../renderer/export/pdf');

const MINIMAL_HTML = `<!DOCTYPE html><html><head><style>
  .slide { width: 1280px; height: 720px; background: #0d1f4e; color: white; display: flex; align-items: center; justify-content: center; }
</style></head><body>
  <div class="slide"><h1>Test Slide</h1></div>
</body></html>`;

describe('exportPdf', () => {
  let outPath;

  beforeEach(() => {
    outPath = path.join(os.tmpdir(), `test-${Date.now()}.pdf`);
  });

  afterEach(() => {
    if (fs.existsSync(outPath)) fs.unlinkSync(outPath);
  });

  test('creates a PDF file at the specified path', async () => {
    await exportPdf(MINIMAL_HTML, outPath);
    expect(fs.existsSync(outPath)).toBe(true);
  }, 30000);

  test('output file has non-zero size', async () => {
    await exportPdf(MINIMAL_HTML, outPath);
    const stats = fs.statSync(outPath);
    expect(stats.size).toBeGreaterThan(1000);
  }, 30000);

  test('output file starts with PDF magic bytes', async () => {
    await exportPdf(MINIMAL_HTML, outPath);
    const fd = fs.openSync(outPath, 'r');
    const buf = Buffer.alloc(4);
    fs.readSync(fd, buf, 0, 4, 0);
    fs.closeSync(fd);
    expect(buf.toString('ascii')).toBe('%PDF');
  }, 30000);
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/pdf.test.js --no-coverage`
Expected: FAIL — "Cannot find module '../renderer/export/pdf'"

- [ ] **Step 3: Create renderer/export/pdf.js**

```js
const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

/**
 * Export HTML to PDF using Puppeteer (headless Chrome).
 * Output: A4 landscape, print-perfect, 1280×720 pt equivalent.
 *
 * @param {string} html     - Complete HTML document string
 * @param {string} outPath  - Absolute path for the output PDF file
 */
async function exportPdf(html, outPath) {
  // Ensure output directory exists
  fs.mkdirSync(path.dirname(outPath), { recursive: true });

  const browser = await puppeteer.launch({
    headless: 'new',
    args: ['--no-sandbox', '--disable-setuid-sandbox'],
  });

  try {
    const page = await browser.newPage();

    // Set viewport to match slide dimensions
    await page.setViewport({ width: 1280, height: 720 });

    // Load HTML content
    await page.setContent(html, { waitUntil: 'networkidle0' });

    // Wait for Chart.js to render (if any charts present)
    await page.evaluate(() => {
      return new Promise(resolve => {
        if (typeof Chart === 'undefined') { resolve(); return; }
        setTimeout(resolve, 500);
      });
    });

    await page.pdf({
      path: outPath,
      width: '1280px',
      height: '720px',
      printBackground: true,
      margin: { top: 0, right: 0, bottom: 0, left: 0 },
    });
  } finally {
    await browser.close();
  }
}

module.exports = { exportPdf };
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd presentation-skill && npx jest tests/pdf.test.js --no-coverage`
Expected: PASS (all 3 tests green, may take ~10s for Puppeteer)

- [ ] **Step 5: Commit**

```bash
git add renderer/export/pdf.js tests/pdf.test.js
git commit -m "feat(presentation): Puppeteer PDF export with A4 landscape dimensions"
```

---

## Task 11: PPTX Export

**Files:**
- Create: `presentation-skill/renderer/export/pptx.js`
- Create: `presentation-skill/tests/pptx.test.js`

- [ ] **Step 1: Write failing tests**

Create `presentation-skill/tests/pptx.test.js`:

```js
const fs = require('fs');
const path = require('path');
const os = require('os');
const { exportPptx } = require('../renderer/export/pptx');

const SAMPLE_MANIFEST = {
  title: 'Test PPTX',
  theme: 'commercial',
  slides: [
    { layout: 'cover', title: 'Test Deck', subtitle: 'Sous-titre', index: 1, total: 3 },
    { layout: 'metrics', title: 'KPIs', metrics: [{ value: '23%', label: 'Croissance' }], index: 2, total: 3 },
    { layout: 'quote', quote: 'Excellence matters.', attribution: '— Test', index: 3, total: 3 },
  ],
};

describe('exportPptx', () => {
  let outPath;

  beforeEach(() => {
    outPath = path.join(os.tmpdir(), `test-${Date.now()}.pptx`);
  });

  afterEach(() => {
    if (fs.existsSync(outPath)) fs.unlinkSync(outPath);
  });

  test('creates a PPTX file at the specified path', async () => {
    await exportPptx(SAMPLE_MANIFEST, outPath);
    expect(fs.existsSync(outPath)).toBe(true);
  });

  test('output file is a valid ZIP (PPTX is ZIP-based)', async () => {
    await exportPptx(SAMPLE_MANIFEST, outPath);
    const fd = fs.openSync(outPath, 'r');
    const buf = Buffer.alloc(4);
    fs.readSync(fd, buf, 0, 4, 0);
    fs.closeSync(fd);
    // PPTX starts with PK (ZIP magic bytes)
    expect(buf[0]).toBe(0x50); // 'P'
    expect(buf[1]).toBe(0x4B); // 'K'
  });

  test('output file has non-zero size', async () => {
    await exportPptx(SAMPLE_MANIFEST, outPath);
    const stats = fs.statSync(outPath);
    expect(stats.size).toBeGreaterThan(1000);
  });
});
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd presentation-skill && npx jest tests/pptx.test.js --no-coverage`
Expected: FAIL — "Cannot find module '../renderer/export/pptx'"

- [ ] **Step 3: Create renderer/export/pptx.js**

```js
const PptxGenJS = require('pptxgenjs');
const fs = require('fs');
const path = require('path');
const { getTheme } = require('../theme');

/**
 * Export a slide manifest to PPTX using pptxgenjs.
 * Best-effort: captures title, subtitle, key text, and accent colors.
 * Complex CSS gradients and blobs are simplified to flat colors.
 *
 * @param {Object} manifest   - { title, theme, slides: [{ layout, title, ... }] }
 * @param {string} outPath    - Absolute path for the output PPTX file
 */
async function exportPptx(manifest, outPath) {
  fs.mkdirSync(path.dirname(outPath), { recursive: true });

  const pres = new PptxGenJS();
  pres.layout = 'LAYOUT_WIDE'; // 13.33" × 7.5"

  const theme = getTheme(manifest.theme || 'commercial');
  const BASE = theme.base;       // #0d1f4e
  const ACCENT = theme.accent;
  const WHITE = 'FFFFFF';
  const WHITE_FADED = 'AAAACC';

  for (const slide of manifest.slides) {
    const s = pres.addSlide();

    // Dark background
    s.background = { fill: BASE.replace('#', '') };

    // Accent top bar
    s.addShape(pres.ShapeType.rect, {
      x: 0, y: 0, w: '100%', h: 0.05,
      fill: { color: ACCENT.replace('#', '') },
      line: { type: 'none' },
    });

    switch (slide.layout) {
      case 'cover':
        s.addText(slide.title || '', {
          x: 0.8, y: 2.5, w: 8, h: 2,
          fontSize: 44, bold: true, color: WHITE, fontFace: 'Arial',
        });
        if (slide.subtitle) {
          s.addText(slide.subtitle, {
            x: 0.8, y: 4.6, w: 7, h: 0.8,
            fontSize: 20, color: WHITE_FADED, fontFace: 'Arial',
          });
        }
        break;

      case 'metrics':
        s.addText(slide.title || '', {
          x: 0.8, y: 0.4, w: 11, h: 0.8,
          fontSize: 28, bold: true, color: WHITE, fontFace: 'Arial',
        });
        (slide.metrics || []).slice(0, 4).forEach((m, i) => {
          const x = 0.8 + i * 3.1;
          s.addText(m.value, { x, y: 1.8, w: 2.8, h: 1.2, fontSize: 40, bold: true, color: ACCENT.replace('#', ''), fontFace: 'Arial', align: 'center' });
          s.addText(m.label, { x, y: 3.0, w: 2.8, h: 0.6, fontSize: 13, color: WHITE_FADED, fontFace: 'Arial', align: 'center' });
        });
        break;

      case 'quote':
        s.addText(`"${slide.quote || ''}"`, {
          x: 1, y: 1.5, w: 11, h: 3,
          fontSize: 28, color: WHITE, fontFace: 'Arial', italic: true, align: 'center',
        });
        if (slide.attribution) {
          s.addText(slide.attribution, {
            x: 1, y: 4.7, w: 11, h: 0.6,
            fontSize: 16, color: ACCENT.replace('#', ''), fontFace: 'Arial', align: 'center',
          });
        }
        break;

      default:
        // Generic fallback: title + content lines
        s.addText(slide.title || slide.layout, {
          x: 0.8, y: 0.4, w: 11, h: 0.8,
          fontSize: 28, bold: true, color: WHITE, fontFace: 'Arial',
        });
        if (slide.subtitle) {
          s.addText(slide.subtitle, {
            x: 0.8, y: 1.3, w: 11, h: 0.5,
            fontSize: 16, color: WHITE_FADED, fontFace: 'Arial',
          });
        }
        break;
    }

    // Slide number
    s.addText(`${slide.index} / ${slide.total}`, {
      x: 11.8, y: 6.9, w: 1.3, h: 0.3,
      fontSize: 10, color: '444466', fontFace: 'Arial', align: 'right',
    });
  }

  await pres.writeFile({ fileName: outPath });
}

module.exports = { exportPptx };
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd presentation-skill && npx jest tests/pptx.test.js --no-coverage`
Expected: PASS (all 3 tests green)

- [ ] **Step 5: Commit**

```bash
git add renderer/export/pptx.js tests/pptx.test.js
git commit -m "feat(presentation): pptxgenjs PPTX export with theme colors"
```

---

## Task 12: Full Test Suite Pass

- [ ] **Step 1: Run the complete test suite**

Run: `cd presentation-skill && npx jest --no-coverage`
Expected: All tests in theme, layouts, renderer, unsplash, logo-sourcer, pdf, pptx pass.

- [ ] **Step 2: Fix any failures**

If tests fail, read the error message carefully and fix the root cause in the relevant source file. Do not modify tests to make them pass — only fix the implementation.

- [ ] **Step 3: Commit any fixes**

```bash
git add -p   # stage only fixed files
git commit -m "fix(presentation): resolve test suite failures"
```

---

## Task 13: Claude Skill Definition

**Files:**
- Create: `presentation-skill/skill.md`

- [ ] **Step 1: Create the skill definition**

Create `presentation-skill/skill.md`:

```markdown
---
name: présentation
description: Génère une présentation professionnelle niveau cabinet marketing — design premium, photos contextuelles, logos auto-sourcés, graphiques — exportée en PDF et PPTX.
---

# Skill: /présentation

## Syntaxe

```
/présentation [--theme commercial|tech|prestige|neutre] [--slides N]
<contenu libre en langage naturel>
```

## Processus

1. **Parse** le prompt pour extraire :
   - Type de deck (pitch, investor deck, rapport, benchmark...)
   - Thème (auto-detect ou --theme forcé)
   - Nombre de slides souhaité (--slides N ou adaptatif)
   - Marques/concurrents mentionnés
   - Données chiffrées pour les graphiques
   - Phases chronologiques pour les timelines

2. **Construis** un manifest JSON :
```json
{
  "title": "Titre du deck",
  "theme": "commercial|tech|prestige|neutral",
  "slides": [
    {
      "layout": "cover|metrics|chart|visual-text|comparison|timeline|process|orgchart|quote",
      "title": "...",
      "...": "champs selon le layout"
    }
  ]
}
```

3. **Layouts disponibles** et leurs champs :

| Layout | Champs requis |
|---|---|
| cover | title, subtitle, date?, clientName?, clientDomain? |
| metrics | title, metrics: [{value, label, context?}] |
| chart | title, chartType: bar|line|doughnut|area, chartData: {labels, datasets} |
| visual-text | title, bullets: string[], imageQuery? |
| comparison | title, companies: [{name, domain?, highlight?}], criteria: [{label, values}] |
| timeline | title, milestones: [{date, label, description?, current?}] |
| process | title, steps: [{number, title, description}] |
| orgchart | title, root: {name, role, children: [...]} |
| quote | quote, attribution? |

4. **Appelle** le renderer Node.js :
```bash
node presentation-skill/renderer/index.js --manifest '<JSON>' --out /tmp/presentation
```

5. **Exporte** :
   - PDF : `/tmp/presentation.pdf`
   - PPTX : `/tmp/presentation.pptx`

6. **Livre** les fichiers avec un résumé des slides générées.

## Règles de structure

- Commence toujours par un slide `cover`
- Termine toujours par un slide `quote` ou `cover` de conclusion
- Maximum 15 slides, minimum 4
- Alterne les layouts pour varier le rythme visuel
- Détecte automatiquement les données chiffrées → layout `metrics` ou `chart`
- Détecte automatiquement les phases chronologiques → layout `timeline`
- Détecte automatiquement les concurrents/comparaisons → layout `comparison`

## Thèmes

| Thème | Accent | Déclencheurs |
|---|---|---|
| commercial | #ff7832 Orange | pitch, client, proposal, deal |
| tech | #818cf8 Indigo | SaaS, startup, investor, ARR, MRR |
| prestige | #d4af37 Or | conseil, rapport annuel, luxe |
| neutral | #e2e8f0 Blanc | usage interne, non défini |
```

- [ ] **Step 2: Commit**

```bash
git add skill.md
git commit -m "feat(presentation): Claude skill definition for /présentation command"
```

---

## Task 14: End-to-End Integration Test

**Files:**
- Create: `presentation-skill/tests/e2e.test.js`

- [ ] **Step 1: Write e2e test**

Create `presentation-skill/tests/e2e.test.js`:

```js
const fs = require('fs');
const path = require('path');
const os = require('os');
const { renderPresentation } = require('../renderer/index');
const { exportPdf } = require('../renderer/export/pdf');
const { exportPptx } = require('../renderer/export/pptx');

const IAD_MANIFEST = {
  title: 'MONGA × IAD — Partenariat stratégique',
  theme: 'commercial',
  slides: [
    {
      layout: 'cover',
      title: 'MONGA × IAD',
      subtitle: 'Optimisation des acquisitions immobilières premium',
      date: 'Avril 2026',
    },
    {
      layout: 'metrics',
      title: 'Impact mesuré',
      metrics: [
        { value: '23%', label: 'Réduction délais acquisition', context: 'vs. marché' },
        { value: '×3.2', label: 'ROI projeté', context: 'sur 24 mois' },
        { value: '18M€', label: 'Volume géré', context: 'portefeuille actif' },
        { value: '94%', label: 'Satisfaction client', context: 'NPS 2025' },
      ],
    },
    {
      layout: 'process',
      title: 'Notre approche en 4 étapes',
      steps: [
        { number: 1, title: 'Audit', description: 'Analyse du portefeuille et des objectifs' },
        { number: 2, title: 'Sourcing', description: 'Identification des opportunités ciblées' },
        { number: 3, title: 'Négociation', description: 'Structuration et closing des deals' },
        { number: 4, title: 'Suivi', description: 'Reporting et optimisation continue' },
      ],
    },
    {
      layout: 'quote',
      quote: "L'immobilier de demain se construit avec les données d'aujourd'hui.",
      attribution: '— Monga, 2026',
    },
  ],
};

describe('End-to-end pipeline', () => {
  let tmpDir;

  beforeAll(() => {
    tmpDir = path.join(os.tmpdir(), `e2e-pres-${Date.now()}`);
    fs.mkdirSync(tmpDir, { recursive: true });
  });

  afterAll(() => {
    fs.rmSync(tmpDir, { recursive: true, force: true });
  });

  test('renders a complete HTML document', async () => {
    const html = await renderPresentation(IAD_MANIFEST, { skipEnrich: true });
    expect(html).toContain('<!DOCTYPE html>');
    expect(html).toContain('MONGA × IAD');
    expect(html).toContain('23%');
    expect(html).toContain('Notre approche en 4 étapes');
    expect(html).toContain('--accent: #ff7832');
  }, 15000);

  test('exports a valid PDF', async () => {
    const html = await renderPresentation(IAD_MANIFEST, { skipEnrich: true });
    const pdfPath = path.join(tmpDir, 'test.pdf');
    await exportPdf(html, pdfPath);

    expect(fs.existsSync(pdfPath)).toBe(true);
    const buf = Buffer.alloc(4);
    const fd = fs.openSync(pdfPath, 'r');
    fs.readSync(fd, buf, 0, 4, 0);
    fs.closeSync(fd);
    expect(buf.toString('ascii')).toBe('%PDF');
  }, 60000);

  test('exports a valid PPTX', async () => {
    const enrichedSlides = IAD_MANIFEST.slides.map((s, i) => ({
      ...s, index: i + 1, total: IAD_MANIFEST.slides.length
    }));
    const manifest = { ...IAD_MANIFEST, slides: enrichedSlides };
    const pptxPath = path.join(tmpDir, 'test.pptx');
    await exportPptx(manifest, pptxPath);

    expect(fs.existsSync(pptxPath)).toBe(true);
    const fd = fs.openSync(pptxPath, 'r');
    const buf = Buffer.alloc(2);
    fs.readSync(fd, buf, 0, 2, 0);
    fs.closeSync(fd);
    expect(buf[0]).toBe(0x50); // PK zip magic
    expect(buf[1]).toBe(0x4B);
  }, 30000);

  test('both output files have non-trivial size', async () => {
    const html = await renderPresentation(IAD_MANIFEST, { skipEnrich: true });
    const pdfPath = path.join(tmpDir, 'size-check.pdf');
    await exportPdf(html, pdfPath);

    const enrichedSlides = IAD_MANIFEST.slides.map((s, i) => ({
      ...s, index: i + 1, total: IAD_MANIFEST.slides.length
    }));
    const manifest = { ...IAD_MANIFEST, slides: enrichedSlides };
    const pptxPath = path.join(tmpDir, 'size-check.pptx');
    await exportPptx(manifest, pptxPath);

    expect(fs.statSync(pdfPath).size).toBeGreaterThan(10000);
    expect(fs.statSync(pptxPath).size).toBeGreaterThan(5000);
  }, 60000);
});
```

- [ ] **Step 2: Run e2e tests**

Run: `cd presentation-skill && npx jest tests/e2e.test.js --no-coverage --verbose`
Expected: PASS (all 4 tests green — may take ~30s total for PDF generation)

- [ ] **Step 3: Run full suite one final time**

Run: `cd presentation-skill && npx jest --no-coverage`
Expected: All tests pass. Note total count.

- [ ] **Step 4: Commit**

```bash
git add tests/e2e.test.js
git commit -m "test(presentation): end-to-end pipeline test covering HTML, PDF, and PPTX output"
```

---

## Summary

After all tasks are complete, the repository will contain:

```
presentation-skill/
├── skill.md                         # /présentation Claude command definition
├── package.json + package-lock.json
├── .env.example
├── jest.config.js
├── renderer/
│   ├── index.js                     # Orchestrator: manifest → HTML
│   ├── theme.js                     # Theme system + auto-detection
│   ├── templates/
│   │   ├── base.html                # Shell template with CSS var slots
│   │   └── layouts/
│   │       ├── cover.js, metrics.js, chart.js, visual-text.js
│   │       ├── comparison.js, timeline.js, process.js
│   │       ├── orgchart.js, quote.js
│   ├── services/
│   │   ├── logo-sourcer.js          # Multi-strategy logo pipeline (existing)
│   │   └── unsplash.js              # Photo sourcing
│   └── export/
│       ├── pdf.js                   # Puppeteer → PDF
│       └── pptx.js                  # pptxgenjs → PPTX
└── tests/
    ├── theme.test.js, layouts.test.js, renderer.test.js
    ├── unsplash.test.js, logo-sourcer.test.js
    ├── pdf.test.js, pptx.test.js, e2e.test.js
```

To generate a presentation manually:
```bash
cd presentation-skill
node -e "
const { renderPresentation } = require('./renderer/index');
const { exportPdf } = require('./renderer/export/pdf');
const { exportPptx } = require('./renderer/export/pptx');
const manifest = require('./tests/sample-manifest.json');
renderPresentation(manifest).then(html => {
  exportPdf(html, '/tmp/presentation.pdf');
  exportPptx({ ...manifest, slides: manifest.slides.map((s,i) => ({...s, index: i+1, total: manifest.slides.length})) }, '/tmp/presentation.pptx');
});
"
```
