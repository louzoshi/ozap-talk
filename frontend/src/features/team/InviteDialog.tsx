import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { teamApi, type InvitationCreated } from "@/features/team/api";
import { RoleSelect } from "@/features/team/RoleSelect";
import { assignableRoles, type MembershipRole } from "@/shared/auth/roles";

export function InviteDialog({
  actorRole,
  onClose,
}: {
  actorRole: MembershipRole;
  onClose: () => void;
}) {
  const queryClient = useQueryClient();
  const roles = assignableRoles(actorRole);
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<MembershipRole>(roles[roles.length - 1] ?? "Member");
  const [created, setCreated] = useState<InvitationCreated | null>(null);

  const invite = useMutation({
    mutationFn: () => teamApi.invite(email.trim(), role),
    onSuccess: (result) => {
      setCreated(result);
      queryClient.invalidateQueries({ queryKey: ["invitations"] });
    },
  });

  function submit(e: FormEvent) {
    e.preventDefault();
    if (email.trim()) invite.mutate();
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        {created ? (
          <>
            <h2>Convite criado</h2>
            <p>
              Enquanto o envio de e-mail não está ligado, compartilhe este link com{" "}
              <strong>{created.invitation.email}</strong>:
            </p>
            <input
              className="invite-link"
              readOnly
              value={created.acceptUrl}
              onFocus={(e) => e.target.select()}
            />
            <div className="modal-actions">
              <button type="button" onClick={onClose}>
                Fechar
              </button>
            </div>
          </>
        ) : (
          <form onSubmit={submit}>
            <h2>Convidar pessoa</h2>
            <label>
              E-mail
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoFocus
              />
            </label>
            <label>
              Papel
              <RoleSelect value={role} options={roles} onChange={setRole} />
            </label>
            {invite.isError && (
              <span className="error">
                {invite.error instanceof Error ? invite.error.message : "Falha ao convidar"}
              </span>
            )}
            <div className="modal-actions">
              <button type="button" onClick={onClose}>
                Cancelar
              </button>
              <button type="submit" disabled={invite.isPending || !email.trim()}>
                {invite.isPending ? "Enviando…" : "Enviar convite"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
