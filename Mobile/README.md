# Mobile Applications (Flutter)

This directory contains the cross-platform mobile applications for the platform, built using **Flutter**.

## Structure
-   **`apps/`**: Contains the specific application implementations.
    -   **`fitit/`**: The Fitness Tracking App (Blue Theme).
    -   **`wissler/`**: The Dating & Social App (Orange/Blue Theme).
-   **`packages/`**: Shared libraries used by all apps.
    -   **`api_client`**: Auto-generated or shared API client code.
    -   **`shared_core`**: Authentication, State Management (Riverpod), and Common Utilities.
    -   **`shared_ui`**: Reusable UI components and widgets.

## Features
-   **Dynamic Theming**: Apps automatically load theme settings from the backend based on the App ID.
-   **Unified Auth**: Shared authentication flow using the Identity Microservice.

## Getting Started
1.  **Prerequisites**: Flutter SDK, Android Studio / Xcode.
2.  **Run FitIt**:
    ```bash
    cd apps/fitit
    flutter run
    ```
3.  **Run Wissler**:
    ```bash
    cd apps/wissler
    flutter run
    ```
