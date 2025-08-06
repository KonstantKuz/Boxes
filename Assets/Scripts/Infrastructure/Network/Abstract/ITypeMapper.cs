using System;

namespace Infrastructure.Network.Abstract
{
    public interface ITypeMapper<TResolver>
    {
        TResolver GetKey<TTarget>();
        Type GetType<TTarget>(TResolver key);
    }
}
