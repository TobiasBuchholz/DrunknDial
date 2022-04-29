namespace DrunknDial

open System
open DrunknDial.Models
open Fabulous
open Fabulous.XamarinForms
open MediaManager.Player
open Xamarin.Forms

module RecentDetailPage =
    // declarations
    type Msg =
        | NoOp
        | StartAudioTapped
        | StopAudioTapped
        | AudioPlayingStarted
        | AudioPlayingStopped
        
    type Model =
        { Recent: Recent
          IsAudioPlaying: bool }
        
    // functions
    let getAudioStateChangeMsgs =
        let toMsg state =
            match state with
            | MediaPlayerState.Loading | MediaPlayerState.Buffering | MediaPlayerState.Playing -> AudioPlayingStarted
            | MediaPlayerState.Stopped | MediaPlayerState.Paused | MediaPlayerState.Failed -> AudioPlayingStopped
            | _ -> ArgumentOutOfRangeException() |> raise
            
        AudioUtils.audioStateChanged () |> Event.map (fun x -> toMsg x.State)
    
    let private playAudioCmd (filePath: FilePath) =
        async { let! _ = AudioUtils.playAudioFile filePath
                return NoOp }
        |> Cmd.ofAsyncMsg
        
    let private stopAudioCmd =
        async { do! AudioUtils.stopCurrentAudio ()
                return NoOp }
        |> Cmd.ofAsyncMsg
        
    // lifecycle
    let init recent = { Recent = recent; IsAudioPlaying = false }, Cmd.none
    
    let update msg model =
        match msg with
        | NoOp -> model, Cmd.none
        | StartAudioTapped -> model, playAudioCmd model.Recent.AudioInfo.FilePath
        | StopAudioTapped -> model, stopAudioCmd
        | AudioPlayingStarted -> { model with IsAudioPlaying = true }, Cmd.none
        | AudioPlayingStopped -> { model with IsAudioPlaying = false }, Cmd.none
        
    let waveImage =
        Image.fromPath (sprintf "soundwave_0%i.jpg" (int (Random().Next(1, 10))))
        
    let view (model: Model) dispatch =
        View.ContentPage(
            backgroundColor = Colors.primaryDark,
            content = View.Grid(
                rowdefs = [ Dimension.Auto; Dimension.Auto; Dimension.Star; Dimension.Auto; Dimension.Auto; Dimension.Auto ],
                coldefs = [ Dimension.Star; Dimension.Star ],
                backgroundColor = Colors.primary,
                children = [
                    yield (Controls.avatar model.Recent.Contact 160.)
                        .Row(0)
                        .ColumnSpan(2)
                        .Margin(Thickness (0., 32.))
                    yield View.Label(
                        margin = Thickness (64., 0., 64., 32.),
                        verticalOptions = LayoutOptions.Center,
                        horizontalOptions = LayoutOptions.Center,
                        fontSize = InputTypes.FontSize.fromValue 22.,
                        text = model.Recent.Contact.DisplayName,
                        textColor = Colors.primaryText)
                        .Row(1)
                        .ColumnSpan(2)
                    yield View.Image(
                        aspect = Aspect.Fill,
                        horizontalOptions = LayoutOptions.Fill,
                        source = waveImage)
                        .Row(2)
                        .ColumnSpan(2)
                    yield View.Label(
                        margin = Thickness (24., 32., 0., 0.),
                        horizontalOptions = LayoutOptions.Start,
                        text = Translations.RecentDetail.date (),
                        textColor = Colors.primaryText,
                        fontAttributes = FontAttributes.Bold)
                        .Row(3)
                        .Column(0)
                    yield View.Label(
                        margin = Thickness (0., 32., 24., 0.),
                        horizontalOptions = LayoutOptions.End,
                        text = Translations.Common.dateFormatted model.Recent.CreatedAt,
                        textColor = Colors.primaryText)
                        .Row(3)
                        .Column(1)
                    yield View.Label(
                        margin = Thickness (24., 0., 0., 0.0),
                        horizontalOptions = LayoutOptions.Start,
                        text = Translations.RecentDetail.length (),
                        textColor = Colors.primaryText,
                        fontAttributes = FontAttributes.Bold)
                        .Row(4)
                        .Column(0)
                    yield View.Label(
                        margin = Thickness (0., 0., 24., 0.),
                        horizontalOptions = LayoutOptions.End,
                        text = model.Recent.AudioInfo.Duration.ToString(if model.Recent.AudioInfo.Duration.Hours > 0 then @"hh\:mm\:ss" else @"mm\:ss"),
                        textColor = Colors.primaryText)
                        .Row(4)
                        .Column(1)
                    if not model.IsAudioPlaying then
                        yield (Controls.iconButton "ic_play_circle.png" (Translations.RecentDetail.startAudio ()) Colors.accent (fun _ -> dispatch StartAudioTapped))
                            .Row(5)
                            .ColumnSpan(2)
                            .Margin(Thickness (64., 32.))
                    if model.IsAudioPlaying then
                        yield (Controls.iconButton "ic_stop_circle.png" (Translations.RecentDetail.stopAudio ()) Colors.accent (fun _ -> dispatch StopAudioTapped))
                            .Row(5)
                            .ColumnSpan(2)
                            .Margin(Thickness (64., 32.))
                    ]
                )
        )