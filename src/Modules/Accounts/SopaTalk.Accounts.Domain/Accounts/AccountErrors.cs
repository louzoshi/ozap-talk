using SopaTalk.SharedKernel.Results;

namespace SopaTalk.Accounts.Domain.Accounts;

public static class AccountErrors
{
    public static readonly Error CompanyNameRequired =
        Error.Validation("account.company_name_required", "Informe o nome da empresa.");

    public static readonly Error SlugRequired =
        Error.Validation("account.slug_required", "Informe um identificador para a empresa.");

    public static readonly Error SlugTaken =
        Error.Conflict("account.slug_taken", "Esse identificador de empresa já está em uso.");

    public static readonly Error EmailTaken =
        Error.Conflict("account.email_taken", "Já existe uma conta com esse e-mail.");

    public static readonly Error NotFound =
        Error.NotFound("account.not_found", "Conta não encontrada.");
}
