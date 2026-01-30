# Mobile App Monorepo (Flutter)

This directory contains the Flutter mobile applications for **FitIt** and **Wissler**.

## Structure
- **apps/**
    - `fitit`: Workouts App.
    - `wissler`: Matching App.
- **packages/**
    - `shared_core`: Networking, Auth, State Management.
    - `shared_ui`: Design System components.
    - `api_client`: Generated API Client.

## Prerequisites
- Flutter SDK (3.x or later)

## Setup
Since the folders were scaffolded manually, you may need to run `flutter create .` inside each app folder to generate the native Android/iOS directories.

1. **Hydrate Dependencies**:
    ```bash
    cd packages/shared_core && flutter pub get
    cd ../shared_ui && flutter pub get
    cd ../api_client && flutter pub get
    cd ../../apps/fitit && flutter pub get
    cd ../wissler && flutter pub get
    ```

2. **Generate Native Code** (If missing android/ios folders):
    ```bash
    cd apps/fitit
    flutter create . --platforms=android,ios
    
    cd ../wissler
    flutter create . --platforms=android,ios
    ```

3. **Run**:
    ```bash
    flutter run -d chrome
    # or select emulator
    ```
