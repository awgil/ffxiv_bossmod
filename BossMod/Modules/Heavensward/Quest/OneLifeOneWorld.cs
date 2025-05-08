namespace BossMod.Heavensward.Quest.OneLifeForOneWorld;

public enum OID : uint
{
    Boss = 0x17CD,
    KnightOfDarkness = 0x17CE, // R0.500, x1
    BladeOfLight = 0x1EA19E,
}

public enum AID : uint
{
    Overpower = 6683, // Boss->self, 2.5s cast, range 6+R 90-degree cone
    UnlitCyclone = 6684, // Boss->self, 4.0s cast, range 5+R circle
    UnlitCycloneAdds = 6685, // 18D6->location, 4.0s cast, range 9 circle
    Skydrive = 6686, // Boss->player, 5.0s cast, single-target
    UtterDestruction = 6690, // FirstWard->self, 3.0s cast, range 20+R circle
    RollingBladeCircle = 6691, // Boss->self, 3.0s cast, range 7 circle
    RollingBladeCone = 6692, // FirstWard->self, 3.0s cast, range 60+R 30-degree cone
}

public enum SID : uint
{
    Invincibility = 325, // KnightOfDarkness->Boss/FirstWard, extra=0x0
}

class Overpower(BossModule module) : Components.StandardAOEs(module, AID.Overpower, new AOEShapeCone(7, 45.Degrees()));
class UnlitCyclone(BossModule module) : Components.StandardAOEs(module, AID.UnlitCyclone, new AOEShapeCircle(6));
class UnlitCycloneAdds(BossModule module) : Components.StandardAOEs(module, AID.UnlitCycloneAdds, 9);

class Skydrive(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), 23, AID.Skydrive, centerAtTarget: true);
class SkydrivePuddle(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(0x1EA19C).Where(x => x.EventState != 7));
class RollingBlade(BossModule module) : Components.StandardAOEs(module, AID.RollingBladeCircle, new AOEShapeCircle(7));
class RollingBladeCone(BossModule module) : Components.StandardAOEs(module, AID.RollingBladeCone, new AOEShapeCone(60, 15.Degrees()));

class BladeOfLight(BossModule module) : BossComponent(module)
{
    public Actor? Blade => WorldState.Actors.FirstOrDefault(x => x.OID == 0x1EA19E && x.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Blade != null)
            Arena.Actor(Blade, ArenaColor.Vulnerable);
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [0x17CE, 0x17CF, 0x17D0, 0x17D1]);
class TargetPriorityHandler(BossModule module) : BossComponent(module)
{
    private Actor? Knight => Module.Enemies(OID.KnightOfDarkness).FirstOrDefault();
    private Actor? Covered => WorldState.Actors.FirstOrDefault(s => s.OID != 0x18D6 && s.FindStatus(SID.Invincibility) != null);
    private Actor? BladeOfLight => WorldState.Actors.FirstOrDefault(s => (OID)s.OID == OID.BladeOfLight && s.IsTargetable);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Knight != null && Covered != null)
            Arena.AddLine(Knight.Position, Covered.Position, ArenaColor.Danger, 1);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (BladeOfLight != null)
        {
            var playerIsAttacked = false;

            foreach (var e in hints.PotentialTargets)
            {
                if (e.Actor.TargetID == actor.InstanceID)
                {
                    playerIsAttacked = true;
                    e.Priority = 0;
                }
                else
                {
                    e.Priority = AIHints.Enemy.PriorityUndesirable;
                }
            }

            if (!playerIsAttacked)
            {
                if (actor.DistanceToHitbox(BladeOfLight) > 5.5f)
                    hints.ForcedMovement = (BladeOfLight!.Position - actor.Position).ToVec3();
                else
                    hints.InteractWithTarget = BladeOfLight;
            }
        }
        else
        {
            foreach (var e in hints.PotentialTargets)
            {
                if (e.Actor == Knight)
                    e.Priority = 2;
                else if (e.Actor == Covered)
                    e.Priority = 0;
                else
                    e.Priority = 1;
            }
        }
    }
}

class UtterDestruction(BossModule module) : Components.StandardAOEs(module, AID.UtterDestruction, new AOEShapeDonut(10, 20));

class WarriorOfDarknessStates : StateMachineBuilder
{
    public WarriorOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<TargetPriorityHandler>()
            .ActivateOnEnter<UnlitCyclone>()
            .ActivateOnEnter<UnlitCycloneAdds>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<SkydrivePuddle>()
            .ActivateOnEnter<BladeOfLight>()
            .ActivateOnEnter<RollingBlade>()
            .ActivateOnEnter<RollingBladeCone>()
            .ActivateOnEnter<UtterDestruction>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67885, NameID = 5240)]
public class WarriorOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
