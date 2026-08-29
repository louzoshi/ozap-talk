import { NavLink, Outlet } from "react-router-dom";

const nav = [
  { to: "/inbox", label: "Inbox" },
  { to: "/crm", label: "CRM" },
  { to: "/chatbot", label: "ChatBot" },
  { to: "/ai-agent", label: "Agente IA" },
];

export function AppLayout() {
  return (
    <div style={{ display: "flex", minHeight: "100vh", fontFamily: "system-ui" }}>
      <nav style={{ width: 200, borderRight: "1px solid #ddd", padding: 16 }}>
        <strong>sopa-talk</strong>
        <ul style={{ listStyle: "none", padding: 0, marginTop: 16 }}>
          {nav.map((item) => (
            <li key={item.to} style={{ margin: "8px 0" }}>
              <NavLink to={item.to}>{item.label}</NavLink>
            </li>
          ))}
        </ul>
      </nav>
      <main style={{ flex: 1, padding: 24 }}>
        <Outlet />
      </main>
    </div>
  );
}
