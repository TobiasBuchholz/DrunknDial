namespace DrunknDial

open System
open DrunknDial.Models
open MediaManager
open Plugin.AudioRecorder
open System.IO
open Xamarin.Essentials

module AudioUtils =
    let private recorder = new AudioRecorderService(StopRecordingAfterTimeout = false, StopRecordingOnSilence = false)
    
    let startRecording () =
        async { let! task = recorder.StartRecording() |> Async.AwaitTask
                task |> Async.AwaitTask |> ignore
                return () }

    let stopRecording () =
        async { do! recorder.StopRecording() |> Async.AwaitTask
                let filePath = Path.Combine(FileSystem.AppDataDirectory, DateTimeOffset.Now.Ticks.ToString() + Path.GetExtension(recorder.FilePath));
                File.Move(recorder.FilePath, filePath)
                
                let extractor = CrossMediaManager.Current.Extractor
                let! mediaItem = extractor.CreateMediaItem(filePath) |> Async.AwaitTask
                let! metaData = extractor.GetMetadata(mediaItem) |> Async.AwaitTask
                return { FilePath = FilePath filePath; Duration = metaData.Duration }}
        
    let playAudioFile (filePath: FilePath) =
        CrossMediaManager.Current.Play(filePath.Value) |> Async.AwaitTask
        
    let playAudioResource (fileName: FileName) =
        CrossMediaManager.Current.PlayFromAssembly(fileName.Value) |> Async.AwaitTask
        
    let stopCurrentAudio () =
        CrossMediaManager.Current.Stop() |> Async.AwaitTask
        
    let audioStateChanged () =
        CrossMediaManager.Current.StateChanged