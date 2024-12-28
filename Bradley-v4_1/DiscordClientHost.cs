using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bradley_v4_1
{
    internal sealed class DiscordClientHost: IHostedService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LavalinkManager _lavalinkManager;
        private string _token;
        public DiscordClientHost(
        DiscordSocketClient discordSocketClient,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(discordSocketClient);
            ArgumentNullException.ThrowIfNull(interactionService);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _discordSocketClient = discordSocketClient;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _lavalinkManager = new LavalinkManager();

            string value = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"Environment variable '{"DISCORD_TOKEN"}' is not set.");
            }
            else
            {
                _token = value;
            }
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _lavalinkManager.DownloadAndStartLavalinkAsync();
            _discordSocketClient.InteractionCreated += InteractionCreated;
            _discordSocketClient.Ready += ClientReady;
            _discordSocketClient.Log += Log;
            var token = _token;

            await _discordSocketClient.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await _discordSocketClient.StartAsync().ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _lavalinkManager.StopLavalink();
            await _discordSocketClient
                .StopAsync()
                .ConfigureAwait(false);
        }

        private Task InteractionCreated(SocketInteraction interaction)
        {
            var interactionContext = new SocketInteractionContext(_discordSocketClient, interaction);
            return _interactionService!.ExecuteCommandAsync(interactionContext, _serviceProvider);
        }

        private async Task ClientReady()
        {
            await _interactionService
                .AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider)
                .ConfigureAwait(false);

            // Put your guild id to test here
            await _interactionService
                .RegisterCommandsToGuildAsync(198892786584518656)
                .ConfigureAwait(false);
        }
    }
}
