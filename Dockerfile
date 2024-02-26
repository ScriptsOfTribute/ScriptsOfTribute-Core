FROM ubuntu:22.04

RUN export DEBIAN_FRONTEND=noninteractive

RUN apt-get update && \
    apt-get install -y rsync sudo wget

RUN apt-get install -y dotnet-sdk-7.0 && \
    apt-get install -y aspnetcore-runtime-7.0

COPY . /tot
