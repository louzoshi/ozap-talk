using FluentValidation;
using SopaTalk.Channels.Application.Abstractions;
using SopaTalk.Channels.Domain.WhatsApp;
using SopaTalk.SharedKernel.MultiTenancy;
using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Channels.Application.Channels;

public sealed record ConnectChannelCommand(
    string PhoneNumberId,
    string DisplayPhoneNumber,
    string WabaId,
    string AccessToken);

public sealed record ChannelSummary(
    Guid Id, string PhoneNumberId, string DisplayPhoneNumber, bool IsActive);

public sealed class ConnectChannelCommandValidator : AbstractValidator<ConnectChannelCommand>
{
    public ConnectChannelCommandValidator()
    {
        RuleFor(x => x.PhoneNumberId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.AccessToken).NotEmpty();
    }
}

public sealed class ConnectChannelHandler(
    IChannelRepository channels,
    IChannelsUnitOfWork unitOfWork,
    ITenantContext tenant,
    IValidator<ConnectChannelCommand> validator)
{
    public static readonly Error PhoneNumberAlreadyConnected =
        Error.Conflict("channel.phone_number_already_connected", "Esse número já está conectado.");

    public async Task<Result<ChannelSummary>> HandleAsync(ConnectChannelCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return Error.Validation("channel.invalid", validation.Errors[0].ErrorMessage);

        if (await channels.ExistsForPhoneNumberIdAsync(command.PhoneNumberId, ct))
            return PhoneNumberAlreadyConnected;

        var channel = WhatsAppChannel.Connect(
            tenant.TenantId, command.PhoneNumberId, command.DisplayPhoneNumber, command.WabaId, command.AccessToken);

        channels.Add(channel);
        await unitOfWork.SaveChangesAsync(ct);

        return new ChannelSummary(channel.Id, channel.PhoneNumberId, channel.DisplayPhoneNumber, channel.IsActive);
    }
}

public sealed class ListChannelsHandler(IChannelRepository channels)
{
    public async Task<IReadOnlyList<ChannelSummary>> HandleAsync(CancellationToken ct)
    {
        var all = await channels.ListAsync(ct);
        return all.Select(c => new ChannelSummary(c.Id, c.PhoneNumberId, c.DisplayPhoneNumber, c.IsActive)).ToList();
    }
}
