using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

var input = @"Sensor at x=98246, y=1908027: closest beacon is at x=1076513, y=2000000
Sensor at x=1339369, y=2083853: closest beacon is at x=1076513, y=2000000
Sensor at x=679177, y=3007305: closest beacon is at x=1076513, y=2000000
Sensor at x=20262, y=3978297: closest beacon is at x=13166, y=4136840
Sensor at x=3260165, y=2268955: closest beacon is at x=4044141, y=2290104
Sensor at x=2577675, y=3062584: closest beacon is at x=2141091, y=2828176
Sensor at x=3683313, y=2729137: closest beacon is at x=4044141, y=2290104
Sensor at x=1056412, y=370641: closest beacon is at x=1076513, y=2000000
Sensor at x=2827280, y=1827095: closest beacon is at x=2757345, y=1800840
Sensor at x=1640458, y=3954524: closest beacon is at x=2141091, y=2828176
Sensor at x=2139884, y=1162189: closest beacon is at x=2757345, y=1800840
Sensor at x=3777450, y=3714504: closest beacon is at x=3355953, y=3271922
Sensor at x=1108884, y=2426713: closest beacon is at x=1076513, y=2000000
Sensor at x=2364307, y=20668: closest beacon is at x=2972273, y=-494417
Sensor at x=3226902, y=2838842: closest beacon is at x=3355953, y=3271922
Sensor at x=22804, y=3803886: closest beacon is at x=13166, y=4136840
Sensor at x=2216477, y=2547945: closest beacon is at x=2141091, y=2828176
Sensor at x=1690953, y=2203555: closest beacon is at x=1076513, y=2000000
Sensor at x=3055156, y=3386812: closest beacon is at x=3355953, y=3271922
Sensor at x=3538996, y=719130: closest beacon is at x=2972273, y=-494417
Sensor at x=2108918, y=2669413: closest beacon is at x=2141091, y=2828176
Sensor at x=3999776, y=2044283: closest beacon is at x=4044141, y=2290104
Sensor at x=2184714, y=2763072: closest beacon is at x=2141091, y=2828176
Sensor at x=2615462, y=2273553: closest beacon is at x=2757345, y=1800840";
var testInput = @"Sensor at x=2, y=18: closest beacon is at x=-2, y=15
Sensor at x=9, y=16: closest beacon is at x=10, y=16
Sensor at x=13, y=2: closest beacon is at x=15, y=3
Sensor at x=12, y=14: closest beacon is at x=10, y=16
Sensor at x=10, y=20: closest beacon is at x=10, y=16
Sensor at x=14, y=17: closest beacon is at x=10, y=16
Sensor at x=8, y=7: closest beacon is at x=2, y=10
Sensor at x=2, y=0: closest beacon is at x=2, y=10
Sensor at x=0, y=11: closest beacon is at x=2, y=10
Sensor at x=20, y=14: closest beacon is at x=25, y=17
Sensor at x=17, y=20: closest beacon is at x=21, y=22
Sensor at x=16, y=7: closest beacon is at x=15, y=3
Sensor at x=14, y=3: closest beacon is at x=15, y=3
Sensor at x=20, y=1: closest beacon is at x=15, y=3";

var sw = Stopwatch.StartNew();

var parsed = input.Split('\n');

var sensors = new List<Sensor>();
var beacons = new HashSet<(int X, int Y)>();
var impossiblePositions = new HashSet<(int X, int Y)>();
var SEARCHSPACE = 4000000;

/// <summary>
/// Key is Y
/// </summary>
//var segments = new Dictionary<int, HashSet<(int Y, int X1, int X2)>>();

foreach (var line in parsed)
{
    var split = line.Replace("Sensor at ", "").Replace("x=", "").Replace("y=", "").Split(": closest beacon is at ");

    var sPos = split[0].Split(',').Select(s => s.Trim()).ToList();
    var bPos = split[1].Split(',').Select(s => s.Trim()).ToList();

    var beac = (X: int.Parse(bPos[0]), Y: int.Parse(bPos[1]));
    beacons.Add(beac);

    var sensor = new Sensor
    {
        Pos = (X: int.Parse(sPos[0]), Y: int.Parse(sPos[1])),
        ClosestBeacon = beac
    };

    sensors.Add(sensor);
}


var segments = new List<((int X, int Y) Point1, (int X, int Y) Point2)>();

foreach (var sensor in sensors)
{
    // Each sensor has a zone of coverage
    // Each zone is defined by 4 segments
    // Each segment can be stored has a 2 2d points definition
    // Each segment will later be used for collision checks

    var dist = CalculateDistance(sensor.Pos, sensor.ClosestBeacon);

    // Top left
    (int X, int Y) p1 = (X: sensor.Pos.X - dist, Y: sensor.Pos.Y);
    (int X, int Y) p2 = (X: sensor.Pos.X, sensor.Pos.Y + dist);
    ((int X, int Y) Point1, (int X, int Y) Point2) s1 = (Point1: p1, Point2: p2);
    segments.Add(s1);

    // Top right
    (int X, int Y) p3 = (X: sensor.Pos.X + dist, Y: sensor.Pos.Y);
    (int X, int Y) p4 = (X: sensor.Pos.X, sensor.Pos.Y + dist);
    ((int X, int Y) Point1, (int X, int Y) Point2) s2 = (Point1: p3, Point2: p4);
    segments.Add(s2);

    // Bottom left
    (int X, int Y) p5 = (X: sensor.Pos.X - dist, Y: sensor.Pos.Y);
    (int X, int Y) p6 = (X: sensor.Pos.X, sensor.Pos.Y - dist);
    ((int X, int Y) Point1, (int X, int Y) Point2) s3 = (Point1: p5, Point2: p6);
    segments.Add(s3);

    // Bottom right
    (int X, int Y) p7 = (X: sensor.Pos.X + dist, Y: sensor.Pos.Y);
    (int X, int Y) p8 = (X: sensor.Pos.X, sensor.Pos.Y - dist);
    ((int X, int Y) Point1, (int X, int Y) Point2) s4 = (Point1: p7, Point2: p8);
    segments.Add(s4);
}


var intersections = new HashSet<(int X, int Y)>();
for (int i = 0; i < segments.Count; i++)
{
    var currSegment = segments[i];
    for (int j = i + 1; j < segments.Count; j++)
    {
        var againstSeg = segments[j];
        if (TryGetLineIntersection(currSegment.Point1, currSegment.Point2, againstSeg.Point1, againstSeg.Point2,
                out var intersection))
        {
            if (intersection.X > 0 -5 && intersection.X <= SEARCHSPACE + 5 && intersection.Y > 0 - 5 && intersection.Y <= SEARCHSPACE + 5)
                intersections.Add(intersection);
        }
    }
}

// x + 1 x - 1 y + 1 y - 1
var directions = new List<(int X, int Y)>()
{
    (X: 1, Y: 0),
    (X: -1, Y: 0),
    (X: 0, Y: 1),
    (X: 0, Y: -1),

};

foreach (var intersection in intersections)
{
    foreach (var dir in directions)
    {
        var testedPoint = (X: intersection.X + dir.X, Y: intersection.Y + dir.Y);


        if (IsFreeSpace(testedPoint))
        {
            Console.WriteLine((long)testedPoint.X * 4000000l + (long)testedPoint.Y);
            sw.Stop();
            // With this solution without any low level optimization I got it down to 50ms
            // Probably doable to divide this time by 2 at least with some micro optimizations
            // Not the purpose of the exercise I wanted to do, I just wanted to have an instant solution under 100ms
            Console.WriteLine($"Time spent : {sw.ElapsedMilliseconds}ms");
            return;
        }
    }
}

bool IsFreeSpace((int X, int Y) point)
{
    if (beacons.Contains(point)) return false;

    foreach (var sensor in sensors)
    {
        var dist = CalculateDistance(sensor.Pos, sensor.ClosestBeacon);
        if (CalculateDistance(point, sensor.Pos) <= dist) return false;
    }

    return true;
}

int CalculateDistance((int X, int Y) left, (int X, int Y) right)
{
    return Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y);
}

/// <summary>
/// p0, p1 is the first segment
/// p1, p2 is the second segment
/// intersection is the intersection point
/// </summary>
bool TryGetLineIntersection((int X, int Y) p0, (int X, int Y) p1,
    (int X, int Y) p2, (int X, int Y) p3,
    out (int X, int Y) intersection)
{
    (float X, float Y) s1;
    (float X, float Y) s2;

    s1.X = p1.X - p0.X; s1.Y = p1.Y - p0.Y;
    s2.X = p3.X - p2.X; s2.Y = p3.Y - p2.Y;

    float s, t;

    var sdiv = (-s2.X * s1.Y + s1.X * s2.Y);
    var tdiv = (-s2.X * s1.Y + s1.X * s2.Y);

    if (sdiv == 0 || tdiv == 0)
    {
        // Collinear, we don't care in our exercise
        intersection = (X: 0, Y: 0);
        return false; // No collision
    }

    s = (-s1.Y * (p0.X - p2.X) + s1.X * (p0.Y - p2.Y)) / sdiv;
    t = (s2.X * (p0.Y - p2.Y) - s2.Y * (p0.X - p2.X)) / tdiv;

    if (s is >= 0 and <= 1 && t is >= 0 and <= 1)
    {
        // Collision detected
        intersection.X = p0.X + (int)(t * s1.X);
        intersection.Y = p0.Y + (int)(t * s1.Y);
        return true;
    }

    intersection = (X: 0, Y: 0);
    return false; // No collision
}


public class Sensor
{
    public (int X, int Y) Pos { get; set; }
    public (int X, int Y) ClosestBeacon { get; set; }
}













// This is actually the solution I used to submit my answer for the exercise, knowing I should do something with the intersection to speed up the process but not having the full solution in mind right away
// This is still running in a few seconds, maybe 10-20 max
#region OldSolution

//var sensorNb = 1;
//foreach (var sensor in sensors)
//{
//    Console.WriteLine(sensorNb++);
//    // Clac Dist
//    var dist = CalculateDistance(sensor.Pos, sensor.ClosestBeacon);

//    var i = 1;
//    var xRange = 0;
//    for (int y = sensor.Pos.Y - dist; y <= sensor.Pos.Y + dist; y++)
//    {
//        if (y >= 0 && y <= SEARCHSPACE)
//        {
//            var seg = (Y: y, X1: Math.Max(sensor.Pos.X - xRange, 0), X2: Math.Min(sensor.Pos.X + xRange, SEARCHSPACE));
//            if (segments.TryGetValue(y, out var lineSegments))
//            {
//                lineSegments.Add(seg);
//            }
//            else
//            {
//                segments.Add(y, new HashSet<(int Y, int X1, int X2)> { seg });
//            }
//        }

//        if (i > dist)
//            xRange--;
//        else
//            xRange++;
//        i++;
//    }
//}





//    // Loop on all positions in dist

//    // If position is not closest beacon mark it has impossible position
//}

//    var tot = 0;
//var max = SEARCHSPACE;
//var idx = 0;
//for (int y = 0; y < max; y++)
//{
//    for (int x = 0; x < max; x++)
//    {
//        var currPos = (X: x, Y: y);

//        if (beacons.Contains(currPos)) continue;
//        var possible = true;
//        foreach (var sensor in sensors)
//        {
//            var sToBDist = CalculateDistance(sensor.Pos, sensor.ClosestBeacon);
//            if (CalculateDistance(currPos, sensor.Pos) <= sToBDist)
//            {
//                possible = false;
//                break;
//            }
//        }

//        if (idx++ % 1000000 == 0)
//            Console.WriteLine($"{idx}, X: {currPos.X}, Y: {currPos.Y}");

//        if (possible)
//        {
//            Console.WriteLine(currPos);
//            Console.WriteLine(currPos.X * SEARCHSPACE + currPos.Y);
//            return;
//        }
//    }

//}

// DEBUG

//for (int i = 0; i < 50; i++)
//{

//    Console.Write($"{i}\t");

//    for (int j = -10; j < 50; j++)
//    {
//        if (segments.TryGetValue(i, out var lineSeg) && lineSeg.ToList().Exists(s => j >= s.X1 && j <= s.X2))
//        {
//            Console.Write("#");
//        }
//        else if (sensors.Exists(s => s.Pos.X == j && s.Pos.Y == i))
//        {
//            Console.Write("S");
//        }
//        else if (beacons.Contains((X: j, Y: i)))
//        {
//            Console.Write("B");
//        }
//        else
//        {
//            Console.Write(".");
//        }
//    }
//    Console.Write("\n");
//}


//var aze = 0;
//foreach (var lineSegments in segments)
//{
//    if (aze++ % 50000 == 0)
//        Console.WriteLine(aze);

//    if (lineSegments.Value.Count == 1) continue;

//    var hasRemoved = true;
//    while (hasRemoved)
//    {
//        hasRemoved = false;
//        var segment = lineSegments.Value.First();
//        foreach (var segAgainst in lineSegments.Value.Skip(1))
//        {
//            if (((segment.X1 >= segAgainst.X1 && segment.X1 -1 <= segAgainst.X2)
//                || (segment.X2 +1 >= segAgainst.X1 && segment.X2 <= segAgainst.X2))

//                || ((segAgainst.X1 >= segment.X1 && segAgainst.X1 -1 <= segment.X2)
//                    || (segAgainst.X2 +1 >= segment.X1 && segAgainst.X2 <= segment.X2)))
//            {
//                segments[segAgainst.Y].Remove(segAgainst);
//                segments[segAgainst.Y].Remove(segment);

//                segments[segAgainst.Y].Add((Y: segment.Y, X1: Math.Min(segment.X1, segAgainst.X1),
//                    X2: Math.Max(segment.X2, segAgainst.X2)));
//                hasRemoved = true;
//                break;
//            }
//        }
//    }

//    if (lineSegments.Value.Count > 1)
//    {
//        hasRemoved = true;
//        while (hasRemoved)
//        {
//            hasRemoved = false;
//            var segment = lineSegments.Value.Skip(1).First();
//            foreach (var segAgainst in lineSegments.Value)
//            {
//                if (segAgainst == segment) continue;

//                if (((segment.X1 >= segAgainst.X1 && segment.X1 - 1 <= segAgainst.X2)
//                     || (segment.X2 + 1 >= segAgainst.X1 && segment.X2 <= segAgainst.X2))

//                    || ((segAgainst.X1 >= segment.X1 && segAgainst.X1 - 1 <= segment.X2)
//                        || (segAgainst.X2 + 1 >= segment.X1 && segAgainst.X2 <= segment.X2)))
//                {
//                    segments[segAgainst.Y].Remove(segAgainst);
//                    segments[segAgainst.Y].Remove(segment);

//                    segments[segAgainst.Y].Add((Y: segment.Y, X1: Math.Min(segment.X1, segAgainst.X1),
//                        X2: Math.Max(segment.X2, segAgainst.X2)));
//                    hasRemoved = true;
//                    break;
//                }
//            }
//        }

//        Console.WriteLine(((long)lineSegments.Value.First().X2 + 1l) * 4000000l + (long)lineSegments.Key);

//        return;

//    }
//}

//void DrawResult()
//{
//    var nbPossible = segments.Count(s => s.Value.Count > 1);
//    var allPossible = segments
//        .Where(s => s.Value.Count > 1 && s.Key >= 0 && s.Key <= SEARCHSPACE)
//        //.Where(kv => kv.Value.First().X2 + 1 == kv.Value.Last().X1)
//        //.Select(kv => kv.Value.ToList())
//        .ToList();

//    var resultSegment = segments.First(s => s.Value.Count > 1);

//    Console.WriteLine(segments.First(s => s.Value.Count > 1));
//    Console.WriteLine((resultSegment.Value.First().X2 + 1) * 4000000 + resultSegment.Key);
//    Console.WriteLine((resultSegment.Value.First().X2 + 1) * SEARCHSPACE + resultSegment.Key);
//}

#endregion