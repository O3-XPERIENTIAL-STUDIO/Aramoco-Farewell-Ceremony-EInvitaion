# Aramco Farewells — project features

E-invitation, RSVP, e-badge, Apple Wallet, check-in, and seating for the Olivier Thorel farewell ceremony.

Public site: `https://aramcocorporateevents.com`  
API: Azure App Service, route prefix `api/AramcoFarewell`

This file describes the farewell project as it is built. It is not a second event.

---

## 1. What the system does

Staff manage guests in a dashboard. Each guest gets a personal invite link. The guest accepts or declines. An acceptance shows an e-badge with a check-in QR, a download, and an Apple Wallet pass. At the door, staff scan that QR. On the seating plan, staff assign chairs and can mark a guest as actually seated.

```text
Dashboard
    |
    +-- create / edit guests and groups
    +-- send invite email
    +-- download invite card, e-badge image, e-badge PDF
    |
    v
Guest opens invite link
    |
    +-- Accept  -> thank-you, QR, e-badge, Apple Wallet
    +-- Decline -> recorded, no badge
    |
    v
Door scanner
    |
    +-- check-in recorded
    +-- assigned seat shown
    +-- Mark as Seated is a separate click
    |
    v
Seating plan
    |
    +-- chairs, tables, guest names
    +-- yellow = assigned, green = seated, red = declined
```

Ashraq is a different event in the same API. Its tables, routes, and pages are not part of this guest flow.

---

## 2. Event facts used in the product

| Item | Value |
|---|---|
| Event | Olivier Thorel Farewell Ceremony |
| When | Monday 5 October 2026, 11:00 a.m. |
| Where | Plaza Conference Center, Dhahran |
| Guest language | English on guest pages |
| Guest category in use | General |
| Accept control | Left |
| Decline control | Right |
| Invite greeting | `Dear {FullName},` |
| Position line | Under the name. Hidden when empty |

---

## 3. How it is hosted

| Layer | What it is |
|---|---|
| Site | Static HTML, JavaScript, and CSS. No React or other app framework |
| Site host | Cloudflare Worker `aramoco-farewell-ceremony-einvitaion` |
| Domain | `aramcocorporateevents.com` and `www` |
| Page URLs | `page.html` is served as `/page/`. A folder file `folder/page.html` is `/folder/page/` |
| Worker first | `/wallet/*`, `/qr-card/*`, and `/e-badge/*` are handled by `wallet-worker.js` before static files |
| API | ASP.NET on Azure. JSON is camelCase |
| Database | SQL Server `UDCGameDB`. Farewell tables start with `aramcoFarewell` |
| Publish | The site and the API are published separately. Guests do not see a change until the matching side is published |

There is no server login on the farewell API. The seating page asks for a password that is checked only in the browser.

---

## 4. Guest journey

### Invite

The guest link is:

`https://aramcocorporateevents.com` plus the RSVP path and the guest `uniqueId`.

The invite page (`emailInvite.html`, version 8 artwork) shows:

- The guest name in white on the `#313131` band
- `Dear {FullName},`
- Position under the name in Manifa Regular Italic, white, when a position exists
- Accept on the left, Decline on the right

Artwork lives in `assets/emailInvitation/emailInvite-v8/`.

### RSVP

`rsvp.html` is English only. The Aramco logo sits fixed at the top right, outside the page card, so it stays on screen while the card moves.

Accept or Decline does not call the API until a real button click. A synthetic click is ignored. The confirm button label is Accept or Decline.

After accept, the page shows:

- Dear {name}
- Thank-you copy and a non-transferable note
- A QR code whose value is the e-badge URL: `https://aramcocorporateevents.com/e-badge/{uniqueId}`
- “Please scan the QR Code above…”
- Download E-Badge
- Add to Apple Wallet
- Entry note, Corporate Protocol, and Warm regards

Decline is stored as the guest status. It does not issue a badge.

`directRsvp.html` and `confirmAccept.html` support the same RSVP path from a direct link.

### E-badge

`/e-badge/{uniqueId}` is a page from `wallet-worker.js`. The badge image comes from the API.

The badge image shows:

- Background `badge-bg.png` (402×718), drawn at 3×
- Guest name in Manifa Bold, color `#00B0F0`, one line, shrinking for long names
- A check-in QR. The QR payload is the guest GUID, not a link back to the badge

The on-screen RSVP QR and the printed badge QR are different on purpose:

| QR | Encodes |
|---|---|
| RSVP / invite QR | `https://aramcocorporateevents.com/e-badge/{uniqueId}` |
| Badge image QR | The `uniqueId` GUID only |

### Apple Wallet

`/wallet/{uniqueId}` is a landing page. `/wallet/{uniqueId}.pkpass` downloads the pass.

Pass type: `pass.com.entourage.aramcofarewell`  
Team: `UT3LHA922Q`

Pass fields: Oliver Thorel Farewell Ceremony, Monday October 5 2026, 11:00 a.m., Plaza Conference Center Dhahran, guest name. Barcode value is the `uniqueId`. Background is white, text is dark, labels are cyan. The pass cannot embed the Manifa font.

Each pass download is logged as Wallet pass sent. Repeated lines at the same minute come from the browser prefetching the file, not from the guest adding the pass twice. The system cannot know if the pass was actually saved to Wallet.

---

## 5. Staff dashboard

File: `dashboard.html`  
Font on this page only: Inter.

### Guest list

Guests are listed in groups. Each guest card can show status, company, seat, invite link, and activity.

Per guest:

| Action | What it does |
|---|---|
| Download invite-card | PDF of the invitation from `GET /invite-pdf/{uniqueId}` |
| E-Badge image | JPEG badge from the API |
| E-Badge PDF | The same JPEG wrapped into a one-page PDF in the browser. The API image is not regenerated |
| Send invite | Queues the invitation email |
| Copy invite link | Personal `aramcocorporateevents.com` link, including position when present |
| Status | Accept, decline, or pending |
| Edit / delete | Update or remove that guest |

The invite link includes `&position=` so the position line on the invite matches the guest.

### Groups

Staff can create groups, move guests, and act on a whole group:

- Invite PDFs (ZIP)
- E-badge images (ZIP)
- E-badge PDFs (ZIP)
- RSVP report for that group (built in the browser as a spreadsheet)
- Send unsent invites
- Delete the group

### Bulk download

Staff paste email addresses. Up to 250 guests per ZIP.

| Button | Result |
|---|---|
| Download PDFs (ZIP) | Invite PDFs from the API |
| Download E-Badges (ZIP) | Badge JPEGs from the API |
| Download E-Badge PDFs (ZIP) | The JPEG ZIP is unpacked in the browser, each image is wrapped in a PDF, and a new ZIP is saved |

Staff downloads of the badge image from a guest card are not written as “e-badge downloaded” activity. That log line is only when the badge URL is opened with `?download=1`.

### Email

Invitation mail uses the v8 template. Sending is queued. The dashboard can show queue status. SMS send exists on the API as a separate action.

Activity stored per guest includes invite sent, reminders, status changes, name changes, check-in, wallet pass sent, and e-badge download when `?download=1` is used.

---

## 6. Door scanner

File: `scanner.html`

### Choosing a device

Before scanning, staff can open Settings (gear, top right) and pick:

- A camera, after Find cameras, so the browser can show device names
- Wireless barcode scanner (keyboard wedge, including WM930)
- Laser scanner (keyboard wedge, including MUNBYN)

Browsers cannot list scanner guns by product name. Those two choices cover every scanner that types like a keyboard. The main screen stays on the barcode box. Camera start, stop, and the camera list stay in Settings.

### How a scan is read

The code box stays visible. Keystrokes are captured even if another control has focus, as long as this browser window is in front. If Notepad or another app is in front, the code stays in that app.

A finished scan is sent to check-in on its own. Staff do not press Enter. A GUID that arrives with slashes, such as `bc6fbd5c/f422/...`, is turned back into hyphens before the API call. That fixes a keyboard layout that types `/` for the hyphen key. Stored guest IDs are not changed.

The scanner must be in normal keyboard mode (HID keyboard, suffix Enter). Storage mode only types setup barcodes such as a stored-count code, not each guest badge.

### After a scan

The API records a check-in. The page shows:

- Guest name, company, and position
- RSVP status
- Check-in count and times for that guest only
- Assigned seat in large type, or No seat
- **Mark as Seated** when the guest has a chair and is not yet marked seated

Check-in and seating are separate.

| Result | What staff see |
|---|---|
| Seat assigned, not seated | **✓ Mark as Seated** |
| No seat | **No Seat Assigned**. The button is hidden |
| Already seated | **✓ Already Seated**. The time is shown |
| Unknown code | Guest not found |

Mark as Seated sends only the guest id. The API finds that guest’s chair and sets the seated time once. A second scanner sees already seated and does not write another time.

---

## 7. Seating plan

File: `seats.html`  
Password gate: client-side only, session key `aramcoFarewellSeatAdmin`.

### Designer

Staff build the room on a grid:

- Select and pan
- Unlimited pan and zoom. The plan fits on screen when it opens
- Round, rectangular, square, oval, crescent, U-shape, and row seating
- Chair spacing: even, one opening, or uneven
- Table numbers by pattern: across, down, or snake
- Text labels, walls, stage, sofa, and a custom shape
- Upload an SVG or CAD floor plan
- Duplicate, delete, rotate, scale, align, and group
- Hide either sidebar
- Touch pan and pinch. A touch drag that starts on a chair pans the grid
- After a chair is deleted, a prompt can renumber the remaining chairs if the numbers skip

Seat codes stored for the API stay in the form `C-001`. On screen, a chair on a table is labeled from that table:

```text
Table 1 - C-001
Table 1 - C-002
Table 2 - C-001
Table 2 - C-002
```

The number restarts at C-001 for each table. The code saved on the assignment is not rewritten by that label, so guests stay on the same chair.

### Guest list on the plan

Filters: all, assigned, not assigned, accepted, declined.

The counts line shows assigned, not assigned, accepted, and declined.

For a guest who has a chair, the seat line under the name is 40px, for example `Table 2 - C-005`. The name stays the normal size. Guests with no chair still show a small “No seat”.

Declined guests can be filtered and seen. Clicking a declined guest does not assign them. If they already have a chair, the plan jumps to it. Their chair is red with a white name. Other assigned chairs are yellow. A chair whose guest has been marked seated is green.

The side panel heading for the selected chair uses the same `Table N - C-00x` label at a large size.

### Save

Save layout posts the plan to `POST /api/AramcoFarewell/seats/map`. That replaces the stored map for this project. Chairs whose codes are not in the new file are dropped, and their assignments go with them. Assignments for codes that remain are kept.

### Live refresh

The footer has Refresh and Auto refresh.

| Interval | Off, 5 seconds, 10, 15, 30, 60, 2 minutes, 5 minutes |
| Default | 30 seconds. The choice is remembered in this browser |
| Manual refresh | Works even when auto refresh is off |
| Page reload | Not used |
| Overlap | A refresh that is still running is not started again |
| Hidden tab | Auto refresh waits. One refresh runs when the tab is visible again |
| Failure | The plan stays as it was. The status shows update failed and the last good time |

Green chairs come from `SeatedAt` on the assignment, not from a local flag. Every open seating screen sees the same state after it refreshes.

`SeatedAt` is added by the API the first time a seat request runs after that API build is published. Existing assignment rows stay empty until staff click Mark as Seated. No separate SQL script has to be run for that column.

---

## 8. Data

All farewell data is in `UDCGameDB`. Tables:

| Table | Holds |
|---|---|
| `aramcoFarewellUser` | Guest: name, email, type, company, position, phone, status, group, invite flags, QR path |
| `aramcoFarewellUserActivity` | Invite, status, check-in, wallet, download, and similar events |
| `aramcoFarewellCheckIn` | One row per door scan, with time and source |
| `aramcoFarewellGroup` | Guest groups |
| `aramcoFarewellSeatMap` | One SVG plan per project code. Default project `AramcoFarewell` |
| `aramcoFarewellSeat` | Chair codes taken from the plan |
| `aramcoFarewellSeatAssignment` | Which guest is on which chair, assign time, and `SeatedAt` |

Guest identity for links and QR is `UniqueId` (a GUID). Email plus full name is unique. Status defaults to Pending.

Check-in rows and seated time are different. A scan adds a check-in row. Mark as Seated sets `SeatedAt` on the assignment only.

---

## 9. API surface

Base path: `/api/AramcoFarewell`

### Guests and groups

| Method | Path | Role |
|---|---|---|
| GET | `/users` | All guests, with check-ins, seat, and activity |
| POST | `/create-user` | Add a guest |
| POST | `/update-user` | Edit a guest |
| POST | `/user-status` and PUT `/user-status` | Accept, decline, or other status |
| POST | `/admin/user-status` | Staff status change |
| DELETE | `/users/{uniqueId}` | Remove a guest and related rows |
| GET | `/groups` | List groups |
| POST | `/groups` | Create a group |
| DELETE | `/groups/{id}` | Delete a group |
| GET | `/user-activity/{uniqueId}` | Activity for one guest |

### Invites

| Method | Path | Role |
|---|---|---|
| POST | `/send-invitation-email` | Queue the invite email |
| GET | `/invitation-email-status/{jobId}` | One job |
| GET | `/invitation-email-status-by-email` | Status by address |
| GET | `/invitation-email-queue-status` | Queue summary |
| POST | `/send-invitation-sms` | SMS invite path |
| GET | `/invite-pdf/{uniqueId}` | Invite card PDF |
| POST | `/bulk-download-pdf` | ZIP of invite PDFs, max 250 |
| POST | `/direct-rsvp` | Accept or decline from a link |
| GET | `/rsvp-check/{uniqueId}` | Current RSVP state |

### Badge, wallet, check-in

| Method | Path | Role |
|---|---|---|
| GET | `/qr/{uniqueId}` | QR asset |
| POST | `/ensure-missing-qr` | Create missing QR files |
| GET | `/qr-card/{uniqueId}` | Badge JPEG. `?download=1` logs the download |
| POST | `/bulk-download-qr` | ZIP of badge JPEGs, max 250 |
| GET | `/wallet/{uniqueId}` | Apple Wallet pass |
| POST | `/check-in?uniqueId=` | Record a scan and return guest, seat, seated flag |
| GET | `/check-in/{uniqueId}` | Check-in history for that guest |

### Seats

| Method | Path | Role |
|---|---|---|
| GET | `/seats` | Chairs, assignments, guest names, `seatedAt` |
| GET | `/seats/map` | Saved floor plan SVG |
| POST | `/seats/map` | Replace the plan |
| DELETE | `/seats/map` | Restore the placeholder plan |
| PUT | `/seats/{seatCode}/assign` | Assign a guest |
| PUT | `/seats/{seatCode}/reserve` | Reserve a chair |
| PUT | `/seats/{seatCode}/release` | Clear a chair |
| POST | `/seats/mark-seated?uniqueId=` | Set `SeatedAt` for that guest’s chair |

Mark seated rejects a guest with no chair. If the guest is already seated, it returns success with `alreadySeated: true` and does not change the time.

VVIP body and document upload routes also exist on the controller for image and document files tied to a guest.

---

## 10. Pages in this folder

| File | Who uses it |
|---|---|
| `dashboard.html` | Staff. Guests, groups, downloads, invites |
| `emailInvite.html` | Invite artwork, English, v8 |
| `emailInviteEn.html` | English invite variant |
| `emailInviteVvip.html` | VVIP invite artwork |
| `rsvp.html` | Accept or decline, then badge QR and wallet |
| `confirmAccept.html` | Acceptance confirmation |
| `directRsvp.html` | RSVP from a direct link |
| `scanner.html` | Door check-in and mark seated |
| `seats.html` | Floor plan, assignment, live seated color |
| `wallet-worker.js` | `/e-badge/`, `/qr-card/`, `/wallet/` |
| `assets/` | Invite images, RSVP background, logo, badge background, wallet button, fonts |

`capital-markets-conversation/PROJECT.md` is a separate proposal. It is not part of the farewell guest flow.

---

## 11. Fonts and colors guests see

| Surface | Type |
|---|---|
| Invite, RSVP, badge, scanner | Manifa Pro 2 |
| Dashboard | Inter only |
| Apple Wallet | System font. Manifa cannot be embedded |
| Invite name | `#ffffff` |
| Badge name | `#00B0F0` |
| Assigned chair | Yellow `#ffe14a` |
| Declined assigned chair | Red `#e23b32` |
| Seated chair | Green `#1f8a5b` |

---

## 12. What a change needs before guests see it

| Change | Publish |
|---|---|
| HTML, scanner, seats, dashboard, worker | Cloudflare site (`npx wrangler deploy`) |
| Check-in payload, badge drawing, email template, PDF, `SeatedAt`, mark seated | Azure API |

Both are required when a feature is split across the page and the API. Badge images already sent to guests are not regenerated unless that badge API is changed and the guest downloads again.
