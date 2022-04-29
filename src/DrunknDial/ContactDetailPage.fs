namespace DrunknDial

open System
open System.IO
open DrunknDial.Models
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Essentials
open Xamarin.Forms

module ContactDetailPage =
    // declarations
    type Msg =
        | NoOp
        | ContactLoaded of Models.Contact
        | PermissionDenied
        | StartCallTapped
        | StopCallTapped
        | CallStarted of CallService.State
        | CallStopped of CallService.State
        | StartAudioSequenceRecordingTapped
        | StopAudioSequenceRecordingTapped
        | AudioSequenceRecordingStarted
        | AudioSequenceRecordingStopped of Models.Contact
        | PlayAudioSequenceTapped of FilePath
        | DeleteAudioSequenceTapped of FilePath
        | AudioSequenceDeleted of Models.Contact
        
    type Model =
        { Contact: Models.Contact
          CallState: CallService.State
          IsRecordingAudioSequence: bool }
        
    // functions
    let private loadContactCmd id =
        async { let! contact = Repository.getContact id
                return ContactLoaded contact }
        |> Cmd.ofAsyncMsg
    
    let private startCallCmd contact callState =
        async { let! status =  Permissions.RequestAsync<Permissions.Microphone>() |> Async.AwaitTask
                if status = PermissionStatus.Granted then
                    let! state = CallService.startCall contact callState
                    return CallStarted state
                else
                    return PermissionDenied }
        |> Cmd.ofAsyncMsg
        
    let private stopCallCmd contact callState =
        async { let! state = CallService.stopCall contact callState
                return CallStopped state }
        |> Cmd.ofAsyncMsg
        
    let private startRecordingAudioSequenceCmd =
        async { do! AudioUtils.startRecording ()
                return AudioSequenceRecordingStarted }
        |> Cmd.ofAsyncMsg
        
    let private stopRecordingAudioSequenceCmd contact =
        async { let! audioInfo = AudioUtils.stopRecording ()
                let updatedContact = { contact with AudioSequencePaths = audioInfo.FilePath::contact.AudioSequencePaths |> List.rev }
                do! Repository.updateContact updatedContact
                return AudioSequenceRecordingStopped updatedContact }
        |> Cmd.ofAsyncMsg
        
    let private playAudioSequence filePath =
        async { let! _ = AudioUtils.playAudioFile filePath
                return NoOp }
        |> Cmd.ofAsyncMsg
        
    let private deleteAudioSequence contact (filePath:FilePath) =
        async { let updatedContact = { contact with AudioSequencePaths = contact.AudioSequencePaths |> List.filter (fun x -> x.Value <> filePath.Value) }
                do! Repository.updateContact updatedContact
                File.Delete(filePath.Value)
                return AudioSequenceDeleted updatedContact }
        |> Cmd.ofAsyncMsg
    
    // lifecycle
    let init contact = { Contact = contact; CallState = CallService.initialState; IsRecordingAudioSequence = false }, loadContactCmd contact.Id
    
    let update msg model =
        match msg with
        | NoOp ->
            model, Cmd.none
            
        | ContactLoaded contact ->
            { model with Model.Contact = contact }, Cmd.none
            
        | PermissionDenied ->
            model, Cmd.none
            
        | StartCallTapped ->
            model, startCallCmd model.Contact model.CallState
            
        | StopCallTapped ->
            model, stopCallCmd model.Contact model.CallState
            
        | CallStarted callState ->
            { model with CallState = callState }, Cmd.none
            
        | CallStopped callState ->
            { model with CallState = callState }, Cmd.none
            
        | StartAudioSequenceRecordingTapped ->
            model, startRecordingAudioSequenceCmd
            
        | StopAudioSequenceRecordingTapped ->
            model, stopRecordingAudioSequenceCmd model.Contact
            
        | AudioSequenceRecordingStarted ->
            { model with IsRecordingAudioSequence = true }, Cmd.none
            
        | AudioSequenceRecordingStopped contact ->
            { model with Contact = contact; IsRecordingAudioSequence = false }, Cmd.none
            
        | PlayAudioSequenceTapped filePath ->
            model, playAudioSequence filePath
            
        | DeleteAudioSequenceTapped filePath ->
            model, deleteAudioSequence model.Contact filePath
            
        | AudioSequenceDeleted contact ->
            { model with Contact = contact }, Cmd.none
        
    let view (model: Model) dispatch =
        let getAudioSequenceCell (filePath: FilePath) =
            let playItem =
                View.SwipeItem(icon = Image.Value.ImagePath "ic_play_arrow.png", command = fun _ -> dispatch (PlayAudioSequenceTapped filePath))
                
            let deleteItem =
                View.SwipeItem(icon = Image.Value.ImagePath "ic_delete.png", command = fun _ -> dispatch (DeleteAudioSequenceTapped filePath))
                
            let swipeItems =
                View.SwipeItems(items = [ deleteItem; playItem ])
                
            let imageName =
                Path.GetFileNameWithoutExtension(filePath.Value)
                |> List.ofSeq
                |> Seq.map int
                |> Seq.sum
                |> fun x -> x % 10
                |> sprintf "soundwave_0%i.jpg"
                
            View.SwipeView(
                isEnabled = true,
                rightItems = swipeItems,
                backgroundColor = Colors.primaryDark.WithLuminosity(0.17),
                content = View.StackLayout(
                    children = [
                        View.Image(
                            height = 40.,
                            aspect = Aspect.Fill,
                            inputTransparent = true,
                            horizontalOptions = LayoutOptions.Fill,
                            source = Image.fromPath imageName)
                    ]))
            
        let items =
            model.Contact.AudioSequencePaths
            |> Seq.map getAudioSequenceCell
            |> Seq.toList
            
        let itemsLayout = LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        itemsLayout.ItemSpacing <- 4.
                    
        View.ContentPage(
            backgroundColor = Colors.primaryDark,
            content = View.Grid(
                rowdefs = [ Dimension.Auto; Dimension.Auto; Dimension.Auto; Dimension.Star; Dimension.Auto; Dimension.Absolute 200.; ],
                coldefs = [ Dimension.Star; Dimension.Auto ],
                backgroundColor = Colors.primary,
                children = [
                    yield View.Label(
                        margin = Thickness (64., 0., 0., 0.),
                        verticalOptions = LayoutOptions.Center,
                        fontSize = InputTypes.FontSize.fromValue 12.,
                        text = Translations.ContactDetail.customAnswersTitle (),
                        textColor = Colors.primaryText)
                        .Row(4)
                        .Column(0)
                    if model.IsRecordingAudioSequence then
                        yield View.Button(
                            width = 56.,
                            margin = Thickness (0., 0., 64., 0.),
                            horizontalOptions = LayoutOptions.End,
                            fontSize = InputTypes.FontSize.fromValue 12.,
                            text = Translations.ContactDetail.stopSequenceRecording (),
                            textColor = Colors.accent,
                            backgroundColor = Color.Transparent,
                            isVisible = model.IsRecordingAudioSequence,
                            command = fun _ -> dispatch StopAudioSequenceRecordingTapped)
                            .Row(4)
                            .Column(1)
                    else
                        yield View.Button(
                            width = 44.,
                            margin = Thickness (0., 0., 64., 0.),
                            horizontalOptions = LayoutOptions.End,
                            text = "+",
                            textColor = Colors.primaryText,
                            backgroundColor = Color.Transparent,
                            command = fun _ -> dispatch StartAudioSequenceRecordingTapped)
                            .Row(4)
                            .Column(1)
                    yield View.CollectionView(
                        itemsLayout = itemsLayout,
                        margin = Thickness (64., 0., 64., 16.),
                        selectionMode = SelectionMode.None,
                        opacity = (if model.IsRecordingAudioSequence then 0.1 else 1.),
                        items = items)
                        .Row(5)
                        .ColumnSpan(2)
                    if model.Contact.AudioSequencePaths.IsEmpty || model.IsRecordingAudioSequence then
                        yield View.Frame( // Frame as a stupid workaround so there won't be a drop shadow on android
                            margin = Thickness (64., 0., 64., 16.),
                            padding = Thickness 0.,
                            hasShadow = false,
                            cornerRadius = 0.,
                            backgroundColor = Color.Transparent,
                            content =
                                if model.IsRecordingAudioSequence then
                                    View.Label(
                                        horizontalTextAlignment = TextAlignment.Center,
                                        verticalTextAlignment = TextAlignment.Center,
                                        padding = Thickness 16.,
                                        text = Translations.ContactDetail.sequenceRecordDescription (),
                                        fontSize = InputTypes.FontSize.fromValue 16.,
                                        textColor = Colors.primaryText,
                                        backgroundColor = (if items.IsEmpty then Colors.primary.WithLuminosity(0.25) else Color.Transparent))
                                else
                                    View.Label(
                                        horizontalTextAlignment = TextAlignment.Center,
                                        verticalTextAlignment = TextAlignment.Center,
                                        padding = Thickness 16.,
                                        text = Translations.ContactDetail.customAnswersDescription (),
                                        fontSize = InputTypes.FontSize.fromValue 16.,
                                        textColor = Colors.primaryText,
                                        backgroundColor = Colors.primary.WithLuminosity(0.25)))
                            .Row(5)
                            .ColumnSpan(2)
                    if model.CallState.IsRunning then
                        yield View.Rectangle(
                            opacity = 0.8,
                            backgroundColor = Color.Black)
                            .RowSpan(6)
                            .ColumnSpan(2)
                    yield (Controls.avatar model.Contact 160.)
                        .Row(0)
                        .ColumnSpan(2)
                        .Margin(Thickness (0., 32.))
                    yield View.Label(
                        margin = Thickness (64., 0.),
                        verticalOptions = LayoutOptions.Center,
                        horizontalOptions = LayoutOptions.Center,
                        fontSize = InputTypes.FontSize.fromValue 22.,
                        text = model.Contact.DisplayName,
                        textColor = Colors.primaryText)
                        .Row(1)
                        .ColumnSpan(2)
                    if not model.CallState.IsRunning then
                        yield (Controls.iconButton "ic_call.png" (Translations.ContactDetail.startCall ()) Colors.accent (fun _ -> dispatch StartCallTapped))
                            .Row(2)
                            .ColumnSpan(2)
                            .Margin(Thickness (64., 32.))
                    if model.CallState.IsRunning then
                        yield (Controls.iconButton "ic_call_end.png" (Translations.ContactDetail.stopCall ()) Color.DarkRed (fun _ -> dispatch StopCallTapped))
                            .Row(2)
                            .ColumnSpan(2)
                            .Margin(Thickness (64., 32.))
                    ]
                )
            )