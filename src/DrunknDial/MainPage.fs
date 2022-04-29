namespace DrunknDial

open Models
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms.PlatformConfiguration
open Xamarin.Forms.PlatformConfiguration.AndroidSpecific

// TODO:
// - play/delete audio fetzen
module MainPage =
    // declarations
    type Msg =
        | TabContactsMsg of ContactsPage.Msg
        | TabRecentsMsg of RecentsPage.Msg
        
    type ExternalMsg =
        | NoOp
        | UpdateContacts
        | NavigateToContactDetail of Contact
        | NavigateToRecentDetail of Recent
        
    type Model =
        { ContactsPageModel: ContactsPage.Model
          RecentsPageModel: RecentsPage.Model }
    
    // lifecycle
    let init () =
        let modelContacts, msgContacts = ContactsPage.init ()
        let modelRecents, msgRecents = RecentsPage.init ()
        let initModel =
            { ContactsPageModel = modelContacts
              RecentsPageModel = modelRecents }
        let batchCmd = Cmd.batch [
            Cmd.map TabContactsMsg msgContacts
            Cmd.map TabRecentsMsg msgRecents
        ]
        initModel, batchCmd
    
    let update msg model =
        match msg with
        | TabContactsMsg msg ->
            let m, mCmd, mMsg = ContactsPage.update msg model.ContactsPageModel
            let externalMsg =
               match mMsg with
               | ContactsPage.ExternalMsg.NoOp -> ExternalMsg.NoOp
               | ContactsPage.ExternalMsg.NavigateToContactDetail contact -> ExternalMsg.NavigateToContactDetail contact
               | ContactsPage.ExternalMsg.UpdateContacts -> ExternalMsg.UpdateContacts
                
            { model with ContactsPageModel = m }, (Cmd.map TabContactsMsg mCmd), externalMsg
            
        | TabRecentsMsg msg ->
            let m, mCmd, mMsg = RecentsPage.update msg model.RecentsPageModel
            let externalMsg =
                match mMsg with
                | RecentsPage.ExternalMsg.NoOp -> ExternalMsg.NoOp
                | RecentsPage.ExternalMsg.NavigateToRecentDetail recent -> ExternalMsg.NavigateToRecentDetail recent
                
            { model with RecentsPageModel = m }, (Cmd.map TabRecentsMsg mCmd), externalMsg
    
    let view (model: Model) dispatch =
        let tabContacts = 
            (ContactsPage.view model.ContactsPageModel (TabContactsMsg >> dispatch))
                .IconImageSource(Image.fromPath "ic_contacts.png")
                
        let tabRecents =
            (RecentsPage.view model.RecentsPageModel (TabRecentsMsg >> dispatch))
                .IconImageSource(Image.fromPath "ic_recents.png")
        
        View.TabbedPage(
            created = (fun target -> target.On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom) |> ignore),
            selectedTabColor = Colors.accent,
            unselectedTabColor = Colors.primaryText,
            barBackgroundColor = Colors.primaryDark,
            currentPageChanged = (fun x -> if x.IsSome && x.Value = 1 then dispatch (TabRecentsMsg RecentsPage.Msg.LoadRecents)),
            children = [ tabContacts; tabRecents ])