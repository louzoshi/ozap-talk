import {
  createContext,
  use,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { api, tokenStore } from "@/shared/api/client";

export interface CurrentUser {
  id: string;
  accountId: string;
  email: string;
  displayName: string;
  role: string;
  companyName: string;
}

interface AuthResult {
  accessToken: string;
  expiresAtUtc: string;
  user: CurrentUser;
}

interface AuthState {
  user: CurrentUser | null;
  loading: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  register: (input: RegisterInput) => Promise<void>;
  signOut: () => void;
}

export interface RegisterInput {
  companyName: string;
  slug: string;
  ownerName: string;
  email: string;
  password: string;
}

const AuthCtx = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!tokenStore.get()) {
      setLoading(false);
      return;
    }
    api<CurrentUser>("/accounts/me")
      .then(setUser)
      .catch(() => tokenStore.clear())
      .finally(() => setLoading(false));
  }, []);

  const apply = useCallback((result: AuthResult) => {
    tokenStore.set(result.accessToken);
    setUser(result.user);
  }, []);

  const signIn = useCallback(
    async (email: string, password: string) => {
      apply(
        await api<AuthResult>("/accounts/sessions", {
          method: "POST",
          body: JSON.stringify({ email, password }),
        }),
      );
    },
    [apply],
  );

  const register = useCallback(
    async (input: RegisterInput) => {
      apply(
        await api<AuthResult>("/accounts/register", {
          method: "POST",
          body: JSON.stringify(input),
        }),
      );
    },
    [apply],
  );

  const signOut = useCallback(() => {
    tokenStore.clear();
    setUser(null);
  }, []);

  const value = useMemo<AuthState>(
    () => ({ user, loading, signIn, register, signOut }),
    [user, loading, signIn, register, signOut],
  );

  return <AuthCtx value={value}>{children}</AuthCtx>;
}

export function useAuth(): AuthState {
  const ctx = use(AuthCtx);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
