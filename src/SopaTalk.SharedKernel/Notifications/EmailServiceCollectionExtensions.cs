using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SopaTalk.SharedKernel.Notifications;

public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Registra o envio de e-mail. Com <c>Email:ApiKey</c> preenchida, envia de verdade
    /// pelo Resend; sem ela, grava os .eml em disco. Assim a máquina de desenvolvimento
    /// nunca dispara e-mail para cliente real por esquecimento de configuração.
    /// </summary>
    public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(EmailOptions.SectionName);

        services.AddOptions<EmailOptions>()
            .Bind(section)
            .ValidateDataAnnotations();

        if (string.IsNullOrWhiteSpace(section[nameof(EmailOptions.ApiKey)]))
        {
            services.AddSingleton<IEmailSender, FileSystemEmailSender>();
            return services;
        }

        services.AddHttpClient<IEmailSender, ResendEmailSender>((provider, client) =>
        {
            var apiKey = provider.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<EmailOptions>>().Value.ApiKey;

            client.BaseAddress = new Uri("https://api.resend.com/");
            client.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
