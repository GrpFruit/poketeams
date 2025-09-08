PokéTeams Microservices — .NET Core + Angular Learning Project

A hands-on project to learn microservices with .NET (ASP.NET Core), get comfortable with Angular, and integrate the public PokéAPI. You’ll build small, independently deployable services, wire them up with a gateway, and add an Angular UI for a smooth learning curve.

0) What you’ll build

Goal: A tiny app where users can search Pokémon, view details (from PokéAPI), and build/share teams.

Services (Phase 1):

catalog-service (read-only): Proxies and caches data from PokéAPI (https://pokeapi.co) — list Pokémon, get details.

teambuilder-service (CRUD): Manages user-created teams (name, notes, 1–6 Pokémon). Uses PostgreSQL.

api-gateway (BFF): Fronts the two services, applies CORS, request aggregation, and one base URL for the Angular app.

Later (Phase 2+):

identity-service (optional): Simple JWT auth (ASP.NET Identity + EF Core) or Auth0/Entra for quicker setup.

notifications-service (optional): Sends events (e.g., “team.created”) through a message broker (RabbitMQ) and emails/webhooks.

Data stores:

Redis: For catalog-service caching of PokéAPI responses.

PostgreSQL: For teambuilder-service (teams & members).

Front end:

Angular app (Angular 17+): Single-page frontend hitting api-gateway only.

Infra:

Docker Compose for dev; optional upgrade to Kubernetes later.