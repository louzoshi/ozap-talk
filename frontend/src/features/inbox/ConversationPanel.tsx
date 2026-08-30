import { useEffect, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { inboxApi } from "@/features/inbox/api";

export function ConversationPanel({ conversationId }: { conversationId: string | null }) {
  const queryClient = useQueryClient();
  const [draft, setDraft] = useState("");
  const threadRef = useRef<HTMLDivElement>(null);

  const { data: thread } = useQuery({
    queryKey: ["thread", conversationId],
    queryFn: () => inboxApi.thread(conversationId!),
    enabled: !!conversationId,
  });

  useEffect(() => {
    if (conversationId) void inboxApi.markRead(conversationId).catch(() => {});
  }, [conversationId]);

  useEffect(() => {
    threadRef.current?.scrollTo({ top: threadRef.current.scrollHeight });
  }, [thread?.messages.length]);

  const reply = useMutation({
    mutationFn: (text: string) => inboxApi.reply(conversationId!, text),
    onSuccess: () => {
      setDraft("");
      queryClient.invalidateQueries({ queryKey: ["thread", conversationId] });
      queryClient.invalidateQueries({ queryKey: ["conversations"] });
    },
  });

  const close = useMutation({
    mutationFn: () => inboxApi.close(conversationId!),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["conversations"] }),
  });

  if (!conversationId) {
    return (
      <section className="panel">
        <p className="empty">Selecione uma conversa para começar.</p>
      </section>
    );
  }

  return (
    <section className="panel">
      <div className="head">
        <span className="avatar">{thread?.contactName?.[0]?.toUpperCase() ?? "?"}</span>
        <div>
          <div className="name">{thread?.contactName ?? "…"}</div>
          <div className="phone">{thread?.contactPhone}</div>
        </div>
        <div className="actions">
          <button title="Encerrar conversa" onClick={() => close.mutate()}>
            ✓
          </button>
        </div>
      </div>

      <div className="thread" ref={threadRef}>
        {thread?.messages.map((m) => (
          <div
            key={m.id}
            className={`msg ${m.direction === "Inbound" ? "in" : "out"} ${
              m.status === "Failed" ? "failed" : ""
            }`}
          >
            {m.body}
            <span className="meta">
              {new Date(m.createdAtUtc).toLocaleTimeString("pt-BR", {
                hour: "2-digit",
                minute: "2-digit",
              })}
              {m.direction === "Outbound" && ` · ${statusLabel(m.status)}`}
            </span>
          </div>
        ))}
      </div>

      <form
        className="composer"
        onSubmit={(e) => {
          e.preventDefault();
          if (draft.trim()) reply.mutate(draft.trim());
        }}
      >
        <textarea
          rows={1}
          placeholder="Digite uma mensagem…"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.shiftKey) {
              e.preventDefault();
              if (draft.trim()) reply.mutate(draft.trim());
            }
          }}
        />
        <button type="submit" disabled={!draft.trim() || reply.isPending}>
          Enviar
        </button>
      </form>
    </section>
  );
}

function statusLabel(status: string) {
  switch (status) {
    case "Queued":
      return "enviando";
    case "Sent":
      return "enviada";
    case "Failed":
      return "falhou";
    default:
      return status.toLowerCase();
  }
}
