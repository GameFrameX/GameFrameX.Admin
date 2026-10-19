using System.Reflection;
using GameFrameX.Core.Api;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace GameFrameX.Web.Api.ApiRouting;

public sealed class AdminApiRuntimeChangeProvider : IAdminApiRuntimeChangeProvider, IActionDescriptorChangeProvider
{
    private readonly ApplicationPartManager _partManager;
    private CancellationTokenSource _changeTokenSource = new();

    public AdminApiRuntimeChangeProvider(ApplicationPartManager partManager)
    {
        _partManager = partManager;
    }

    public IChangeToken GetChangeToken()
    {
        return new CancellationChangeToken(_changeTokenSource.Token);
    }

    public void AddAssembliesWithNotifyChanges(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        if (!string.IsNullOrWhiteSpace(name)
            && _partManager.ApplicationParts.OfType<AssemblyPart>().All(u => u.Name != name))
        {
            _partManager.ApplicationParts.Add(new AssemblyPart(assembly));
        }

        NotifyChanges();
    }

    public void RemoveAssembliesWithNotifyChanges(string assemblyName)
    {
        var remove = _partManager.ApplicationParts
                                 .OfType<AssemblyPart>()
                                 .Where(u => string.Equals(u.Name, assemblyName, StringComparison.Ordinal))
                                 .Cast<ApplicationPart>()
                                 .ToList();
        foreach (var part in remove)
        {
            _partManager.ApplicationParts.Remove(part);
        }

        NotifyChanges();
    }

    private void NotifyChanges()
    {
        var previous = Interlocked.Exchange(ref _changeTokenSource, new CancellationTokenSource());
        previous.Cancel();
        previous.Dispose();
    }
}