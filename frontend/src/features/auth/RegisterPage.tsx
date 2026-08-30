import { useState, type FormEvent } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";

const slugify = (s: string) =>
  s
    .toLowerCase()
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 40);

export function RegisterPage() {
  const { user, register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ companyName: "", ownerName: "", email: "", password: "" });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (user) return <Navigate to="/inbox" replace />;

  const set = (k: keyof typeof form) => (e: { target: { value: string } }) =>
    setForm((f) => ({ ...f, [k]: e.target.value }));

  async function submit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await register({ ...form, slug: slugify(form.companyName) });
      navigate("/inbox");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Falha ao criar conta");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth">
      <form onSubmit={submit}>
        <h1>Criar empresa</h1>
        <label>
          Nome da empresa
          <input value={form.companyName} onChange={set("companyName")} required />
        </label>
        <label>
          Seu nome
          <input value={form.ownerName} onChange={set("ownerName")} required />
        </label>
        <label>
          E-mail
          <input type="email" value={form.email} onChange={set("email")} required />
        </label>
        <label>
          Senha (mín. 8 caracteres)
          <input
            type="password"
            value={form.password}
            onChange={set("password")}
            minLength={8}
            required
          />
        </label>
        {error && <span className="error">{error}</span>}
        <button type="submit" disabled={busy}>
          {busy ? "Criando…" : "Criar e entrar"}
        </button>
        <span className="switch">
          Já tem conta? <Link to="/login">Entrar</Link>
        </span>
      </form>
    </div>
  );
}
