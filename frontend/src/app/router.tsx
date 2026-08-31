import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { RequireRole } from "@/shared/auth/RequireRole";
import { AppShell } from "@/app/AppShell";
import { LoginPage } from "@/features/auth/LoginPage";
import { RegisterPage } from "@/features/auth/RegisterPage";
import { AcceptInvitePage } from "@/features/auth/AcceptInvitePage";
import { InboxPage } from "@/features/inbox/InboxPage";
import { TeamPage } from "@/features/team/TeamPage";
import { Placeholder } from "@/shared/Placeholder";

function RequireAuth() {
  const { user, loading } = useAuth();
  if (loading) return <div className="auth">Carregando…</div>;
  return user ? <Outlet /> : <Navigate to="/login" replace />;
}

export const router = createBrowserRouter([
  { path: "/login", element: <LoginPage /> },
  { path: "/register", element: <RegisterPage /> },
  { path: "/convite/:token", element: <AcceptInvitePage /> },
  {
    element: <RequireAuth />,
    children: [
      {
        path: "/",
        element: <AppShell />,
        children: [
          { index: true, element: <Navigate to="/inbox" replace /> },
          { path: "inbox", element: <InboxPage /> },
          { path: "crm", element: <Placeholder title="CRM" /> },
          { path: "chatbot", element: <Placeholder title="ChatBot" /> },
          { path: "ai-agent", element: <Placeholder title="Agente IA" /> },
          {
            element: <RequireRole allow={["Owner", "Admin"]} />,
            children: [{ path: "equipe", element: <TeamPage /> }],
          },
        ],
      },
    ],
  },
]);
