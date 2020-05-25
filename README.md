## 介绍

该软件是 `ffmpeg` 的 `GUI` 包装, 主要是为了方便 `OSX` 用户, 但实际上是跨平台的

是个把其他格式的视频转成 mp4 的工具，顺带有压缩视频的功效

**注意:** 因为使用 `Chrome/Chromium` 来显示界面所以用户运行之前需要先安装好 `Chrome/Chromium`, 要不然会报错

### 可执行文件依赖

这些文件需要事先下载好放到 `src/TFFmpeg2/external-bin/osx-x64` 目录下

- [ffmpeg](https://www.ffmpeg.org/download.html) 核心, 用以转换视频
- [lorca](https://github.com/shynome/go-chrome-gui/releases) 调用 `Chrome/Chromium` 显示界面

### 构建依赖

- [dotnet core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
- [nodejs v12](https://nodejs.org/)
- [yarn v1](https://yarnpkg.com/getting-started/install)

### Build

```sh
git clone https://github.com/shynome/TFFmpeg2.git
cd TFFmpeg2
# 安装 npm 依赖
yarn
# 编译前台界面 (会比较慢, 这步会在 `src/Client` 里执行 `dotnet restore`, 如果是第一次允许的话会自动下载依赖)
yarn webpack
cd src/TFFmpeg2/
# 构建 osx 的程序
# 构建完成后会在 `src/TFFmpeg2/` 文件夹下生成 `bin/Release/netcoreapp3.1/osx-x64/publish/TFFmpeg2.app` 文件夹
# 把这个文件夹压缩后发给其他安装好 `Chrome/Chromium` 的 `osx` 用户解压就能使用了
./osx-build.sh

# windows 构建也介绍下
# PublishSingleFile 打包成单个文件, 每次运行的时候都会自动解压缩, 好处是升级/分发方便
# PublishTrimmed 对运行时进行优化, 需要花更多时间进行 build
# 完成后会在 `bin/Release/netcoreapp3.1/win-x64/publish` 找到一个 `TFFmpeg2.exe` 文件, 把这个发给其他用户就能用了
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true
```

### 开发

```sh
# 起两个命令行分别运行以下命令
# 后台逻辑
dotnet watch -p src/TFFmpeg2 run
# 前台界面
yarn webpack-dev-server
```

架构是 `MUV`, 因为这个原来就有一版, 也是 `MUV` 架构的, 就不用动太多直接抄过来就好了

`MU` 都是直接同步 `Server` 端的, 客户端只做 `Msg` 和 `View`,
所以逻辑部分看 `src/TFFmpeg2`, 界面部分看 `src/Client`
