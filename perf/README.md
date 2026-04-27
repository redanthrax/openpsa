# perf/

k6 load tests for the OpenPsa API. These are smoke-grade baselines, not
capacity tests — the goal is to detect performance regressions, not to
benchmark hardware.

## Prereqs

- Install k6:  `brew install k6`
- Run the API locally with the seed database loaded:
    ```
    docker compose up -d postgres
    dotnet run --project src/Seed
    dotnet run --project src/Api      # listens on http://localhost:5000
    ```

## Run

```
k6 run perf/baseline.js
```

Override anything via env:

```
BASE_URL=http://localhost:5001 VUS=25 DURATION=2m k6 run perf/baseline.js
```

## What it measures

- Login latency (one request per VU, captured in `login_duration`).
- p95 latency for the hottest list endpoints: tickets, clients, contacts,
  projects, dashboard.
- Overall HTTP error rate (`http_req_failed`).

Thresholds in `baseline.js` are deliberately loose for the first run.
After collecting a few baselines, tighten them so CI fails on regressions.
