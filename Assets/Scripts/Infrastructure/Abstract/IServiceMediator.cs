using System;
using Infrastructure.Bootstrap;

namespace Infrastructure.Abstract
{
    public interface IServiceMediator : IPostBuildInjectable
    {
        Type BindType { get; }
    }
}
