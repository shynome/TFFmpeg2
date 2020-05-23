namespace Shared

open Fable.RemoteData

type VideoMetadata = {
    Width: int
    Height: int
    // 视频时长
    Duration: string
    Duration2: int64
}

type Progress =
    | NotReady
    | Ready
    | Progress of float
    | Finished
    | Error of string

type Video = {
    Path: string
    Metadata: RemoteData<string, VideoMetadata>
    /// 转换进度
    Progress: Progress
}

/// dirs * files
type FileEntries = string [] * string []

type Tab =
    | Explorer
    | Videos

type Model = {
    Root: string
    Files: RemoteData<string,FileEntries>
    SelectedVideos: Video []
    Tab: Tab
}

type ServerMsg =
    | GetFiles of string
    | SetFiles of RemoteData<string, FileEntries>
    | Sync'
    | SelectFile of string
    | UnSelectFile of string
    | SetVideo of Video
    | SetModel of Model
    | SetTab of Tab
    | Transform of Video
    | CancelTransform of Video
#if SERVER
    | SubVideoProgress
        of Video * System.Reactive.Subjects.BehaviorSubject<Progress>
#endif

type ClientMsg =
    | SyncModel of Model
