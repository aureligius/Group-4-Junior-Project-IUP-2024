---
name: Carbon Kickers
description: A sports-season console for greener travel, as a native Windows (WPF) desktop app.
colors:
  forest: "#0A4842"
  forest-deep: "#073632"
  forest-raised: "#0F564F"
  teal: "#44706B"
  teal-soft: "#E1EBE8"
  sage: "#7B8F8F"
  gold: "#F2B33D"
  gold-hover: "#F6C35E"
  gold-edge: "#8A5E0A"
  coral: "#EEA898"
  peach: "#F8D5B9"
  game-text: "#FFF4E8"
  game-muted: "#B9CCC7"
  cream-ground: "#FBF4EC"
  surface: "#FFFFFF"
  surface-alt: "#FDF1E6"
  border: "#EBDCCC"
  text-muted: "#5C716F"
  chart: "#0F8770"
  danger: "#B3412E"
  danger-soft: "#FBE3DD"
  warning: "#7A4A1E"
  warning-soft: "#FCE7D6"
  tier-bronze: "#B87333"
  tier-silver: "#A8B4BA"
  tier-platinum: "#6CC3C1"
  tier-diamond: "#6FA8F5"
typography:
  display:
    fontFamily: "Press Start 2P"
    fontSize: "40px"
    lineHeight: "48px"
  score:
    fontFamily: "Press Start 2P"
    fontSize: "32px"
    lineHeight: "36px"
  payout-title:
    fontFamily: "Press Start 2P"
    fontSize: "24px"
    lineHeight: "32px"
  headline:
    fontFamily: "Press Start 2P"
    fontSize: "20px"
    lineHeight: "28px"
  title:
    fontFamily: "Press Start 2P"
    fontSize: "12px"
    lineHeight: "20px"
  label-pixel:
    fontFamily: "Press Start 2P"
    fontSize: "11px"
  label-pixel-small:
    fontFamily: "Press Start 2P"
    fontSize: "9px"
  heading:
    fontFamily: "Segoe UI"
    fontSize: "16px"
    fontWeight: 600
  body:
    fontFamily: "Segoe UI"
    fontSize: "14px"
    fontWeight: 400
  label:
    fontFamily: "Segoe UI"
    fontSize: "13px"
    fontWeight: 600
  caption:
    fontFamily: "Segoe UI"
    fontSize: "12px"
    fontWeight: 400
rounded:
  none: "0px"
  control: "4px"
  card: "6px"
  pill: "12px"
spacing:
  inset-tight: "12px"
  inset: "14px"
  gutter: "20px"
  panel: "24px"
  screen-x: "36px"
  screen-top: "32px"
components:
  button-game:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.forest}"
    typography: "{typography.label-pixel}"
    rounded: "{rounded.none}"
    padding: "14px 18px"
  button-game-hover:
    backgroundColor: "{colors.gold-hover}"
  button-game-small:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.forest}"
    typography: "{typography.label-pixel-small}"
    padding: "9px 12px"
  button-game-ghost:
    backgroundColor: "transparent"
    textColor: "{colors.game-text}"
    typography: "{typography.label-pixel}"
    rounded: "{rounded.none}"
    padding: "14px 18px"
  button-primary:
    backgroundColor: "{colors.teal}"
    textColor: "{colors.surface}"
    typography: "{typography.body}"
    rounded: "{rounded.control}"
    padding: "10px 18px"
  button-primary-hover:
    backgroundColor: "{colors.forest}"
  button-secondary:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.forest}"
    rounded: "{rounded.control}"
    padding: "10px 18px"
  button-secondary-hover:
    backgroundColor: "{colors.surface-alt}"
  button-link:
    textColor: "{colors.teal}"
    typography: "{typography.label}"
  button-list-row:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.forest}"
    rounded: "{rounded.control}"
    padding: "8px 12px"
  button-list-row-hover:
    backgroundColor: "{colors.surface-alt}"
  button-list-row-pressed:
    backgroundColor: "{colors.teal-soft}"
  arcade-panel:
    backgroundColor: "{colors.forest}"
    textColor: "{colors.game-text}"
    rounded: "{rounded.none}"
    padding: "24px"
  game-inset:
    backgroundColor: "{colors.forest-deep}"
    rounded: "{rounded.none}"
    padding: "12px 14px"
  card:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.forest}"
    rounded: "{rounded.card}"
    padding: "24px"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.forest}"
    typography: "{typography.body}"
    rounded: "{rounded.control}"
    padding: "8px 10px"
  nav-item:
    backgroundColor: "transparent"
    textColor: "{colors.game-muted}"
    typography: "{typography.body}"
    padding: "10px 12px"
  nav-item-active:
    backgroundColor: "{colors.forest}"
    textColor: "{colors.peach}"
  game-tab:
    textColor: "{colors.game-muted}"
    typography: "{typography.label-pixel-small}"
    padding: "9px 12px"
  game-tab-active:
    textColor: "{colors.gold}"
  xp-bar:
    backgroundColor: "{colors.forest-deep}"
    height: "22px"
  xp-bar-fill:
    backgroundColor: "transparent"
    height: "22px"
  preview-tag:
    textColor: "{colors.danger}"
    padding: "3px 8px"
  pill:
    rounded: "{rounded.pill}"
    padding: "6px 10px 5px"
  tooltip:
    backgroundColor: "{colors.forest-deep}"
    textColor: "{colors.game-text}"
    typography: "{typography.caption}"
    padding: "6px 10px"
---

# Design System: Carbon Kickers

All tokens live in `CarbonKickers.Client/Themes/Theme.xaml` (merged in `App.xaml`); sprites and the pixel icon set live in `CarbonKickers.Client/Themes/PixelArt.xaml`. XAML resource keys are given next to each value so new screens reuse the resource rather than retyping the hex.

## Overview

**Creative North Star: "The Season Console"**

Carbon Kickers is a sports-season console for greener travel. The app runs in two materials. Screens that reward and rank (player scoreboard, trip result, leaderboard, weekly league, arena, badges) are dark forest arcade "screens" with a stepped, corner-notched edge, faint scanlines and Press Start 2P type. Screens that take input (log in, log a trip, trip history) are calm cream paper with white work surfaces and Segoe UI. The two materials sit together in one product: the dashboard is one full-width arcade scoreboard band over a calm paper row, and Log a Trip is a paper form that gives way to a full arcade TRIP COMPLETE screen once the trip is saved.

Density is that of a working desktop tool: one 1200x760 window (minimum 1024x700) with a fixed forest sidebar rail and a single scrolling content column. Pixel art is hand-drawn, crisp and integer-scaled (aliased edges), used as small sprites and a pixel icon set, never as decoration filling space. Gold is the reward colour and is kept scarce. Motion is a two-part system and only answers a change in value: the segmented gold XP bar fills when progress changes, and saving a trip plays a short (about 2 s), cancellable TRIP COMPLETE payout. Both settle and stop; nothing idles or loops.

It refuses the generic green eco-dashboard: white cards with leaf icons and donut charts.

**Key Characteristics:**
- Two materials, one rule: arcade forest for reward and rank, cream paper for input.
- Stepped, notched edges (square pixel corners) on every arcade surface and game button; soft 4-6px radii only on paper.
- Pixel face for titles, scores, game buttons and tabs; Segoe UI for anything read at length or typed.
- Gold means XP, rewards and the primary game action, nothing else; one gold button per screen.
- Crisp, aliased pixel sprites and icons at integer scales.
- Two motions only (XP bar fill, TRIP COMPLETE payout), both triggered by a value change; no idle or looping animation.

## Colors

A deep forest-and-cream palette from the team's colours, with gold as the scarce reward accent and coral as the rival/highlight voice.

### Primary
- **Season Forest** (`Color.Forest`, `Brush.Game`, `Brush.Text`): the arcade panel surface and the primary text colour on paper. One colour carries both worlds, which is why the light and dark screens read as one product.
- **Deep Pitch** (`Brush.GameDeep`): the sidebar rail, the notched panel edge, inset rows inside arcade panels, the scoreboard divider and road strip, tooltips, and the XP bar trough.
- **Raised Pitch** (`Brush.GameRaised`): the highlighted row (the player's own) in rank lists and pressed states on dark.

### Secondary
- **Trophy Gold** (`Color.Gold`, `Brush.Gold`): XP numbers, the XP bar segments, the main game action button, active nav icon, active game tab, gold-tier chips, and the 2px frame of an unlocked-badge card. Hover lifts to `Brush.GoldHover`; the button edge is `Brush.GoldEdge`.
- **Rival Coral** (`Color.Coral`, `Brush.Coral`): rivals and highlights: the "KICKERS" half of the wordmark, the preview tag outline, the keyboard focus ring, text selection, relegation-zone marks, the rival half of the arena `VsBar`, and short caution notes on dark (the route-estimate note on the trip result).

### Tertiary
- **Pixel Peach** (`Color.Peach`, `Brush.Peach`): heading type on dark (`Text.GameHeading`, the TRIP COMPLETE! title), the active nav label, highlighted figures in arcade copy (kg kicked, trip count), the "CARBON" half of the wordmark, and, as `Brush.XpSegmentsGained`, the segments of XP just earned before the gold fill catches up.
- **Tier colours** (`Brush.Tier.Bronze` / `Silver` / `Gold` / `Platinum` / `Diamond`): only on 10px tier squares and tier chips, resolved at runtime via `FindResource($"Brush.Tier.{name}")`.

### Neutral
- **Cream Ground** (`Brush.Ground`): the window background behind every content column and the right half of the login window.
- **Paper White** (`Brush.Surface`): cards, list rows, inputs, data grid, dropdowns.
- **Warm Paper** (`Brush.SurfaceAlt`): hover rows and list rows, data grid header, secondary button hover.
- **Paper Edge** (`Brush.Border`): 1px card, list-row, input and grid lines; the chart baseline.
- **Moss Muted** (`Brush.TextMuted`): subtitles, helper text, sublines, the list-row chevron at rest, chart day labels on paper.
- **Sage** (`Brush.Sage`): input hover border; at 30-60% alpha, the scrollbar thumb.
- **Arcade Ink** (`Brush.GameText`) and **Arcade Muted** (`Brush.GameMuted`): primary and secondary text on dark; muted also draws the XP bar frame, ghost-button edges and links on dark.
- **Calm Teal** (`Brush.Primary`, soft `Brush.PrimarySoft`): paper-side links, input focus border, calm primary buttons, selected rows, list-row icons and the list-row hover border and chevron; soft is the list-row pressed fill.
- **Chart Green** (`Brush.Chart`): the 7-day CO2 bars on paper only.
- **Status** (`Brush.Danger` / `DangerSoft`, `Brush.Warning` / `WarningSoft`): errors and warnings on paper. Danger is also the preview-tag text, because the tag sits on cream or white where coral text is too pale to read.

### Named Rules
**The Gold Is Earned Rule.** Gold appears only on XP, rewards, the primary game action of a screen, and the active state of game navigation. If gold is on something that is not a reward or the one main move, it is wrong.

**The One Gold Button Rule.** A screen shows at most one gold `Button.Game` at a time. Secondary game moves are `Button.GameGhost`; joins and row actions are `Button.GameSmall` on dark or a compact `Button.Primary` on paper.

**The Tier Chip Rule.** Tier colours appear only as tier markers (the 10px square or a chip). They never tint panels, text blocks or buttons.

## Typography

**Display / Game Font:** Press Start 2P (bundled, `Font.Pixel`, SIL OFL), rendered with `TextFormattingMode=Display` and `TextRenderingMode=Aliased` via `Text.Pixel`.
**Body Font:** Segoe UI (`Font.Body`, set on the window).

**Character:** A loud 8-bit face that only speaks in short bursts (titles, numbers, button verbs), against a quiet native Windows sans that carries every sentence, label and input.

### Hierarchy
- **Display** (pixel, 40px / 48px): the login wordmark, and the two payout figures on TRIP COMPLETE (gold "+XP", peach kg).
- **Score** (pixel, 32px / 36px): the big gold Green XP figure on the dashboard scoreboard.
- **Payout title** (pixel, 24px / 32px, peach): TRIP COMPLETE! only. The unlocked-badge title on the same screen is pixel 16px / 24px (gold badge name, peach "UNLOCKED!").
- **Headline** (`Text.ScreenTitle`, pixel, 20px / 28px, forest): one per screen, at the top-left of the content column.
- **Title** (`Text.GameHeading`, pixel, 12px / 20px, peach): headings inside arcade panels, uppercase (COMPETE, TIERS, SILVER LEAGUE) and the player name on the scoreboard.
- **Pixel label** (pixel, 11px): game buttons, XP and ranks in lists. Small variant 9px for `Button.GameSmall`, `GameTab` and the sidebar XP.
- **Heading** (`Text.Heading`, Segoe UI SemiBold 16px, forest): card titles on paper.
- **Body** (Segoe UI 14px): subtitles (`Text.Subtitle`, muted), inputs, nav items, buttons on paper, list-row titles (SemiBold), payout captions on dark.
- **Label** (`Text.Label`, Segoe UI SemiBold 13px): form field labels. `Text.Muted` / `Text.GameMuted` / `Text.Error` share 13px regular.
- **Caption** (Segoe UI 12px): grid headers, chart labels, list-row sublines, helper notes, tooltips.

### Named Rules
**The Short Burst Rule.** Press Start 2P is for titles, numbers, button verbs and tab names: a few words at most. Any sentence, description, form label, helper or error is Segoe UI.

**The Nine Pixel Floor Rule.** The pixel face stays at 9px or larger and always goes through `Text.Pixel` (or a style based on it) so it renders aliased and crisp.

## Layout

One window: a fixed 232px forest-deep sidebar rail on the left (wordmark, seven destinations with a "Compete" group label, player chip and log out pinned to the bottom), and a content host on cream to the right. Screens are a single `ScrollViewer` with a 36px side margin, 32px top and 36px bottom. The dashboard (24px top, 16px bottom) and Log a Trip (24px top and bottom) run tighter so their main content lands in the first viewport. Each screen opens with the pixel screen title and a one-line Segoe subtitle 8px below, then content 16-24px below that.

Content is built from rows and two-column grids with a 20px gutter, using proportional star widths. The dashboard is one full-width `ArcadePanel` scoreboard (28px sides, 20px top, no bottom padding so its 36px scenery strip runs edge to edge along the bottom), split by a 2px Deep Pitch divider into a flexible player column and a fixed 260px action column; 12px below it sits a calm paper row (1.2* / 1*) of the 7-day chart card and the Compete card. Log a Trip is a single full-width column holding either the form (From and To side by side, then the two transport selects) or the payout screen, never both. Panels and cards pad 24px (22px/20px on the dashboard row); inset rows inside arcade panels pad 12px vertical and 14px horizontal and stack 8px apart. Layout targets 1200x760 and must hold at the 1024x700 minimum (no horizontal scroll). The login window uses the same size, split into an arcade half (scanlines, skyline, bike sprite) and a paper half with the form card.

### Named Rules
**The One Scoreboard Rule.** The dashboard carries exactly one dark band: the scoreboard holding identity, tier, XP, stats, the gold LOG A TRIP and the latest badges. Everything below it (chart, Compete) is paper. Do not add a second arcade panel to the dashboard.

## Elevation & Depth

The system is flat: no drop shadows or blur anywhere. Depth comes from tonal stacking and pixel geometry. On dark, Deep Pitch insets sit inside the Forest panel, and the panel itself is framed by a 6px Deep Pitch edge whose corners are notched out. On paper, depth is a 1px Paper Edge border on white over cream. The only "lift" is the game button's 4px bottom shade (black at 18% alpha), which moves to the top when pressed while the button drops 2px, like a physical arcade key.

### Named Rules
**The No Soft Shadow Rule.** Never add `DropShadowEffect` or blur. Depth is tone (forest, deep, raised) or a stepped pixel edge.

## Shapes

Two form languages, matched to the two materials. Arcade surfaces are square and stepped: `ArcadePanel` draws four 6px edge bars that stop 6px short of each corner, leaving a notch; `Button.Game` does the same at 4px, `Button.GameGhost` at 2px, and `XpBar` at 3px. Insets, the XP trough and tier squares are hard-edged rectangles (0 radius). Paper surfaces are gently rounded: cards 6px, list rows, inputs, dropdowns and calm buttons 4px, pills 12px, scrollbar thumbs 3px. Chart bars are square-cornered even on paper. Pixel geometry (sprites, icons, the combo and list-row chevrons) is drawn on a whole-pixel grid with `RenderOptions.EdgeMode="Aliased"` and `SnapsToDevicePixels`.

### Named Rules
**The Notched Corner Rule.** Anything on an arcade surface that frames or presses gets the stepped corner notch, not a radius.

## Components

### Buttons
Tactile on dark, calm on paper.
- **Game action** (`Button.Game`): gold face, forest pixel text 11px, 4px Gold Edge frame with notched corners, 14px/18px padding. Hover lightens the face to Gold Hover; press drops 2px and flips the shade; disabled at 50% opacity. One per screen, for the main move (LOG A TRIP, SAVE TRIP, LOG ANOTHER TRIP, SEND CHALLENGE, LOG IN). It is used on paper too when that action is the screen's game move (SAVE TRIP on the form card).
- **Game ghost** (`Button.GameGhost`): transparent, Arcade Ink pixel text, 2px Arcade Muted notched frame; hover adds a 10% white wash, press a 20% black wash. Secondary moves on dark (BACK TO DASHBOARD beside the gold button on the payout).
- **Small** (`Button.GameSmall`, `Button.GameGhostSmall`): 9px pixel text, 9px/12px padding, for row actions on dark: accept/decline, BADGES on the scoreboard's badge strip, JOIN COMPETITION and JOIN WEEKLY LEAGUE on the rank-screen join banners.
- **Calm primary / secondary** (`Button.Primary`, `Button.Secondary`): Segoe SemiBold 14px, 4px radius, 10px/18px padding; primary is Calm Teal going to Forest on hover; secondary is white with Paper Edge, going to Warm Paper with a teal border on hover. A compact primary (6px/16px padding, 13px) is the Compete card's "Join".
- **Link** (`Button.Link`): Calm Teal Segoe SemiBold 13px, no padding, Forest on hover. On dark it takes Arcade Muted ("View trip history" under LOG A TRIP).
- **Focus:** every button and nav item uses `FocusRing`: a 2px coral rectangle 3px outside the control.

### List Rows
`Button.ListRow`: a full-width clickable row on a paper card. White, 1px Paper Edge, 4px radius, 8px/12px padding, 6px between rows, holding an 18px aliased pixel icon in Calm Teal, a Segoe SemiBold 14px title, a 12px Moss Muted subline, and a 6x10 aliased pixel chevron (Moss Muted) at the right. Hover fills Warm Paper and turns the border and chevron Calm Teal; pressed fills Teal Soft. Used for the dashboard's Compete destinations (leaderboard, weekly league, arena).

### Chips and Tags
- **Tier marker:** a 10px square in the tier colour followed by the tier name in Segoe 13px.
- **Preview tag** (`PreviewTag`): 1px Rival Coral outline, 3px/8px padding, tooltip explaining the sample data, with "Preview · sample data" in Segoe `Brush.Danger` text (12px beside a screen title, 11px in a card header). Sits top-right of any screen or card running on sample data. Every shipped tag sits on cream or white.
- **Pill** (`Pill`): 12px radius, 6/10/5 padding, for status (won/lost, tier).

### Cards / Containers
- **Arcade panel** (`ArcadePanel`, a `ContentControl` style): Forest fill, tiled 4px scanlines at 5% white, 6px Deep Pitch notched edge, 24px padding, Arcade Ink foreground.
- **Game inset** (`GameInset`): Deep Pitch block, 12px/14px padding, 0 radius; rows, rank entries, join banners, the latest-badges strip (24px medals only; titles live in tooltips beside a short count/latest summary), the trip result's route-details footer, and, with a 2px gold border, the unlocked-badge card.
- **Card** (`Card`): white, 1px Paper Edge, 6px radius, 24px padding. Forms, the CO2 chart, the dashboard Compete card.
- **Scenery strip:** a 36px band along the bottom edge of the dashboard scoreboard: the pixel skyline tiled at 55% opacity, a 14px Deep Pitch road with a dashed peach centre line, and integer-scaled bike and tree sprites. Static.
- **Chart bars:** 24px-wide Chart Green rectangles, square-cornered, on a 1px Paper Edge baseline, value label above in 12px SemiBold, day labels below in 12px muted.

### Inputs / Fields
- **Style:** TextBox, PasswordBox and ComboBox on white, 1px Paper Edge, 4px radius, Segoe 14px, 8px/10px padding (combo reserves 32px right for a pixel chevron). Selection is coral.
- **Hover / Focus:** hover turns the border Sage; keyboard focus turns it Calm Teal at 2px (inputs suppress the external focus ring; combos keep it).
- **Error / Disabled:** errors are a `Text.Error` line (Danger, 13px) under the field or form; disabled inputs drop to 60% opacity.
- **Suggestions:** `SuggestionList` under place-search boxes: white, 1px border, Warm Paper hover, Teal Soft selected.

### Navigation
- **Sidebar** (`NavItem`, a RadioButton style): 18px aliased pixel icon from `PixelArt.xaml` via `Tag`, Segoe 14px label, 12px/10px row padding. Rest is Arcade Muted on Deep Pitch; hover fills Forest with Arcade Ink; active fills Forest with a Peach label and a Gold icon.
- **Game tabs** (`GameTab`): pixel 9px, muted at rest, Arcade Ink on hover, gold text with a 3px gold underline when checked (leaderboard period).

### Data
- **Trip history grid:** white, horizontal Paper Edge lines only, 56px rows, alternating #FFFCF8, Warm Paper column header in muted 12px SemiBold, Teal Soft selection.
- **Rank rows:** Game Inset rows with the rank in pixel 12px, name in Segoe, XP in gold pixel 11px, right-aligned; the player's row on Raised Pitch.
- **Scrollbar:** 10px, transparent track, Sage thumb at 30% (60% on hover), 3px radius.
- **Tooltip:** Deep Pitch, Arcade Ink 12px, 10px/6px padding, max 320px wide.

### XP Bar (signature)
`XpBar`: 22px tall, 3px Arcade Muted notched frame around a Deep Pitch trough with 3px inner padding; the fill comes from `Foreground`, by default `Brush.XpSegments` (14px gold segments with 4px gaps). On the dashboard it fills from 0 to the player's progress over 700ms with an exponential ease-out (exponent 5). The arena's head-to-head `VsBar` (12px, flat fill on Forest) is a plain local variant and does not replace it.

**Gained-XP layering.** To show XP earned by one trip, stack two bars in one cell: an `XpBar` with `Foreground="{StaticResource Brush.XpSegmentsGained}"` (peach segments) set to the new total, and an `XpBarFill` over it (fill only, no frame, 6px inset, not hit-testable) in gold set to the banked XP. The gold then animates up to cover the peach.

### Trip Complete payout (signature)
When a trip is saved, the page header and form collapse and the `ArcadePanel` result screen takes their place (28px/20px padding): the TRIP COMPLETE! title, a muted route summary, the two display-size figures, the layered tier bar with its progress line, any unlocked-badge card (70px medal), then LOG ANOTHER TRIP (gold) beside BACK TO DASHBOARD (ghost), and the route details as a quiet inset footer. The sequence runs about 2 s:
- The panel fades in over 200ms while scaling 0.96 to 1 over 250ms (ease-out, exponent 5).
- "+XP" and kg count up in 12 chunky steps of 55ms.
- The tier bar's gold fills from banked to new XP over 600ms (ease-out, exponent 5). On a tier-up it first fills the old tier to 100 over 300ms, then resets and fills the new tier.
- An unlocked-badge card, whose space is reserved from the start so nothing below moves, pops 0.6 to 1.08 to 1 over 380ms with a 150ms fade, while three `Sprite.Sparkle` images twinkle once (opacity 0 to 1 to 0.7), staggered 80ms apart.
- Focus lands on LOG ANOTHER TRIP. The sequence cancels cleanly if the player logs another trip or leaves.
- When `SystemParameters.ClientAreaAnimation` is false, the payout skips the fade, scale, count-up, fill and pop and shows the final state at once.

### Named Rules
**The Two Motions Rule.** The system has exactly two motions: the XP bar fill and the TRIP COMPLETE payout. Both are triggered by a change in value, play once, settle, and can be interrupted; celebration motion respects the Windows animation setting. Everything else on screen is still.

## Do's and Don'ts

### Do:
- **Do** put reward and ranking content in an `ArcadePanel` and input content in a `Card`, and let both sit together on one screen; on the dashboard, keep to the one scoreboard band.
- **Do** reference brushes and styles by key (`Brush.Gold`, `Text.GameHeading`, `Button.Game`) instead of hex literals.
- **Do** render every pixel sprite, icon and pixel-face text aliased and at integer scales (`RenderOptions.EdgeMode="Aliased"`, `Text.Pixel`).
- **Do** give each screen exactly one pixel screen title and one Segoe subtitle.
- **Do** mark sample-data screens and cards with the coral-outlined `PreviewTag` (Danger text on light).
- **Do** keep a 2px coral `FocusRing` on every focusable control.
- **Do** use `Button.ListRow` for navigational rows on a paper card, and keep one gold `Button.Game` per screen.
- **Do** skip celebration motion when `SystemParameters.ClientAreaAnimation` is false, showing the final values at once.

### Don't:
- **Don't** use gold for anything other than XP, rewards, the main game action, or active game navigation.
- **Don't** set sentences, labels, helper text or input values in Press Start 2P.
- **Don't** add drop shadows, blur, gradients, or rounded corners on arcade surfaces.
- **Don't** add leaf icons, donut charts or the generic white-card eco-dashboard look.
- **Don't** add idle or looping animation, or a third kind of motion; motion is reserved for the XP bar fill and the TRIP COMPLETE payout.
- **Don't** use tier colours outside tier markers.
