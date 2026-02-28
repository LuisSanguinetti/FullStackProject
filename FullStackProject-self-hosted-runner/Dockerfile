FROM ubuntu:22.04

ARG ARCH
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update && \
    apt-get install -y curl tar git jq sudo python3 python3-pip wget apt-transport-https ca-certificates gnupg software-properties-common && \
    apt-get clean

RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0 && \
    apt-get clean

RUN useradd -m runner && mkdir -p /runner && chown runner:runner /runner
WORKDIR /runner
USER runner

ENV RUNNER_VERSION=2.323.0

RUN curl -o actions-runner.tar.gz -L https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-linux-${ARCH}-${RUNNER_VERSION}.tar.gz && \
    tar xzf ./actions-runner.tar.gz && rm actions-runner.tar.gz

COPY --chown=runner:runner entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

USER root
RUN ./bin/installdependencies.sh
RUN mkdir -p /runner/_work && chown -R runner:runner /runner
USER runner

ENTRYPOINT ["/entrypoint.sh"]