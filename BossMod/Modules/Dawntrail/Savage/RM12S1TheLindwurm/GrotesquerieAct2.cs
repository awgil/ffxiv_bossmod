namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class Act2PhagocyteSpotlight(BossModule module) : Components.StandardAOEs(module, AID.PhagocyteSpotlightFixed, 5);

class CruelCoil(BossModule module) : Components.GenericAOEs(module)
{
    bool _active;

    public static readonly AOEShape SkinsplitterShape = new AOEShapeDonutSector(9, 14, 157.5f.Degrees());
    public static readonly AOEShape CoilShape = new AOEShapeDonutSector(9.5f, 13.5f, 157.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(SkinsplitterShape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees());
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_active)
        {
            hints.TemporaryObstacles.Add(CoilShape.CheckFn(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SkinsplitterBoss)
            _active = true;

        if ((AID)spell.Action.ID == AID.ConstrictorKill)
            _active = false;
    }
}

class Act2Assignments(BossModule module) : BossComponent(module)
{
    public record struct Assignment(int Order, char Letter); // A = 1, B = 2

    public readonly Assignment[] Assignments = new Assignment[8];

    public int PartnerSlot(int slot)
    {
        var a = Assignments.BoundSafeAt(slot);
        return Array.FindIndex(Assignments, s => s.Order == a.Order && s.Letter != a.Letter);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var letter = (SID)status.ID switch
        {
            SID.BondsA => 'A',
            SID.BondsB => 'B',
            _ => '\0'
        };
        if (letter > 0 && Raid.TryFindSlot(actor, out var slot))
        {
            Assignments[slot].Letter = letter;
            Finish();
        }

        var order = (SID)status.ID switch
        {
            SID.FirstInLine => 1,
            SID.SecondInLine => 2,
            SID.ThirdInLine => 3,
            SID.FourthInLine => 4,
            _ => 0
        };
        if (order > 0 && Raid.TryFindSlot(actor, out var s2))
        {
            Assignments[s2].Order = order;
            Finish();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var ass = Assignments.BoundSafeAt(slot);
        if (ass.Letter == 0 || ass.Order == 0)
            return;

        hints.Add($"Order: {ass.Order}, assignment: {(ass.Letter == 'A' ? "out" : "in")}", false);
    }

    void Finish()
    {
        if (Assignments.Any(a => a.Order == 0 || a.Letter == 0))
            return;
    }
}

class RoilingMass(BossModule module) : Components.GenericTowers(module, AID.RoilingMass2)
{
    readonly Act2Assignments _assignments = module.FindComponent<Act2Assignments>()!;

    public int BlobCasts;
    public int ChainCasts;

    int _blobOrder;
    bool _towersB;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BloodVessel && id == 0x2488)
        {
            _blobOrder++;
            if (_blobOrder == 5)
            {
                _blobOrder = 1;
                _towersB = true;
            }

            var letter = _towersB ? 'B' : 'A';
            var activation = _towersB
                // chain towers explode 4s after spawn
                ? WorldState.FutureTime(4)
                // blob towers explode 5 seconds apart
                : Towers.Count == 0 ? WorldState.FutureTime(23.1f) : Towers[^1].Activation.AddSeconds(5);

            var soakOrder = _blobOrder switch
            {
                1 => 3,
                2 => 4,
                3 => 1,
                4 => 2,
                _ => 0
            };
            var soaker = Array.FindIndex(_assignments.Assignments, a => a.Order == soakOrder && a.Letter == letter);
            Towers.Add(new(actor.Position, 3, 1, 1, ~BitMask.Build(soaker), activation));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Towers.Any(t => t.ForbiddenSoakers[slot] && t.IsInside(actor)))
        {
            hints.Add("GTFO from tower!");
        }
        else if (Towers.FindIndex(t => !t.ForbiddenSoakers[slot] && t.IsInside(actor)) is var soakedIndex && soakedIndex >= 0) // note: this assumes towers don't overlap
        {
            var count = Towers[soakedIndex].NumInside(Module);
            if (count < Towers[soakedIndex].MinSoakers)
                hints.Add("Too few soakers in the tower!");
            else if (count > Towers[soakedIndex].MaxSoakers)
                hints.Add("Too many soakers in the tower!");
        }
        else if (Towers.Any(t => !t.ForbiddenSoakers[slot] && !t.CorrectAmountInside(Module) && t.Activation < WorldState.FutureTime(5)))
        {
            hints.Add("Soak the tower!");
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var i = (AID)spell.Action.ID switch
        {
            AID.RoilingMass2 => 1,
            AID.RoilingMass1 => 2,
            _ => 0
        };
        if (i > 0)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 0.5f));
            if (i == 1)
                BlobCasts++;
            else
                ChainCasts++;
        }
    }
}

class Act2DramaticLysis(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Cell)
        {
            Spreads.Add(new(source, 4, WorldState.FutureTime(4)));
            if (WorldState.Actors.Find(tether.Target) is { } tar)
                Spreads.Add(new(tar, 4, WorldState.FutureTime(4)));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Cell)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == source.InstanceID || s.Target.InstanceID == tether.Target);
        }
    }
}

class Act2CellChains(BossModule module) : Components.Chains(module, (uint)TetherID.Cell, null, 14)
{
    readonly Act2Assignments _assignments = module.FindComponent<Act2Assignments>()!;
    readonly DateTime[] _chainedAt = new DateTime[8];

    public int NumChains;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.BondsA or SID.BondsB && Raid.TryFindSlot(actor, out var slot))
        {
            _chainedAt[slot] = status.ExpireAt;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);

        if (tether.ID == TID)
        {
            NumChains++;
            if (Raid.TryFindSlot(source, out var s1))
                _chainedAt[s1] = default;
            if (Raid.TryFindSlot(tether.Target, out var s2))
                _chainedAt[s2] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var deadline = _chainedAt[slot];
        if (deadline > WorldState.CurrentTime && deadline < WorldState.FutureTime(4))
        {
            var partner = _assignments.PartnerSlot(slot);
            if (Raid[partner] is { } p)
                hints.Add("Stack with partner!", !actor.Position.InCircle(p.Position, 2));
            return;
        }

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var deadline = _chainedAt[slot];
        if (deadline > WorldState.CurrentTime && deadline < WorldState.FutureTime(2.5f))
        {
            var partner = _assignments.PartnerSlot(slot);
            if (Raid[partner] is { } p)
                hints.AddForbiddenZone(ShapeContains.Donut(p.Position, 2, 60), deadline);
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var deadline = _chainedAt[pcSlot];
        if (deadline > WorldState.CurrentTime && deadline < WorldState.FutureTime(2.5f))
        {
            var partner = _assignments.PartnerSlot(pcSlot);
            if (Raid[partner] is { } p)
                Arena.AddCircle(p.Position, 2, ArenaColor.Safe);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_chainedAt[playerSlot] != default && _assignments.PartnerSlot(playerSlot) == pcSlot)
            return PlayerPriority.Interesting;

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }
}

// 171.64 -> 176.56
class Constrictor(BossModule module) : Components.GenericAOEs(module, AID.ConstrictorKill)
{
    int _splitterCounter;
    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(13), Arena.Center, Activation: _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SkinsplitterDonut)
        {
            _splitterCounter++;
            if (_splitterCounter >= 14)
                _activation = WorldState.FutureTime(4.9f);
        }

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}
