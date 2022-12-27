/// <summary>
/// This one has probably the worst implementation I have done so far, basically I had several ideas through the exercise and the more I tweaked and refactored it the more unreadable it became
/// And I had to adapt even more for part 2
/// The depth guessing technique I did is kinda inspired by the monte carlo algorithm, and let me tell you that it's really bad to solve this exercise :D
/// Basically we often run into local maximum that are really a pain to overpass, and the whole idea of the depth calculation was to avoid the full depth search that was clearly too heavy to be computed
/// but even with this the implementation got stuck on a lot of possibilities
///
/// TLDR; don't do this, this is a bad solution, but I found the answer after fighting against the exercise so I guess it's a win
/// </summary>

var input = @"Valve TN has flow rate=0; tunnels lead to valves IX, ZZ
Valve DS has flow rate=0; tunnels lead to valves IF, OU
Valve OP has flow rate=0; tunnels lead to valves UH, ZQ
Valve FS has flow rate=0; tunnels lead to valves IF, UH
Valve WO has flow rate=0; tunnels lead to valves IS, RW
Valve KQ has flow rate=0; tunnels lead to valves SI, WZ
Valve IX has flow rate=0; tunnels lead to valves IF, TN
Valve OU has flow rate=0; tunnels lead to valves EB, DS
Valve ZZ has flow rate=10; tunnels lead to valves II, GR, HA, BO, TN
Valve OW has flow rate=0; tunnels lead to valves RI, IS
Valve DV has flow rate=0; tunnels lead to valves FR, MT
Valve ZK has flow rate=0; tunnels lead to valves WG, VE
Valve XB has flow rate=0; tunnels lead to valves WG, HM
Valve XC has flow rate=0; tunnels lead to valves IS, MT
Valve KO has flow rate=0; tunnels lead to valves NH, AA
Valve RN has flow rate=0; tunnels lead to valves AA, MT
Valve ZQ has flow rate=5; tunnels lead to valves MF, LK, OP
Valve MF has flow rate=0; tunnels lead to valves ZQ, BH
Valve HA has flow rate=0; tunnels lead to valves LK, ZZ
Valve GB has flow rate=0; tunnels lead to valves KZ, RW
Valve KZ has flow rate=24; tunnels lead to valves GB, RI
Valve TC has flow rate=0; tunnels lead to valves SI, AA
Valve II has flow rate=0; tunnels lead to valves SI, ZZ
Valve EZ has flow rate=0; tunnels lead to valves DF, MT
Valve LK has flow rate=0; tunnels lead to valves HA, ZQ
Valve DU has flow rate=0; tunnels lead to valves NH, IU
Valve MT has flow rate=3; tunnels lead to valves EZ, XC, RN, DV, RU
Valve GR has flow rate=0; tunnels lead to valves SX, ZZ
Valve SX has flow rate=0; tunnels lead to valves UH, GR
Valve BO has flow rate=0; tunnels lead to valves ZZ, AO
Valve WG has flow rate=16; tunnels lead to valves FR, MX, XB, ZK
Valve IP has flow rate=8; tunnels lead to valves HM, RU, WZ, IU
Valve RI has flow rate=0; tunnels lead to valves OW, KZ
Valve NP has flow rate=0; tunnels lead to valves WN, EB
Valve IF has flow rate=19; tunnels lead to valves IX, DS, VX, FS
Valve AA has flow rate=0; tunnels lead to valves RN, KO, TC, MX
Valve IS has flow rate=15; tunnels lead to valves OW, WO, XC
Valve BH has flow rate=11; tunnel leads to valve MF
Valve SI has flow rate=4; tunnels lead to valves KQ, II, TC
Valve WN has flow rate=0; tunnels lead to valves UH, NP
Valve RW has flow rate=18; tunnels lead to valves WO, GB
Valve DF has flow rate=0; tunnels lead to valves NH, EZ
Valve WZ has flow rate=0; tunnels lead to valves KQ, IP
Valve HM has flow rate=0; tunnels lead to valves XB, IP
Valve VX has flow rate=0; tunnels lead to valves AO, IF
Valve MX has flow rate=0; tunnels lead to valves AA, WG
Valve NH has flow rate=13; tunnels lead to valves VE, KO, DU, DF
Valve RU has flow rate=0; tunnels lead to valves MT, IP
Valve IU has flow rate=0; tunnels lead to valves IP, DU
Valve VE has flow rate=0; tunnels lead to valves ZK, NH
Valve FR has flow rate=0; tunnels lead to valves WG, DV
Valve AO has flow rate=21; tunnels lead to valves BO, VX
Valve EB has flow rate=22; tunnels lead to valves OU, NP
Valve UH has flow rate=12; tunnels lead to valves WN, OP, SX, FS";
var testInput = @"Valve AA has flow rate=0; tunnels lead to valves DD, II, BB
Valve BB has flow rate=13; tunnels lead to valves CC, AA
Valve CC has flow rate=2; tunnels lead to valves DD, BB
Valve DD has flow rate=20; tunnels lead to valves CC, AA, EE
Valve EE has flow rate=3; tunnels lead to valves FF, DD
Valve FF has flow rate=0; tunnels lead to valves EE, GG
Valve GG has flow rate=0; tunnels lead to valves FF, HH
Valve HH has flow rate=22; tunnel leads to valve GG
Valve II has flow rate=0; tunnels lead to valves AA, JJ
Valve JJ has flow rate=21; tunnel leads to valve II";

// It is above 2397
var parsed = input.Split("\n");

var nodes = new List<Node>();

foreach (var line in parsed)
{
    var words = line.Split(' ');

    var name = words[1];

    var found = nodes.Find(n => n.Name == name);

    Node node;
    if (found != null)
    {
        node = found;
        node.Rate = long.Parse(words[4].Replace("rate=", "").Replace(";", ""));
    }
    else
    {
        node = new Node
        {
            Name = name,
            Rate = long.Parse(words[4].Replace("rate=", "").Replace(";", ""))
        };
        nodes.Add(node);
    }


    var rawNeighbors = words.Skip(9).ToList();
    foreach (var rawNeighbor in rawNeighbors)
    {
        Node nodeNeighbor;

        var neighborName = rawNeighbor.Replace(",", "");
        var foundNeighbor = nodes.Find(n => n.Name == neighborName);

        if (foundNeighbor != null)
        {
            nodeNeighbor = foundNeighbor;
        }
        else
        {
            nodeNeighbor = new Node
            {
                Name = neighborName
            };
            nodes.Add(nodeNeighbor);
        }

        node.Neighbors.Add(new Neighbor
        {
            Node = nodeNeighbor
        });
    }
}

// Graph simplification

foreach (var node in nodes.Where(n => n.Rate == 0 && n.Name != "AA").ToList())
{
    // Simplify neighbors
    foreach (var neighbor in node.Neighbors)
    {
        var workingNode = neighbor.Node;
        var neighborToChange = workingNode.Neighbors.Find(ne => ne.Node == node);
        var saveDist = neighborToChange.Distance;
        workingNode.Neighbors.Remove(neighborToChange);

        foreach (var otherNeighbor in node.Neighbors)
        {
            if (otherNeighbor == neighbor) continue;

            workingNode.Neighbors.Add(new Neighbor
            {
                Node = otherNeighbor.Node,
                Distance = saveDist + otherNeighbor.Distance
            });
        }

        workingNode.Neighbors = workingNode.Neighbors.GroupBy(nei => nei.Node.Name).Select(g => g.OrderBy(n => n.Distance).First()).ToList();
    }


    nodes.Remove(node);
}


var maxTicks = 30L;
var maxPressure = 0L;
var alternativeMaxPressure = 0L;

var minDepth = 16;
// Play with this value to give larger depth to the guess
var maxDepth = 30;
var tasks = new List<Task>();

for (var depth = minDepth; depth <= maxDepth; depth++)
{
    var locDepth = depth;
    //tasks.Add(Task.Run(() =>
    //{
        var realOpenValves = new HashSet<string>();
        var realPressure = 0L;

        var me = new Person
        {
            CurrNode = nodes.Find(n => n.Name == "AA"),
            CurrTicks = 5L
        };
        var elephant = new Person
        {
            CurrNode = nodes.Find(n => n.Name == "AA"),
            CurrTicks = 5L
        };

        while (me.CurrTicks < maxTicks || elephant.CurrTicks < maxTicks)
        {
            if (me.CurrTicks < maxTicks)
            {
                MakeMove(realOpenValves, me, elephant, ref realPressure, locDepth);
            }

            if (elephant.CurrTicks < maxTicks)
            {
                MakeMove(realOpenValves, elephant, me, ref realPressure, locDepth);
            }
        }

        Console.WriteLine();

        maxPressure = Math.Max(realPressure, maxPressure);
    //}));
}

//await Task.WhenAll(tasks);

Console.WriteLine($"End of traversals tries max pressure is {Math.Max(maxPressure, alternativeMaxPressure)}");

void MakeMove(HashSet<string> realOpenValves, Person actor, Person passive, ref long realPressure, int depth)
{
    var guessActor = new Person
    {
        CurrNode = actor.CurrNode,
        CurrTicks = actor.CurrTicks
    };
    var guessPassive = new Person
    {
        CurrNode = passive.CurrNode,
        CurrTicks = passive.CurrTicks
    };

    var (pressure, move) = GuessNextMove(realOpenValves.ToHashSet(), guessActor, guessPassive, realPressure, guessActor.CurrTicks, depth);
    Console.WriteLine($"Current Depth : {depth}, Current Tick : {guessActor.CurrTicks}, Found Max Pressure : {pressure}, Alternative Max Pressure : {alternativeMaxPressure}");


    if (move == "END")
    {
        // Next move is unguessable because it is an end move
        return;
    }
    else if (move == "NOMOVE")
    {
        // Should not happen, no move chosen
        actor.CurrTicks = 30;
        return;
    }
    else if (move == "OPEN")
    {
        // Open the valve on the current node
        OpenValve(actor, ref realPressure, realOpenValves);
    }
    else
    {
        var dest = move;
        MoveNode(actor, actor.CurrNode.Neighbors.Find(nei => nei.Node.Name == dest));
    }
}

// Pressure is returning the max pressure from above stack level
// Move is returning the move chosen to obtain this pressure on the above stack level
// Moves can be : END, OPEN, or any valve name
(long Pressure, string Move) GuessNextMove(HashSet<string> openValves, Person actor, Person passive, long currPressure, long realTicks, long realDepth)
{
    var locDepth = Math.Min(maxTicks, realTicks + realDepth);

    if (actor.CurrTicks == locDepth)
        return (currPressure, "END");

    var maxLocPressure = (Pressure: -1L, Move: "NOMOVE");

    if (CanOpenValve(actor.CurrNode, openValves))
    {
        var nextOpen = openValves.ToHashSet();
        var nextPressure = currPressure;
        var nextActor = new Person
        {
            CurrNode = actor.CurrNode,
            CurrTicks = actor.CurrTicks
        };
        var nextPassive = new Person
        {
            CurrNode = passive.CurrNode,
            CurrTicks = passive.CurrTicks
        };
        OpenValve(nextActor, ref nextPressure, nextOpen);
        // Reverse actor and passive on next guess turn
        var locPressure = GuessNextMove(nextOpen, nextPassive, nextActor, nextPressure, realTicks, realDepth);
        if (maxLocPressure.Pressure < locPressure.Pressure)
        {
            maxLocPressure = (Pressure: locPressure.Pressure, Move: "OPEN");
        }
    }

    //if (currTick == locDepth - 1) return (currPressure, "END");

    foreach (var neighbor in actor.CurrNode.Neighbors)
    {
        if (!CanMoveToDest(actor.CurrTicks, neighbor, locDepth) || !ShouldMoveToDest(actor.CurrTicks, neighbor, openValves, passive.CurrNode.Name))
            continue;

        var nextActor = new Person
        {
            CurrNode = actor.CurrNode,
            CurrTicks = actor.CurrTicks
        };
        var nextPassive = new Person
        {
            CurrNode = passive.CurrNode,
            CurrTicks = passive.CurrTicks
        };

        var nextPressure = currPressure;
        var nextOpen = openValves.ToHashSet();
        MoveNode(nextActor, neighbor);
        // Reverse actor and passive on next guess turn
        var locPressure = GuessNextMove(nextOpen, nextPassive, nextActor, nextPressure, realTicks, realDepth);
        if (maxLocPressure.Pressure < locPressure.Pressure)
        {
            maxLocPressure = (Pressure: locPressure.Pressure, Move: neighbor.Node.Name);
        }
    }

    alternativeMaxPressure = Math.Max(maxLocPressure.Pressure, alternativeMaxPressure);

    if (maxLocPressure.Move == "NOMOVE") maxLocPressure.Pressure = currPressure;

    return maxLocPressure;
}


bool CanMoveToDest(long currTicks, Neighbor dest, long locDepth)
{
    return currTicks + dest.Distance <= locDepth;
}

bool ShouldMoveToDest(long currTicks, Neighbor dest, HashSet<string> openValves, string otherPersonPos)
{
    return dest.Node.Name != otherPersonPos;
    //if (openValves.Contains(dest.Node.Name)) return false;

    var score = !openValves.Contains(dest.Node.Name) ? (dest.Node.Rate / dest.Distance) : 0
                + dest.Node.Neighbors.Sum(nei => !openValves.Contains(nei.Node.Name) ?  nei.Node.Rate / nei.Distance : 0)
                + dest.Node.Neighbors.Sum(nei => nei.Node.Neighbors.Sum(neinei => !openValves.Contains(neinei.Node.Name) ? neinei.Node.Rate / neinei.Distance : 0));

    return score > 1;
}

void MoveNode(Person actor, Neighbor dest)
{
    actor.CurrNode = dest.Node;
    actor.CurrTicks += dest.Distance;
}

bool CanOpenValve(Node node, HashSet<string> openValves)
{
    return node.Rate > 0 && !openValves.Contains(node.Name);
}

void OpenValve(Person actor, ref long locPressure, HashSet<string> openValves)
{
    actor.CurrTicks++;
    //currNode.TickOnOpen = locTicks;
    locPressure += actor.CurrNode.Rate * (maxTicks - actor.CurrTicks + 1);

    openValves.Add(actor.CurrNode.Name);
}

List<Node> DeepCopyGraph(List<Node> src)
{
    var dest = new List<Node>();

    foreach (var srcNode in src)
    {
        var found = dest.Find(n => n.Name == srcNode.Name);

        Node node;
        if (found != null)
        {
            node = found;
            node.Rate = srcNode.Rate;
        }
        else
        {
            node = new Node
            {
                Name = srcNode.Name,
                Rate = srcNode.Rate
            };
            dest.Add(node);
        }


        foreach (var srcNodeNeighbor in srcNode.Neighbors)
        {
            Node nodeNeighbor;

            var foundNeighbor = dest.Find(n => n.Name == srcNodeNeighbor.Node.Name);

            if (foundNeighbor != null)
            {
                nodeNeighbor = foundNeighbor;
            }
            else
            {
                nodeNeighbor = new Node
                {
                    Name = srcNodeNeighbor.Node.Name
                };
                dest.Add(nodeNeighbor);
            }

            node.Neighbors.Add(new Neighbor
            {
                Node = nodeNeighbor,
                Distance = srcNodeNeighbor.Distance
            });
        }

    }

    return dest;
}



public class Node
{
    public List<Neighbor> Neighbors { get; set; } = new();
    public string Name { get; init; }
    public long Rate { get; set; }

    // This was a Djikstra idea, not sure it will work let's put that aside for now
    //public long Tentative { get; set; } = 0;
    //public long TickOnOpen { get; set; } = 0;

}

public class Neighbor
{
    public Node Node { get; set; }
    public int Distance { get; set; } = 1;
}

public class Person
{
    public Node CurrNode { get; set; }
    public long CurrTicks { get; set; }
}

