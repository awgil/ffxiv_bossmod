namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class P1Explosion(BossModule module) : Components.CastTowers(module, AID.P1Explosion, 3);

class EmblazonCounter(BossModule module) : Components.CastCounter(module, AID.Emblazon);

class SpecterOfTheLost(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(48, 30.Degrees()), (uint)TetherID.SpecterOfTheLost, AID.SpecterOfTheLost);
class SpecterOfTheLostAOE(BossModule module) : Components.StandardAOEs(module, AID.SpecterOfTheLost, new AOEShapeCone(48, 30.Degrees()));

class P2Explosion(BossModule module) : Components.CastTowers(module, AID.AddsExplosion, 3, minSoakers: 3, maxSoakers: 4)
{
    private BitMask TetheredPlayers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.SpearpointPush && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            TetheredPlayers.Set(slot);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.SpearpointPush && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
            TetheredPlayers.Clear(slot);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers |= TetheredPlayers;
    }
}

class StockBreak(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            AddStack(actor, WorldState.FutureTime(8.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StockBreak1:
            case AID.StockBreak2:
                NumCasts++;
                break;
            case AID.StockBreak3:
                NumCasts++;
                Stacks.Clear();
                break;
        }
    }
}

class RosebloodDrop(BossModule module) : Components.Adds(module, (uint)OID.RosebloodDrop1)
{
    public bool Spawned { get; private set; }

    public override void Update()
    {
        Spawned |= ActiveActors.Any();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.RosebloodDrop1, 1);

        if (actor.Role is Role.Healer or Role.Ranged && ActiveActors.MaxBy(a => a.HPMP.CurHP) is { } target)
            hints.SetPriority(target, 2);
    }
}

class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, AID.PerfumedQuietus);

class AlexandrianThunderII(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle Rotation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.ThunderCCW:
                Rotation = 10.Degrees();
                for (var i = 0; i < Sequences.Count; i++)
                    Sequences.Ref(i).Rotation = Rotation;
                break;
            case IconID.ThunderCW:
                Rotation = -10.Degrees();
                for (var i = 0; i < Sequences.Count; i++)
                    Sequences.Ref(i).Rotation = Rotation;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AlexandrianThunderIIStart)
            Sequences.Add(new(new AOEShapeCone(24, 22.5f.Degrees()), caster.Position, caster.Rotation, Rotation, Module.CastFinishAt(spell), 1, 15));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AlexandrianThunderIIStart or AID.AlexandrianThunderIIRepeat)
        {
            NumCasts++;
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }
}

class Bloom1AlexandrianThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.AlexandrianThunderIII, AID.AlexandrianThunderIII, 4, 5)
{
    private readonly Tiles Tiles = module.FindComponent<Tiles>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Tiles != null && Spreads.Count > 0)
            hints.AddForbiddenZone(Tiles.TileShape(), Spreads[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Count > 0 && Tiles != null && Tiles.InActiveTile(actor))
            hints.Add($"GTFO from rose tile!");
    }
}

class Voidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime NextActivation;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 1 && state == 0x00020001)
            NextActivation = WorldState.CurrentTime;
        if (index == 1 && state == 0x00080004)
            NextActivation = default;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NextActivation != default)
            yield return new AOEInstance(new AOEShapeCircle(2), Arena.Center, Activation: NextActivation);
    }
}
