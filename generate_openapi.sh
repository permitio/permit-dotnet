#!/bin/bash
set -e  # Exit on any error

echo "Downloading OpenAPI schema files"
curl -f https://api.permit.io/v2/openapi.json -o openapi.json || { echo "Failed to download main OpenAPI schema"; exit 1; }
curl -f https://pdp-api.permit.io/openapi.json -o pdp-openapi.json || { echo "Failed to download PDP OpenAPI schema"; exit 1; }

echo "Transforming OpenAPI union types to simple types"
python transform_openapi.py openapi.json openapi.json || { echo "Failed to transform main OpenAPI schema"; exit 1; }
python transform_openapi.py pdp-openapi.json pdp-openapi.json || { echo "Failed to transform PDP OpenAPI schema"; exit 1; }

echo "Generating OpenAPI client"
npm run update-api || { echo "Failed to generate main API client"; exit 1; }
npm run update-pdp-api || { echo "Failed to generate PDP API client"; exit 1; }

echo "Fixing status code checks"
# Cross-platform sed replacement using temporary files
sed -f update_status_check.sed ./src/permit/PermitOpenAPI.cs > ./src/permit/PermitOpenAPI.cs.tmp || { echo "Failed to fix status checks for main API"; exit 1; }
mv ./src/permit/PermitOpenAPI.cs.tmp ./src/permit/PermitOpenAPI.cs || { echo "Failed to move temporary file for main API"; exit 1; }

sed -f update_status_check.sed ./src/permit/PermitPDPOpenAPI.cs > ./src/permit/PermitPDPOpenAPI.cs.tmp || { echo "Failed to fix status checks for PDP API"; exit 1; }
mv ./src/permit/PermitPDPOpenAPI.cs.tmp ./src/permit/PermitPDPOpenAPI.cs || { echo "Failed to move temporary file for PDP API"; exit 1; }

echo "Done"
