# eCommerce Gateway API

## Gateway Signature

The gateway registers a YARP request transform so forwarded requests include an
internal signature header for downstream services.

Configure the header name and value with:

```json
{
  "Gateway": {
    "HeaderName": "X-Gateway-Signature",
    "Signature": "your-signature-value"
  }
}
```

If either `Gateway:HeaderName` or `Gateway:Signature` is missing or blank, the
transform does not add a header.
