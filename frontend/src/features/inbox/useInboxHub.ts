import { useEffect } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { tokenStore } from "@/shared/api/client";

/**
 * Keeps the inbox live: on every `conversationChanged` push from the server, the
 * conversation list and the open thread are refetched.
 */
export function useInboxHub() {
  const queryClient = useQueryClient();

  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`/hubs/inbox?access_token=${tokenStore.get() ?? ""}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("conversationChanged", () => {
      queryClient.invalidateQueries({ queryKey: ["conversations"] });
      queryClient.invalidateQueries({ queryKey: ["thread"] });
    });

    connection.start().catch(() => {
      /* automatic reconnect will keep trying */
    });

    return () => {
      void connection.stop();
    };
  }, [queryClient]);
}
