import { ROLE_DESCRIPTIONS, ROLE_LABELS, type MembershipRole } from "@/shared/auth/roles";

/**
 * Dropdown de papel: cada opção mostra o rótulo; a descrição do papel escolhido
 * aparece abaixo. `options` limita ao que o usuário atual pode atribuir.
 */
export function RoleSelect({
  value,
  options,
  disabled,
  onChange,
}: {
  value: MembershipRole;
  options: MembershipRole[];
  disabled?: boolean;
  onChange: (role: MembershipRole) => void;
}) {
  // Mantém o papel atual visível mesmo quando o ator não pode atribuí-lo.
  const shown = options.includes(value) ? options : [value, ...options];

  return (
    <div className="role-select">
      <select
        value={value}
        disabled={disabled}
        onChange={(e) => onChange(e.target.value as MembershipRole)}
      >
        {shown.map((role) => (
          <option key={role} value={role} disabled={role !== value && !options.includes(role)}>
            {ROLE_LABELS[role]}
          </option>
        ))}
      </select>
      <small>{ROLE_DESCRIPTIONS[value]}</small>
    </div>
  );
}
