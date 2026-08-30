using SopaTalk.SharedKernel.Modules;

namespace SopaTalk.Api;

/// <summary>
/// The one place the host names its modules. Order matters only for endpoint registration.
/// Add a module here and nowhere else in the host.
/// </summary>
internal static class ModuleRegistry
{
    public static IReadOnlyList<IModuleInstaller> All { get; } =
    [
        new SopaTalk.Accounts.Api.AccountsModuleInstaller(),
        new SopaTalk.Channels.Api.ChannelsModuleInstaller(),
        new SopaTalk.Inbox.Api.InboxModuleInstaller(),
        new SopaTalk.Crm.Api.CrmModuleInstaller(),
        new SopaTalk.Chatbot.Api.ChatbotModuleInstaller(),
        new SopaTalk.AiAgent.Api.AiAgentModuleInstaller(),
    ];
}
