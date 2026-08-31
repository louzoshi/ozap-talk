import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { canManageTeam } from "@/shared/auth/roles";

const nav = [
  { to: "/inbox", icon: "💬", label: "Inbox" },
  { to: "/crm", icon: "📊", label: "CRM" },
  { to: "/chatbot", icon: "🤖", label: "ChatBot" },
  { to: "/ai-agent", icon: "✨", label: "Agente IA" },
];

export function AppShell() {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const items =
    user && canManageTeam(user.role)
      ? [...nav, { to: "/equipe", icon: "👥", label: "Equipe" }]
      : nav;

  return (
    <div className="app">
      <nav className="rail">
        {items.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            title={item.label}
            className={({ isActive }) => (isActive ? "active" : "")}
          >
            <span>{item.icon}</span>
          </NavLink>
        ))}
        <span className="spacer" />
        <button
          title={`${user?.displayName} — sair`}
          onClick={() => {
            signOut();
            navigate("/login");
          }}
        >
          ⎋
        </button>
      </nav>
      <Outlet />
    </div>
  );
}
