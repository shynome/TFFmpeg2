set -xe
dotnet publish \
    -r osx-x64 \
    -c Release \
    -p:PublishSingleFile=true \
    -P:PublishTrimmed=true
rm -rf TFFmpeg
mkdir TFFmpeg
cp -r bin/Release/netcoreapp3.0/osx-x64/publish/Server TFFmpeg/TFFmpeg
zip -r TFFmpeg.zip TFFmpeg/
