namespace DrunknDial

open System
open System.Threading
open DrunknDial.Models

module VoiceGenerator =
    type CancelFunction = CancelFunction of (unit -> unit)
    
    type AudioSequenceResource =
        | Recorded of FilePath
        | Embedded of FileName
        
    let private playAudioResource resource =
        async {
            let! _ = match resource with
                     | Recorded x -> AudioUtils.playAudioFile x
                     | Embedded x -> AudioUtils.playAudioResource x
            ()
        }
    
    let private startWithResources (resources: AudioSequenceResource list) =
        let cts = new CancellationTokenSource()
        async {
            while true do
                let random = Random()
                do! Async.SwitchToThreadPool()
                do! Async.Sleep (random.Next(5, 10) * 1000)
                let! _ = playAudioResource resources.[random.Next(resources.Length)]
                () }
        |> (fun x -> Async.Start(x, cts.Token))
        CancelFunction (fun () -> cts.Cancel())
        
    let start contact =
        let embedded =
            [0 .. 4]
            |> Seq.map (fun x -> Embedded (FileName (sprintf "default_male_%i.m4a" x)))
            |> Seq.toList
            
        let recorded filePaths =
            filePaths
            |> Seq.map Recorded
            |> Seq.toList
        
        let resources =
            match contact.AudioSequencePaths with
            | [] -> embedded
            | x -> recorded x
            
        startWithResources resources