namespace DrunknDial

open DrunknDial
open Models
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

module App =
    type Msg =
        | NoOp
        | MainPageMsg of MainPage.Msg
        | ContactsPageMsg of ContactsPage.Msg
        | ContactDetailPageMsg of ContactDetailPage.Msg
        | RecentsPageMsg of RecentsPage.Msg
        | RecentDetailPageMsg of RecentDetailPage.Msg
        | ContactsUpdated
        | NavigateToContactDetail of Contact
        | NavigateToRecentDetail of Recent
        | NavigationPopped
        
    type Model = 
        { MainPageModel: MainPage.Model
          ContactDetailPageModel: ContactDetailPage.Model option
          RecentDetailPageModel: RecentDetailPage.Model option }
        
    type Pages =
        { MainPage: ViewElement
          ContactDetailPage: ViewElement option
          RecentDetailPage: ViewElement option }
        
    let init () =
        let mainModel, mainCmd = MainPage.init ()
        let initialModel =
            { MainPageModel = mainModel
              ContactDetailPageModel = None
              RecentDetailPageModel = None }
            
        initialModel, Cmd.map MainPageMsg mainCmd
        
    let handleMainExternalMsg msg =
        let updateContactsCmd =
            async {
                do! Repository.updateContacts ()
                return ContactsUpdated }
            |> Cmd.ofAsyncMsg
            
        match msg with
        | MainPage.ExternalMsg.NoOp -> Cmd.none
        | MainPage.ExternalMsg.UpdateContacts -> updateContactsCmd
        | MainPage.ExternalMsg.NavigateToContactDetail contact -> Cmd.ofMsg (NavigateToContactDetail contact)
        | MainPage.ExternalMsg.NavigateToRecentDetail recent -> Cmd.ofMsg (NavigateToRecentDetail recent)
        
    let handleContactsExternalMsg msg =
        match msg with
        | ContactsPage.ExternalMsg.NoOp -> Cmd.none
        | ContactsPage.ExternalMsg.NavigateToContactDetail _ -> Cmd.none
        | ContactsPage.ExternalMsg.UpdateContacts -> Cmd.none
        
    let handleRecentsExternalMsg msg =
        match msg with
        | RecentsPage.ExternalMsg.NoOp -> Cmd.none
        | RecentsPage.ExternalMsg.NavigateToRecentDetail _ -> Cmd.none
        
    let navigationMapper model =
        match model.ContactDetailPageModel, model.RecentDetailPageModel with
        | None, None -> model
        | Some _, _ -> { model with ContactDetailPageModel = None }
        | _, Some _ -> { model with RecentDetailPageModel = None }
        
    let handleNavigationPoppedCmd model =
        async {
            // TODO: solve the cancellation of stuff in a better way
            let! _ = match model.ContactDetailPageModel with
                     | Some detailModel -> CallService.stopCall detailModel.Contact detailModel.CallState
                     | None -> async { return { IsRunning = false; CancelFunction = None } }
            do! AudioUtils.stopCurrentAudio ()
            return NoOp
        }
        |> Cmd.ofAsyncMsg
    
    let update msg model =
        match msg with
        | NoOp ->
            model, Cmd.none
            
        | MainPageMsg msg ->
            let m, mCmd, mMsg = MainPage.update msg model.MainPageModel
            let cmd = handleMainExternalMsg mMsg
            let batchCmd = Cmd.batch [ (Cmd.map MainPageMsg mCmd); cmd ]
            { model with MainPageModel = m }, batchCmd
            
        | ContactsPageMsg msg ->
            let m, mCmd, mMsg = ContactsPage.update msg model.MainPageModel.ContactsPageModel
            let cmd = handleContactsExternalMsg mMsg
            let batchCmd = Cmd.batch [ (Cmd.map ContactsPageMsg mCmd); cmd ]
            { model with MainPageModel = { model.MainPageModel with ContactsPageModel = m } }, batchCmd
            
        | ContactDetailPageMsg msg ->
            match model.ContactDetailPageModel with
            | None ->
                model, Cmd.none
            | Some cm -> 
                let m, mCmd = ContactDetailPage.update msg cm
                { model with ContactDetailPageModel = Some m }, (Cmd.map ContactDetailPageMsg mCmd)
            
        | RecentsPageMsg msg ->
            let m, mCmd, mMsg = RecentsPage.update msg model.MainPageModel.RecentsPageModel
            let cmd = handleRecentsExternalMsg mMsg
            let batchCmd = Cmd.batch [ (Cmd.map RecentsPageMsg mCmd); cmd ]
            { model with MainPageModel = { model.MainPageModel with RecentsPageModel = m } }, batchCmd
            
        | RecentDetailPageMsg msg ->
            match model.RecentDetailPageModel with
            | None ->
                model, Cmd.none
            | Some rm -> 
                let m, mCmd = RecentDetailPage.update msg rm
                { model with RecentDetailPageModel = Some m }, (Cmd.map RecentDetailPageMsg mCmd)
        
        | ContactsUpdated ->
            model, Cmd.ofMsg (ContactsPageMsg ContactsPage.Msg.LoadContacts)
            
        | NavigateToContactDetail contact ->
            let m, cmd = ContactDetailPage.init contact
            { model with ContactDetailPageModel = Some m }, (Cmd.map ContactDetailPageMsg cmd)
            
        | NavigateToRecentDetail recent ->
            let m, cmd = RecentDetailPage.init recent
            { model with RecentDetailPageModel = Some m }, (Cmd.map RecentDetailPageMsg cmd)
            
        | NavigationPopped ->
            navigationMapper model, handleNavigationPoppedCmd model
            
    let getPages allPages =
        let mainPage = allPages.MainPage
        let contactDetailPage = allPages.ContactDetailPage
        let recentDetailPage = allPages.RecentDetailPage
        
        match contactDetailPage, recentDetailPage with
        | None, None -> [ mainPage ]
        | Some contactDetail, _ -> [ mainPage; contactDetail ]
        | _, Some recentDetailPage -> [ mainPage; recentDetailPage ]
        
    let view (model: Model) dispatch =
        let mainPage = MainPage.view model.MainPageModel (MainPageMsg >> dispatch)
        let contactDetailPage =
            model.ContactDetailPageModel
            |> Option.map (fun m -> ContactDetailPage.view m (ContactDetailPageMsg >> dispatch))
            
        let recentDetailPage =
            model.RecentDetailPageModel
            |> Option.map (fun m -> RecentDetailPage.view m (RecentDetailPageMsg >> dispatch))
            
        let allPages =
            { MainPage = mainPage
              ContactDetailPage = contactDetailPage
              RecentDetailPage = recentDetailPage }
        
        View.NavigationPage(
            barTextColor = Color.White,
            barBackgroundColor = Colors.primaryDark,
            popped = (fun _ -> dispatch NavigationPopped),
            pages = getPages allPages)

    // Note, this declaration is needed if you enable LiveUpdate
    
    let subAudioStateChangeMsgs dispatch =
        RecentDetailPage
            .getAudioStateChangeMsgs
            .Subscribe (fun x -> dispatch (RecentDetailPageMsg x))
        |> ignore
        
    let program =
        XamarinFormsProgram.mkProgram init update view
        |> Program.withSubscription (fun _ -> Cmd.ofSub subAudioStateChangeMsgs)
#if DEBUG
        |> Program.withConsoleTrace
#endif

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    do runner.EnableLiveUpdate()
#endif    


