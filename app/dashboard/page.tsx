"use client";

import { useEffect, useState } from "react";
import { getUserFromToken } from "@/lib/auth";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { FolderKanban, Users, ListTodo, TrendingUp } from "lucide-react";
import type { Project, Team } from "@/lib/types";

export default function DashboardPage() {
  const [user, setUser] = useState<{ name?: string; email?: string } | null>(
    null
  );
  const [projects, setProjects] = useState<Project[]>([]);
  const [teams, setTeams] = useState<Team[]>([]);

  useEffect(() => {
    setUser(getUserFromToken());

    const storedProjects = localStorage.getItem("taskly_projects");
    const storedTeams = localStorage.getItem("taskly_teams");
    if (storedProjects) setProjects(JSON.parse(storedProjects));
    if (storedTeams) setTeams(JSON.parse(storedTeams));
  }, []);

  const stats = [
    {
      label: "Projetos",
      value: projects.length,
      icon: FolderKanban,
      color: "text-primary",
      bg: "bg-primary/10",
    },
    {
      label: "Equipes",
      value: teams.length,
      icon: Users,
      color: "text-blue-400",
      bg: "bg-blue-400/10",
    },
    {
      label: "Membros totais",
      value: teams.reduce((acc, t) => acc + (t.memberIds?.length || 0), 0),
      icon: TrendingUp,
      color: "text-amber-400",
      bg: "bg-amber-400/10",
    },
    {
      label: "Tarefas",
      value: "--",
      icon: ListTodo,
      color: "text-rose-400",
      bg: "bg-rose-400/10",
    },
  ];

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight text-foreground text-balance">
          {user?.name ? `Ola, ${user.name}` : "Dashboard"}
        </h1>
        <p className="mt-1 text-muted-foreground">
          Visao geral dos seus projetos e equipes.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.label}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {stat.label}
              </CardTitle>
              <div className={`rounded-lg p-2 ${stat.bg}`}>
                <stat.icon className={`h-4 w-4 ${stat.color}`} />
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-2xl font-bold text-foreground">{stat.value}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Projetos recentes</CardTitle>
          </CardHeader>
          <CardContent>
            {projects.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                Nenhum projeto criado ainda. Vá para Projetos para criar o
                primeiro.
              </p>
            ) : (
              <div className="flex flex-col gap-3">
                {projects.slice(0, 5).map((project) => (
                  <div
                    key={project.id}
                    className="flex items-center justify-between rounded-lg border p-3"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">
                        {project.name}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {project.description}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Equipes</CardTitle>
          </CardHeader>
          <CardContent>
            {teams.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                Nenhuma equipe criada ainda. Vá para Equipes para criar a
                primeira.
              </p>
            ) : (
              <div className="flex flex-col gap-3">
                {teams.slice(0, 5).map((team) => (
                  <div
                    key={team.id}
                    className="flex items-center justify-between rounded-lg border p-3"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">
                        {team.name}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {team.memberIds?.length || 0} membros
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
