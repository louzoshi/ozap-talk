import { api } from "@/shared/api/client";

export interface ConversationListItem {
  id: string;
  contactId: string;
  contactName: string;
  contactPhone: string;
  lastMessagePreview: string | null;
  lastActivityAtUtc: string;
  unreadCount: number;
  status: "Open" | "Waiting" | "Closed";
  assignedAgentId: string | null;
}

export interface MessageView {
  id: string;
  direction: "Inbound" | "Outbound";
  status: string;
  body: string;
  authorAgentId: string | null;
  createdAtUtc: string;
}

export interface ConversationThread {
  id: string;
  contactName: string;
  contactPhone: string;
  status: string;
  assignedAgentId: string | null;
  messages: MessageView[];
}

export const inboxApi = {
  list: (status: string) =>
    api<ConversationListItem[]>(`/inbox/conversations?status=${encodeURIComponent(status)}`),
  thread: (id: string) => api<ConversationThread>(`/inbox/conversations/${id}`),
  reply: (id: string, text: string) =>
    api<{ messageId: string }>(`/inbox/conversations/${id}/messages`, {
      method: "POST",
      body: JSON.stringify({ text }),
    }),
  close: (id: string) => api<void>(`/inbox/conversations/${id}/close`, { method: "POST" }),
  markRead: (id: string) => api<void>(`/inbox/conversations/${id}/read`, { method: "POST" }),
};
