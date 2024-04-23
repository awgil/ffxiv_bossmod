using System.Linq.Expressions;

namespace BossMod;

// utility for invoking parametrized constructors in generic context
public static class New<T>
{
    private static readonly Type TypeOfT = typeof(T);

    public static T Create() => Build.Instantiate();
    public static T Create<Arg1>(Arg1 arg1) => Build<Arg1>.Instantiate(arg1);
    public static T Create<Arg1, Arg2>(Arg1 arg1, Arg2 arg2) => Build<Arg1, Arg2>.Instantiate(arg1, arg2);

    public static Func<T> Constructor() => Build.Constructor;
    public static Func<Arg1, T> Constructor<Arg1>() => Build<Arg1>.Constructor;
    public static Func<Arg1, Arg2, T> Construct<Arg1, Arg2>() => Build<Arg1, Arg2>.Constructor;

    public static Func<T> ConstructorDerived(Type derived) => Build.CreateConstructor(derived);
    public static Func<Arg1, T> ConstructorDerived<Arg1>(Type derived) => Build<Arg1>.CreateConstructor(derived);
    public static Func<Arg1, Arg2, T> ConstructorDerived<Arg1, Arg2>(Type derived) => Build<Arg1, Arg2>.CreateConstructor(derived);

    private static class Build
    {
        public static readonly Func<T> Constructor = CreateConstructor(TypeOfT);
        public static T Instantiate() => Constructor.Invoke();

        public static Func<T> CreateConstructor(Type builtType) => Expression.Lambda<Func<T>>(Expression.New(builtType)).Compile();
    }

    private static class Build<Arg1>
    {
        public static readonly Func<Arg1, T> Constructor = CreateConstructor(TypeOfT);
        public static T Instantiate(Arg1 arg) => Constructor.Invoke(arg);

        public static Func<Arg1, T> CreateConstructor(Type builtType)
        {
            var (expression, param) = CreateExpressions(builtType, typeof(Arg1));
            return Expression.Lambda<Func<Arg1, T>>(expression, param).Compile();
        }
    }

    private static class Build<Arg1, Arg2>
    {
        public static Func<Arg1, Arg2, T> Constructor = CreateConstructor(TypeOfT);
        public static T Instantiate(Arg1 arg1, Arg2 arg2) => Constructor.Invoke(arg1, arg2);

        public static Func<Arg1, Arg2, T> CreateConstructor(Type builtType)
        {
            var (expression, param) = CreateExpressions(builtType, typeof(Arg1), typeof(Arg2));
            return Expression.Lambda<Func<Arg1, Arg2, T>>(expression, param).Compile();
        }
    }

    private static (NewExpression, ParameterExpression[]) CreateExpressions(Type builtType, params Type[] argsTypes)
    {
        var constructorInfo = builtType.GetConstructor(argsTypes) ?? throw new ArgumentException($"{builtType} is not constructible from ({string.Join(", ", argsTypes.Select(t => t))})");
        var constructorParameters = argsTypes.Select(Expression.Parameter).ToArray();
        var expression = Expression.New(constructorInfo, constructorParameters);
        return (expression, constructorParameters);
    }
}
