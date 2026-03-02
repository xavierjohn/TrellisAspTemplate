FROM mcr.microsoft.com/devcontainers/base:bookworm

# Install prerequisites
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
        wget \
        ca-certificates \
        apt-transport-https \
        software-properties-common

# Install .NET 10 SDK
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --channel 10.0 --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh

# Add dotnet to PATH
ENV PATH="${PATH}:/usr/share/dotnet"
ENV DOTNET_ROOT="/usr/share/dotnet"

# Enable dotnet CLI telemetry optout
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Verify installation
RUN dotnet --version
