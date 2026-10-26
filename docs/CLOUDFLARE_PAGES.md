# Cloudflare Pages

The canonical public website is **https://sunroom-593.pages.dev/**.

- Account-owned Cloudflare Pages project: `sunroom`.
- Production branch: `main`; direct-upload output: `site/dist/`.
- Downloads are ordinary Pages assets, not redirects to GitHub or ChatGPT Sites.
- Public release notes: `/release-notes.html`.
- The GitHub repository remains private. The public page therefore omits source and private GitHub release links.

## Deploy an update

From the repository root, with GitHub CLI access to the release and Cloudflare Wrangler access to the project:

```sh
./scripts/prepare-site-downloads.sh
npx wrangler@4.153.0 pages deploy site/dist --project-name sunroom --branch main
```

Use `npx wrangler@4.153.0 login` for an operator's ordinary Wrangler login. For non-interactive deployment, supply `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID` through the environment. The token needs **Account → Cloudflare Pages → Edit**. Keep credentials out of the repository.

The initial deployment used a temporary Pages-only token, revoked after upload. No deployment credential is stored in this repository.

`prepare-site-downloads.sh` fetches the exact versioned DMG, ZIP, and checksums from the GitHub release, then verifies them. Binary downloads are ignored by Git; prepare them before every deployment from a fresh checkout. Do not substitute a newly rebuilt DMG under an existing release version.

`_headers` marks download responses as attachments. `404.html` prevents missing download URLs from falling back to the demo HTML with a successful status.

To roll back, restore a previous production deployment in Cloudflare Pages or redeploy the desired source revision with its matching release assets. The earlier private ChatGPT Sites deployment remains separate and is no longer the canonical address.
