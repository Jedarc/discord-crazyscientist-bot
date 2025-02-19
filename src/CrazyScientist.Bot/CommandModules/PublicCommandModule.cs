using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CrazyScientist.Bot.Commands;

public class PublicCommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<PublicCommandModule> _logger;

    public PublicCommandModule(
        ILogger<PublicCommandModule> logger)
    {
        _logger = logger;
    }

    [SlashCommand("ping", "responds with a 'pong'")]
    public async Task PingAsync()
    {
        await RespondAsync("Pong!");
    }

    [SlashCommand("echo", "repeat the input")]
    public async Task EchoAsync(string input)
    {
        await RespondAsync($"**{Context.User.Mention} said:** _{input}_");
    }

    public override Task BeforeExecuteAsync(ICommandInfo command)
    {
        _logger.LogInformation($"ExecutingAsync command {command.Name}");
        return Task.CompletedTask;
    }

    public override Task AfterExecuteAsync(ICommandInfo command)
    {
        _logger.LogInformation($"ExecutedAsync command {command.Name}");
        return Task.CompletedTask;
    }

    public override void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
    {
        _logger.LogInformation($"Building module {module.Name}");
    }
}
