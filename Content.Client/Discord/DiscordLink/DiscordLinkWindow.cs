using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Discord.DiscordLink;

/// <summary>
///     Popup that shows the player their one-shot Discord link token and the
///     two actions they need to take: copy the token, open the invite.
///
///     Token is rendered in a big monospace label so it's easy to retype if
///     the clipboard didn't work for some reason. The instruction sentence
///     names the channel ("/link <token>" in #channel) so the player knows
///     where to paste after switching to Discord.
/// </summary>
public sealed class DiscordLinkWindow : DefaultWindow
{
    public readonly Button CopyButton;
    public readonly Button OpenDiscordButton;
    private readonly Label _tokenLabel;
    private readonly RichTextLabel _instructionLabel;

    public string Token { get; private set; } = string.Empty;
    public string InviteUrl { get; private set; } = string.Empty;

    public DiscordLinkWindow()
    {
        Title = "Привяжи Discord";
        MinSize = new Vector2(420, 0);

        _instructionLabel = new RichTextLabel();
        _tokenLabel = new Label
        {
            StyleClasses = { "monospace" },
            FontColorOverride = Robust.Shared.Maths.Color.White,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Robust.Shared.Maths.Thickness(0, 8, 0, 8),
        };
        // The token is short, but bump the visual weight so it stands out as
        // the actionable thing in the popup.
        _tokenLabel.AddStyleClass("LabelHeading");

        CopyButton = new Button { Text = "Скопировать токен" };
        OpenDiscordButton = new Button { Text = "Открыть Discord" };

        ContentsContainer.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Margin = new Robust.Shared.Maths.Thickness(8),
            Children =
            {
                _instructionLabel,
                _tokenLabel,
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    Align = AlignMode.Center,
                    SeparationOverride = 12,
                    Children = { CopyButton, OpenDiscordButton },
                },
            },
        });
    }

    public void SetState(string token, string inviteUrl, string channelName)
    {
        Token = token;
        InviteUrl = inviteUrl;

        _tokenLabel.Text = token;

        var channel = string.IsNullOrWhiteSpace(channelName) ? "канале бота" : $"#{channelName}";
        _instructionLabel.SetMessage(
            $"Твой SS14-аккаунт не привязан к Discord. Нажми кнопку справа чтобы открыть наш Discord, в канале {channel} отправь:\n\n    [bold]/link {token}[/bold]");

        // Open is meaningless if we have no invite URL.
        OpenDiscordButton.Disabled = string.IsNullOrWhiteSpace(inviteUrl);
    }
}
