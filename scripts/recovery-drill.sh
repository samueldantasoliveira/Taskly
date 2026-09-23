#!/usr/bin/env bash
set -euo pipefail

# Synthetic data only. No port, host volume, credentials or production URI.
engine="${CONTAINER_ENGINE:-docker}"
command -v "$engine" >/dev/null
container_id=""
cleanup() {
  if [[ -n "$container_id" ]]; then "$engine" rm -f "$container_id" >/dev/null; fi
}
trap cleanup EXIT
container_id="$("$engine" run -d --network none --tmpfs /data/db mongo:8)"
ready=false
for attempt in {1..30}; do
  if "$engine" exec "$container_id" mongosh --quiet --eval 'quit(db.adminCommand({ping:1}).ok ? 0 : 1)' >/dev/null 2>&1; then
    ready=true
    break
  fi
  sleep 1
done
[[ "$ready" == true ]] || { echo 'MongoDB did not start.' >&2; exit 1; }

"$engine" exec "$container_id" mongosh --quiet --eval '
const source = db.getSiblingDB("TasklyDrillSource");
source.Users.createIndex({Email:1}, {unique:true});
source.Users.insertOne({_id:"user-1", Email:"synthetic@example.test"});
source.Teams.insertOne({_id:"team-1", OwnerId:"user-1", UserIds:["user-1"]});
source.Projects.insertOne({_id:"project-1", TeamId:"team-1"});
source.TodoTasks.insertOne({_id:"task-1", ProjectId:"project-1", AssignedUserId:"user-1", Version:3});
'
"$engine" exec "$container_id" mongodump --db TasklyDrillSource --gzip --archive=/tmp/taskly-drill.gz
"$engine" exec "$container_id" mongorestore --gzip --archive=/tmp/taskly-drill.gz \
  --nsInclude 'TasklyDrillSource.*' --nsFrom 'TasklyDrillSource.*' --nsTo 'TasklyDrillRestored.*' --stopOnError
"$engine" exec "$container_id" mongosh --quiet --eval '
const restored = db.getSiblingDB("TasklyDrillRestored");
for (const name of ["Users", "Teams", "Projects", "TodoTasks"])
  if (restored.getCollection(name).countDocuments({}) !== 1) throw Error("Invalid count: " + name);
const task = restored.TodoTasks.findOne({_id:"task-1"});
if (task.Version !== 3 || !restored.Projects.findOne({_id:task.ProjectId}) ||
    !restored.Users.findOne({_id:task.AssignedUserId})) throw Error("Broken references");
if (!restored.Users.getIndexes().some(i => i.unique && i.key.Email === 1)) throw Error("Missing unique index");
print("Recovery drill passed: documents, references and unique index restored.");
'
