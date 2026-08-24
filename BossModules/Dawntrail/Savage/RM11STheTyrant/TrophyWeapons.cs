namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class VoidStardustBait(BossModule module) : BossComponent(module)
{
    public bool Enabled = true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Cometite)
            Enabled = false;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Enabled)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(pc.Position, 6, 0xFF000000, 2);
            Arena.AddCircle(pc.Position, 6, ArenaColor.Danger);
        }
    }
}

class Cometite(BossModule module) : Components.StandardAOEs(module, AID.Cometite, 6);
class Comets(BossModule module) : Components.CastStackSpread(module, AID.CrushingCometStack, AID.CometSpread, 6, 6);

abstract class WeaponsAOE(BossModule module) : Components.GenericAOEs(module)
{
    protected readonly List<Actor> Weapons = [];
    protected DateTime Next;
    protected WPos Previous;
    public bool Risky = true;

    public static readonly AOEShape Axe = new AOEShapeCircle(8);
    public static readonly AOEShape Scythe = new AOEShapeDonut(5, 60);
    public static readonly AOEShape Sword = new AOEShapeCross(40, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Next != default && Weapons.FirstOrDefault() is { } w)
        {
            var rotation = (w.Position - Previous).ToAngle();
            var shape = (OID)w.OID switch
            {
                OID.Axe => Axe,
                OID.Scythe => Scythe,
                OID.Sword => Sword,
                _ => null
            };
            if (shape != null)
                yield return new(shape, w.Position, rotation, Next, Risky: Risky);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AssaultEvolvedSwordAOE or AID.AssaultEvolvedAxeAOE or AID.AssaultEvolvedScytheAOE)
        {
            NumCasts++;
            Previous = caster.Position;
            Next = WorldState.FutureTime(5.2f);
            if (Weapons.Count > 0)
                Weapons.RemoveAt(0);
        }
    }
}

abstract class WeaponsBait(BossModule module) : Components.UntelegraphedBait(module)
{
    protected readonly List<Actor> Weapons = [];
    protected DateTime Next;

    public static readonly AOEShape Stack = new AOEShapeCircle(8);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AssaultEvolvedSwordAOE or AID.AssaultEvolvedAxeAOE or AID.AssaultEvolvedScytheAOE)
        {
            NumCasts++;
            Next = WorldState.FutureTime(5.2f);
            if (Weapons.Count > 0)
                Weapons.RemoveAt(0);
            Redraw();
        }
    }

    protected void Redraw()
    {
        CurrentBaits.Clear();
        if (Weapons.FirstOrDefault() is not { } weapon)
            return;

        switch ((OID)weapon.OID)
        {
            case OID.Axe:
                CurrentBaits.Add(new(weapon.Position, new(0xFF), new AOEShapeCircle(6), activation: Next, count: 1, stackSize: 6, centerAtTarget: true));
                break;
            case OID.Sword:
                CurrentBaits.Add(new(weapon.Position, Raid.WithSlot().WhereActor(h => h.Role == Role.Healer).Take(2).Mask(), new AOEShapeRect(60, 3), Next, count: 2, stackSize: 4));
                break;
            case OID.Scythe:
                CurrentBaits.Add(new(weapon.Position, new(0xFF), new AOEShapeCone(60, 15f.Degrees()), Next));
                break;
        }
    }
}

abstract class WeaponsHints(BossModule module) : BossComponent(module)
{
    protected readonly List<Actor> Weapons = [];
    protected readonly RM11STheTyrantConfig Config = Service.Config.Get<RM11STheTyrantConfig>();
    protected readonly PartyRolesConfig Roles = Service.Config.Get<PartyRolesConfig>();
    protected DateTime Next;
    protected WPos Previous;

    protected readonly WPos[] Destination = new WPos[8];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id is 0x11D1 or 0x11D2 or 0x11D3)
            Weapons.Add(actor);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Destination[pcSlot] != default)
            Arena.AddCircle(Destination[pcSlot], 0.75f, ArenaColor.Safe);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Destination[slot] != default)
            movementHints.Add((actor.Position, Destination[slot], ArenaColor.Safe));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AssaultEvolvedLongCast)
        {
            Next = Module.CastFinishAt(spell, 2.2f);
            Previous = caster.Position;
            var weapon1 = Weapons.FirstOrDefault(w => w.Position.InCone(caster.Position, spell.Rotation, 10.Degrees()));
            if (weapon1 != null)
            {
                var sorted = Weapons.ClockOrder(weapon1, Arena.Center).ToList();
                Weapons.Clear();
                Weapons.AddRange(sorted);
                Redraw();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AssaultEvolvedSwordAOE or AID.AssaultEvolvedAxeAOE or AID.AssaultEvolvedScytheAOE)
        {
            Previous = caster.Position;
            Next = WorldState.FutureTime(5.2f);
            if (Weapons.Count > 0)
                Weapons.RemoveAt(0);
            Redraw();
        }
    }

    protected void Redraw()
    {
        Array.Fill(Destination, default);
        if (Weapons.FirstOrDefault() is not { } weapon)
            return;

        var rotation = (weapon.Position - Previous).ToAngle();

        switch ((OID)weapon.OID)
        {
            case OID.Scythe:
                foreach (var (i, _) in Raid.WithSlot())
                {
                    var assignment = Roles[Raid.Members[i].ContentId];
                    if (assignment != PartyRolesConfig.Assignment.Unassigned)
                    {
                        var order = Config.WeaponHintsScythe[assignment];
                        if (order >= 0)
                        {
                            var offset = rotation - 45.Degrees() * order;
                            Destination[i] = weapon.Position + offset.ToDirection() * 3.5f;
                        }
                    }
                }
                break;
            case OID.Sword:
                var sh = Config.WeaponHintsSword;
                if (sh == RM11STheTyrantConfig.HintSword.None)
                    return;

                foreach (var (i, _) in Raid.WithSlot())
                {
                    var lp = Roles[Raid.Members[i].ContentId] switch
                    {
                        PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 => 1,
                        PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.H2 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 => 2,
                        _ => 0
                    };

                    if (lp == 0)
                        return;

                    if (sh == RM11STheTyrantConfig.HintSword.Flipped)
                        lp = lp == 1 ? 2 : 1;

                    var angle = rotation + (lp == 1 ? 135.Degrees() : -135.Degrees());
                    Destination[i] = weapon.Position + angle.ToDirection() * 8.5f;
                }
                break;
            case OID.Axe:
                var dirToCenter = (Arena.Center - weapon.Position).Normalized();
                foreach (var (i, _) in Raid.WithSlot())
                    Destination[i] = weapon.Position + dirToCenter * 10;
                break;
        }
    }
}

class TrophyWeaponsAOE(BossModule module) : WeaponsAOE(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id is 0x11D1 or 0x11D2 or 0x11D3)
            Weapons.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // TODO: we can actually determine first weapon as soon as they spawn based on their rotation relative to the boss
        if ((AID)spell.Action.ID == AID.AssaultEvolvedLongCast)
        {
            Next = Module.CastFinishAt(spell, 2.2f);
            Previous = caster.Position;
            var weapon1 = Weapons.FirstOrDefault(w => w.Position.InCone(caster.Position, spell.Rotation, 10.Degrees()));
            if (weapon1 != null)
            {
                var sorted = Weapons.ClockOrder(weapon1, Arena.Center).ToList();
                Weapons.Clear();
                Weapons.AddRange(sorted);
            }
        }
    }
}

class TrophyWeaponsBait(BossModule module) : WeaponsBait(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id is 0x11D1 or 0x11D2 or 0x11D3)
            Weapons.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultEvolvedLongCast)
        {
            Next = Module.CastFinishAt(spell, 2.2f);
            var weapon1 = Weapons.FirstOrDefault(w => w.Position.InCone(caster.Position, spell.Rotation, 10.Degrees()));
            if (weapon1 != null)
            {
                var sorted = Weapons.ClockOrder(weapon1, Arena.Center).ToList();
                Weapons.Clear();
                Weapons.AddRange(sorted);
                Redraw();
            }
        }
    }
}

class TrophyWeaponsHints(BossModule module) : WeaponsHints(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id is 0x11D1 or 0x11D2 or 0x11D3)
            Weapons.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultEvolvedLongCast)
        {
            Next = Module.CastFinishAt(spell, 2.2f);
            Previous = caster.Position;
            var weapon1 = Weapons.FirstOrDefault(w => w.Position.InCone(caster.Position, spell.Rotation, 10.Degrees()));
            if (weapon1 != null)
            {
                var sorted = Weapons.ClockOrder(weapon1, Arena.Center).ToList();
                Weapons.Clear();
                Weapons.AddRange(sorted);
                Redraw();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AssaultEvolvedSwordAOE or AID.AssaultEvolvedAxeAOE or AID.AssaultEvolvedScytheAOE)
        {
            Previous = caster.Position;
            Next = WorldState.FutureTime(5.2f);
            if (Weapons.Count > 0)
                Weapons.RemoveAt(0);
            Redraw();
        }
    }
}

class UltimateTrophyWeaponsAOE(BossModule module) : WeaponsAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.UltimateTrophyWeapons)
            Previous = caster.Position;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID is OID.Axe or OID.Scythe or OID.Sword && id is 0x11D1 or 0x11D2 or 0x11D3)
        {
            Weapons.Add(actor);
            if (Weapons.Count == 1)
                Next = WorldState.FutureTime(8.6f);
        }
    }
}

class UltimateTrophyWeaponsBait(BossModule module) : WeaponsBait(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID is OID.Axe or OID.Scythe or OID.Sword && id is 0x11D1 or 0x11D2 or 0x11D3)
        {
            Weapons.Add(actor);
            if (Weapons.Count == 1)
            {
                Next = WorldState.FutureTime(8.6f);
                Redraw();
            }
        }
    }
}

class UltimateTrophyWeaponsHints(BossModule module) : WeaponsHints(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.UltimateTrophyWeapons)
            Previous = caster.Position;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID is OID.Axe or OID.Scythe or OID.Sword && id is 0x11D1 or 0x11D2 or 0x11D3)
        {
            Weapons.Add(actor);
            if (Weapons.Count == 1)
            {
                Next = WorldState.FutureTime(8.6f);
                Redraw();
            }
        }
    }
}
