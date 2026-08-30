import { useState } from "react";
import { useInboxHub } from "@/features/inbox/useInboxHub";
import { ConversationList } from "@/features/inbox/ConversationList";
import { ConversationPanel } from "@/features/inbox/ConversationPanel";

export function InboxPage() {
  useInboxHub();
  const [selected, setSelected] = useState<string | null>(null);

  return (
    <>
      <ConversationList selected={selected} onSelect={setSelected} />
      <ConversationPanel conversationId={selected} />
    </>
  );
}
