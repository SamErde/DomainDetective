using System;
using System.Threading.Tasks;

namespace DomainDetective.Example {
    internal class ExampleValidateServerAsnClass {
        public static async Task Run() {
            var analysis = new DnsPropagationAnalysis();
            analysis.LoadBuiltinServers();
            var logger = new InternalLogger();
            logger.OnWarningMessage += (_, e) => Console.WriteLine(e.FullMessage);
            await analysis.ValidateServerAsnsAsync(logger);
        }
    }
}