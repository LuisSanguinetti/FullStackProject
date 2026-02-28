#!/bin/bash
set -e

if [[ -z "$GITHUB_PAT" || -z "$REPO_URL" || -z "$RUNNER_NAME" ]]; then
  echo "Missing required environment variables: GITHUB_PAT, REPO_URL or RUNNER_NAME"
  exit 1
fi

REPO_PATH=$(echo "$REPO_URL" | sed -E 's|https://github.com/||')
API_URL="https://api.github.com/repos/$REPO_PATH/actions/runners/registration-token"

register_runner() {
  echo "Fetching registration token for $REPO_PATH..."
  RUNNER_TOKEN=$(curl -sX POST -H "Authorization: token $GITHUB_PAT" \
    -H "Accept: application/vnd.github+json" "$API_URL" | jq -r .token)

  if [[ "$RUNNER_TOKEN" == "null" || -z "$RUNNER_TOKEN" ]]; then
    echo "❌ Failed to fetch registration token"
    exit 1
  fi

  echo "Registering the runner..."
  ./config.sh \
    --url "$REPO_URL" \
    --token "$RUNNER_TOKEN" \
    --name "$RUNNER_NAME" \
    --work _work \
    --unattended \
    --replace
}

cleanup() {
  echo "Removing runner from GitHub..."
  REMOVE_TOKEN=$(curl -sX POST -H "Authorization: token $GITHUB_PAT" \
    -H "Accept: application/vnd.github+json" "$API_URL" | jq -r .token)

  if [[ "$REMOVE_TOKEN" != "null" && -n "$REMOVE_TOKEN" ]]; then
    ./config.sh remove --unattended --token "$REMOVE_TOKEN"
  else
    echo "⚠️ Could not obtain removal token; skipping unregister"
  fi
}

trap 'cleanup; exit 130' INT
trap 'cleanup; exit 143' TERM

if [[ -f ".runner" ]]; then
  echo "✅ Existing runner config found, reusing it..."
else
  echo "🆕 No config found, registering new runner..."
  register_runner
fi

echo "🚀 Starting runner..."
./run.sh