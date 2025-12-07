# Payment Gateway Integration (Suggested: Stripe)

This document describes step-by-step how to integrate a payment gateway into the MediSync project (ASP.NET backend, React Vite frontend, SQL Server DB). It's tailored to the existing codebase under `Backend/` and `Frontend/medisync/`.

## Goals
- Allow patients to pay consultation fees when booking an appointment.
- Record transactions in the `Transactions` table.
- Verify payments server-side (webhook) and update appointment status.

## Overview (high level)
1. Add payment provider SDK to backend (Stripe SDK as example).
2. Add endpoints to create payment intents and handle webhooks.
3. Add frontend payment UI using the provider's client SDK.
4. Update DB schema (Transactions table) and backend logic to store payment info.
5. Test with sandbox/test keys and webhooks.

---

## 1) Backend changes (ASP.NET Core)

Files to update/create:
- `Backend/Controllers/PaymentsController.cs` (new)
- `Backend/Models/Transaction.cs` (review; already exists)
- `appsettings.json` (add provider keys)
- `Program.cs` (register webhook secret/config)

Steps:
1. Install Stripe SDK nuget: `dotnet add package Stripe.net`
2. Add configuration to `appsettings.json`:

```json
"Stripe": {
  "SecretKey": "sk_test_...",
  "WebhookSecret": "whsec_...",
  "PublishableKey": "pk_test_..."
}
```

3. Create `PaymentsController.cs` with endpoints:
- `POST /api/payments/create-intent` — receives amount & appointmentId, creates PaymentIntent and returns `clientSecret`.
- `POST /api/payments/webhook` — Stripe webhook receiver to update transaction/appointment status.

Key points in controller implementation:
- Use `StripeConfiguration.ApiKey = config["Stripe:SecretKey"]`.
- Create PaymentIntent with metadata: `AppointmentId` and maybe `PatientId`.
- Save a `Transaction` row with status `pending` and provider payment id.
- In webhook, confirm `payment_intent.succeeded` and mark `Transaction.Status = succeeded` and optionally `Appointment.Status = paid`.

4. Secure webhook route (validate Stripe signature) and store `WebhookSecret` in appsettings.

## 2) Database changes

Your project already has `Backend/Models/Transaction.cs`. Ensure it contains fields for provider transaction id, status, amount, and timestamp. Example additions:
- `string ProviderPaymentId`
- `string Status` (or enum)
- `decimal Amount`
- `DateTime PaymentDate`

If missing, add those columns and create a migration.

## 3) Frontend changes (Vite React)

Files to update/create:
- `Frontend/medisync/src/pages/Checkout.jsx` (new) or integrate into booking flow `BookAppointment.jsx`.
- Use `@stripe/stripe-js` and `@stripe/react-stripe-js`.

Steps in UI:
1. During booking, call backend `POST /api/payments/create-intent` with `{ amount, appointmentId }`.
2. Backend returns `clientSecret`.
3. Use Stripe React elements (`CardElement`) and call `stripe.confirmCardPayment(clientSecret, { payment_method: { card } })`.
4. On success, call your backend to mark transaction completed (if your webhook isn't handling DB update) or rely on webhook.

## 4) Webhook and reliability

- Webhooks are the ground truth. Use them to update transaction records.
- Protect the webhook endpoint with the provider's signing secret and verify signatures.
- Implement idempotency in webhook handling: check if payment already processed before updating DB.

## 5) Testing

- Use test keys and test cards (Stripe docs).
- For local development, use `stripe CLI` or `ngrok` to forward webhooks to localhost.

## 6) Example flow (detailed)
1. Patient chooses a slot and clicks "Pay".
2. Frontend calls `POST /api/payments/create-intent` with amount and appointmentId.
3. Backend creates `PaymentIntent`, stores Transaction (status: pending), returns `clientSecret`.
4. Frontend collects card details and confirms payment with Stripe.
5. Stripe calls webhook to `POST /api/payments/webhook`.
6. Webhook verifies signature, marks Transaction as `succeeded`, and updates Appointment/Transaction tables.
7. Frontend polls or listens for confirmation and shows success UI.

---

## Files & code snippets (starter)

Example backend `PaymentsController.cs` (starter):
```csharp
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public PaymentsController(IConfiguration config, AppDbContext db) { _config = config; _db = db; }

    [HttpPost("create-intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CreateIntentDto dto)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        var options = new PaymentIntentCreateOptions { Amount = (long)(dto.Amount * 100), Currency = "usd", Metadata = new Dictionary<string,string>{{"AppointmentId", dto.AppointmentId.ToString()}} };
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        // Save Transaction in DB
        var tx = new Transaction { AppointmentId = dto.AppointmentId, Amount = dto.Amount, ProviderPaymentId = intent.Id, Status = "pending", PaymentDate = DateTime.UtcNow };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();

        return Ok(new { clientSecret = intent.ClientSecret });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSig = Request.Headers["Stripe-Signature"];
        var webhookSecret = _config["Stripe:WebhookSecret"];
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSig, webhookSecret);

        if (stripeEvent.Type == Events.PaymentIntentSucceeded) {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            var tx = _db.Transactions.FirstOrDefault(t => t.ProviderPaymentId == intent.Id);
            if (tx != null) { tx.Status = "succeeded"; await _db.SaveChangesAsync(); }
        }

        return Ok();
    }
}
```

## Next steps & Notes
- Pick currency and provider-specific options (e.g., 3D Secure, receipts).
- Add logging and error handling.
- Ensure sensitive keys are in environment variables in production.

---

If you'd like, I can generate the controller, DTOs, migrations, and a sample frontend `Checkout` component next. Please tell me which provider you prefer (Stripe, PayPal, or a local provider).