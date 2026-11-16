# Rafio

**A personal product and inventory workspace built with Vue 3 and ASP.NET Core.**

Rafio brings product details, stock quantities, and inventory value into one dashboard. Each account manages its own products, with ownership enforced by the API. The interface supports English and Turkish, with light and dark themes.

## Features

- JWT-based registration and sign-in.
- Private inventory: list, view, create, update, and delete your own products.
- Dashboard summaries for product count, stock quantity, inventory value, and low stock.
- Search by product name or description; filter by active, inactive, or low-stock status.
- Sort by creation date, name, or stock quantity; browse eight products per page.
- Product editor and a separate deletion confirmation dialog.
- CSV export of all products matching the current search and filter, across all pages.
- English/Turkish interface, including notifications, form labels, page titles, and export headers.
- Light/dark theme controls on sign-in, registration, and dashboard screens.
- Responsive layouts, keyboard-accessible dialogs, and a shared brand icon for navigation and the browser tab.

## Technology

| Layer | Stack |
| --- | --- |
| Frontend | Vue 3, TypeScript, Vue Router, Vite, Axios |
| Styling | Shared CSS, responsive layouts, SVG icons |
| Backend | ASP.NET Core Web API, .NET 10 |
| Persistence | Entity Framework Core 10, SQL Server |
| Authentication | JWT bearer tokens, ASP.NET Core password hashing |
| Tests | xUnit, ASP.NET Core WebApplicationFactory, temporary SQLite databases |

## Repository structure

```text
ProductStorage/
├── backend/ProductStorageAPI/
│   ├── ProductStorageAPI.slnx
│   ├── ProductStorageAPI/
│   │   ├── Abstractions/       # Service interfaces
│   │   ├── Controllers/        # Authentication and product HTTP endpoints
│   │   ├── Data/               # EF Core context and relationship configuration
│   │   ├── DTOs/               # Request and response contracts
│   │   ├── Migrations/         # SQL Server schema changes
│   │   ├── Models/             # User and product entities
│   │   ├── Services/           # Authentication, tokens, and product operations
│   │   └── Program.cs          # Dependency injection, JWT, CORS, Swagger
│   └── ProductStorageAPI.Tests/ # Ownership and authorization integration tests
├── frontend/product-storage-ui/
│   ├── public/
│   │   ├── rafio.svg           # Shared navigation icon and favicon
│   │   └── preferences-init.js # Apply appearance before the first paint
│   └── src/
│       ├── api/                # Axios client and token interceptors
│       ├── components/         # Auth screen, icons, branding, preferences
│       ├── locales/            # English and Turkish message dictionaries
│       ├── models/             # TypeScript API models
│       ├── router/             # Routes and sign-in guard
│       ├── views/              # Login, registration, product dashboard
│       ├── preferences.ts      # Reactive language/theme state and translation
│       └── styles.css          # Layout, components, and theme styles
└── README.md
```

## Prerequisites

- .NET 10 SDK.
- SQL Server or SQL Server Express LocalDB. The default development configuration uses Windows LocalDB and the `ProductStorageDB` database. On other platforms, configure an accessible SQL Server instance.
- Node.js `^22.18.0` or `>=24.12.0`, as specified in the frontend package, and npm.
- EF Core CLI matching the EF Core 10 packages:

```powershell
dotnet tool install --global dotnet-ef --version "10.*"
```

If the tool is already installed, use `dotnet tool update` with the same version argument.

## Run locally

The commands below use PowerShell and start from the repository root unless noted otherwise.

### 1. Configure the API

The API reads `ConnectionStrings:DefaultConnection`, `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:ExpiresInMinutes` from configuration. Environment variables override values in `appsettings.json`.

For example, set these in the terminal that will run migrations and the API:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\mssqllocaldb;Database=ProductStorageDB;Trusted_Connection=True;TrustServerCertificate=True'
$env:Jwt__Key = '<replace-with-a-random-secret-of-at-least-32-bytes>'
$env:Jwt__Issuer = 'Rafio.Api'
$env:Jwt__Audience = 'Rafio.Web'
$env:Jwt__ExpiresInMinutes = '60'
```

Replace the JWT placeholder before starting. Keep deployment credentials and signing keys outside version control. The example connection string is for local development; configure the database connection and certificate settings for your own environment.

### 2. Restore and migrate

```powershell
dotnet restore backend/ProductStorageAPI/ProductStorageAPI.slnx
dotnet ef database update --project backend/ProductStorageAPI/ProductStorageAPI
```

Migrations are applied explicitly; the application does not apply them automatically at startup.

### 3. Start the API

```powershell
dotnet run --project backend/ProductStorageAPI/ProductStorageAPI --launch-profile http
```

- API: `http://localhost:5264/api`
- Swagger in development: `http://localhost:5264/swagger`

### 4. Start the frontend

In a second terminal:

```powershell
cd frontend/product-storage-ui
npm ci
Copy-Item .env.example .env
npm run dev
```

If `.env` already exists, update its `VITE_API_URL` instead of replacing it:

```dotenv
VITE_API_URL=http://localhost:5264/api
```

Open `http://localhost:5173`, create an account, and add your first product. Restart Vite after changing `.env`. The API's current CORS policy permits `http://localhost:5173`; update `Program.cs` if the frontend uses another origin or port.

## Language and appearance

Use **TR / EN** to switch languages and the **sun / moon** button to switch themes. Both controls are available before and after sign-in.

- Turkish is the default language.
- The initial theme follows the operating system preference until you explicitly choose a theme.
- Preferences are stored as `rafio.language` and `rafio.theme` in local storage and synchronized between tabs.
- `src/preferences.ts` exposes typed translation keys, parameter substitution, and the selected formatting locale.
- Add matching keys to `src/locales/tr.json` and `src/locales/en.json` when adding interface text.
- Dates, numbers, name sorting, and search casing follow the selected locale. Prices remain **TRY** in either language; switching languages does not convert currency.
- Product names and descriptions are user data and are not automatically translated.
- Browser-provided validation messages follow the browser's own language settings.

The startup script applies the theme before rendering to avoid a light-theme flash. Font families are loaded from Google Fonts, with a system sans-serif fallback when unavailable.

## Inventory rules and ownership

The API reads the owner ID from the validated JWT. Clients cannot select or change a product's owner through the request body. All product queries and mutations are scoped to that user, and requests for another user's product return `404`.

Product names are unique **per user**, so two different users can use the same product name. The database enforces the relationship and the composite unique index.

The `AddProductOwnership` migration preserves pre-existing products with a nullable `UserId`. These records remain invisible through the API until their correct owner is assigned directly in the database. They are never automatically assigned to the first user or the next person to sign in. See [backend migration notes](backend/ProductStorageAPI/README.md).

Dashboard calculations:

- **Total stock:** sum of quantities across all of the user's products.
- **Inventory value:** sum of unit price multiplied by quantity, including inactive products.
- **Low stock:** active products with a quantity of five or fewer.

Dashboard summaries use the full inventory; search and filter controls affect the product list and CSV export. CSV prices use raw numeric values with TRY identified in the column header.

## API overview

| Method | Endpoint | Purpose | Authentication |
| --- | --- | --- | --- |
| POST | `/api/Auth/register` | Create an account and return a token | Public |
| POST | `/api/Auth/login` | Sign in and return a token | Public |
| GET | `/api/Products` | List the current user's products | Bearer JWT |
| GET | `/api/Products/{id}` | Get an owned product | Bearer JWT |
| POST | `/api/Products` | Create a product for the current user | Bearer JWT |
| PUT | `/api/Products/{id}` | Update an owned product | Bearer JWT |
| DELETE | `/api/Products/{id}` | Delete an owned product | Bearer JWT |

The list endpoint accepts an optional `search` parameter. The dashboard performs its own name-and-description search locally. Use Swagger for request schemas.

The frontend sends the token in `Authorization: Bearer <token>`. A `401` response to an authenticated request clears the session and redirects to sign-in. Signing out clears the token, while language and theme preferences remain saved.

## Build and verify

From the repository root:

```powershell
dotnet build backend/ProductStorageAPI/ProductStorageAPI.slnx
dotnet test backend/ProductStorageAPI/ProductStorageAPI.slnx
```

The integration tests exercise real JWT authentication and isolated temporary SQLite databases. They verify ownership boundaries, invalid/unauthenticated access, legacy product isolation, and per-user name uniqueness without modifying the local SQL Server database.

From `frontend/product-storage-ui`:

```powershell
npm run type-check
npm run build
npm run preview
```

`npm run build` includes type checking and writes production assets to `dist/`. Preview uses a different origin from the development server; configure CORS if you want to exercise API calls from that origin.

For deployment, set `VITE_API_URL` before building, configure the API connection/JWT/CORS settings for the target environment, and configure the frontend server to return `index.html` for client-side routes such as `/products`.
