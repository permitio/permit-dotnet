echo "Downloading OpenAPI schema files"
curl https://api.permit.io/v2/openapi.json -o openapi.json
curl https://pdp-api.permit.io/openapi.json -o pdp-openapi.json

echo "Transforming OpenAPI union types to simple types"
python transform_openapi.py openapi.json openapi.json
python transform_openapi.py pdp-openapi.json pdp-openapi.json 

echo "Generating OpenAPI client"
npm run update-api
npm run update-pdp-api

echo "Fixing status code checks"
sed -i '' -f update_status_check.sed ./src/permit/PermitOpenAPI.cs
sed -i '' -f update_status_check.sed ./src/permit/PermitPDPOpenAPI.cs

echo "Done"
