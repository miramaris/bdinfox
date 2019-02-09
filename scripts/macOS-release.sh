#!/usr/bin/env bash

find .

if [[ "$CIRRUS_RELEASE" == "" ]]; then
  echo "Not a release. No need to deploy!"
  exit 0
fi

if [[ "$GITHUB_TOKEN" == "" ]]; then
  echo "Please provide GitHub access token via GITHUB_TOKEN environment variable!"
  exit 1
fi

hub release edit -m "" --attach ./bin/release/netcoreapp2.1/osx-x64/native/BDInfo#bdinfox-macOS $CIRRUS_RELEASE