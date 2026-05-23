# Full-Stack E-Commerce Web Platform (.NET)

## Description / Context
Build a complete e-commerce web application that allows users to browse a local product catalog, manage a shopping cart, and complete a simplified checkout process to generate orders. The entire application must run fully offline with all data bundled locally. No external APIs, no live data fetching, and no external dataset downloads are permitted. The frontend must be functional, visually clean, and meet professional freelance standards for usability. This task is targeted at a mid-junior developer level, using a conventional ASP.NET Core architecture with clear separation between controllers, services, and data access.

## Tech Stack
- **Language:** C# 12
- **Framework:** ASP.NET Core 8.0
- **Database:** SQLite (embedded, file-based or in-memory via connection string)
- **ORM:** Entity Framework Core
- **Build Tool:** .NET CLI / `.csproj`
- **Frontend:** HTML5, CSS3, Vanilla JavaScript (served as static files from `wwwroot`)
- **Testing:** xUnit, Moq (tests will be written against the solution)
- **Persistence:** EF Core with SQLite provider

## Key Requirements

### Data & Asset Requirements
- All product and order data must be stored locally using the embedded SQLite database.
- No external API calls, CDN dependencies for data, or dataset downloads.
- Product images must use CSS-generated placeholders, SVG icons, or text initials. **Do NOT use images from Unsplash or any copyrighted stock photo source.** If unsure, use a styled placeholder div.
- The application must run completely offline after `dotnet run`.

### Product Catalog
- Products must have: `Id`, `Name`, `Description`, `Price` (decimal), `StockQuantity` (int), `Category` (string).
- The homepage (`/`) must display all products in a responsive grid layout.
- Users must be able to filter products by category via a UI control that calls the backend.
- Users must be able to search products by name (case-insensitive, partial match).
- Out-of-stock products must be visually indicated and cannot be added to the cart.

### Shopping Cart
- Users can add products to the cart from the product listing.
- Users can view the cart at `/cart`.
- Users can update item quantities or remove items entirely.
- The cart must calculate and display: subtotal, tax (fixed 8% rate), and grand total.
- Stock availability must be validated when adding items; users cannot add more than available stock.
- Cart state may be maintained via frontend `localStorage` or backend session, but checkout must use the backend to validate final stock.

### Checkout & Order Processing
- Checkout page at `/checkout` must collect: customer full name, email, and shipping address (all required fields).
- On submission, the backend must:
  1. Validate that all customer fields are non-empty and email contains an `'@'` character.
  2. Verify that all cart items still have sufficient stock.
  3. Create an order with a generated order ID (GUID or auto-increment integer).
  4. Atomically reduce product stock quantities.
  5. Clear the cart and return an order confirmation.
- If stock is insufficient during checkout, return a clear error without creating the order.
- The confirmation page must display the order ID, customer email, and total amount.

### Admin Product Management API
- RESTful endpoints must support full CRUD operations for products.
- `POST /api/products` — create a new product.
- `GET /api/products` — list all; support query params `?category=` and `?search=`.
- `GET /api/products/{id}` — get single product details.
- `PUT /api/products/{id}` — update product details and stock.
- `DELETE /api/products/{id}` — remove a product.

## Expected Interface

### Application Entry Point
- **Path:** `Program.cs`
- **Name:** `Program`
- **Type:** Class / Top-level statements
- **Input:** `args: string[]`
- **Output:** `void`
- **Description:** Configures the `WebApplication` builder, registers EF Core with SQLite, registers application services in DI, configures the HTTP request pipeline (static files, routing, authorization if needed), ensures the database is created and seeded, and starts the application.

### Product Entity
- **Path:** `Models/Product.cs`
- **Name:** `Product`
- **Type:** Class / EF Entity
- **Input:** N/A (instantiated via constructors or object initializers)
- **Output:** N/A
- **Description:** Represents a catalog product. Fields: `int Id`, `string Name`, `string Description`, `decimal Price`, `int StockQuantity`, `string Category`. Must include a parameterless constructor and appropriate properties with public getters and setters.

### Order Entity
- **Path:** `Models/Order.cs`
- **Name:** `Order`
- **Type:** Class / EF Entity
- **Input:** N/A
- **Output:** N/A
- **Description:** Represents a completed purchase. Fields: generated order identifier, `string CustomerName`, `string CustomerEmail`, `string ShippingAddress`, `decimal TotalAmount`, `DateTime OrderDate`, and a collection of associated `OrderItem` entities.

### OrderItem Entity
- **Path:** `Models/OrderItem.cs`
- **Name:** `OrderItem`
- **Type:** Class / EF Entity
- **Input:** N/A
- **Output:** N/A
- **Description:** Represents a single line item within an order. Fields: `int Id`, `int OrderId`, `int ProductId`, `string ProductName`, `int Quantity`, `decimal UnitPrice`.

### ApplicationDbContext
- **Path:** `Data/ApplicationDbContext.cs`
- **Name:** `ApplicationDbContext`
- **Type:** Class
- **Inheritance:** `: DbContext`
- **Input:** `DbContextOptions&lt;&lt;ApplicationDbContext&gt;` (via constructor injection)
- **Output:** N/A
- **Description:** EF Core database context exposing `DbSet&lt;Product&gt;` and `DbSet&lt;Order&gt;`. Configures the SQLite connection via options. Used by services to persist and query data.

### IProductService / ProductService
- **Path:** `Services/IProductService.cs` and `Services/ProductService.cs`
- **Name:** `IProductService` (Interface) / `ProductService` (Class)
- **Type:** Interface / Class
- **Annotations:** `ProductService` registered as scoped in DI
- **Input:** `Product` objects, `int id`, `string category`, `string searchTerm`
- **Output:** `Product`, `List&lt;Product&gt;`, `void`, or `bool`
- **Description:** Defines business operations for the product catalog. Must enforce that `Price` is non-negative and `StockQuantity` is non-negative before saving. Must throw an exception (e.g., `InvalidOperationException` or `KeyNotFoundException`) if a product ID is not found during update or delete. Search by name must be case-insensitive and match partial strings.

### ICartService / CartService
- **Path:** `Services/ICartService.cs` and `Services/CartService.cs`
- **Name:** `ICartService` (Interface) / `CartService` (Class)
- **Type:** Interface / Class
- **Annotations:** `CartService` registered as scoped in DI
- **Input:** `int productId`, `int quantity`, and optionally a list/dictionary representing current cart items
- **Output:** `CartDto` or equivalent object containing items, subtotal, tax, and grand total
- **Description:** Validates stock availability via `IProductService` or `ApplicationDbContext` before adding/updating items. Calculates subtotal, applies a fixed tax rate of 8%, and computes the grand total. Throws an exception if requested quantity exceeds available stock.

### IOrderService / OrderService
- **Path:** `Services/IOrderService.cs` and `Services/OrderService.cs`
- **Name:** `IOrderService` (Interface) / `OrderService` (Class)
- **Type:** Interface / Class
- **Annotations:** `OrderService` registered as scoped in DI
- **Input:** `OrderRequest` DTO (containing customer details and a collection of cart line items)
- **Output:** `OrderConfirmation` DTO
- **Description:** Orchestrates the checkout flow. Validates that customer fields are non-empty and email contains `'@'`. Verifies sufficient stock for every item. Creates and persists an `Order` with its `OrderItems`, reduces `Product.StockQuantity` values, and returns confirmation details. Must throw an exception if stock is insufficient, ensuring no partial order is persisted.

### ProductsController
- **Path:** `Controllers/ProductsController.cs`
- **Name:** `ProductsController`
- **Type:** Class / API Controller
- **Annotations:** `[ApiController]`, `[Route("api/[controller]")]`
- **Input:** HTTP GET/POST/PUT/DELETE with JSON bodies, route values, and query parameters
- **Output:** JSON responses (`Product` or list), HTTP 200/201/404/400
- **Description:** Exposes REST endpoints for product catalog and admin CRUD. `GET /api/products` supports optional `?category=` and `?search=` query parameters. `POST /api/products` returns 201 Created. `PUT /api/products/{id}` and `DELETE /api/products/{id}` return 200/204 on success or 404 if the product does not exist.

### OrdersController
- **Path:** `Controllers/OrdersController.cs`
- **Name:** `OrdersController`
- **Type:** Class / API Controller
- **Annotations:** `[ApiController]`, `[Route("api/[controller]")]`
- **Input:** HTTP POST with `OrderRequest` JSON body
- **Output:** `OrderConfirmation` JSON (201), or error JSON (400/409)
- **Description:** `POST /api/orders` accepts customer and cart data, delegates to `IOrderService`, and returns the confirmation. Returns 400 for validation errors (missing fields or invalid email format) and 409 for stock conflicts.

### Data Seeder
- **Path:** `Data/DbSeeder.cs`
- **Name:** `DbSeeder`
- **Type:** Class
- **Input:** `ApplicationDbContext` (injected), `ILogger&lt;&lt;DbSeeder&gt;` (optional)
- **Output:** `void` or `Task`
- **Description:** Executes on application startup (invoked from `Program.cs`) and inserts at least 10 sample products spanning at least 3 distinct categories (e.g., Electronics, Clothing, Home) into the SQLite database. Must only seed when the Products table is empty to prevent duplicates on restart.

### Frontend Static Files
- **Path:** `wwwroot/index.html`, `wwwroot/cart.html`, `wwwroot/checkout.html`, `wwwroot/confirmation.html`, `wwwroot/css/styles.css`, `wwwroot/js/app.js`
- **Name:** Frontend Bundle
- **Type:** Static Files
- **Input:** N/A
- **Output:** N/A
- **Description:** A functional, interactive frontend that consumes the REST APIs. Must include: (1) Product grid with search/filter controls, (2) Cart page with quantity controls and total display, (3) Checkout form with validation feedback, (4) Confirmation page displaying order details. The UI must be responsive, use a cohesive color scheme, and be acceptable for freelance client delivery. No external CSS frameworks are required, but the layout must not be broken or unstyled.

### Project File
- **Path:** `EcommerceApp.csproj`
- **Name:** `EcommerceApp.csproj`
- **Type:** File
- **Input:** N/A
- **Output:** N/A
- **Description:** .NET 8 Web SDK project file. Must reference `Microsoft.AspNetCore.App`, `Microsoft.EntityFrameworkCore.Sqlite`, and `Microsoft.EntityFrameworkCore.Tools`. Must build and run successfully with `dotnet build` and `dotnet run`.

### App Settings
- **Path:** `appsettings.json`
- **Name:** `appsettings.json`
- **Type:** File
- **Input:** N/A
- **Output:** N/A
- **Description:** JSON configuration containing the SQLite connection string (e.g., `"Data Source=ecommerce.db"` or in-memory equivalent) and logging configuration. Must allow the application to start with zero external configuration or environment variables.

## Current State
Empty repository with test files only.

## Required Implementation
- Implement the complete ASP.NET Core backend with all entities, DbContext, services, and API controllers specified in the Expected Interface.
- Implement the frontend static files to create a seamless user experience for browsing, cart management, and checkout.
- Seed the database with at least 10 realistic sample products across 3+ categories via `DbSeeder`.
- Ensure the cart correctly calculates an 8% tax rate on the subtotal.
- Ensure checkout validates customer fields and stock levels, creates a persisted order, and reduces stock atomically.
- Ensure out-of-stock products are handled gracefully in both frontend and backend.
- Provide a working build that runs with `dotnet run` and serves the application on the default ASP.NET Core port.

## Optional Enhancements
- Order history view for the current session.
- Category sidebar or dropdown filter with active state styling.
- Simple client-side form validation styling (red borders on invalid fields).
- These are NOT required and must not be tested.

## Deliverables
- `EcommerceApp.csproj` — Project configuration
- `Program.cs` — Application entry point and DI configuration
- `appsettings.json` — SQLite connection and app settings
- `Models/` — Product, Order, OrderItem entities
- `Data/` — ApplicationDbContext and DbSeeder
- `Services/` — IProductService, ProductService, ICartService, CartService, IOrderService, OrderService
- `Controllers/` — ProductsController, OrdersController
- `wwwroot/` — Complete frontend (HTML, CSS, JS)
- Working application executable via `dotnet run`
- All data local; zero external dependencies for runtime data