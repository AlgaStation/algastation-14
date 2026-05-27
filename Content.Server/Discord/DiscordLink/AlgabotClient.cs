using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.Discord.DiscordLink;

/// <summary>
///     Thin HTTP client for the algabot internal API. Lives on the compose
///     network alongside the bot, so all calls are short-haul HTTP with a
///     bearer token. Bot URL + secret come from CVars (see CCVars.Algabot).
/// </summary>
public sealed class AlgabotClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public AlgabotClient(HttpClient http, string baseUrl, string secret)
    {
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secret);
    }

    /// <summary>
    ///     Returns whether the given Wizden user id has an active Discord link.
    ///     Throws on transport errors so the caller can decide whether to retry
    ///     or skip the prompt for this session.
    /// </summary>
    public async Task<LinkStatus> GetLinkStatusAsync(string userId, CancellationToken cancel = default)
    {
        var resp = await _http.GetAsync($"{_baseUrl}/api/v1/link?user_id={userId}", cancel);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<LinkStatus>(cancellationToken: cancel))!;
    }

    /// <summary>
    ///     Asks the bot to mint a one-shot redemption token for this player.
    ///     The bot replies with the token plus the invite URL and channel name
    ///     so the in-game prompt can be assembled from a single response.
    /// </summary>
    public async Task<IssuedToken> IssueTokenAsync(string userId, string username, CancellationToken cancel = default)
    {
        var body = new IssueTokenRequest { UserId = userId, Username = username };
        var resp = await _http.PostAsJsonAsync($"{_baseUrl}/api/v1/tokens", body, cancel);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<IssuedToken>(cancellationToken: cancel))!;
    }

    public sealed class LinkStatus
    {
        [JsonPropertyName("linked")] public bool Linked { get; set; }
        [JsonPropertyName("discord_id")] public string? DiscordId { get; set; }
        [JsonPropertyName("wizden_username")] public string? WizdenUsername { get; set; }
    }

    public sealed class IssuedToken
    {
        [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
        [JsonPropertyName("expires_at")] public long ExpiresAt { get; set; }
        [JsonPropertyName("ttl_seconds")] public int TtlSeconds { get; set; }
        [JsonPropertyName("invite_url")] public string InviteUrl { get; set; } = string.Empty;
        [JsonPropertyName("link_channel")] public string LinkChannel { get; set; } = string.Empty;
        [JsonPropertyName("redeem_hint")] public string RedeemHint { get; set; } = string.Empty;
    }

    private sealed class IssueTokenRequest
    {
        [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    }
}
