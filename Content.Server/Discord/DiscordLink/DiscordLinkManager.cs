using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Player;

namespace Content.Server.Discord.DiscordLink;

/// <summary>
///     On player connect, asks algabot whether this Wizden account has a
///     Discord link. If not, mints a one-shot token and DMs the player a
///     system message telling them how to redeem it on Discord.
///
///     Failure modes are intentionally quiet: if the bot is down or
///     misconfigured (empty URL/secret), the manager logs once at startup and
///     skips the per-connect calls. We don't want a broken Discord
///     integration to block players from joining the round.
/// </summary>
public sealed partial class DiscordLinkManager : IDiscordLinkManager
{
    // No `readonly` and no constructor: SS14's IoC writes [Dependency] fields
    // via reflection after construction, and the source generator needs the
    // class to be `partial` to emit a matching InjectDependencies override.
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IChatManager _chat = default!;

    private ISawmill _sawmill = default!;
    private AlgabotClient? _client;
    private string _url = string.Empty;
    private string _secret = string.Empty;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("discord.link");

        // Invite URL lives on the bot side (returned with each issued token),
        // so we don't need a CVar for it here. URL and secret are all we own.
        _cfg.OnValueChanged(CCVars.AlgabotUrl, OnUrlChanged, true);
        _cfg.OnValueChanged(CCVars.AlgabotSecret, OnSecretChanged, true);

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        if (_client == null)
            _sawmill.Info("algabot integration disabled: algabot.url or algabot.secret is empty");
        else
            _sawmill.Info($"algabot integration enabled, bot at {_url}");
    }

    private void OnUrlChanged(string v)
    {
        _url = v;
        RebuildClient();
    }

    private void OnSecretChanged(string v)
    {
        _secret = v;
        RebuildClient();
    }

    private void RebuildClient()
    {
        if (string.IsNullOrWhiteSpace(_url) || string.IsNullOrWhiteSpace(_secret))
        {
            _client = null;
            return;
        }
        // Short timeout: algabot is on the same compose network, anything
        // taking more than a couple seconds is dead. A player joining doesn't
        // benefit from us hanging for 30s waiting on a sick bot.
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        _client = new AlgabotClient(http, _url, _secret);
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        // InGame is the moment chat is reliably available to the player —
        // Connected is too early, the client hasn't finished handshake and
        // the chat panel hasn't initialized.
        if (e.NewStatus != SessionStatus.InGame)
            return;

        if (_client == null)
            return;

        try
        {
            await CheckAndPromptAsync(e.Session, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _sawmill.Warning($"link check failed for {e.Session.Name}: {ex.Message}");
        }
    }

    private async Task CheckAndPromptAsync(ICommonSession session, CancellationToken cancel)
    {
        var userId = session.UserId.UserId.ToString();
        var status = await _client!.GetLinkStatusAsync(userId, cancel);
        if (status.Linked)
            return;

        var tok = await _client.IssueTokenAsync(userId, session.Name, cancel);

        var minutes = tok.TtlSeconds / 60;
        var invite = string.IsNullOrWhiteSpace(tok.InviteUrl) ? string.Empty : $" ({tok.InviteUrl})";
        var channel = string.IsNullOrWhiteSpace(tok.LinkChannel) ? "канал бота" : $"#{tok.LinkChannel}";
        var msg =
            $"[Discord] Твой аккаунт пока не привязан к Discord. " +
            $"Зайди в наш Discord{invite}, в {channel}, и отправь:\n" +
            $"    {tok.RedeemHint}\n" +
            $"Токен живёт {minutes} мин. После привязки тебе автоматически выдадутся роли с Discord.";

        _chat.DispatchServerMessage(session, msg);
    }
}
