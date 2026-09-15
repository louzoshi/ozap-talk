export type MembershipRole = "Owner" | "Admin" | "Operator" | "Member";

export const ROLE_ORDER: MembershipRole[] = ["Owner", "Admin", "Operator", "Member"];

export const ROLE_LABELS: Record<MembershipRole, string> = {
  Owner: "Proprietário",
  Admin: "Admin",
  Operator: "Operador",
  Member: "Membro",
};

export const ROLE_DESCRIPTIONS: Record<MembershipRole, string> = {
  Owner: "Tem todas as permissões na ferramenta e acesso ao painel financeiro.",
  Admin: "Tem todas as permissões na ferramenta, porém não tem acesso ao painel financeiro.",
  Operator:
    "É capaz de enviar mensagens para contatos e pode alterar configurações básicas que são úteis aos atendentes.",
  Member:
    "Não é capaz de alterar nenhuma configuração, e não consegue enviar mensagens para contatos.",
};

/** Espelha OzapTalk.Accounts.Domain.Users.MembershipRules.CanManage. */
export function canManage(actor: MembershipRole, target: MembershipRole): boolean {
  if (actor === "Owner") return target !== "Owner";
  if (actor === "Admin") return target === "Operator" || target === "Member";
  return false;
}

/** Papéis que o ator pode atribuir a outra pessoa. */
export function assignableRoles(actor: MembershipRole): MembershipRole[] {
  if (actor === "Owner") return ["Admin", "Operator", "Member"];
  if (actor === "Admin") return ["Operator", "Member"];
  return [];
}

export const canManageTeam = (role: MembershipRole | string): boolean =>
  role === "Owner" || role === "Admin";
