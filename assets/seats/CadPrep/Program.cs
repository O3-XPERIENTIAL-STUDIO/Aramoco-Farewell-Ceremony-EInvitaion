using ACadSharp;
using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tables;

string input = args.Length > 0 ? args[0] : @"C:\Users\mmuthu\Downloads\CEO_3.dwg";
if (!File.Exists(input))
    input = @"c:\ENTOURAGE\PROJECTS\CLIENT\2026\Ashraq\Ashreq_e-Invitation\assets\seats\CEO_3.dwg";
string outDir = Path.GetDirectoryName(input) ?? ".";
string stem = Path.GetFileNameWithoutExtension(input);

Console.WriteLine("Reading " + input);
CadDocument doc = Path.GetExtension(input).Equals(".dxf", StringComparison.OrdinalIgnoreCase)
    ? DxfReader.Read(input)
    : DwgReader.Read(input);

Dump("BEFORE", doc);

PrintLineStats(doc);

int exploded = ExplodeAssemblies(doc, maxPasses: 8);
Console.WriteLine($"Exploded {exploded} nested inserts.");

int clustered = ClusterLineChairs(doc);
Console.WriteLine($"Created {clustered} chair circles from line groups.");

Dump("AFTER", doc);

string dxfPath = Path.Combine(@"c:\ENTOURAGE\PROJECTS\CLIENT\2026\Ashraq\Ashreq_e-Invitation\assets\seats", stem + "_seats.dxf");
using (var writer = new DxfWriter(dxfPath, doc, binary: false))
    writer.Write();
Console.WriteLine("Wrote " + dxfPath);

static void Dump(string title, CadDocument doc)
{
    var model = EntitiesOf(doc).ToList();
    Console.WriteLine($"\n=== {title} ===");
    Console.WriteLine($"Model entities: {model.Count}");
    foreach (var g in model.GroupBy(e => e.GetType().Name).OrderByDescending(g => g.Count()))
        Console.WriteLine($"  {g.Key}: {g.Count()}");

    var inserts = model.OfType<Insert>().ToList();
    Console.WriteLine($"Inserts: {inserts.Count}");
    foreach (var ins in inserts.Take(30))
    {
        string name = ins.Block?.Name ?? "?";
        int kids = BlockEntities(doc, ins).Count();
        int kidIns = BlockEntities(doc, ins).OfType<Insert>().Count();
        Console.WriteLine($"  block={name} kids={kids} nestedInserts={kidIns} rows={ins.RowCount} cols={ins.ColumnCount} at ({ins.InsertPoint.X:0.##},{ins.InsertPoint.Y:0.##})");
    }

    Console.WriteLine("Block records:");
    foreach (BlockRecord rec in doc.BlockRecords)
    {
        if (rec.Name.StartsWith("*")) continue;
        Console.WriteLine($"  {rec.Name}: {rec.Entities.Count()} entities, inserts={rec.Entities.OfType<Insert>().Count()}");
    }
}

static void PrintLineStats(CadDocument doc)
{
    var lines = EntitiesOf(doc).OfType<Line>().ToList();
    if (lines.Count == 0) return;
    var lens = lines.Select(l => Dist(l.StartPoint, l.EndPoint)).OrderBy(x => x).ToList();
    double minX = lines.Min(l => Math.Min(l.StartPoint.X, l.EndPoint.X));
    double maxX = lines.Max(l => Math.Max(l.StartPoint.X, l.EndPoint.X));
    double minY = lines.Min(l => Math.Min(l.StartPoint.Y, l.EndPoint.Y));
    double maxY = lines.Max(l => Math.Max(l.StartPoint.Y, l.EndPoint.Y));
    Console.WriteLine($"Line lengths: n={lens.Count} min={lens.First():0.##} med={lens[lens.Count/2]:0.##} max={lens.Last():0.##}");
    Console.WriteLine($"Drawing span: {(maxX-minX):0.##} x {(maxY-minY):0.##}");

    double med = lens[lens.Count / 2];
    int horiz = lines.Count(l => Math.Abs(l.StartPoint.Y - l.EndPoint.Y) < 0.02 * med);
    int vert = lines.Count(l => Math.Abs(l.StartPoint.X - l.EndPoint.X) < 0.02 * med);
    Console.WriteLine($"Axis-aligned: horiz={horiz} vert={vert} other={lines.Count - horiz - vert}");

    var sample = lines.Take(20).ToList();
    Console.WriteLine("Sample lines:");
    foreach (var l in sample)
        Console.WriteLine($"  ({l.StartPoint.X:0.###},{l.StartPoint.Y:0.###})-({l.EndPoint.X:0.###},{l.EndPoint.Y:0.###}) len={Dist(l.StartPoint,l.EndPoint):0.###}");

    var circles = EntitiesOf(doc).OfType<Circle>().ToList();
    Console.WriteLine($"Existing circles: {circles.Count}");
    foreach (var c in circles)
        Console.WriteLine($"  center=({c.Center.X:0.###},{c.Center.Y:0.###}) r={c.Radius:0.###}");
}

static int ClusterLineChairs(CadDocument doc)
{
    var lines = EntitiesOf(doc).OfType<Line>().ToList();
    if (lines.Count < 6) return 0;
    var lens = lines.Select(l => Dist(l.StartPoint, l.EndPoint)).OrderBy(x => x).ToList();
    double med = lens[lens.Count / 2];
    double shortMax = med * 3.5;
    var shorts = lines.Where(l => Dist(l.StartPoint, l.EndPoint) <= shortMax).ToList();
    if (shorts.Count < 4) shorts = lines;

    var joins = new[] { med * 0.02, med * 0.04, med * 0.06, med * 0.08, med * 0.12, med * 0.18, med * 0.25 };
    List<(double cx, double cy, double r)>? best = null;
    int bestCount = 0;
    double bestJoin = 0;

    foreach (double join in joins)
    {
        var groups = ClusterLines(shorts, join);
        var chairs = groups
            .Where(g => g.Count is >= 4 and <= 8)
            .Select(ChairFromLines)
            .Where(c => c.size >= med * 0.4 && c.size <= med * 4)
            .ToList();
        var sizes = groups.OrderByDescending(g => g.Count).Take(8).Select(g => g.Count);
        Console.WriteLine($"  join={join:0.###} groups={groups.Count} chair-sized={chairs.Count} top={string.Join(",", sizes)}");
        if (chairs.Count > bestCount)
        {
            bestCount = chairs.Count;
            bestJoin = join;
            best = chairs.Select(c => (c.cx, c.cy, c.r)).ToList();
        }
    }

    if (best == null || best.Count == 0)
        return 0;

    Console.WriteLine($"Using join={bestJoin:0.###} → {best.Count} chairs");
    var model = doc.ModelSpace;
    int created = 0;
    foreach (var (cx, cy, r) in best)
    {
        try
        {
            model.Entities.Add(new Circle(new CSMath.XYZ(cx, cy, 0), Math.Max(r, med * 0.2)));
            created++;
        }
        catch { }
    }
    return created;
}

static List<List<Line>> ClusterLines(List<Line> shorts, double join)
{
    int[] parent = Enumerable.Range(0, shorts.Count).ToArray();
    int Find(int a) { while (parent[a] != a) a = parent[a] = parent[parent[a]]; return a; }
    void Union(int a, int b) { a = Find(a); b = Find(b); if (a != b) parent[a] = b; }

    for (int i = 0; i < shorts.Count; i++)
    {
        for (int j = i + 1; j < shorts.Count; j++)
        {
            if (LineDist(shorts[i], shorts[j]) <= join)
                Union(i, j);
        }
    }

    return shorts.Select((l, i) => (l, g: Find(i)))
        .GroupBy(x => x.g)
        .Select(g => g.Select(x => x.l).ToList())
        .ToList();
}

static (double cx, double cy, double r, double size) ChairFromLines(List<Line> segs)
{
    double minX = segs.Min(l => Math.Min(l.StartPoint.X, l.EndPoint.X));
    double maxX = segs.Max(l => Math.Max(l.StartPoint.X, l.EndPoint.X));
    double minY = segs.Min(l => Math.Min(l.StartPoint.Y, l.EndPoint.Y));
    double maxY = segs.Max(l => Math.Max(l.StartPoint.Y, l.EndPoint.Y));
    double w = maxX - minX;
    double h = maxY - minY;
    return ((minX + maxX) / 2, (minY + maxY) / 2, Math.Max(w, h) / 2, Math.Max(w, h));
}

static double Dist(CSMath.XYZ a, CSMath.XYZ b)
{
    double dx = a.X - b.X, dy = a.Y - b.Y;
    return Math.Sqrt(dx * dx + dy * dy);
}

static double LineDist(Line a, Line b)
{
    return Math.Min(
        Math.Min(Dist(a.StartPoint, b.StartPoint), Dist(a.StartPoint, b.EndPoint)),
        Math.Min(Dist(a.EndPoint, b.StartPoint), Dist(a.EndPoint, b.EndPoint)));
}

static int ExplodeAssemblies(CadDocument doc, int maxPasses)
{
    int total = 0;
    for (int pass = 0; pass < maxPasses; pass++)
    {
        var toExplode = EntitiesOf(doc).OfType<Insert>().Where(ins => ShouldExplode(doc, ins)).ToList();
        if (toExplode.Count == 0)
            break;
        foreach (Insert ins in toExplode)
        {
            if (ExplodeOne(doc, ins))
                total++;
        }
        Console.WriteLine($"  pass {pass + 1}: exploded {toExplode.Count}");
    }
    return total;
}

static bool ShouldExplode(CadDocument doc, Insert ins)
{
    var kids = BlockEntities(doc, ins).ToList();
    int nestedInserts = kids.OfType<Insert>().Count();
    int nestedCircles = kids.Count(e => e is Circle or Ellipse);
    int nestedClosed = kids.Count(e => e is LwPolyline lw && lw.IsClosed || e is Polyline2D p && p.IsClosed);
    if (nestedInserts >= 1) return true;
    if (nestedCircles >= 4 || nestedClosed >= 4) return true;
    if (kids.Count >= 25) return true;
    return false;
}

static bool ExplodeOne(CadDocument doc, Insert ins)
{
    var model = doc.ModelSpace;
    List<Entity> pieces;
    try
    {
        pieces = ins.Explode().OfType<Entity>().ToList();
    }
    catch
    {
        pieces = new List<Entity>();
        foreach (Entity child in BlockEntities(doc, ins))
        {
            try
            {
                Entity clone = (Entity)child.Clone();
                clone.ApplyTransform(ins.GetTransform());
                pieces.Add(clone);
            }
            catch { }
        }
    }

    if (pieces.Count == 0)
        return false;

    foreach (Entity piece in pieces)
    {
        try { model.Entities.Add(piece); }
        catch { }
    }
    try { model.Entities.Remove(ins); }
    catch { }
    return true;
}

static IEnumerable<Entity> EntitiesOf(CadDocument doc)
{
    if (doc.ModelSpace?.Entities != null)
    {
        foreach (Entity e in doc.ModelSpace.Entities)
            yield return e;
    }
    else if (doc.Entities != null)
    {
        foreach (Entity e in doc.Entities)
            yield return e;
    }
}

static IEnumerable<Entity> BlockEntities(CadDocument doc, Insert ins)
{
    if (ins.Block?.Entities != null)
    {
        foreach (Entity e in ins.Block.Entities)
            yield return e;
        yield break;
    }
    string? name = ins.Block?.Name;
    if (string.IsNullOrWhiteSpace(name))
        yield break;
    foreach (BlockRecord rec in doc.BlockRecords)
    {
        if (!string.Equals(rec.Name, name, StringComparison.OrdinalIgnoreCase))
            continue;
        foreach (Entity e in rec.Entities)
            yield return e;
    }
}
