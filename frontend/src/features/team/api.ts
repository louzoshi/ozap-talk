import { api } from "@/shared/api/client";
import type { MembershipRole } from "@/shared/auth/roles";

export interface Member {
  id: string;
  email: string;
  displayName: string;
  role: MembershipRole;
  isActive: boolean;
  createdAtUtc: string;
  lastSignedInAtUtc: string | null;
}

export interface Invitation {
  id: string;
  email: string;
  role: MembershipRole;
  createdAtUtc: string;
  expiresAtUtc: string;
}

export interface InvitationCreated {
  invitation: Invitation;
  acceptUrl: string;
}

export interface InvitationPreview {
  companyName: string;
  email: string;
  role: MembershipRole;
}

export const teamApi = {
  members: () => api<Member[]>("/accounts/members"),
  changeRole: (id: string, role: MembershipRole) =>
    api<void>(`/accounts/members/${id}/role`, { method: "PATCH", body: JSON.stringify({ role }) }),
  removeMember: (id: string) => api<void>(`/accounts/members/${id}`, { method: "DELETE" }),

  invitations: () => api<Invitation[]>("/accounts/invitations"),
  invite: (email: string, role: MembershipRole) =>
    api<InvitationCreated>("/accounts/invitations", {
      method: "POST",
      body: JSON.stringify({ email, role }),
    }),
  revoke: (id: string) => api<void>(`/accounts/invitations/${id}`, { method: "DELETE" }),

  preview: (token: string) =>
    api<InvitationPreview>(`/accounts/invite/${encodeURIComponent(token)}`),
};
