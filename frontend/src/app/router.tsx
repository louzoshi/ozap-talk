import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "@/app/AppLayout";
import { Placeholder } from "@/shared/Placeholder";

// Feature routes mirror the backend modules. Each feature owns its own route subtree
// under src/features/<feature>/routes.tsx as it gets built.
export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/inbox" replace /> },
      { path: "inbox", element: <Placeholder title="Inbox" /> },
      { path: "crm", element: <Placeholder title="CRM" /> },
      { path: "chatbot", element: <Placeholder title="ChatBot" /> },
      { path: "ai-agent", element: <Placeholder title="Agente IA" /> },
    ],
  },
]);
