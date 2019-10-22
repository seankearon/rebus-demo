using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Extensions;

namespace common
{
    internal static class Queues
    {
        public static string WelcomePackService => "WelcomePackService";
        public static string Audit => "Audit";
        public static string Error => "Error";
        public static string CreateCustomerService => "CreateCustomerService";
    }

    internal static class Extensions
    {
        public static void SetLogging(this RebusLoggingConfigurer l)
        {
            l.ColoredConsole();
            //l.None();
        }

        public static string SBConnectionString { get; set; }
        public static string MSSqlConnectionString { get; set; }

        public static string Log(this string msg, ConsoleColor color = ConsoleColor.DarkCyan)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ResetColor();
            return msg;
        }

        public static void SetTitle(string msg)
        {
            Console.Title = Assembly.GetEntryAssembly()?.GetName().Name + " - " + msg;
        }

        public static async void SecondsSleep(this int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }
    }
}