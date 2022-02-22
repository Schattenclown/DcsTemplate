using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DcsTemplate.Model.Discord;
using DcsTemplate.Model.HelpClasses;

namespace DcsTemplate
{
    /// <summary>
    /// The program boot class.
    /// </summary>
    class Program
    {
        private static DiscordBot dBot;
        /// <summary>
        /// the boot task
        /// </summary>
        /// <returns>Nothing</returns>
        static async Task Main()
        {
            await Task.Run(async () =>
            {
                try
                {
                    dBot = new DiscordBot();
                    await dBot.RunAsync();
                }
                catch (Exception ex)
                {
                    Reset.RestartProgram(ex);
                }
            });
        }
    }
}