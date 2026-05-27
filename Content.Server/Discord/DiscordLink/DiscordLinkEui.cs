using Content.Server.EUI;
using Content.Shared.Discord.DiscordLink;
using Content.Shared.Eui;

namespace Content.Server.Discord.DiscordLink;

/// <summary>
///     Server-side EUI that shows a player their freshly minted link token in
///     a popup with Copy and Open-Discord buttons. State (token + invite URL
///     + channel) is captured at construction. The EUI doesn't change while
///     it's open, and closing the window doesn't invalidate the token: it
///     stays redeemable until its TTL on the bot side.
/// </summary>
public sealed class DiscordLinkEui : BaseEui
{
    private readonly string _token;
    private readonly string _inviteUrl;
    private readonly string _channelName;

    public DiscordLinkEui(string token, string inviteUrl, string channelName)
    {
        _token = token;
        _inviteUrl = inviteUrl;
        _channelName = channelName;
    }

    public override EuiStateBase GetNewState()
    {
        return new DiscordLinkEuiState
        {
            Token = _token,
            InviteUrl = _inviteUrl,
            ChannelName = _channelName,
        };
    }
}
