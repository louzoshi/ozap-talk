import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import type { MembershipRole } from "@/shared/auth/roles";

/** Rota aninhada: só entra quem tem um dos papéis em `allow`. */
export function RequireRole({ allow }: { allow: MembershipRole[] }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="auth">Carregando…</div>;
  if (!user) return <Navigate to="/login" replace />;
  return allow.includes(user.role) ? <Outlet /> : <Navigate to="/inbox" replace />;
}
