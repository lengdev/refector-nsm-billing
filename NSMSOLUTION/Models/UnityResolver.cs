using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Unity;

public class UnityResolver : IDependencyResolver
{
    protected IUnityContainer container;

    public UnityResolver(IUnityContainer container)
    {
        this.container = container;
    }

    public object GetService(Type serviceType)
    {
        try
        {
            return container.Resolve(serviceType);
        }
        catch
        {
            return null;
        }
    }

    public IEnumerable<object> GetServices(Type serviceType)
    {
        try
        {
            return container.ResolveAll(serviceType);
        }
        catch
        {
            return new List<object>();
        }
    }

    public IDependencyScope BeginScope()
    {
        return new UnityResolver(container.CreateChildContainer());
    }

    public void Dispose()
    {
        container.Dispose();
    }
}
