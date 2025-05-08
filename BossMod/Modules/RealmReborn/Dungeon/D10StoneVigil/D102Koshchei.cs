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
    private readonly IReadOnlyList<Actor> _maelstroms = module.Enemies(OID.MaelstromVisual);

    public override void Update()
    {
        foreach (var m in _maelstroms)
        {
            var line = FindLine(m.Position.Z);
            if (m.IsDead && line != null)
                Lines.Remove(line);
            else if (!m.IsDead && line == null)
                Lines.Add(new() { Next = m.Position, Advance = new(-1.745f, 0), TimeToMove = 0.6f, ExplosionsLeft = 4, MaxShownExplosions = 4 });
        }
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

            if (line.MaxShownExplosions <= 4)
            {
                // first move
                line.MaxShownExplosions = 10;
                line.ExplosionsLeft = 15;
            }
            AdvanceLine(line, caster.Position);
        }
    }

    private Line? FindLine(float z) => Lines.Find(l => MathF.Abs(l.Next.Z - z) < 1);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1678)]
public class D102Koshchei(WorldState ws, Actor primary) : BossModule(ws, primary, new(40, -80), new ArenaBoundsSquare(10));
