namespace DrunknDial

open Fabulous.XamarinForms
open Models
open Fabulous
open Xamarin.Essentials
open Xamarin.Forms

module ContactsPage =
    // declarations
    type Msg =
        | AskForPermission
        | PermissionGranted
        | LoadContacts
        | ContactsLoaded of Models.Contact list
        | ContactSelected of Models.Contact
        | FilterChanged of string
        | ShowPermissionDialog of PermissionStatus
        
    type ExternalMsg =
        | NoOp
        | NavigateToContactDetail of Models.Contact
        | UpdateContacts
        
    type Model = 
      { AllContacts: Models.Contact list
        FilteredContacts: Models.Contact list
        ShowsProgress: bool 
        PermissionStatus: PermissionStatus }
    
    // functions
    let private askForPermissionCmd =
        async {
            let! status =  Permissions.RequestAsync<Permissions.ContactsRead>() |> Async.AwaitTask
            return if status = PermissionStatus.Granted then PermissionGranted else ShowPermissionDialog status }
        |> Cmd.ofAsyncMsg
    
    let private loadContactsCmd =
        async {
            let! contacts = Repository.getAllContacts ()
            return ContactsLoaded contacts }
        |> Cmd.ofAsyncMsg

    // lifecycle
    let initModel = { AllContacts = []; FilteredContacts = []; ShowsProgress = false; PermissionStatus = PermissionStatus.Unknown }
    let init () =
        initModel, Cmd.batch [Cmd.ofMsg AskForPermission; loadContactsCmd]

    let update msg model =
        match msg with
        | AskForPermission ->
            model, askForPermissionCmd, ExternalMsg.NoOp
            
        | PermissionGranted ->
            { model with PermissionStatus = PermissionStatus.Granted; ShowsProgress = model.AllContacts.IsEmpty }, Cmd.none, ExternalMsg.UpdateContacts
            
        | LoadContacts ->
            { model with ShowsProgress = true }, loadContactsCmd, ExternalMsg.NoOp
            
        | ContactsLoaded contacts ->
            { model with AllContacts = contacts; FilteredContacts = contacts; ShowsProgress = false }, Cmd.none, ExternalMsg.NoOp
            
        | ContactSelected contact ->
            model, Cmd.none, ExternalMsg.NavigateToContactDetail contact
            
        | FilterChanged filter ->
            let filtered =
                match filter with
                | null -> model.AllContacts
                | _ -> model.AllContacts |> List.filter (fun x -> x.DisplayName.ToLower().StartsWith(filter.ToLower()))
            { model with FilteredContacts = filtered }, Cmd.none, ExternalMsg.NoOp
        | ShowPermissionDialog status ->
            { model with PermissionStatus = status }, Cmd.none, ExternalMsg.NoOp

    let view (model: Model) dispatch =
        let getContactCell contact dispatch =
            View.Grid(
                padding = Thickness (32.0, 8.0),
                backgroundColor = Colors.primary,
                coldefs = [ Dimension.Auto; Dimension.Star ],
                columnSpacing = 16.0,
                children = [
                    (Controls.avatar contact 50.)
                        .Column(0)
                    View.Label(
                        verticalOptions = LayoutOptions.Center,
                        text = contact.DisplayName,
                        textColor = Colors.primaryText,
                        lineBreakMode = LineBreakMode.TailTruncation)
                        .Column(1)
                    View.Button(
                        backgroundColor = Color.Transparent,
                        command = fun _ -> dispatch (ContactSelected contact))
                        .ColumnSpan(2)
                ])
            
        let items =
            model.FilteredContacts
            |> Seq.map (fun c -> getContactCell c dispatch)
            |> Seq.toList
            
        let itemsLayout = LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        itemsLayout.ItemSpacing <- 4.
                    
        View.ContentPage(
            title = Translations.Contacts.tab (),
            content = View.Grid(
                backgroundColor = Colors.primaryDark,
                rowdefs = [ Dimension.Auto; Dimension.Star; ],
                children = [
                    View.ActivityIndicator(
                        isVisible = model.ShowsProgress,
                        isRunning = true,
                        color = Colors.accent,
                        verticalOptions = LayoutOptions.Center)
                        .RowSpan(2)
                    (Controls.searchBar (Translations.Contacts.searchBarHint ()) (fun x -> dispatch (FilterChanged x.NewTextValue)))
                        .Row(0)
                    View.CollectionView(
                        itemsLayout = itemsLayout,
                        selectionMode = SelectionMode.Single,
                        items = items)
                        .Row(1)
                    if model.PermissionStatus = PermissionStatus.Denied then
                        View.StackLayout(
                            horizontalOptions = LayoutOptions.Center,
                            verticalOptions = LayoutOptions.Center,
                            spacing = 12.,
                            padding = Thickness 32.,
                            children = [
                                View.Label(
                                    horizontalOptions = LayoutOptions.Center,
                                    horizontalTextAlignment = TextAlignment.Center,
                                    text = Translations.Contacts.askPermission (),
                                    fontSize = InputTypes.FontSize.fromValue 16.,
                                    textColor = Colors.primaryText)
                                View.Button(
                                    horizontalOptions = LayoutOptions.Center,
                                    text = Translations.Contacts.grantPermission (),
                                    fontSize = InputTypes.FontSize.fromValue 16.,
                                    textColor = Colors.accent,
                                    backgroundColor = Color.Transparent,
                                    command = (fun _ -> dispatch AskForPermission))
                            ])
                            .RowSpan(2)
                ]
            )
        )
