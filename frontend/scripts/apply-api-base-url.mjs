import { writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const frontendRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const target = join(frontendRoot, 'src/environments/environment.prod.ts');
const apiBaseUrl = (process.env.LABINSIGHT_API_BASE_URL ?? '').trim().replace(/\/$/, '');

if (!apiBaseUrl) {
  process.exit(0);
}

writeFileSync(
  target,
  `export const environment = {
  production: true,
  apiBaseUrl: ${JSON.stringify(apiBaseUrl)}
};
`
);
