namespace DiscordCancelModal.Modules;

public class ModalCancel : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ModalCancel> _logger;

    public ModalCancel(ILogger<ModalCancel> logger)
    {
        _logger = logger;
    }

    [SlashCommand("show_modal", "モーダルを表示します")]
    public async Task ShowModal()
    {
        var modal = new ModalBuilder()
            .WithTitle("Fav Food")
            .WithCustomId("food_menu")
            .AddTextInput("What??", "food_name", placeholder: "Pizza")
            .AddTextInput("Why??", "food_reason", TextInputStyle.Paragraph,
                placeholder: "Because it's delicious")
            .Build();

        await RespondWithModalAsync(modal);

        var eventTrigger = new TaskCompletionSource<bool>();
        
        Task OnModalSubmitted(SocketModal socketModal)
        {
            if (socketModal.Data.CustomId == "food_menu")
            {
                eventTrigger.SetResult(true);
            }

            return Task.CompletedTask;
        }

        Context.Client.ModalSubmitted += OnModalSubmitted;

        var trigger = eventTrigger.Task;
        var timeout = Task.Delay(TimeSpan.FromMinutes(2));
        var task = await Task.WhenAny(trigger, timeout).ConfigureAwait(false);

        Context.Client.ModalSubmitted -= OnModalSubmitted;
        if (task != trigger)
        {
            _logger.LogInformation($"modal_id: {modal.CustomId} cancelled.");
            await FollowupAsync("modal cancelled.", ephemeral: true);
        }
    }

    [ModalInteraction("food_menu")]
    public async Task HandleModalResponse(Modal modal)
    {
        await Context.Interaction.FollowupAsync("モーダルが送信されました。", ephemeral: true);
    }
}
