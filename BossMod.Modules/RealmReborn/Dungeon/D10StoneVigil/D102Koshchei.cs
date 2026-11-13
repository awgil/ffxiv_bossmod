namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D102Koshchei;

public enum OID : uint
{
    Boss = 0x38C7, // x1
    MaelstromVisual = 0x38C8, // spawn during fight
    MaelstromHelper = 0x38D0, // x4
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    SpikedTail = 28732, // Boss->player, 5.0s cast, tankbuster
    SonicStorm = 29053, // Boss->location, 3.0s cast, range 6 aoe
    Typhoon = 28730, // Boss->self, 3.0s cast, visual
    TyphoonAOE = 28731, // MaelstromHelper->self, no cast, range 3 aoe
}

class SpikedTail(BossModule module) : Components.SingleTargetCast(module, AID.SpikedTail);
class SonicStorm(BossModule module) : Components.StandardAOEs(module, AID.SonicStorm, 6);

class Typhoon(BossModule module) : Components.Exaflare(module, 3)
{
    private class MaelstromLine(Actor source) : Line
    {
        public readonly Actor Source = source;
    }

    private const float EndX = 30.4f;
    private const float Step = 1.745f;
    private readonly IReadOnlyList<Actor> _maelstroms = module.Enemies(OID.MaelstromVisual);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe with { Origin = new(Math.Max(EndX, aoe.Origin.X), aoe.Origin.Z) };
    }

    public override void Update()
    {
        // The visual can disappear before the helper's final pulse at the wall.
        Lines.RemoveAll(l => ((MaelstromLine)l).Source.IsDeadOrDestroyed && (l.ExplosionsLeft <= 0 || WorldState.CurrentTime > l.NextExplosion.AddSeconds(l.TimeToMove)));
        foreach (var m in _maelstroms)
            if (!m.IsDeadOrDestroyed && !Lines.Any(l => ((MaelstromLine)l).Source == m))
                Lines.Add(new MaelstromLine(m) { Next = m.Position, Advance = new(-Step, 0), TimeToMove = 0.6f, ExplosionsLeft = 4, MaxShownExplosions = 4 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TyphoonAOE && caster.Position.X < 56)
        {
            var line = FindLine(caster.Position.Z);
            if (line == null)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X} @ {caster.Position}");
                return;
            }

            line.MaxShownExplosions = 10;
            if (caster.Position.X > EndX + 0.1f)
            {
                // Batched events can share a position; derive remaining movement from distance instead of counting them.
                line.ExplosionsLeft = (int)MathF.Ceiling((caster.Position.X - EndX) / Step) + 2;
            }
            else if (line.Advance != default)
            {
                line.Advance = default;
                line.ExplosionsLeft = 2;
            }
            AdvanceLine(line, caster.Position);
        }
    }

    private Line? FindLine(float z) => Lines.Find(l => l.ExplosionsLeft > 0 && MathF.Abs(l.Next.Z - z) < 1);
}

class D102KoshcheiStates : StateMachineBuilder
{
    public D102KoshcheiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpikedTail>()
            .ActivateOnEnter<SonicStorm>()
            .ActivateOnEnter<Typhoon>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1678)]
public class D102Koshchei(ModuleInit init) : BossModule(init, new(40, -80), new ArenaBoundsSquare(10));
