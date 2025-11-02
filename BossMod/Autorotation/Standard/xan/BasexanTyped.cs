using BossMod.Autorotation.xan;

namespace BossMod.Autorotation;

public record struct TypedStrategyTrack<T>(T Value, StrategyValue Raw) where T : struct
{
    public readonly float ExpireIn => Raw.ExpireIn;

    public static implicit operator T(TypedStrategyTrack<T> self) => self.Value;

    public override readonly string ToString() => $"Track({Value}, Raw={Raw})";
}

static class ValueConverter
{
    public static T FromValues<T>(StrategyValues values) where T : struct
    {
        object val = default(T);

        var i = 0;
        foreach (var field in typeof(T).GetFields())
        {
            switch (values.Values[i])
            {
                case StrategyValueTrack t:
                    field.SetValue(val, Activator.CreateInstance(field.FieldType, [Enum.ToObject(field.FieldType.GenericTypeArguments[0], t.Option), t]));
                    break;
                case StrategyValueFloat f:
                    field.SetValue(val, f.Value);
                    break;
                case StrategyValueInt i2:
                    field.SetValue(val, i2.Value);
                    break;
            }
            i++;
        }

        return (T)val;
    }
}

public abstract class BasexanTyped<AID, TraitID, TValues>(RotationModuleManager manager, Actor player, PotionType potType) : Basexan<AID, TraitID>(manager, player, potType)
    where TValues : struct
    where AID : struct, Enum
    where TraitID : Enum
{
    public abstract void Exec(TValues strategy, AIHints.Enemy? primaryTarget);

    public sealed override void Exec(StrategyValues strategy, AIHints.Enemy? primaryTarget)
    {
        Exec(ValueConverter.FromValues<TValues>(strategy), primaryTarget);
    }
}

public abstract class CastxanTyped<AID, TraitID, TValues>(RotationModuleManager manager, Actor player, PotionType potType = PotionType.None) : BasexanTyped<AID, TraitID, TValues>(manager, player, potType)
    where TValues : struct
    where AID : struct, Enum
    where TraitID : Enum
{
    protected sealed override float GCDLength => SpellGCDLength;
}
