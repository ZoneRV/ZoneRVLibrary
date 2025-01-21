using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Models;

public class TryObject<T> : Tuple<bool, T?>
{
    [MemberNotNullWhen(returnValue: true, member: nameof(Object))]
    public bool Success { get { return base.Item1; } }

    public T? Object { get { return base.Item2; } }

    public TryObject(bool success, T? o) : base(success, o)
    {
    }
}