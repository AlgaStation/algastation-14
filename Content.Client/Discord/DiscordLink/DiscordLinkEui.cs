using Content.Client.Eui;
using Content.Shared.Discord.DiscordLink;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.IoC;

namespace Content.Client.Discord.DiscordLink;

/// <summary>
///     Client-side counterpart to <see cref="Content.Server.Discord.DiscordLink.DiscordLinkEui"/>.
///     Renders the popup with the token + Copy/Open buttons and wires the
///     buttons to the local clipboard / URI opener. There's no roundtrip to
///     the server for either action: the state is already in the EUI.
///     Matched to the server class by simple name (Robust EUI uses
///     LooseGetType for the type lookup).
/// </summary>
[UsedImplicitly]
public sealed class DiscordLinkEui : BaseEui
{
    private readonly DiscordLinkWindow _window;

    public DiscordLinkEui()
    {
        var clipboard = IoCManager.Resolve<IClipboardManager>();
        var uri = IoCManager.Resolve<IUriOpener>();

        _window = new DiscordLinkWindow();
        _window.CopyButton.OnPressed += _ => clipboard.SetText(_window.Token);
        _window.OpenDiscordButton.OnPressed += _ =>
        {
            if (!string.IsNullOrWhiteSpace(_window.InviteUrl))
                uri.OpenUri(_window.InviteUrl);
        };
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not DiscordLinkEuiState s)
            return;
        _window.SetState(s.Token, s.InviteUrl, s.ChannelName);
    }
}
