import { defineConfig, devices } from '@playwright/test'

const frontendUrl = 'http://127.0.0.1:4173'
const apiUrl = 'http://127.0.0.1:5220'

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: process.env.CI ? 1 : undefined,
  retries: process.env.CI ? 2 : 0,
  reporter: [
    ['list'],
    ['html', { open: 'never' }],
  ],
  use: {
    baseURL: frontendUrl,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      name: 'API',
      command: 'dotnet run --no-launch-profile --project ../Rivulus.API/Rivulus.API.csproj',
      url: `${apiUrl}/health/ready`,
      timeout: 120_000,
      reuseExistingServer: !process.env.CI,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        ASPNETCORE_URLS: apiUrl,
        MongoDb__ConnectionString: 'mongodb://127.0.0.1:27018',
        MongoDb__DatabaseName: 'RivulusE2E',
        // Rate limiting itself is covered by isolated API integration tests.
        AuthenticationLimits__LoginPerMinute: '10000',
        AuthenticationLimits__RegistrationPerMinute: '10000',
        Cors__AllowedOrigins__0: frontendUrl,
      },
    },
    {
      name: 'Frontend',
      command: 'npm run dev -- --host 127.0.0.1 --port 4173 --strictPort',
      cwd: '../Rivulus.Web',
      url: frontendUrl,
      timeout: 120_000,
      reuseExistingServer: !process.env.CI,
      env: {
        VITE_API_URL: apiUrl,
      },
    },
  ],
})
