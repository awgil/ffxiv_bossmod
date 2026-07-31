namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA3AbsoluteVirtue;

sealed class BrightDarkAuroraExplosion(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private readonly List<(Actor source, ulong target)> tetherByActor = [with(8)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = tetherByActor.Count;
        if (count == 0)
            return [];

        var isActorTarget = false;

        for (var i = 0; i < count; ++i)
        {
            var tether = tetherByActor[i];
            if (tether.target == actor.InstanceID)
            {
                isActorTarget = true;
                break;
            }
        }
        var countAdj = isActorTarget ? count - 1 : count;
        var aoes = new AOEInstance[countAdj];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var tether = tetherByActor[i];
            if (tether.target != actor.InstanceID)
                aoes[index++] = new(circle, tetherByActor[i].source.Position.Quantized(), risky: !isActorTarget);
        }
        return aoes;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        tetherByActor.Add((source, tether.Target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        tetherByActor.Remove((source, tether.Target));
    }
}

abstract class Towers(BossModule module, uint oid, uint tid) : Components.GenericTowersOpenWorld(module)
{
    private readonly List<(Actor source, Actor target)> tetherByActor = [with(4)];
    private const string Hint = "Stand in a tower of opposite tether element!";

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == oid && state == 0x00040008u)
        {
            var count = Towers.Count;
            var pos = actor.Position;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].Position == pos)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == oid)
        {
            Towers.Add(new(actor.Position, 2f, 1, 1, [], WorldState.FutureTime(20d)));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == tid)
        {
            tetherByActor.Add((source, WorldState.Actors.Find(tether.Target)!));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == tid)
        {
            tetherByActor.Remove((source, WorldState.Actors.Find(tether.Target)!));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = tetherByActor.Count;
        if (count == 0)
            return;

        var isActorTarget = false;

        for (var i = 0; i < count; ++i)
        {
            var tether = tetherByActor[i];
            if (tether.target == actor)
            {
                isActorTarget = true;
                break;
            }
        }

        if (isActorTarget)
        {
            var soakedIndex = -1;
            var countT = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < countT; ++i)
            {
                ref var t = ref towers[i];
                t.InitializeAllowedSoakers(Module);
                if (t.AllowedSoakers!.Contains(actor) && t.IsInside(actor))
                {
                    soakedIndex = i;
                    break;
                }
            }
            if (soakedIndex == -1)
            {
                hints.Add(Hint);
            }
            else
            {
                hints.Add(Hint, false);
            }
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var count = tetherByActor.Count;
        if (count == 0)
        {
            return;
        }

        Actor? source = null;
        for (var i = 0; i < count; ++i)
        {
            var tether = tetherByActor[i];
            if (tether.target == pc)
            {
                source = tether.source;
                break;
            }
        }
        if (source != null)
        {
            Arena.AddLine(source.Position, pc.Position);
            Arena.ZoneCircleOutline(source.Position, 2f);
            Arena.Actor(source, Colors.Object, true);
        }
    }

    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        HashSet<Actor> allowed = [with(4)];
        for (var i = 0; i < tetherByActor.Count; ++i)
        {
            allowed.Add(tetherByActor[i].target);
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            towers[i].AllowedSoakers = allowed;
        }
    }
}

sealed class BrightAuroraTether(BossModule module) : Towers(module, (uint)OID.DarkAuroraHelper, (uint)TetherID.BrightAurora);
sealed class DarkAuroraTether(BossModule module) : Towers(module, (uint)OID.BrightAuroraHelper, (uint)TetherID.DarkAurora);
