# OpenPSA Helm chart

Skeleton chart that deploys the OpenPSA API and Blazor WASM web frontend.

## Quick start

    helm install openpsa ./deploy/helm/openpsa \
      --namespace openpsa --create-namespace \
      --set image.tag=0.1.0 \
      --set secrets.existingSecret=openpsa-secrets

## Required secret keys

Whether you use `secrets.existingSecret` or `secrets.create=true`, the API expects:

- `ConnectionStrings__DefaultConnection`
- `Redis__ConnectionString`
- `Jwt__Key`

Wire them with `api.envFrom`:

    api:
      envFrom:
        - secretRef:
            name: openpsa-secrets

## Images

Built from `src/Api/Dockerfile` and `src/Web/Dockerfile` and pushed as:

    {registry}/{repository}-api:{tag}
    {registry}/{repository}-web:{tag}

Defaults to `ghcr.io/redanthrax/openpsa-{api|web}:{Chart.AppVersion}`.

## Status

This is a starter chart — no HPA, no PodDisruptionBudget, no NetworkPolicy yet.
PRs welcome.
