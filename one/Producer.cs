using System;
using common;
using messages;
using Rebus.Bus;

namespace one
{
    public class Producer
    {
        private readonly IBus _bus;

        public Producer(IBus bus)
        {
            _bus = bus;
        }

        public void Produce()
        {
            var keepRunning = true;

            while (keepRunning)
            {
                Console.WriteLine(@"a) PUBLISH a Simon Says event
b) SEND DEFERRED command
q) Quit");
                var key = char.ToLower(Console.ReadKey(true).KeyChar);

                switch (key)
                {
                    case 'a':
                        var s1 = $"Message from ONE at {DateTime.Now:HH:mm:ss}.";
                        s1.LogColored();
                        Publish(s1);
                        break;
                    case 'b':
                        var s2 = $"Keep writing those features!  DEFERRED message from ONE created at {DateTime.Now:HH:mm:ss}.";
                        s2.LogColored();
                        SendDeferred(s2);
                        break;
                    case 'q':
                        Console.WriteLine("Quitting");
                        keepRunning = false;
                        break;
                }
            }

            "Listening".LogColored();
            Console.ReadLine();
        }

        void Publish(string s)
        {
            var message = new SimonsNews {Message = s};
            _bus.Publish(message).Wait();
        }

        /// <summary>
        /// Sends, not published. Reasoning: https://github.com/rebus-org/Rebus/issues/777
        /// </summary>
        void SendDeferred(string s)
        {
            var message = new SimonsNews {Message = s};
            _bus.Defer(TimeSpan.FromSeconds(5), message).Wait();
        }
    }
}