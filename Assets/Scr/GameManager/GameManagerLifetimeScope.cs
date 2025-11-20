using MessagePipe;
using RinaSymbol;
using VContainer;

namespace Scr.GameManager {
    public class GameManagerLifetimeScope : SymbolLifetimeScope {
        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);

            builder
                .RegisterMessagePipe();
        }
    }
}