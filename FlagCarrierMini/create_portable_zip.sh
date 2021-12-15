#!/bin/bash
DOTNET="dotnet.exe"
command -v "$DOTNET" >/dev/null 2>&1 || DOTNET="dotnet"

if ! command -v "$DOTNET" >/dev/null 2>&1; then
    echo "dotnet not found"
    exit -1
fi

cd "$(dirname "$0")"

rm -rf bin/FlagCarrierMini
"$DOTNET" publish -o bin/FlagCarrierMini -f net6.0 -c Release || exit -1
OF="${PWD}/bin/FlagCarrierMini.zip"

mv bin/FlagCarrierMini /tmp/FlagCarrierMini || exit -2
cd /tmp

printf '#!/bin/sh\ndtn=dotnet\n! command -v dotnet >/dev/null 2>&1 && command -v dotnet.exe >/dev/null 2>&1 && dtn=dotnet.exe\ncd "$(dirname "$0")"\nexec $dtn FlagCarrierMini.dll\n' > FlagCarrierMini/FlagCarrierMini.sh

find FlagCarrierMini -type f -exec chmod 644 {} \;
find FlagCarrierMini -type d -exec chmod 755 {} \;
chmod +x FlagCarrierMini/FlagCarrierMini.sh

rm -f "$OF"
zip -ro9 "$OF" FlagCarrierMini

rm -rf FlagCarrierMini
