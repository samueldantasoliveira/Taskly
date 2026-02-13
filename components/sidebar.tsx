"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import {
  LayoutDashboard,
  FolderKanban,
  Users,
  LogOut,
  CheckSquare,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { removeToken, getUserFromToken } from "@/lib/auth";

const navItems = [
  {
    label: "Dashboard",
    href: "/dashboard",
    icon: LayoutDashboard,
  },
  {
    label: "Projetos",
    href: "/dashboard/projects",
    icon: FolderKanban,
  },
  {
    label: "Equipes",
    href: "/dashboard/teams",
    icon: Users,
  },
];

export function Sidebar() {
  const pathname = usePathname();
  const router = useRouter();
  const user = getUserFromToken();

  function handleLogout() {
    removeToken();
    router.push("/login");
  }

  return (
    <aside className="flex h-screen w-64 flex-col border-r bg-card">
      <div className="flex h-16 items-center gap-2 border-b px-6">
        <CheckSquare className="h-6 w-6 text-primary" />
        <span className="text-lg font-bold text-foreground">Taskly</span>
      </div>

      <nav className="flex flex-1 flex-col gap-1 p-4">
        {navItems.map((item) => {
          const isActive =
            pathname === item.href ||
            (item.href !== "/dashboard" && pathname.startsWith(item.href));

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                isActive
                  ? "bg-primary/10 text-primary"
                  : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
              )}
            >
              <item.icon className="h-4 w-4" />
              {item.label}
            </Link>
          );
        })}
      </nav>

      <div className="border-t p-4">
        {user && (
          <div className="mb-3 px-3">
            <p className="text-sm font-medium text-foreground truncate">
              {user.name || "Usuario"}
            </p>
            <p className="text-xs text-muted-foreground truncate">
              {user.email || ""}
            </p>
          </div>
        )}
        <button
          onClick={handleLogout}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
        >
          <LogOut className="h-4 w-4" />
          Sair
        </button>
      </div>
    </aside>
  );
}
