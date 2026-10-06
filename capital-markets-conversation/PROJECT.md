# Capital Markets Conversation

Standalone event system. This document is the proposed design. No tables have been created, no API has been added, and no farewell page has been changed.

Official event name: **Capital Markets Conversation**

---

## 1. Production safety

The Aramco Farewells site and API are already live. This module must not change them.

| Rule | Status |
|---|---|
| Existing HTML, JavaScript, and CSS | Not modified |
| Existing API controllers and services | Not modified |
| Existing `aramcoFarewell*` tables | Not modified |
| Existing `ashraq*` tables | Not modified |
| Existing production rows | Not modified |
| SQL run from this work | None |
| Deploy | None, until you ask |

New work is limited to:

- a new site folder, `capital-markets-conversation/`
- new API files, only after you approve them
- new `cmc*` tables, only after you approve and run the script yourself

---

## 2. What exists today

### Site

| Item | Detail |
|---|---|
| Kind | Static HTML, JavaScript, and CSS. No React, Vue, or other app framework |
| Host | Cloudflare Worker `aramoco-farewell-ceremony-einvitaion` |
| Domain | `https://aramcocorporateevents.com` |
| File rule | `folder/dashboard.html` is served as `/folder/dashboard/` |
| Worker routes that run first | `/wallet/*`, `/qr-card/*`, `/e-badge/*` (farewell wallet and badge only) |

Farewell pages that stay as they are:

- `dashboard.html`
- `scanner.html`
- `seats.html`
- `rsvp.html`
- `emailInvite.html`, `emailInviteEn.html`, `emailInviteVvip.html`
- `confirmAccept.html`, `directRsvp.html`
- `wallet-worker.js`

### API

| Item | Detail |
|---|---|
| Stack | ASP.NET, one Azure App Service |
| Farewell route | `api/AramcoFarewell` |
| Ashraq | Separate controller in the same API. Do not change it |
| JSON | camelCase |
| Login on these routes | None |
| Connection | Existing SQL Server connection `DefaultConnection` |
| Database name | `UDCGameDB` |

Farewell was added beside Ashraq as **new tables and a new controller**. That is the isolation pattern to copy. The farewell seating designer, badge artwork, and Manifa font belong to the Olivier Thorel farewell and are not the Capital Markets design.

### Database conventions already in use

| Convention | Farewell example |
|---|---|
| Engine | SQL Server |
| Prefix | `aramcoFarewell` |
| Primary key | `Id INT IDENTITY` |
| Public guest key | `UniqueId NVARCHAR(64)`, unique |
| Time | `DATETIME2`, stored with `GETUTCDATE()` |
| Status text | `NVARCHAR`, for example `Pending` |
| Seat link | Assignment row: `SeatCode` + `UniqueId` + `SeatedAt` |

### PDF and QR already in the API

| Piece | How it works |
|---|---|
| PDF | `AramcoFarewellInvitePdfService` writes a PDF from JPEG assets. It does not use QuestPDF |
| QR library | `QRCoder` |
| Badge drawing | `System.Drawing` on Windows |
| Farewell badge QR | Guest GUID |
| Farewell invite QR | Public e-badge URL |

### Access today

| Surface | Access |
|---|---|
| Farewell dashboard | Open |
| Farewell scanner | Open |
| Farewell seats | Password checked only in the browser |

---

## 3. Proposed locations

### Pages

**Existing site root**

`c:\ENTOURAGE\PROJECTS\CLIENT\2026\Oliver Farewell Cermony\Aramoco Farewell Ceremony EInvitaion`

**New folder, inside that root**

`capital-markets-conversation\`

**Public URLs after the site is published**

| File | URL |
|---|---|
| `dashboard.html` | `https://aramcocorporateevents.com/capital-markets-conversation/dashboard/` |
| `scanner.html` | `https://aramcocorporateevents.com/capital-markets-conversation/scanner/` |
| `seats.html` | `https://aramcocorporateevents.com/capital-markets-conversation/seats/` |
| `badge.html` | `https://aramcocorporateevents.com/capital-markets-conversation/badge/?id={token}` |

A trailing slash is added by the current Cloudflare setting. `dashboard.html` and `/dashboard` redirect to `/dashboard/`.

Putting files in this folder does not edit farewell pages. A Cloudflare publish of this same worker would upload the new folder together with the farewell site.

### API

**Existing API root**

`C:\ENTOURAGE\PROJECTS\INHOUSE\AI entourage project Hub\Git\PROJECT HUB BACKEND`

**Proposed new files only, after approval**

| File | Role |
|---|---|
| `Controllers/CapitalMarketsConversationController.cs` | Route `api/CapitalMarketsConversation` |
| `Services/CapitalMarketsConversationGuestService.cs` | Guests and import |
| `Services/CapitalMarketsConversationSeatService.cs` | Tables, seats, mark seated |
| `Services/CapitalMarketsConversationBadgeService.cs` | Badge image and PDF |

No edit to `AramcoFarewellController` or Ashraq files.

The new controller would share the existing Azure app and the existing database connection. A separate Azure app is possible if you want a separate publish. That choice is still open.

### SQL script you would run

`Scripts/CreateCapitalMarketsConversation.sql` in the API repo, create-only, reviewed by you. It is not written yet and will not be executed from here.

---

## 4. Folder layout

```text
capital-markets-conversation/
  PROJECT.md                 this document
  dashboard.html             guest list, import, downloads
  scanner.html               QR check-in and Mark as Seated
  seats.html                 table and chair view, live refresh
  badge.html                 guest-facing badge page, if a URL QR is chosen
  assets/
    badge/                   background, logo, fonts you supply
    pdf/                     PDF background assets you supply

API, after approval, outside this folder:
  Controllers/CapitalMarketsConversationController.cs
  Services/CapitalMarketsConversation*.cs
  Scripts/CreateCapitalMarketsConversation.sql
```

No new framework. Pages stay plain HTML and JavaScript, matching the farewell site. Styles for this event live in these new files only.

---

## 5. Feature map

```text
Capital Markets Conversation
        |
        +-- Dashboard
        |     guest list
        |     counts
        |     import preview and confirm
        |     download badge image
        |     download PDF
        |
        +-- Guest record
        |     unique token
        |     status
        |     company, position, and the fields you confirm
        |
        +-- E-badge image
        |     your artwork
        |     guest lines
        |     unique QR
        |
        +-- PDF
        |     same guest
        |     same QR token
        |
        +-- Scanner
        |     scan QR
        |     show guest, table, seat
        |     check-in is stored
        |     Mark as Seated is a separate click
        |
        +-- Seating
              tables and chairs for this event only
              assigned chair stays in the assigned color
              seated chair turns green from the database
              manual refresh and auto refresh
              no full page reload
```

---

## 6. Guest lifecycle

Check-in and seating are different.

```text
Import or create guest
        |
        v
Status: Pending / Invited / Confirmed / Declined / Cancelled
        |
        v
Staff scans QR
        |
        v
Row inserted in cmcCheckIn
Guest is checked in
Guest is NOT seated
        |
        v
If the guest has an assigned seat, staff may click Mark as Seated
        |
        v
SeatedAt is written once
Chair can turn green on every open seating screen
```

| Situation | Result |
|---|---|
| Valid QR, guest has a seat, not seated | Show guest, table, seat, and **Mark as Seated** |
| Valid QR, no seat | **No Seat Assigned**. Button hidden |
| Already seated | **Already Seated**. No second timestamp |
| Two scanners click at once | One write. The other gets already seated |
| Unknown QR | Guest not found |
| Seat refresh fails | Current floor stays on screen. Status shows the failure |

Scanning does not turn a chair green.

---

## 7. Dashboard

Page: `dashboard.html`

Header: **Capital Markets Conversation**

Counts, from the new tables only:

| Count | Meaning |
|---|---|
| Guests | Rows in `cmcGuest` |
| Confirmed | Status Confirmed, if you keep that status |
| Pending | Status Pending |
| Declined | Status Declined |
| Checked in | Guests with at least one `cmcCheckIn` row |
| Seated | Guests whose assignment has `SeatedAt` |
| Not seated | Assigned guests with an empty `SeatedAt` |

Guest table columns, once fields are confirmed:

| Column | Source |
|---|---|
| Name | `cmcGuest.FullName` |
| Company | `cmcGuest.Company` |
| Email | `cmcGuest.EmailId` |
| Status | `cmcGuest.Status` |
| Table | Assigned table name |
| Seat | Assigned seat code |
| Checked in | Yes if a check-in row exists, with last time |
| Seated | Yes if `SeatedAt` is set, with that time |
| Actions | View, edit, badge image, PDF |

Import flow:

```text
Choose file
    -> read rows in the browser or on the API
    -> check required columns
    -> show preview
    -> show invalid rows and duplicates
    -> you confirm
    -> insert into cmcGuest only
```

Preview counts:

```text
Total rows
Valid
Invalid
Duplicates
```

Each problem row shows the row number and the reason, for example missing email or duplicate email. Nothing is inserted until confirm. A bad file can be replaced and previewed again.

Each new guest receives a new `UniqueId`. Tokens are never shared.

---

## 8. E-badge and PDF

Not designed until you supply artwork.

Planned content, subject to your file:

| Piece | Notes |
|---|---|
| Event name | Capital Markets Conversation |
| Guest name | From `cmcGuest` |
| Company | If you want it on the art |
| Position | If you want it on the art |
| QR | Unique token for that guest only |

Badge image and PDF are generated on the API when someone downloads them. They are not stored as a second copy of guest data. The token in the QR is the same token the scanner accepts.

The farewell badge layout, cyan name color, and already-sent farewell QR images are not reused and are not changed.

---

## 9. QR

Proposed token: a GUID in `UniqueId`, same kind of key as farewell, stored only in `cmcGuest`.

Two options, you choose one:

| Option | QR contains | Scanner reads |
|---|---|---|
| Token only | `8f73a9c0-....` | The token |
| Badge URL | `https://aramcocorporateevents.com/capital-markets-conversation/badge/?id={token}` | The token is taken from the address |

The QR does not contain email, phone, or other guest details.

The farewell wireless scanner can be used if it is in keyboard mode. The scanner page will also keep a camera choice. Hardware setup is unchanged.

---

## 10. Scanner

Page: `scanner.html`

```text
Scan
  -> POST check-in
  -> show name, company, position
  -> show status
  -> show table and seat, or No Seat Assigned
  -> Mark as Seated only when a seat exists and SeatedAt is empty
```

After a successful mark:

```text
Guest marked as seated
Seated at: {local time}
Button: Already Seated
```

The button is disabled while the request is in flight so a double click cannot create two seated times.

Check-in history for that guest can be listed under the result. It is the list of scans, not the seated flag.

---

## 11. Seating

Page: `seats.html`

This event does not read `aramcoFarewellSeat`, `aramcoFarewellSeatAssignment`, or `aramcoFarewellSeatMap`.

```text
cmcTable
    cmcSeat
        cmcSeatAssignment -> cmcGuest
```

Example:

```text
Table VIP-01
  A01  John Smith   Seated      green
  A02  David Lee    Not seated  assigned color
  A03  Sarah        Seated      green
```

| Chair state | Color |
|---|---|
| No guest | Existing empty-chair style for this new page |
| Guest assigned, `SeatedAt` empty | Assigned color |
| That same guest has `SeatedAt` | Green |
| Declined, if you want that state | Red, and not treated as seated |

Green is per chair. Another guest at the same table does not turn this chair green.

### Refresh

| Control | Behavior |
|---|---|
| Refresh | `GET` seats and update chair colors. The browser page does not reload |
| Auto refresh | Off, 5s, 10s, 15s, 30s, 60s, 2 min, 5 min |
| Default | 30 seconds, unless you pick another |
| Memory | The chosen interval is stored in this browser only. Seat truth stays on the server |
| Overlap | If a request is still running, the next timer tick is skipped |
| Hidden tab | Timer does not call the API. One refresh runs when the tab is visible again |
| Failure | Chairs stay as last loaded. The status line shows update failed and the last good time |

Several scanners and several seating screens all read the same `cmc*` rows.

### Layout choice still open

| Choice | What you get |
|---|---|
| Simple grid | Tables and chairs from import or from the dashboard. Faster to build |
| Visual floor plan | A designer like the farewell page, with its own `cmc` map table. More work, still isolated |

---

## 12. Proposed tables

Prefix: `cmc`. Database: `UDCGameDB`. Create only. No `ALTER` of an existing production table.

Types follow the farewell style. Fields marked optional wait on your import file and badge art.

### cmcGuest

| Column | Type | Null | Key | Description |
|---|---|---|---|---|
| Id | INT IDENTITY | No | Primary key | Internal id |
| UniqueId | NVARCHAR(64) | No | Unique | QR token. New GUID per guest |
| FullName | NVARCHAR(250) | No | | Guest name |
| EmailId | NVARCHAR(250) | Yes | | Email, if you collect it |
| PhoneNumber | NVARCHAR(50) | Yes | | Mobile, if you collect it |
| Company | NVARCHAR(250) | Yes | | Company |
| Position | NVARCHAR(250) | Yes | | Job title |
| Category | NVARCHAR(100) | Yes | | Guest group, if you use one |
| Status | NVARCHAR(50) | No | | Pending by default |
| CreatedOn | DATETIME2 | No | | UTC create time |
| UpdatedOn | DATETIME2 | Yes | | UTC last edit |

Index: unique `UniqueId`. Optional unique `(EmailId, FullName)` only if you confirm email plus name must be unique.

### cmcCheckIn

| Column | Type | Null | Key | Description |
|---|---|---|---|---|
| Id | INT IDENTITY | No | Primary key | One row per scan |
| UniqueId | NVARCHAR(64) | No | Index | Guest token |
| CheckedInOn | DATETIME2 | No | | UTC scan time |
| Source | NVARCHAR(50) | Yes | | `scanner` |

Index: `(UniqueId, CheckedInOn DESC)`.

### cmcTable

| Column | Type | Null | Key | Description |
|---|---|---|---|---|
| Id | INT IDENTITY | No | Primary key | |
| TableNo | INT | No | | Sort and display number |
| Label | NVARCHAR(100) | No | | Example: `VIP-01` or `Table 1` |

### cmcSeat

| Column | Type | Null | Key | Description |
|---|---|---|---|---|
| Id | INT IDENTITY | No | Primary key | |
| TableId | INT | No | Foreign key to `cmcTable.Id` | |
| SeatCode | NVARCHAR(32) | No | Unique | Example: `A01` |
| SortOrder | INT | No | | Order around the table |

### cmcSeatAssignment

| Column | Type | Null | Key | Description |
|---|---|---|---|---|
| Id | INT IDENTITY | No | Primary key | |
| SeatId | INT | No | Unique foreign key to `cmcSeat.Id` | One guest per chair |
| UniqueId | NVARCHAR(64) | No | Unique | One chair per guest |
| AssignedOn | DATETIME2 | No | | UTC assign time |
| SeatedAt | DATETIME2 | Yes | | Empty until Mark as Seated |

`SeatedAt` is the seated flag. There is no second `IsSeated` column. Empty means not seated.

Relationships:

```text
cmcGuest.UniqueId
    <- cmcCheckIn.UniqueId
    <- cmcSeatAssignment.UniqueId
            -> cmcSeat.Id
                    -> cmcTable.Id
```

If you choose a visual floor plan later, a `cmcSeatMap` table can hold that drawing. It is not in the first schema.

---

## 13. Proposed API

Base: `api/CapitalMarketsConversation`

| Method | Path | Purpose |
|---|---|---|
| GET | `/guests` | List guests with check-in and seat summary |
| POST | `/guests` | Add one guest |
| PUT | `/guests/{uniqueId}` | Edit guest fields and status |
| POST | `/guests/import/preview` | Validate a file. No insert |
| POST | `/guests/import` | Insert the confirmed valid rows into `cmcGuest` only |
| GET | `/guests/{uniqueId}` | One guest |
| POST | `/check-in?uniqueId=` | Record a scan. Does not set `SeatedAt` |
| GET | `/check-in/{uniqueId}` | Scan history for that guest |
| GET | `/seats` | Tables, chairs, assignment, `seatedAt` |
| POST | `/seats/assign` | Put a guest on a chair |
| POST | `/seats/release` | Clear a chair |
| POST | `/seats/mark-seated?uniqueId=` | Set `SeatedAt` if they have a chair and are not already seated |
| GET | `/badge/{uniqueId}` | Badge image |
| GET | `/pdf/{uniqueId}` | PDF download |

Mark seated request is only the guest token. The API finds the chair. The browser cannot send a seat id and a seated flag and have that trusted.

| Case | Response |
|---|---|
| Guest missing | 404, guest not found |
| No chair | 409, this guest does not have an assigned seat |
| First mark | 200, `alreadySeated: false`, `seatedAt` set |
| Already seated, or a second scanner won the race | 200, `alreadySeated: true`, existing `seatedAt` kept |

Import never writes `aramcoFarewellUser` or any Ashraq table.

---

## 14. Screens and devices

| Device | Page | Writes |
|---|---|---|
| Staff laptop | Dashboard | Guests, import, assign seat |
| Door scanner | Scanner | Check-in, mark seated |
| Hall display | Seats | Read only, plus refresh |

All of them use the API. Browser storage is only for the refresh interval and, if you ask for one, a staff gate. It is not the guest list or the seated state.

---

## 15. Security

| Topic | Plan |
|---|---|
| Database password | Stays in the existing API configuration. Not copied into HTML |
| QR contents | Token or token URL only |
| Mark seated | Server looks up the chair from the guest token |
| Secrets in JavaScript | None |

If you want a real login, say so. The farewell dashboard has no server login. Copying that open access, or adding a staff password, is your choice.

---

## 16. Build order after approval

1. You confirm this document and the open items in section 17.
2. SQL script is shown to you. You run it. Tables are create-only.
3. New API files: guests, import, check-in, seats, mark seated.
4. Badge and PDF after artwork arrives.
5. Dashboard, scanner, seats pages in this folder.
6. Test import, QR, check-in, no seat, mark seated, already seated, two scanners, green chair, refresh, failed refresh.
7. Publish the site and the API only when you say to.

---

## 17. Still needed from you

**Database**

- Approve `cmc*` tables in `UDCGameDB`.
- Approve a create-only script that you run.
- Say if the prefix should not be `cmc`.

**Hosting**

- Same domain folder, or a new subdomain.
- Same Azure API with a new controller, or a separate API app.

**Access**

- Dashboard open, password, or real login.
- Scanner open or staff-only.
- Who may import and assign seats.

**Import**

- Excel, CSV, or both.
- A sample file.
- Exact columns.
- What makes a duplicate.

**Status**

- Keep or replace: Pending, Invited, Confirmed, Declined, Cancelled.
- A scan stores a check-in row. Should it also change `Status`?

**Badge**

- Background, logo, size.
- Which lines of guest text, and where the QR sits.

**PDF**

- Invitation, badge, or both.
- Page size.
- Same QR as the badge or not.

**QR**

- Token only, or a public badge URL.
- Wireless scanner, camera, or both.

**Seating**

- How many tables, how many chairs, and the names.
- Import assignments, assign on screen, or both.
- Simple grid or a visual floor plan.

**Event text**

- Date, time, and venue for the badge and PDF.

---

## 18. Safety check

| Check | Result |
|---|---|
| Existing production code modified | No |
| Existing production tables modified | No |
| Existing production data modified | No |
| Existing APIs modified | No |
| SQL executed | No |
| Site or API published | No |

This file is the design. Implementation waits for your approval.
