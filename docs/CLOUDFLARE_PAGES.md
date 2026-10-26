# Cloudflare Pages

The canonical public website is **https://sunroom.colefoster.ca/**.

The Pages origin, https://sunroom-593.pages.dev/, remains available. The custom domain uses the same deployment and downloads; it does not proxy through ash.

The Pages project binds `sunroom.colefoster.ca`; its proxied CNAME points to `sunroom-593.pages.dev`. Cloudflare manages the TLS certificate. Keep both the Pages domain binding and DNS record when changing deployments.

- Account-owned Cloudflare Pages project: `sunroom`.
- Production branch: `main`; direct-upload output: `site/dist/`.
- Downloads are ordinary Pages assets, not redirects to GitHub or ChatGPT Sites.
- Public release notes: `/release-notes.html`.
- The GitHub repository is public; the page links to it as "Source".

## Deploy an update

Push to `main` (changes under `site/`) or publish a release: `.github/workflows/pages.yml` runs `prepare-site-downloads.sh` and deploys `site/dist/` with Wrangler. A manual run is also available (workflow_dispatch). To deploy by hand from the repository root instead:

```sh
./scripts/prepare-site-downloads.sh
npx wrangler@4.153.0 pages deploy site/dist --project-name sunroom --branch main
```

Use `npx wrangler@4.153.0 login` for an operator's ordinary Wrangler login. For non-interactive deployment, supply `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID` through the environment. The token needs **Account → Cloudflare Pages → Edit**. Keep credentials out of the repository.

The workflow uses the repository secrets `CLOUDFLARE_API_TOKEN` (Pages Write only) and `CLOUDFLARE_ACCOUNT_ID`. No deployment credential is stored in the repository itself.

`prepare-site-downloads.sh` fetches the exact versioned DMG, ZIP, and checksums from the GitHub release, then verifies them. Binary downloads are ignored by Git; prepare them before every deployment from a fresh checkout. Do not substitute a newly rebuilt DMG under an existing release version.

`_headers` marks download responses as attachments. `404.html` prevents missing download URLs from falling back to the demo HTML with a successful status.

To roll back, restore a previous production deployment in Cloudflare Pages or redeploy the desired source revision with its matching release assets. The earlier private ChatGPT Sites deployment remains separate and is no longer the canonical address.
