using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Discord.DiscordLink;

/// <summary>
///     State pushed from the server-side DiscordLinkEui to the client window.
///     Carries the redemption token plus the invite URL and channel name so
///     the popup is self-contained. The client doesn't have to query the
///     server again to render the prompt.
/// </summary>
[Serializable, NetSerializable]
public sealed class DiscordLinkEuiState : EuiStateBase
{
    public string Token { get; init; } = string.Empty;
    public string InviteUrl { get; init; } = string.Empty;
    public string ChannelName { get; init; } = string.Empty;
}
