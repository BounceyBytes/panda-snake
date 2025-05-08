curl --request POST \
  --url https://api.sandbox.immutable.com/v1/chains/imtbl-zkevm-testnet/collections/0xb9659020f25bd32df76aae6372ab3ab0578357b0/nfts/mint-requests \
  --header 'Accept: application/json' \
  --header 'Content-Type: application/json' \
  --header 'x-immutable-api-key: sk_imapik-test-4cpFv2uNVg9IdadqS1GH_e4a68e' \
  --data '{
  "assets": [
    { 
      "reference_id": "67f7d464-b8f0-4f6a-9a3b-8d3cb4a21af4",
      "owner_address": "0x6312b514845bD765581E807C4D8d37e0C9cC173A",
      "amount": "1",
      "metadata": {
        "name": "Panda on Beach",
        "attributes": [
          {
            "display_type": "number",
            "trait_type": "Aqua Power",
            "value": "Happy"
          }
        ]
      }
    }
  ]
}'