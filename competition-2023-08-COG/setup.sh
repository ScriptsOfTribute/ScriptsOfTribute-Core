# Basics
export DEBIAN_FRONTEND=noninteractive
apt update
apt install -y rsync sudo wget

# .Net Core
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 7.0

export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT

