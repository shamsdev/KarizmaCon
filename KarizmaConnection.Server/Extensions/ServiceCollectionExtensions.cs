using System.Reflection;
using KarizmaConnection.Server.Attributes;
using KarizmaConnection.Server.Base;
using KarizmaConnection.Server.RequestHandler;

namespace KarizmaConnection.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddKarizmaConnection(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddEventHandlers();
        services.AddRequestHandlers();
    }

    private static void AddRequestHandlers(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var handlerRegistry = new RequestHandlerRegistry();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<RequestHandlerAttribute>() != null);

            foreach (var handlerType in handlerTypes)
            {
                services.AddTransient(handlerType);

                var handlerAttribute = handlerType.GetCustomAttribute<RequestHandlerAttribute>();

                var methodsWithActions = handlerType.GetMethods()
                    .Where(m => m.GetCustomAttribute<ActionAttribute>() != null);

                foreach (var method in methodsWithActions)
                {
                    var actionAttribute = method.GetCustomAttribute<ActionAttribute>();

                    var address = $"{handlerAttribute!.Name}/{actionAttribute!.Name}".ToLowerInvariant();

                    handlerRegistry.AddHandler(address, new RequestHandlerAction(handlerType, method));
                }
            }
        }

        services.AddSingleton(handlerRegistry);
    }


    private static void AddEventHandlers(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<EventHandlerAttribute>() != null);

            foreach (var handlerType in handlerTypes)
                services.AddTransient(typeof(BaseEventHandler), handlerType);
        }
    }
}