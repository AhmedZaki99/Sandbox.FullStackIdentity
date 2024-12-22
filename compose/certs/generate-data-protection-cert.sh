#!/bin/bash
set -e

# Check if OpenSSL is available
if ! command -v openssl &> /dev/null; then
    echo "Error: openssl is not installed"
    exit 1
fi

echo "Generating self-signed certificate..."
openssl req -newkey rsa:2048 -noenc -keyout data-protection.key -x509 -days 3650 -out data-protection.crt -subj "/CN=DataProtection"

echo "Converting to PFX format..."
openssl pkcs12 -inkey data-protection.key -in data-protection.crt -export -out data-protection.pfx -password pass:

echo "Cleaning up temporary files..."
rm data-protection.key data-protection.crt

echo "Certificate generation complete. Output file: data-protection.pfx"
exit 0