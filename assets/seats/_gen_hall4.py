from pathlib import Path

W, H = 1400, 980
SEAT_W, SEAT_H, GAP = 46, 38, 10


def seat(code, zone, x, y, reserved=False):
    reserved_attr = "true" if reserved else "false"
    return (
        f'<g id="seat-{code}" class="seat-block" data-seat-code="{code}" '
        f'data-zone="{zone}" data-reserved="{reserved_attr}" '
        f'transform="translate({x:.1f},{y:.1f})">'
        f'<rect class="seat-shape" width="{SEAT_W}" height="{SEAT_H}" rx="7"/>'
        f'<text class="seat-label" x="{SEAT_W/2}" y="24" text-anchor="middle">{code}</text>'
        f'<title>{code} · {zone}</title>'
        f"</g>"
    )


def row_seats(row, zone, count, y, reserved=False, aisle_after=None):
    parts = []
    total_w = count * SEAT_W + (count - 1) * GAP
    if aisle_after is not None:
        total_w += 56
    x0 = (W - total_w) / 2
    x = x0
    for i in range(1, count + 1):
        code = f"{row}-{i:02d}"
        parts.append(seat(code, zone, x, y, reserved))
        x += SEAT_W + GAP
        if aisle_after is not None and i == aisle_after:
            x += 56
    return parts


parts = []
parts += row_seats("A", "VVIP", 10, 168, reserved=True)
parts += row_seats("B", "VIP", 12, 228)
parts += row_seats("C", "VIP", 12, 288)
parts += row_seats("D", "General", 16, 368, aisle_after=8)
parts += row_seats("E", "General", 16, 428, aisle_after=8)
parts += row_seats("F", "General", 16, 488, aisle_after=8)
parts += row_seats("G", "General", 16, 548, aisle_after=8)
parts += row_seats("H", "General", 16, 608, aisle_after=8)

media = []
mx, my = 1288, 200
for i in range(1, 9):
    media.append(seat(f"M-{i:02d}", "Media", mx, my + (i - 1) * (SEAT_H + 8), reserved=True))

svg = f'''<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" role="img" aria-label="Hall 4 seating">
  <style>
    .hall-bg {{ fill: #0b1c33; }}
    .stage {{ fill: #CBA886; }}
    .stage-label, .zone-label, .hall-title {{
      font-family: Inter, Tahoma, Arial, sans-serif;
      fill: #f4efe6;
      text-anchor: middle;
    }}
    .hall-title {{ font-size: 22px; font-weight: 700; letter-spacing: 0.12em; }}
    .stage-label {{ font-size: 15px; font-weight: 700; fill: #002650; }}
    .zone-label {{ font-size: 12px; fill: #8aa0b8; letter-spacing: 0.08em; }}
    .aisle {{ fill: none; stroke: #1e3a5a; stroke-width: 2; stroke-dasharray: 6 8; }}
    .seat-block {{ cursor: pointer; }}
    .seat-shape {{ fill: #163252; stroke: #CBA886; stroke-width: 1.2; }}
    .seat-block[data-zone="VVIP"] .seat-shape {{ fill: #3a2c18; stroke: #CBA886; }}
    .seat-block[data-zone="VIP"] .seat-shape {{ fill: #2a2414; stroke: #d4b07a; }}
    .seat-block[data-zone="Media"] .seat-shape {{ fill: #1a3348; stroke: #7ea0b8; }}
    .seat-label {{ font-family: Inter, Tahoma, Arial, sans-serif; font-size: 9px; font-weight: 700; fill: #e8dcc8; pointer-events: none; }}
    .seat-block.is-available .seat-shape {{ fill: #163252; }}
    .seat-block.is-reserved .seat-shape {{ fill: #7a5a18; stroke: #D29922; }}
    .seat-block.is-assigned .seat-shape {{ fill: #CBA886; stroke: #f0d7b0; }}
    .seat-block.is-assigned .seat-label {{ fill: #002650; }}
    .seat-block.is-blocked .seat-shape {{ fill: #1a1f26; stroke: #30363d; }}
    .seat-block.is-selected .seat-shape {{ stroke: #ffffff; stroke-width: 2.4; }}
    .seat-block:hover .seat-shape {{ filter: brightness(1.18); }}
  </style>
  <rect class="hall-bg" width="{W}" height="{H}" rx="18"/>
  <text class="hall-title" x="{W/2}" y="36">DHAHRAN EXPO · HALL 4</text>
  <text class="zone-label" x="{W/2}" y="58">PLACEHOLDER MAP — REPLACE WITH CAD SVG</text>
  <rect class="stage" x="260" y="78" width="880" height="52" rx="8"/>
  <text class="stage-label" x="{W/2}" y="110">STAGE</text>
  <text class="zone-label" x="{W/2}" y="156">VVIP</text>
  <text class="zone-label" x="{W/2}" y="218">VIP</text>
  <text class="zone-label" x="{W/2}" y="354">GENERAL</text>
  <line class="aisle" x1="{W/2}" y1="360" x2="{W/2}" y2="656"/>
  <text class="zone-label" x="1310" y="188">MEDIA</text>
  {"".join(parts)}
  {"".join(media)}
</svg>
'''

out = Path(__file__).with_name("hall4.svg")
out.write_text(svg, encoding="utf-8")
print(f"Wrote {out} ({out.stat().st_size} bytes)")
