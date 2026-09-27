# WhatsApp Business API — Account Setup Procedure

**Project:** Ashraq Launch Invitation  
**Purpose:** Enable official WhatsApp invitations with a branded sender name and a “Open invitation” button (no raw URL).  
**Audience:** Ashraq / client business owner, marketing, or IT admin  
**Prepared by:** Entourage  

This document is only for **account and approval setup**. Development starts after the items in Section 7 are received.

---

## 1. What we are applying for

We need a **WhatsApp Business Platform** account (official Meta Cloud API), not a normal WhatsApp or WhatsApp Business phone app.

| Channel | Can send a real button? | Suitable for VIP / VVIP? |
|---|---|---|
| Personal WhatsApp | No | No |
| WhatsApp Business app (phone) | No | Temporary only |
| **WhatsApp Business Platform (Cloud API)** | **Yes** | **Yes** |

The guest will receive a message from **أشراق / Ashraq** with an invite image and a button that opens their personal invitation page.

---

## 2. Who must own the account

The account must be created under **Ashraq’s company**, not a personal Facebook profile and not Entourage’s personal login.

**Required owner**

- Ashraq legal business name  
- Company email (e.g. a domain mailbox, not a personal Gmail)  
- Authority to verify the business (trade license / company documents)

Entourage can guide the clicks. Ashraq must sign in and complete verification.

---

## 3. What Ashraq should prepare before starting

Have these ready:

1. **Meta Business Manager** access for Ashraq (or create one).  
2. **Company documents** for verification (trade license, address, website).  
3. **Public website:** https://ashraqlaunch.com  
4. **A WhatsApp sending number**  
   - A **new** dedicated number (recommended), or  
   - An existing Ashraq number (this number will **leave** the consumer WhatsApp app and cannot be used in the phone app anymore).  
5. **Display name** to show in WhatsApp: `أشراق` or `Ashraq`  
6. **Invite artwork** (header image, landscape, high quality) for the message template.  
7. Two people who can approve: one business admin, one technical contact.

**Recommended number:** a new number used only for this event / invitations. Do not migrate the CEO’s personal WhatsApp.

---

## 4. Step-by-step procedure

### Step 1 — Create Meta Business Manager

1. Go to [https://business.facebook.com/](https://business.facebook.com/)  
2. Create a Business Manager for **Ashraq Development Company** (or the legal entity name).  
3. Add at least two admins (so access is not lost).  
4. Confirm the business email.

### Step 2 — Create a Meta Developer App

1. Go to [https://developers.facebook.com/](https://developers.facebook.com/)  
2. Log in with the same Business Manager admin.  
3. **Create App** → use type **Business**.  
4. App name example: `Ashraq Launch Invite`.  
5. Add the product **WhatsApp**.

### Step 3 — Add WhatsApp and a sending number

1. In the app, open **WhatsApp → API Setup**.  
2. Connect the WhatsApp product to the Ashraq Business Manager.  
3. Add a phone number:  
   - Use Meta’s test number first (for internal tests only), then  
   - Add the **real Ashraq number** and complete SMS / voice OTP.  
4. Note the **Phone Number ID** and **WhatsApp Business Account ID** (WABA ID).

### Step 4 — Set the display name

1. Open [WhatsApp Manager](https://business.facebook.com/latest/whatsapp_manager).  
2. Select the WhatsApp account → **Phone numbers**.  
3. Set display name to **أشراق** or **Ashraq**.  
4. Submit for review.  
5. Wait until the name is **Approved**. Guests will see this name.

### Step 5 — Verify the business

1. In Business Manager: **Business settings → Security Center → Start verification**.  
2. Upload the requested company documents.  
3. Complete any domain / email checks.  
4. Wait for **Verified** status.

Without verification, sending limits stay very low and the sender looks less trustworthy for VIP guests.

### Step 6 — Create message templates (required for buttons)

Invitations are sent by the company first, so WhatsApp only allows **pre-approved templates**.

In WhatsApp Manager → **Message templates** → **Create template**.

**Template A — Arabic invite**

| Field | Value |
|---|---|
| Name | `ashraq_invite` |
| Language | Arabic |
| Category | Marketing |
| Header | Image (Ashraq invite visual) |
| Body | See copy below |
| Button | Visit website → **تأكيد الدعوة** |
| Button URL | Dynamic: `https://ashraqlaunch.com/emailInvite?{{1}}` |
| Sample for `{{1}}` | `name=Jill&uniqueId=12e333aa-b9aa-45d1-b0c7-50ccb5d7e2e1` |

**Body copy (Arabic):**

```
{{1}} الكريم،

يسعدنا دعوتكم لحضور حفل إطلاق أشراق.
نرجو تأكيد الدعوة من خلال الزر أدناه.
```

**Template B — English invite**

| Field | Value |
|---|---|
| Name | `ashraq_invite_en` |
| Language | English |
| Category | Marketing |
| Header | Same image |
| Body | See copy below |
| Button | Visit website → **Open invitation** |
| Button URL | Same dynamic URL as above |

**Body copy (English):**

```
Dear {{1}},

You are invited to the Ashraq launch.
Please confirm your attendance using the button below.
```

**Template C — Reminder (optional, same button)**

- Name: `ashraq_reminder` / `ashraq_reminder_en`  
- Short reminder body + the same **تأكيد الدعوة** / **Open invitation** button  

Submit all templates and wait until status is **Approved**. Do not send real guests before that.

### Step 7 — Create a permanent access token

1. In Business Manager → **System users**.  
2. Create a system user (e.g. `Ashraq Invite API`).  
3. Assign the WhatsApp account and the app with permission to send messages.  
4. Generate a **permanent token**.  
5. Send the token to Entourage through a **secure channel** (not WhatsApp, not email in plain text if possible). Prefer a password manager or a short call + paste into Azure.

---

## 5. What to send back to Entourage

When the above is done, send this checklist (values only, via a secure channel):

```
Business Manager name:
WhatsApp Business Account ID (WABA):
Phone Number ID:
Display name (approved):
Sending number (with country code, e.g. 9715…):
App ID:
Permanent access token: (secure share)
Template names + languages approved:
  - ashraq_invite (ar) — Approved / Pending
  - ashraq_invite_en (en) — Approved / Pending
Test numbers we may send to (Entourage + Ashraq mobiles):
```

Also confirm:

- [ ] Business verification submitted / approved  
- [ ] Display name approved  
- [ ] Real number is connected (not only the Meta test number)  
- [ ] https://ashraqlaunch.com is the website used on the business profile  

---

## 6. Expected timeline

| Step | Typical time |
|---|---|
| Business Manager + app + test number | Same day |
| Real number OTP | Same day |
| Display name review | 1–3 business days |
| Business verification | 2–7 business days (sometimes longer) |
| Template approval | 1–2 business days |
| Technical connect + test send | 1 day after credentials |

Start verification **immediately**. Template review can run in parallel.

---

## 7. After the account is ready

Entourage will:

1. Store credentials in the secure API configuration (not in the public website).  
2. Add guest mobile numbers to the invitation dashboard.  
3. Add **Send WhatsApp** next to **Send Mail**.  
4. Send a test to 2–3 internal phones.  
5. Confirm the button opens the correct personal invite (name + unique ID).  
6. Then send to VIP / VVIP guests.

Email invitations stay in place. WhatsApp is the channel guests will tap; email remains the official record and QR delivery.

---

## 8. Rules and risks (please read)

- Do **not** use unofficial WhatsApp “blast” tools. They can ban the number and are not acceptable for this event.  
- Do **not** move a personal or CEO WhatsApp number onto the API unless that is a deliberate decision.  
- Marketing templates may be limited if guests mark the chat as spam. Keep copy short and formal.  
- Only send to guests who are on the official invite list and whose numbers you are allowed to use.  
- The long invite URL must **not** appear in the message body. The approved button carries the link.

---

## 9. Useful links

- Meta Business Manager: https://business.facebook.com/  
- Meta Developers: https://developers.facebook.com/  
- WhatsApp Cloud API: https://developers.facebook.com/docs/whatsapp/cloud-api  
- Message templates: https://developers.facebook.com/docs/whatsapp/api/messages/message-templates/interactive-message-templates/  
- Invite website: https://ashraqlaunch.com  

---

## 10. Contact

For questions while creating the account, contact Entourage.  
Do not start sending guest messages until Entourage has completed a successful test send.
