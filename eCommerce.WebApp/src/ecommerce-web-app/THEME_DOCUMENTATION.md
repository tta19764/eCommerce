# Enterprise eCommerce Theme & Semantic Color System Documentation

This document defines the complete semantic design system, color token mapping, border-radius scale, and accessibility rules for both Light and Dark themes across the application.

---

## 1. Architectural Principles

1. **Clean Semantic Split**:
   - **Light Theme**: Built as an intentional, high-contrast, clean corporate visual system (`#edf2f7` soft slate base background, `#ffffff` card surfaces, `#0f172a` primary text, `#4f46e5` primary indigo accents, and `#cbd5e1` / `#94a3b8` strong border dividers).
   - **Dark Theme**: Deep slate aesthetic (`#020617` rich dark background, `#0f172a` card surfaces, `#f8fafc` primary text, `#6366f1` indigo accents, and `#1e293b` subtle borders).

2. **No Arbitrary Inversions**:
   - Component layouts retain ownership of structural CSS inside their component SCSS stylesheets (`.scss`).
   - Global variables define semantic CSS custom properties in `:root` and `html.light`.

3. **Restrained Border-Radius Scale**:
   - Eliminates excessive `rounded-2xl` and default pill-shaped buttons.
   - **Base Radius (`--radius-sm`)**: `0.25rem` (4px) for tags, badges, and small chips.
   - **Control Radius (`--radius-md`)**: `0.375rem` (6px) for buttons, text inputs, select dropdowns, and textareas.
   - **Surface Radius (`--radius-lg`)**: `0.5rem` (8px) for cards, panels, modals, and container wrappers.

---

## 2. Semantic Color Token Matrix

| Token Name | Light Theme Value | Dark Theme Value | Purpose & Target Components |
| :--- | :--- | :--- | :--- |
| `--app-bg` | `#edf2f7` | `#020617` | Global page body background |
| `--surface-card` | `#ffffff` | `#0f172a` | Cards, panels, modals, dropdown menus |
| `--surface-secondary` | `#f1f5f9` | `#1e293b` | Sub-containers, table headers, chat bubbles |
| `--surface-elevated` | `#ffffff` | `#1e293b` | Sticky headers, floating menus, popovers |
| `--text-primary` | `#0f172a` | `#f8fafc` | Main headings, body text, form labels |
| `--text-secondary` | `#334155` | `#94a3b8` | Subtitles, metadata, timestamps |
| `--text-muted` | `#64748b` | `#64748b` | Disabled text, placeholding text |
| `--border-subtle` | `#cbd5e1` | `#1e293b` | Card inner dividers, table rows |
| `--border-strong` | `#94a3b8` | `#334155` | Input borders, container outlines |
| `--accent-primary` | `#4f46e5` | `#6366f1` | Primary action buttons, active tab indicators |
| `--accent-hover` | `#4338ca` | `#4f46e5` | Primary button hover state |
| `--status-success-bg` | `#d1fae5` | `rgba(16, 185, 129, 0.1)` | Success notification alerts, in-stock badges |
| `--status-success-text` | `#047857` | `#34d399` | Success text labels |
| `--status-error-bg` | `#ffe4e6` | `rgba(244, 63, 94, 0.1)` | Error notification banners, delete buttons |
| `--status-error-text` | `#b91c1c` | `#f87171` | Error text messages |

---

## 3. Usage Rules & Extending the Token System

- **Rule 1**: Component stylesheets must consume semantic tokens or utility classes tied to theme state. Never hardcode `#ffffff` text on light cards or `#000000` text on dark cards.
- **Rule 2**: When a new component requires a dedicated visual state, inspect whether `--surface-card`, `--surface-secondary`, or `--border-strong` applies before adding a new token.
- **Rule 3**: Accessibility requirement — All text vs background combinations must achieve a minimum contrast ratio of 4.5:1 for standard text and 3:1 for large headings (WCAG AA standard).

---

## 4. Icon System Standard

- **No Emojis**: Emojis are strictly banned from UI elements (buttons, badges, review stars, tabs, empty states).
- **SVG Standard**: All icons use inline SVGs with standard viewBox (`0 0 24 24` or `0 0 20 20`), `fill="none" stroke="currentColor"`, and explicit `aria-hidden="true"` or descriptive `<title>` tags for accessibility.
