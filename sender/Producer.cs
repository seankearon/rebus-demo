using System;
using common;
using messages;
using Rebus.Bus;

namespace sender
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
                Console.WriteLine(@"a) PUBLISH a Simon Says message
b) SEND a command 
q) Quit");
                var key = char.ToLower(Console.ReadKey(true).KeyChar);

                switch (key)
                {
                    case 'a':
                        var s1 = $"Published event from SENDER at {DateTime.Now:HH:mm:ss}.";
                        s1.LogColored();
                        Publish(s1);
                        break;
                    case 'b':
                        var s2 = $"Write more features! Sent command from SENDER created at {DateTime.Now:HH:mm:ss}.";
                        s2.LogColored();
                        Send(s2);
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

        void Send(string s)
        {
            var message = new SimonsOrders {Message = s};
            _bus.Send(message).Wait();
        }
    }
}