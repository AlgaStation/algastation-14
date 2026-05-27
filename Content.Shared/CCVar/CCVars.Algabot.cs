using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Base URL of the algabot internal HTTP API (the Go bot living alongside
    ///     this server on the compose network). When empty, the link manager
    ///     stays dormant and no Discord-link prompts are issued.
    /// </summary>
    public static readonly CVarDef<string> AlgabotUrl =
        CVarDef.Create("algabot.url", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Shared secret used as the bearer token when calling algabot. Must
    ///     match ALGABOT_API_SECRET on the bot side. CONFIDENTIAL so it never
    ///     leaks into client-visible config dumps.
    /// </summary>
    public static readonly CVarDef<string> AlgabotSecret =
        CVarDef.Create("algabot.secret", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);
}
