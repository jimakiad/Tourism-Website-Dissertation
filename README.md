# Tourit - A Community-Driven Tourism Forum

"Tourit" is a prototype web application developed as part of a Master's Thesis, designed to be a community-centric forum for sharing and discovering tourism-related information and experiences. It features a React frontend styled with Tailwind CSS and a .NET Core Web API backend with a PostgreSQL database.

## Features Implemented in Prototype

*   User Registration and JWT-based Authentication.
*   Post Creation:
    *   Title (plain text), Body (Markdown).
    *   Mandatory Country association.
    *   Selection from predefined Categories and Tags.
    *   Optional image upload (jpg, png, webp) - single image per post.
    *   Optional map-based geolocation (using OpenLayers via `rlayers`).
*   Post and Comment Voting (Upvote/Downvote).
*   Nested Commenting System (Replies to comments).
*   Homepage displaying posts (sortable by Top/New).
*   Country-specific landing pages displaying filtered posts (sortable by Top/New).
*   Post Detail page showing full post content and comments.
*   User Profile page:
    *   Displays user's own posts and comments.
    *   Allows "deletion" (redaction) of user's own posts and comments.
    *   Allows account deactivation (PII redaction).
*   Newsletter opt-in/opt-out functionality.
*   Basic responsive design.

## Technology Stack

*   **Frontend:**
    *   React (with Vite as build tool)
    *   React Router DOM (for client-side routing)
    *   Tailwind CSS (for styling)
    *   Axios (for API communication)
    *   `rlayers` (for OpenLayers map integration)
    *   `react-markdown` (for rendering Markdown content)
*   **Backend:**
    *   ASP.NET Core Web API (using .NET 6.0 or later)
    *   C#
    *   Entity Framework Core (ORM for PostgreSQL)
    *   PostgreSQL (Relational Database)
    *   BCrypt.Net (for password hashing)
    *   JWT Bearer Authentication
*   **Development Tools:**
    *   Visual Studio / VS Code
    *   Node.js and npm
    *   .NET SDK
    *   pgAdmin (or other PostgreSQL client)

## Setup and Installation

This project consists of a backend API and a frontend React application. Both need to be set up and run.

### Prerequisites

*   **.NET SDK:** Version 6.0 or later. ([Download .NET](https://dotnet.microsoft.com/download))
*   **Node.js and npm:** Node.js LTS version recommended. npm comes with Node.js. ([Download Node.js](https://nodejs.org/))
*   **PostgreSQL Server:** A running instance of PostgreSQL (e.g., version 13+). You'll need to create a database. ([Download PostgreSQL](https://www.postgresql.org/download/))
*   **Git:** For cloning the repository.

### Backend Setup (`TourismReddit.Api` folder)

1.  **Clone the Repository:**
    ```bash
    git clone <your-repository-url>
    cd <your-repository-url>/TourismReddit.Api
    ```

2.  **Configure Database Connection:**
    *   Ensure your PostgreSQL server is running.
    *   Create a new, empty database in PostgreSQL (e.g., named `tourism_reddit_dev`).
    *   Navigate to the `TourismReddit.Api` project folder.
    *   Open `appsettings.Development.json`.
    *   Modify the `ConnectionStrings.DefaultConnection` to point to your PostgreSQL database:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Host=localhost;Port=5432;Database=tourism_reddit_dev;Username=your_postgres_user;Password=your_postgres_password"
        }
        ```
        Replace `your_postgres_user` and `your_postgres_password` with your PostgreSQL credentials.

3.  **Configure JWT Secret:**
    *   In `appsettings.Development.json`, update the `Jwt:Key` value to a strong, unique secret string:
        ```json
        "Jwt": {
          "Key": "YOUR_VERY_STRONG_AND_UNIQUE_SECRET_KEY_HERE_REPLACE_THIS_NOW",
          "Issuer": "TouritApi", // Or your preferred issuer
          "Audience": "TouritFrontend" // Or your preferred audience
        }
        ```
    *   **IMPORTANT:** For any real deployment, this key should be managed via environment variables or a secure secret management service.

4.  **Install EF Core Tools (if not already installed globally):**
    ```bash
    dotnet tool install --global dotnet-ef
    ```
    (You might need to close and reopen your terminal after this).

5.  **Apply Database Migrations:**
    *   Ensure you are in the `TourismReddit.Api` directory.
    *   If you have existing migration files in a `Migrations` folder from the project, run:
        ```bash
        dotnet ef database update
        ```
    *   If there are no migration files yet (e.g., clean clone and you have the models defined), you'll need to generate them first based on the `ApplicationDbContext` and models:
        ```bash
        dotnet ef migrations add InitialCreateAndFeatures # Or your preferred migration name
        dotnet ef database update
        ```

6.  **Build and Run the Backend:**
    ```bash
    dotnet build
    dotnet run
    ```
    The backend API should start, typically listening on `http://localhost:xxxx` and `https://localhost:yyyy`. Note the **HTTP port** (e.g., `5019` from our examples) as the frontend will connect to this. You can access Swagger UI at `http://<your_backend_http_url>/swagger`.

### Frontend Setup (`frontend` folder)

1.  **Navigate to Frontend Directory:**
    From the root of the cloned repository:
    ```bash
    cd frontend
    ```

2.  **Install Dependencies:**
    ```bash
    npm install
    ```

3.  **Configure API Base URL:**
    *   Open `src/services/api.js`.
    *   Ensure the `API_BASE_URL` constant points to your **running backend's HTTP URL**:
        ```javascript
        const API_BASE_URL = 'http://localhost:5019/api'; // Replace 5019 with your backend's HTTP port
        ```

4.  **Copy Leaflet/OpenLayers Assets (If Applicable):**
    *   If using Leaflet (not our final choice but good to note if someone adapts): Copy `marker-icon.png`, `marker-icon-2x.png`, and `marker-shadow.png` from `node_modules/leaflet/dist/images/` into the `public/` folder of your `frontend` project.
    *   For OpenLayers (`rlayers`), its CSS (`ol/ol.css`) is imported directly. No separate assets are usually needed in `public/` unless you customize icons extensively.

5.  **Run the Frontend Development Server:**
    ```bash
    npm run dev
    ```
    The frontend should start, typically on `http://localhost:5173` (Vite's default). Open this URL in your browser.

### Notes

*   **CORS:** The backend `Program.cs` is configured to allow requests from the typical Vite development server URL (e.g., `http://localhost:5173`). If you run the frontend on a different port, you'll need to update the CORS policy in the backend.
*   **Image Uploads:** Images are currently stored locally on the backend server in an `uploads` directory relative to the backend project's content root. This directory will be created automatically when the first image is uploaded. The backend also serves these static files under the `/uploads` URL path. For production, a cloud blob storage solution (Azure Blob, AWS S3) is highly recommended.
*   **Environment Variables:** For a real deployment, sensitive information like database connection strings and JWT secrets should be managed via environment variables or a secure configuration service, not hardcoded in `appsettings.json`.

## Thesis Context

This application was developed as the practical component of a Master's Thesis by Dimitris Vakirtzis for the University of Piraeus, Department of Informatics. It serves as a prototype to explore concepts in community-driven information sharing within the tourism domain.
