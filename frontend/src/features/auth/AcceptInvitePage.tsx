import { useState, type FormEvent } from "react";
import { Navigate, useNavigate, useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/shared/auth/AuthContext";
import { teamApi } from "@/features/team/api";
import { ROLE_LABELS } from "@/shared/auth/roles";

export function AcceptInvitePage() {
  const { token = "" } = useParams();
  const { user, acceptInvite } = useAuth();
  const navigate = useNavigate();
  const [displayName, setDisplayName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const preview = useQuery({
    queryKey: ["invite", token],
    queryFn: () => teamApi.preview(token),
    retry: false,
  });

  if (user) return <Navigate to="/inbox" replace />;

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await acceptInvite(token, { displayName, password });
      navigate("/inbox");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Falha ao aceitar o convite");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth">
      <form onSubmit={submit}>
        <h1>Aceitar convite</h1>
        {preview.isLoading && <span className="switch">Carregando…</span>}
        {preview.isError && (
          <span className="error">
            Convite inválido ou expirado. Peça um novo ao administrador.
          </span>
        )}
        {preview.data && (
          <>
            <span className="switch">
              Você foi convidado para <strong>{preview.data.companyName}</strong> como{" "}
              <strong>{ROLE_LABELS[preview.data.role]}</strong> ({preview.data.email}).
            </span>
            <label>
              Seu nome
              <input
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                required
              />
            </label>
            <label>
              Senha (mín. 8 caracteres)
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                minLength={8}
                required
              />
            </label>
            {error && <span className="error">{error}</span>}
            <button type="submit" disabled={busy}>
              {busy ? "Entrando…" : "Aceitar e entrar"}
            </button>
          </>
        )}
      </form>
    </div>
  );
}
