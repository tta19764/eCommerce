# Frontend Theme System

This document defines the semantic design system, color-token mapping, border-radius scale, icon conventions, and accessibility rules for the Angular application's light and dark themes. It belongs with [[Frontend Architecture]] because component SCSS and global theme variables jointly implement the system.

## Architectural principles

1. **Semantic light and dark themes**
   - Light theme uses a high-contrast corporate palette: `#edf2f7` background, `#ffffff` card surfaces, `#0f172a` primary text, `#4f46e5` primary accents, and `#cbd5e1`/`#94a3b8` borders.
   - Dark theme uses a deep-slate palette: `#020617` background, `#0f172a` card surfaces, `#f8fafc` primary text, `#6366f1` accents, and `#1e293b` borders.
2. **No arbitrary inversions**
   - Component layouts own structural styling in component `.scss` files.
   - Global variables define semantic CSS custom properties in `:root` and `html.light`.
3. **Restrained radius scale**
   - `--radius-sm`: `0.25rem` (4px) for tags, badges, and small chips.
   - `--radius-md`: `0.375rem` (6px) for buttons and form controls.
   - `--radius-lg`: `0.5rem` (8px) for cards, panels, modals, and containers.

## Semantic color tokens

| Token | Light | Dark | Purpose |
| --- | --- | --- | --- |
| `--app-bg` | `#edf2f7` | `#020617` | Global page background |
| `--surface-card` | `#ffffff` | `#0f172a` | Cards, panels, modals, menus |
| `--surface-secondary` | `#f1f5f9` | `#1e293b` | Sub-containers, table headers, chat bubbles |
| `--surface-elevated` | `#ffffff` | `#1e293b` | Sticky headers, floating menus, popovers |
| `--text-primary` | `#0f172a` | `#f8fafc` | Headings, body text, form labels |
| `--text-secondary` | `#334155` | `#94a3b8` | Subtitles, metadata, timestamps |
| `--text-muted` | `#64748b` | `#64748b` | Disabled and placeholder text |
| `--border-subtle` | `#cbd5e1` | `#1e293b` | Inner dividers and table rows |
| `--border-strong` | `#94a3b8` | `#334155` | Input borders and outlines |
| `--accent-primary` | `#4f46e5` | `#6366f1` | Primary actions and active indicators |
| `--accent-hover` | `#4338ca` | `#4f46e5` | Primary-action hover state |
| `--status-success-bg` | `#d1fae5` | `rgba(16, 185, 129, 0.1)` | Success alerts and in-stock badges |
| `--status-success-text` | `#047857` | `#34d399` | Success labels |
| `--status-error-bg` | `#ffe4e6` | `rgba(244, 63, 94, 0.1)` | Error banners and destructive actions |
| `--status-error-text` | `#b91c1c` | `#f87171` | Error messages |

## Usage and extension rules

- Component styles must consume semantic tokens or utilities tied to theme state. Do not hardcode white text on light cards or black text on dark cards.
- Before adding a token, determine whether an existing surface, border, text, accent, or status token expresses the intended meaning.
- Text/background combinations must meet WCAG AA: at least 4.5:1 for normal text and 3:1 for large text.
- Theme switching is owned by the frontend theme service and corresponding root-class/global-variable definitions; local components should not implement independent theme state.

## Icon system

- Do not use emoji as interface icons in buttons, badges, review stars, tabs, or empty states.
- Use inline SVG with a conventional `0 0 24 24` or `0 0 20 20` view box.
- Prefer `fill="none"`, `stroke="currentColor"` so icons inherit semantic foreground color.
- Decorative icons require `aria-hidden="true"`; meaningful icons require an accessible label or descriptive `<title>`.

Related concepts: [[Frontend Architecture]], [[Products]], [[Cart]], [[Orders]], and [[Reviews]].
