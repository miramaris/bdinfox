FROM microsoft/dotnet:2.2-sdk

RUN apt-get update && apt-get install -y \
    clang-3.9 \
    libcurl4-openssl-dev \
    libkrb5-dev \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*