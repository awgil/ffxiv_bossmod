namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D061CausticGrebuloff;

public enum OID : uint
{
    Boss = 0x34C4, // R=6.65
    WeepingMiasma = 0x34C5, // R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BefoulmentVisual = 25923, // Boss->self, 5.0s cast, single-target
    Befoulment = 25924, // Helper->player, 5.2s cast, range 6 circle, spread
    BlightedWaterVisual = 25921, // Boss->self, 5.0s cast, single-target
    BlightedWater = 25922, // Helper->players, 5.2s cast, range 6 circle, stack
    CertainSolitude = 28349, // Boss->self, no cast, range 40 circle, dorito stack
    CoughUp1 = 25917, // Boss->self, 4.0s cast, single-target
    CoughUpAOE = 25918, // Helper->location, 4.0s cast, range 6 circle
    Miasmata = 25916, // Boss->self, 5.0s cast, range 40 circle
    NecroticFluid = 25919, // WeepingMiasma->self, 6.5s cast, range 6 circle
    NecroticMist = 28348, // Helper->location, 1.3s cast, range 6 circle
    PoxFlail = 25920, // Boss->player, 5.0s cast, single-target, tankbuster
    WaveOfNausea = 28347 // Boss->self, 5.5s cast, range 6-40 donut
}

public enum SID : uint
{
    Necrosis = 2965, // Helper->player, extra=0x0, doom
    CravenCompanionship = 2966, // Boss->player, extra=0x0, turns into Hysteria if ignored
    Hysteria = 296 // Boss->player, extra=0x0
}

sealed class CertainSolitude(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask debuff;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CravenCompanionship)
        {
            debuff.Set(Raid.FindSlot(actor.InstanceID));
            if (Stacks.Count == 0)
            {
                Stacks.Add(new(actor, 1.5f, 4, 4, activation: WorldState.FutureTime(5d)));
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CravenCompanionship)
        {
            debuff.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void Update()
    {
        if (Stacks.Count != 0)
        {
            var raid = Raid.WithSlot(false, true, true);
            var len = raid.Length;

            for (var i = 0; i < len; ++i)
            {
                if (debuff[raid[i].Item1])
                {
                    return;
                }
            }
            Stacks.Clear();
        }
    }
}

sealed class Necrosis(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Necrosis);

sealed class NecroticFluidMist(BossModule module) : Components.Exaflare(module, 6f)
{
    public enum Pattern { None, Southward, Northward }
    public Pattern CurrentWind = Pattern.None;
    private WaveOfNausea? wave;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x2B)
        {
            switch (state)
            {
                case 0x00020080u:
                    CurrentWind = Pattern.Southward;
                    break;
                case 0x00200040u:
                    CurrentWind = Pattern.Northward;
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NecroticFluid)
        {
            var numExplosions = CurrentWind switch
            {
                Pattern.Southward => GetSouthwardExplosions(caster.Position, Arena.Center),
                Pattern.Northward => GetNorthwardExplosions(caster.Position, Arena.Center),
                _ => 0
            };
            var advance = 6f * (CurrentWind == Pattern.Southward ? new WDir(default, 1f) : new(default, -1f));
            Lines.Add(new(caster.Position, advance, Module.CastFinishAt(spell), 2d, numExplosions, 5));
        }
    }

    private static int GetSouthwardExplosions(WPos position, WPos center)
    {
        return position.Z switch
        {
            -184.5f => 3,
            -187.5f => 4,
            -191.4f => 5,
            -194.5f => 6,
            -196.9f => 7,
            _ => position.Z > -178f ? 1 : ((position - center).LengthSq() > 100f && position.Z == -178f) ? 3 : 4,
        };
    }

    private static int GetNorthwardExplosions(WPos position, WPos center)
    {
        return position.Z switch
        {
            -171.5f => 3,
            -168.5f => 4,
            -164.6f => 5,
            -161.5f => 6,
            -159.1f => 7,
            _ => position.Z < -178f ? 1 : ((position - center).LengthSq() > 100f && position.Z == -178f) ? 3 : 4,
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NecroticFluid or (uint)AID.NecroticMist)
        {
            var count = Lines.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    if (Lines.Count == 0)
                    {
                        CurrentWind = Pattern.None;
                    }
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        wave ??= Module.FindComponent<WaveOfNausea>();
        if (wave!.AOEs.Count != 0)
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class Befoulment(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Befoulment, 6f);
sealed class BlightedWater(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BlightedWater, 6f, 4, 4);
sealed class CoughUpAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CoughUpAOE, 6f);

sealed class WaveOfNausea(BossModule module) : Components.GenericAOEs(module)
{
    private readonly NecroticFluidMist _exa = module.FindComponent<NecroticFluidMist>()!;
    public readonly List<AOEInstance> AOEs = [with(2)];
    private readonly AOEShapeDonut donut = new(6f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(Angle start, Angle end)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new ConeHA(spell.LocXZ, 6f, start, end)], [new Circle(new(271.473f, -178.027f), 6f), new Circle(new(261.494f, -178.027f), 6f)], invertForbiddenZone: true);
            AOEs.Add(new(shape, center, default, Module.CastFinishAt(spell), Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(center, default)));
        }
        if (spell.Action.ID == (uint)AID.WaveOfNausea)
        {
            AOEs.Add(new(donut, spell.LocXZ, default, Module.CastFinishAt(spell)));
            if (_exa.CurrentWind == NecroticFluidMist.Pattern.Southward)
            {
                AddAOE(180f.Degrees(), 90f.Degrees());
            }
            else if (_exa.CurrentWind == NecroticFluidMist.Pattern.Northward)
            {
                AddAOE(default, 90f.Degrees());
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaveOfNausea)
        {
            AOEs.Clear();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (AOEs.Count != 0 && _exa.Lines.Count != 0)
        {
            hints.Add("Wait in marked safespot for donut resolve!");
        }
    }
}

sealed class PoxFlail(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.PoxFlail);
sealed class Miasmata(BossModule module) : Components.RaidwideCast(module, (uint)AID.Miasmata);

sealed class D061CausticGrebuloffStates : StateMachineBuilder
{
    public D061CausticGrebuloffStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Necrosis>()
            .ActivateOnEnter<Befoulment>()
            .ActivateOnEnter<BlightedWater>()
            .ActivateOnEnter<CoughUpAOE>()
            .ActivateOnEnter<NecroticFluidMist>()
            .ActivateOnEnter<WaveOfNausea>()
            .ActivateOnEnter<PoxFlail>()
            .ActivateOnEnter<CertainSolitude>()
            .ActivateOnEnter<Miasmata>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792u, NameID = 10313u)]
public sealed class D061CausticGrebuloff(WorldState ws, Actor primary) : BossModule(ws, primary, defaultBounds.Center, defaultBounds)
{
    private static readonly ArenaBoundsCustom defaultBounds = new([new Polygon(new(266.5f, -178f), 19.5f * CosPI.Pi32th, 32)],
    [new Rectangle(new(266.5f, -198.75f), 20f, 2f), new Rectangle(new(266.5f, -157f), 20f, 2f)]);
}
