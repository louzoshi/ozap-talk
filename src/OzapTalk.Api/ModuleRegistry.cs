using OzapTalk.SharedKernel.Modules;

namespace OzapTalk.Api;

/// <summary>
/// The one place the host names its modules. Order matters only for endpoint registration.
/// Add a module here and nowhere else in the host.
/// </summary>
internal static class ModuleRegistry
{
    public static IReadOnlyList<IModuleInstaller> All { get; } =
    [
        new OzapTalk.Accounts.Api.AccountsModuleInstaller(),
        new OzapTalk.Channels.Api.ChannelsModuleInstaller(),
        new OzapTalk.Inbox.Api.InboxModuleInstaller(),
        new OzapTalk.Crm.Api.CrmModuleInstaller(),
        new OzapTalk.Chatbot.Api.ChatbotModuleInstaller(),
        new OzapTalk.AiAgent.Api.AiAgentModuleInstaller(),
    ];
}
