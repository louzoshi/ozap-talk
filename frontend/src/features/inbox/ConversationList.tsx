import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { inboxApi, type ConversationListItem } from "@/features/inbox/api";

const TABS = [
  { key: "open", label: "Entrada" },
  { key: "waiting", label: "Esperando" },
  { key: "closed", label: "Encerradas" },
] as const;

function initials(name: string) {
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase() ?? "")
    .join("");
}

function timeAgo(iso: string) {
  const diff = Date.now() - new Date(iso).getTime();
  const min = Math.round(diff / 60000);
  if (min < 1) return "agora";
  if (min < 60) return `${min}m`;
  const h = Math.round(min / 60);
  if (h < 24) return `${h}h`;
  return new Date(iso).toLocaleDateString("pt-BR", { day: "2-digit", month: "2-digit" });
}

export function ConversationList({
  selected,
  onSelect,
}: {
  selected: string | null;
  onSelect: (id: string) => void;
}) {
  const [tab, setTab] = useState<(typeof TABS)[number]["key"]>("open");
  const [search, setSearch] = useState("");

  const { data = [], isLoading } = useQuery({
    queryKey: ["conversations", tab],
    queryFn: () => inboxApi.list(tab),
    refetchInterval: 20000,
  });

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return data;
    return data.filter(
      (c) => c.contactName.toLowerCase().includes(q) || c.contactPhone.includes(q),
    );
  }, [data, search]);

  const waitingCount = data.filter((c: ConversationListItem) => c.status === "Waiting").length;

  return (
    <aside className="list">
      <header>
        <h2>Atendimento</h2>
        <input
          className="search"
          placeholder="Pesquisar contato…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </header>
      <div className="tabs">
        {TABS.map((t) => (
          <button
            key={t.key}
            className={tab === t.key ? "active" : ""}
            onClick={() => setTab(t.key)}
          >
            {t.label}
            {t.key === "waiting" && waitingCount > 0 && (
              <span className="count">{waitingCount}</span>
            )}
          </button>
        ))}
      </div>
      <div className="conversations">
        {isLoading && <p style={{ padding: 16, color: "var(--ink-soft)" }}>Carregando…</p>}
        {!isLoading && filtered.length === 0 && (
          <p style={{ padding: 16, color: "var(--ink-soft)" }}>Nenhuma conversa aqui.</p>
        )}
        {filtered.map((c) => (
          <button
            key={c.id}
            className={`conv ${c.id === selected ? "active" : ""}`}
            onClick={() => onSelect(c.id)}
          >
            <span className="avatar">{initials(c.contactName)}</span>
            <span className="body">
              <span className="row1">
                <span className="name">{c.contactName}</span>
                <span className="time">{timeAgo(c.lastActivityAtUtc)}</span>
              </span>
              <span className="preview">{c.lastMessagePreview ?? "—"}</span>
            </span>
            {c.unreadCount > 0 && <span className="badge">{c.unreadCount}</span>}
          </button>
        ))}
      </div>
    </aside>
  );
}
