using UnityEngine;
using VContainer;
using VContainer.Unity;
using IrsikSoftware.LogSmith.DI;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Example VContainer LifetimeScope showing LogSmith registration.
    /// Attach this to an empty GameObject in your scene.
    /// </summary>
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Register LogSmith services (ILog, ILogFactory, etc.)
            builder.RegisterLogSmith();

            // Register your game services
            builder.RegisterEntryPoint<GameManager>();
            builder.Register<NetworkManager>(Lifetime.Singleton);
            builder.Register<PlayerController>(Lifetime.Transient);
        }
    }
}
