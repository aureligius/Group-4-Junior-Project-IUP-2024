# Product

<!-- impeccable:product-schema 1 -->

## Platform

windows (native WPF desktop client talking to an ASP.NET Core Web API)

## Users
General commuters: anyone who travels daily and wants to reduce their carbon footprint, and who is motivated by competing with friends and in community leagues. They open the app around their trips: before or after travelling to log the trip, and in between to check progress, rankings and battles.

## Product Purpose
Carbon Kickers is a gamified carbon-footprint tracker that turns sustainable travel into a sport. A user logs a trip (origin, destination, transport used, and the transport they would normally have taken); the system measures the route, estimates the CO₂ of both options and credits the difference as CO₂ "kicked" out of the atmosphere. Savings convert into Green XP, badges, league tiers and head-to-head battles. Success is a user who keeps logging greener trips because the progress and competition feel worth returning to.

## Positioning
Savings are measured against the ride the user would normally have taken (a baseline trip), not as an absolute footprint, so every greener choice scores points. Those points feed a sports structure (weekly leagues with tiers Bronze to Diamond, PvP challenges between friends) rather than a passive emissions report.

## Operating Context
- University Junior Project (Databases, Object-Based Programming), Group 4 IUP 2024, three-person team: back-end, front-end (WPF client), UI/UX.
- Course deliverables in `Documents/` (W1 brainstorming, H1–H5 worksheets: use case, activity and class diagrams, ERD, database build).
- Presented live to the class as a running desktop app.

## Capabilities and Constraints
- Architecture: WPF client (`CarbonKickers.Client`) → ASP.NET Core Web API (`CarbonKickers.Api`) → PostgreSQL (Neon) via EF Core. The client never touches the database or third-party API keys.
- Built end to end: register/login (JWT), trip logging with Google Places search and Google Routes distance, emissions per transport mode, Green XP per trip, badge unlocking, dashboard (totals and last 7 days), trip history, unlocked badges.
- Not built in the back end yet: leaderboard, weekly leagues, PvP challenges (Arena), joining competition. The client shows these as preview screens running on labelled sample data until endpoints exist.
- Terminology: Green XP, CO₂ kicked/saved, baseline ("what you'd normally take"), league tiers Bronze, Silver, Gold, Platinum, Diamond, the Arena.

## Brand Commitments
- Name: Carbon Kickers. Tagline in use: "Turn sustainability into a sport."
- Team palette (user-supplied): peach #F8D5B9, coral #EEA898, sage #7B8F8F, teal #44706B, forest #0A4842; gold #F2B33D added (user-approved) for XP and rewards only, plus classic league-tier colours.
- Visual inspiration the user made binding: the retro pixel-arcade landing page on branch `feature/Frontend_Skeleton.Aurellia` (commit e209a25). Game-facing screens carry the arcade treatment; task screens (forms, tables) stay calm and readable.

## Evidence on Hand
- No real users, testimonials, rankings or league results exist. Preview screens use synthetic players and must stay labelled as sample data.
- Real data available through the API: the signed-in user's trips, XP, CO₂ saved, daily impact and unlocked badges; transport modes with emission factors.
- Placeholder pixel art from the landing page (`CarbonKickers.Web/wwwroot/images/pixel/` on the feature branch); the UI/UX teammate may replace it.

## Product Principles
1. Every greener trip must visibly pay off: CO₂ kicked and XP earned are shown the moment a trip is saved.
2. Competition is opt-in and motivating, never shaming: rankings celebrate progress.
3. Game where it motivates, plain where it works: logging and history stay fast and readable.
4. Honest numbers: real data is never mixed with sample data without a label.
