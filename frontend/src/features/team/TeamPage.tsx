import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/shared/auth/AuthContext";
import { teamApi, type Invitation, type Member } from "@/features/team/api";
import { InviteDialog } from "@/features/team/InviteDialog";
import { RoleSelect } from "@/features/team/RoleSelect";
import { assignableRoles, canManage, ROLE_LABELS, type MembershipRole } from "@/shared/auth/roles";

export function TeamPage() {
  const { user } = useAuth();
  const actorRole = user!.role;
  const queryClient = useQueryClient();
  const [inviting, setInviting] = useState(false);

  const members = useQuery({ queryKey: ["members"], queryFn: teamApi.members });
  const invitations = useQuery({ queryKey: ["invitations"], queryFn: teamApi.invitations });

  const changeRole = useMutation({
    mutationFn: ({ id, role }: { id: string; role: MembershipRole }) =>
      teamApi.changeRole(id, role),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["members"] }),
  });
  const removeMember = useMutation({
    mutationFn: (id: string) => teamApi.removeMember(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["members"] }),
  });
  const revoke = useMutation({
    mutationFn: (id: string) => teamApi.revoke(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["invitations"] }),
  });

  const canManageMember = (m: Member) =>
    m.id !== user!.id && m.role !== "Owner" && canManage(actorRole, m.role);

  return (
    <section className="team">
      <header>
        <h1>Equipe</h1>
        <button className="primary" onClick={() => setInviting(true)}>
          Convidar
        </button>
      </header>

      <h2>Membros</h2>
      <table>
        <tbody>
          {members.data?.map((m) => (
            <tr key={m.id} className={m.isActive ? "" : "inactive"}>
              <td>
                <div className="name">{m.displayName}</div>
                <div className="sub">{m.email}</div>
              </td>
              <td className="role-cell">
                {canManageMember(m) ? (
                  <RoleSelect
                    value={m.role}
                    options={assignableRoles(actorRole)}
                    disabled={changeRole.isPending}
                    onChange={(role) => changeRole.mutate({ id: m.id, role })}
                  />
                ) : (
                  <span>{ROLE_LABELS[m.role]}</span>
                )}
              </td>
              <td className="row-actions">
                {canManageMember(m) && m.isActive && (
                  <button onClick={() => removeMember.mutate(m.id)}>Remover</button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>Convites pendentes</h2>
      {invitations.data?.length ? (
        <table>
          <tbody>
            {invitations.data.map((i: Invitation) => (
              <tr key={i.id}>
                <td>
                  <div className="name">{i.email}</div>
                  <div className="sub">
                    {ROLE_LABELS[i.role]} · expira{" "}
                    {new Date(i.expiresAtUtc).toLocaleDateString("pt-BR")}
                  </div>
                </td>
                <td className="row-actions">
                  {canManage(actorRole, i.role) && (
                    <button onClick={() => revoke.mutate(i.id)}>Cancelar</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p className="empty">Nenhum convite pendente.</p>
      )}

      {inviting && <InviteDialog actorRole={actorRole} onClose={() => setInviting(false)} />}
    </section>
  );
}
