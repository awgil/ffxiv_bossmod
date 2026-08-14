namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

public class Doubling(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4)
{
    private readonly WPos southPos = new(-759.988f, -785.093f);
    private readonly WPos westPos = new(-779.885f, -804.986f);
    private readonly WPos eastPos = new(-740.095f, -805.000f);
    private readonly WPos northPos = new(-760.000f, -824.900f);

    public int side; // 1 -> NE & SW, 2 -> NW & SE
    private enum RoleDirection { North, East, South, West }
    public enum TowerSide { East, West }

    private readonly Dictionary<ulong, RoleDirection> tetheredRoles = [];
    private readonly List<ulong> eastPlayers = [];
    private readonly List<ulong> westPlayers = [];
    private readonly List<ulong> priorityPlayers = [];

    private readonly List<Tower> eastTowers = [];
    private readonly List<Tower> westTowers = [];
    private bool towerWave1 = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        // Only check the north actor to see which side is safe since
        if (spell.Action.ID is var id && id is (uint)AID.FableflightLeft or (uint)AID.FableflightLeft1)
        {
            if (caster.Position.AlmostEqual(northPos, 1.0f))
            {
                side = 1;
            }
        }
        else if (id is (uint)AID.FableflightRight or (uint)AID.FableflightRight1)
        {
            if (caster.Position.AlmostEqual(northPos, 1.0f))
            {
                side = 2;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);

        if (spell.Action.ID == (uint)AID.TowerExplosion)
        {
            if (!towerWave1)
            {
                towerWave1 = true;
                priorityPlayers.RemoveAt(0);
                priorityPlayers.RemoveAt(0);
                eastTowers.Clear();
                westTowers.Clear();
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.TowerTether)
        {
            var position = source.Position;

            if (position.AlmostEqual(northPos, 1.0f))
            {
                tetheredRoles.TryAdd(tether.Target, RoleDirection.North);
            }
            else if (position.AlmostEqual(eastPos, 1.0f))
            {
                tetheredRoles.TryAdd(tether.Target, RoleDirection.East);
            }
            else if (position.AlmostEqual(westPos, 1.0f))
            {
                tetheredRoles.TryAdd(tether.Target, RoleDirection.West);
            }
            else if (position.AlmostEqual(southPos, 1.0f))
            {
                tetheredRoles.TryAdd(tether.Target, RoleDirection.South);
            }

            if (tetheredRoles.Count == 4)
            {
                foreach (var player in tetheredRoles.Keys)
                {
                    var role = tetheredRoles[player];

                    var westSide = side == 1
                        ? (role is RoleDirection.West or RoleDirection.South)
                        : (role is RoleDirection.North or RoleDirection.West);

                    if (westSide)
                    {
                        westPlayers.Add(player);
                    }
                    else
                    {
                        eastPlayers.Add(player);
                    }
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BlueRug)
        {
            priorityPlayers.Add(targetID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (eastTowers.Count != 2 || westTowers.Count != 2)
        {
            SortTowerOrder();
        }

        var priority = priorityPlayers.Take(2).Contains(pc.InstanceID);
        var eastPlayer = eastPlayers.Contains(pc.InstanceID);
        var list = eastPlayer ? eastTowers : westTowers;

        ulong targetId = 0;
        if (list.Count >= 2)
        {
            targetId = list[priority ? 0 : 1].ActorID;
        }

        foreach (var tower in Towers.Take(4))
        {
            var colour = tower.ActorID == targetId ? Colors.Safe : Colors.Danger;
            tower.Shape.Outline(Arena, tower.Position, tower.Rotation, colour, 2f);
        }
    }

    // Orders the tower on that specific side, with the 1st slot being the closer tower & the 2nd slot being the further tower
    private void SortTowerOrder()
    {
        eastTowers.Clear();
        westTowers.Clear();

        foreach (var tower in Towers)
        {
            if (tower.Position.X > Arena.Center.X)
            {
                eastTowers.Add(tower);
            }
            else
            {
                westTowers.Add(tower);
            }
        }

        eastTowers.Sort((a, b) =>
        {
            var compare = SortTowerAngle(a, TowerSide.East).Deg.CompareTo(SortTowerAngle(b, TowerSide.East).Deg);
            return compare;
        });

        westTowers.Sort((a, b) =>
        {
            var compare = SortTowerAngle(a, TowerSide.West).Deg.CompareTo(SortTowerAngle(b, TowerSide.West).Deg);
            return compare;
        });
    }

    // Helper function for sorting the towers base on their angle depending on the safe side
    private Angle SortTowerAngle(Tower tower, TowerSide towerSide)
    {
        var position = tower.Position;
        var angle = Angle.FromDirection(position - Module.Center);

        Angle startAngle;
        bool clockwise;

        if (towerSide == TowerSide.East)
        {
            startAngle = side == 1 ? 180f.Degrees() : default;
            clockwise = side == 1;
        }
        else
        {
            startAngle = side == 1 ? default : 180f.Degrees();
            clockwise = side == 1;
        }

        var angleDifference = startAngle.DistanceToAngle(angle);
        if (clockwise)
        {
            angleDifference = -angleDifference;
        }

        if (angleDifference < (Angle)default)
        {
            angleDifference += 360f.Degrees();
        }

        return angleDifference;
    }
}