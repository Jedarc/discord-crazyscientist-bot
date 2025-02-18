using CrazyScientist.Bot.Settings;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CrazyScientist.Bot.Services
{
    public class BotWorker : IHostedService
    {
        private readonly ILogger<BotWorker> _logger;
        private readonly BotOptions _botOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;

        public BotWorker(
            ILogger<BotWorker> logger,
            IOptions<BotOptions> options,
            IServiceProvider serviceProvider,
            DiscordSocketClient client,
            InteractionService interactionService)
        {
            _logger = logger;
            _botOptions = options.Value;
            _serviceProvider = serviceProvider;
            _client = client;
            _interactionService = interactionService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += InteractionCreatedAsync;
            _client.MessageReceived += MessageReceivedAsync;

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _interactionService.InteractionExecuted += HandleInteractionExecute;

            await _client.LoginAsync(TokenType.Bot, _botOptions.Token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _client.StopAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            var fullLogMessage = log.ToString();

            switch (log.Severity)
            {
                case LogSeverity.Info:
                    _logger.LogInformation(log.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(fullLogMessage);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(fullLogMessage);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(log.Exception, fullLogMessage);
                    break;
                case LogSeverity.Critical:
                    _logger.LogCritical(fullLogMessage);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(fullLogMessage);
                    break;
                default:
                    _logger.LogInformation(fullLogMessage);
                    break;
            }

            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            await ClearGlobalCommandsAsync();
            await _interactionService.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Bot is ready and commands have been registered!");
        }

        private async Task ClearGlobalCommandsAsync()
        {
            try
            {
                await _client.Rest.DeleteAllGlobalCommandsAsync();
                _logger.LogInformation("Old global commands have been removed!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing global commands.");
            }
        }

        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

                if (!result.IsSuccess && interaction is SocketSlashCommand slashCommand)
                    _logger.LogError("Error executing command {commandName}: ({error}) {errorReason}", slashCommand.CommandName, result.Error, result.ErrorReason);
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (!message.Author.IsBot)
                _logger.LogInformation($"Message received: {message.Content}");

            return Task.CompletedTask;
        }

        private Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
                _logger.LogError("Error executing command {commandName}: {resultError}", commandInfo.Name, result.Error);

            return Task.CompletedTask;
        }
    }
}
