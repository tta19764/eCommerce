# eCommerce Gateway API

## Gateway Signature Middleware

The gateway registers `UseGatewaySignature()` before `MapReverseProxy()` so forwarded
requests include an internal signature header for downstream services.

Configure the header name and value with:

```json
{
  "Gateway": {
    "HeaderName": "X-Gateway-Signature",
    "Signature": "your-signature-value"
  }
}
```

If either `Gateway:HeaderName` or `Gateway:Signature` is missing or blank, the middleware
does not add a header and simply continues the request pipeline.
