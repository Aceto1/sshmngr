rm -rf ./build/

dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -p:DebugType=embedded -r linux-x64 --self-contained -o ./build/linux-x64
dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -p:DebugType=embedded -r win-x64   --self-contained -o ./build/win-x64
