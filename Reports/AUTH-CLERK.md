# Replacing current auth with Clerk (Login / Signup / Forgot Password)

This guide shows how to replace the existing auth flow in the MediSync project with Clerk (https://clerk.com). It is tailored to your current codebase (ASP.NET backend and React frontend under `Frontend/medisync`). I scanned the repo for the relevant files and noted the current endpoints and DTOs to help mapping.

## What I found in the codebase
- Frontend calls:
  - `POST /api/Auth/login` — used by `Frontend/medisync/src/pages/Login-signup/Login.jsx`
  - `POST /api/Auth/register` — used by `Frontend/medisync/src/pages/Login-signup/Register.jsx`
  - `POST /api/Auth/forgot` — used by `Frontend/medisync/src/pages/Login-signup/Forgot.jsx`
  - `POST /api/Auth/reset` — used by `Frontend/medisync/src/pages/Login-signup/Reset.jsx`
- Backend DTOs (relevant):
  - `Backend/Models/DTOs/RegisterDto.cs` (FullName, Email, ContactNumber, NIC, Password)
  - `Backend/Models/DTOs/LoginDto.cs` (Email, Password)
  - `Backend/Models/DTOs/AuthResponseDto.cs` (Token, ExpiresAtUtc)
- User model has fields for `PasswordResetToken`, `PasswordResetTokenExpiresUtc`.

## High-level plan to integrate Clerk
1. Decide whether Clerk will fully replace backend auth (recommended) or act as an identity provider while backend keeps user records.
   - Option A (recommended): Use Clerk as the primary auth provider. Frontend uses Clerk components, backend validates Clerk session tokens on each request and maps Clerk user ID -> local user record.
   - Option B: Use Clerk only for hosted auth UI, but keep backend registration endpoints. Less recommended.

## Steps for Option A (Clerk as primary auth)

### 1) Sign up and configure Clerk
- Create a Clerk account and application.
- Add allowed origin (e.g., `http://localhost:5173`) in Clerk settings.
- Get Clerk PublishableKey and SecretKey.

### 2) Frontend changes (React)
- Install Clerk React SDK:
  ```bash
  npm install @clerk/clerk-react
  ```
- Wrap app with Clerk provider in `main.jsx` or `AppWrapper`:
  ```jsx
  import { ClerkProvider } from '@clerk/clerk-react';
  const clerkPubKey = import.meta.env.VITE_CLERK_PUBLISHABLE_KEY; // set in .env

  <ClerkProvider publishableKey={clerkPubKey}>
    <AppWrapper />
  </ClerkProvider>
  ```
- Replace existing login/register pages with Clerk components or use `useSignUp`, `useSignIn` hooks.
  - Example: Use `SignIn` and `SignUp` components for quick integration.
- Remove direct calls to `/api/Auth/login` and `/api/Auth/register` in the frontend.
- After successful sign-in, Clerk provides a session; you can get a session token for backend calls using `getAuth()` from `@clerk/nextjs` or `@clerk/clerk-react` utilities.

### 3) Backend changes (ASP.NET)
- Install Clerk server SDK or validate tokens manually. Clerk provides JWT session tokens you can verify.
- In `Program.cs`, add middleware to validate Clerk JWT on protected endpoints.
- Instead of validating passwords, the backend should accept Clerk-verified identity. Create or map local `User` records keyed by Clerk `userId` (store in `User` table as e.g., `ClerkId`).
- Update protected endpoints to require Clerk session and extract `userId` from token.

Example middleware pseudocode:
```csharp
// Validate Clerk JWT in Authorization header
var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
// Verify token signature with Clerk public key(s) or use Clerk SDK.
// Extract clerk user id claim and set in HttpContext.User
```

### 4) Password reset / Forgot password
- Clerk handles password resets and email verification out of the box if you use Clerk's sign-in/signup.
- Remove backend endpoints for `/api/Auth/forgot` and `/api/Auth/reset` or keep them only for legacy flows.

### 5) Persisting user profile in backend
- On first successful Clerk sign-in, fetch Clerk user info server-side and create a `User` row in your DB with fields:
  - `ClerkId`, `FullName`, `Email`, `ContactNumber`, ...
- Keep backend `User` model to store appointments, favorites, etc., and map by `ClerkId`.

### 6) Token usage from frontend to backend
- When calling backend endpoints that require authentication, obtain a token from Clerk and pass it in Authorization header.
- In React you can use `const { getToken } = useAuth(); const t = await getToken();` then `fetch('/api/..', { headers: { Authorization: `Bearer ${t}` }})`.

## Mapping to your existing files
- Replace `Frontend/.../Login.jsx`, `Register.jsx`, `Forgot.jsx`, `Reset.jsx` with Clerk components or update them to call Clerk APIs instead of `apiRequest('/api/Auth/...')`.
- Backend: Remove password validation logic in `Auth` controller (if present). Instead verify Clerk session JWT on protected endpoints.
- Keep `User` model for profile data; add `ClerkId` string property.

## Notes & Considerations
- Clerk is a paid service for production; use free tier for development.
- Ensure CORS and allowed origins are set in Clerk dashboard.
- Clerk supports multi-factor authentication and social logins out of the box.
- Keep a migration plan: you may need to migrate existing users to Clerk by creating Clerk users server-side and mapping them.

---

If you'd like, I can now:
- Create sample `Clerk` integration changes in the frontend (replace `Login.jsx` and `Register.jsx` with Clerk hooks/components).
- Add example middleware or controller changes in the backend to accept Clerk tokens and map `ClerkId` to `User`.

Tell me which action you'd like me to take next (create files, update specific files, or just leave docs).