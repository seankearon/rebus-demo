using System.Threading.Tasks;
using common;
using messages;
using Rebus.Handlers;

namespace one
{
    public class SimonSaysHandler : IHandleMessages<SimonsNews>
    {
        public Task Handle(SimonsNews m)
        {
            $"Received a {m.GetType().Name}: {m.Message}".LogColored();
            return Task.CompletedTask;
        }
    }
}