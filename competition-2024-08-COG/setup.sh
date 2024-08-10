# Basics
export DEBIAN_FRONTEND=noninteractive
apt update
apt install -y gnuplot python3-pip rsync software-properties-common sudo wget

# .Net Core
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 7.0

export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT

# Python
python3 -m pip install -r requirements.txt --break-system-packages
