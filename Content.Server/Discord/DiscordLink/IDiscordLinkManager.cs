namespace Content.Server.Discord.DiscordLink;

/// <summary>
///     Server-side manager that nudges unlinked players toward the algabot
///     /link self-service flow on Discord. Wired in ServerContentIoC and
///     initialized from EntryPoint.
/// </summary>
public interface IDiscordLinkManager
{
    void Initialize();
}
