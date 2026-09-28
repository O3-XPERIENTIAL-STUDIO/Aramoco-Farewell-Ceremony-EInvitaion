const API_BASE = "https://udcwebapi2025-avdxdce9gngffkgx.uaenorth-01.azurewebsites.net";

export default {
  async fetch(request, env) {
    const url = new URL(request.url);

    const cardMatch = url.pathname.match(/^\/qr-card\/([^/]+?)\/?$/i);
    if (cardMatch) {
      return proxyQrCard(decodeURIComponent(cardMatch[1]));
    }

    const match = url.pathname.match(/^\/wallet\/([^/]+?)(\.pkpass)?\/?$/i);
    if (!match) {
      return env.ASSETS.fetch(request);
    }

    const uniqueId = decodeURIComponent(match[1]);
    const wantPass = Boolean(match[2]) || url.searchParams.get("format") === "pkpass";

    if (!wantPass) {
      return new Response(landingHtml(uniqueId), {
        status: 200,
        headers: {
          "Content-Type": "text/html; charset=utf-8",
          "Cache-Control": "no-store"
        }
      });
    }

    const upstream = await fetch(
      `${API_BASE}/api/AramcoFarewell/wallet/${encodeURIComponent(uniqueId)}`,
      {
        headers: {
          Accept: "application/vnd.apple.pkpass",
          "User-Agent": request.headers.get("User-Agent") || "AramcoFarewellWalletProxy"
        },
        cf: { cacheTtl: 0, cacheEverything: false }
      }
    );

    if (!upstream.ok) {
      return new Response(await upstream.text(), {
        status: upstream.status,
        headers: { "Content-Type": "text/plain; charset=utf-8", "Cache-Control": "no-store" }
      });
    }

    const body = await upstream.arrayBuffer();
    const ua = request.headers.get("User-Agent") || "";
    const safari = /Safari/i.test(ua) && !/CriOS|FxiOS|EdgiOS|Chrome|Android/i.test(ua);
    return new Response(body, {
      status: 200,
      headers: {
        "Content-Type": "application/vnd.apple.pkpass",
        "Content-Disposition": `${safari ? "inline" : "attachment"}; filename=aramco-farewell-invite.pkpass`,
        "Content-Length": String(body.byteLength),
        "Cache-Control": "no-store",
        "X-Content-Type-Options": "nosniff"
      }
    });
  }
};

async function proxyQrCard(uniqueId) {
  const upstream = await fetch(
    `${API_BASE}/api/AramcoFarewell/qr-card/${encodeURIComponent(uniqueId)}?download=1`,
    {
      cf: { cacheTtl: 0, cacheEverything: false }
    }
  );

  if (!upstream.ok) {
    return new Response(await upstream.text(), {
      status: upstream.status,
      headers: { "Content-Type": "text/plain; charset=utf-8", "Cache-Control": "no-store" }
    });
  }

  const body = await upstream.arrayBuffer();
  return new Response(body, {
    status: 200,
    headers: {
      "Content-Type": "image/jpeg",
      "Content-Disposition": upstream.headers.get("Content-Disposition")
        || 'attachment; filename="AramcoFarewellInvite.jpg"',
      "Content-Length": String(body.byteLength),
      "Cache-Control": "no-store",
      "X-Content-Type-Options": "nosniff"
    }
  });
}

function landingHtml(uniqueId) {
  const safeId = encodeURIComponent(uniqueId);
  const pkpassUrl = `/wallet/${safeId}.pkpass`;
  const qrUrl = `${API_BASE}/api/AramcoFarewell/qr/${safeId}`;
  return `<!DOCTYPE html>
<html lang="en" dir="ltr">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover" />
  <title>Add to Apple Wallet | أرامكو</title>
  <link rel="icon" type="image/png" href="/assets/icon.png" />
  <style>
    :root { --navy:#002650; --gold:#CBA886; --muted:#4a5d73; }
    * { box-sizing:border-box; margin:0; padding:0; }
    body { min-height:100vh; font-family:Tahoma,Arial,sans-serif; background:#f4f6f8; color:var(--navy); padding:24px 16px 40px; }
    .card { max-width:420px; margin:0 auto; background:#fff; border-radius:16px; padding:28px 22px; text-align:center; box-shadow:0 8px 30px rgba(0,38,80,.08); }
    h1 { font-size:1.15rem; margin-bottom:8px; }
    p { font-size:.92rem; line-height:1.6; color:var(--muted); margin-bottom:12px; }
    img.qr { width:220px; height:220px; background:#fff; margin:12px 0 18px; }
    .btn { display:block; width:100%; text-decoration:none; border:0; border-radius:10px; padding:14px 16px; font-weight:700; font-size:.95rem; cursor:pointer; margin-top:10px; }
    .btn-wallet { display:none; line-height:0; }
    .btn-wallet img { display:block; width:180px; height:auto; margin:10px auto 0; }
    .btn-secondary { background:var(--gold); color:#fff; display:none; }
    .hint { font-size:.78rem; margin-top:14px; display:none; }
  </style>
</head>
<body>
  <div class="card">
    <h1>تذكرة الدخول | Entry pass</h1>
    <p>أضف البطاقة إلى Apple Wallet، أو احتفظ برمز QR للدخول.</p>
    <img class="qr" src="${qrUrl}" alt="QR code" width="220" height="220" />
    <a class="btn-wallet" id="btnWallet" href="${pkpassUrl}" aria-label="إضافة إلى Apple Wallet">
      <img src="/assets/wallet/add-to-apple-wallet-ar.svg" width="180" height="57" alt="إضافة إلى Apple Wallet" />
    </a>
    <a class="btn btn-secondary" id="btnDownload" href="${pkpassUrl}" download="aramco-farewell-invite.pkpass">Download pass</a>
    <p class="hint" id="hint">Safari on iPhone adds the pass directly. Chrome, Outlook, and other browsers download a .pkpass file — open that file to add it to Wallet.</p>
  </div>
  <script>
    (function () {
      var pkpassUrl = ${JSON.stringify(pkpassUrl)};
      var ua = navigator.userAgent || "";
      var platform = navigator.platform || "";
      var iOS = /iPhone|iPad|iPod/i.test(ua) || (platform === "MacIntel" && navigator.maxTouchPoints > 1);
      var isApple = iOS || (/Macintosh|Mac OS X/i.test(ua) && !/Android/i.test(ua));
      var safari = /Safari/i.test(ua) && !/CriOS|FxiOS|EdgiOS|Chrome|Android/i.test(ua);
      var btn = document.getElementById("btnWallet");
      var download = document.getElementById("btnDownload");
      var hint = document.getElementById("hint");
      if (isApple) {
        btn.style.display = "block";
        download.style.display = "block";
        hint.style.display = "block";
        if (iOS && safari) {
          hint.textContent = "Tap Add to Apple Wallet to save this invitation.";
        }
      }
      btn.addEventListener("click", function (e) {
        if (iOS && safari) return;
        e.preventDefault();
        fetch(pkpassUrl).then(function (res) {
          if (!res.ok) throw new Error("pass");
          return res.arrayBuffer();
        }).then(function (buf) {
          var blob = new Blob([buf], { type: "application/vnd.apple.pkpass" });
          var objectUrl = URL.createObjectURL(blob);
          var a = document.createElement("a");
          a.href = objectUrl;
          a.download = "aramco-farewell-invite.pkpass";
          document.body.appendChild(a);
          a.click();
          a.remove();
          setTimeout(function () { URL.revokeObjectURL(objectUrl); }, 2000);
        }).catch(function () {
          window.location.href = pkpassUrl;
        });
      });
    })();
  </script>
</body>
</html>`;
}
