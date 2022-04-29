namespace DrunknDial

open System
open DrunknDial.Models
open Fabulous
open Fabulous.XamarinForms
open Xamarin.Forms

module RecentsPage =
    // declarations
    type Msg =
        | LoadRecents
        | RecentsLoaded of Recent list
        | RecentSelected of Recent
        | FilterChanged of string
        
    type ExternalMsg =
        | NoOp
        | NavigateToRecentDetail of Recent
        
    type Model =
        { AllRecents: Recent list
          FilteredRecents: Recent list
          ShowsProgress: bool }
        
    // functions
    let private loadRecentsCmd =
        async {
            let! recents = Repository.getAllRecents ()
            return RecentsLoaded recents }
        |> Cmd.ofAsyncMsg
        
    // lifecycle
    let initModel = { AllRecents = []; FilteredRecents = []; ShowsProgress = false }
    let init () =
        initModel, Cmd.none
    
    let update msg model =
        match msg with
        | LoadRecents ->
            { model with ShowsProgress = true }, loadRecentsCmd, ExternalMsg.NoOp
            
        | RecentsLoaded recents ->
            { model with AllRecents = recents; FilteredRecents = recents; ShowsProgress = false }, Cmd.none, ExternalMsg.NoOp
            
        | RecentSelected recent ->
            model, Cmd.none, ExternalMsg.NavigateToRecentDetail recent
            
        | FilterChanged filter ->
            let filtered =
                match filter with
                | null -> model.AllRecents
                | _ -> model.AllRecents |> List.filter (fun x -> x.Contact.DisplayName.ToLower().StartsWith(filter.ToLower()))
            { model with FilteredRecents = filtered }, Cmd.none, ExternalMsg.NoOp
        
    let view (model: Model) dispatch =
        let getRecentCell recent dispatch =
            View.Grid(
                padding = Thickness (32., 0.),
                backgroundColor = Colors.primary,
                rowdefs = [ Dimension.Auto; Dimension.Auto ],
                coldefs = [ Dimension.Auto; Dimension.Star ],
                rowSpacing = 4.,
                columnSpacing = 16.,
                children = [
                    (Controls.avatar recent.Contact 50.)
                        .Column(0)
                        .RowSpan(2)
                    View.Label(
                        verticalOptions = LayoutOptions.End,
                        margin = Thickness (0., 16., 0., 0.),
                        fontSize = InputTypes.FontSize.fromValue 16.,
                        text = recent.Contact.DisplayName,
                        textColor = Colors.primaryText,
                        lineBreakMode = LineBreakMode.TailTruncation)
                        .Column(1)
                        .Row(0)
                    View.Label(
                        verticalOptions = LayoutOptions.Start,
                        margin = Thickness (0., 0., 0., 16.),
                        fontSize = InputTypes.FontSize.fromValue 14.,
                        text = Translations.Common.dateFormatted recent.CreatedAt,
                        textColor = Colors.primarySubTitle,
                        lineBreakMode = LineBreakMode.TailTruncation)
                        .Column(1)
                        .Row(1)
                    View.Button(
                        backgroundColor = Color.Transparent,
                        command = fun _ -> dispatch (RecentSelected recent))
                        .ColumnSpan(2)
                        .RowSpan(2)
                ])
            
        let items =
            model.FilteredRecents
            |> Seq.map (fun c -> getRecentCell c dispatch)
            |> Seq.toList
            
        let itemsLayout = LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        itemsLayout.ItemSpacing <- 4.0
                    
        View.ContentPage(
            title = Translations.Recents.tab (),
            content = View.Grid(
                backgroundColor = Colors.primaryDark,
                rowdefs = [ Dimension.Auto; Dimension.Star; ],
                children = [
                    View.ActivityIndicator(
                        verticalOptions = LayoutOptions.Center,
                        color = Colors.accent,
                        isVisible = model.ShowsProgress,
                        isRunning = true)
                        .RowSpan(2)
                    (Controls.searchBar (Translations.Recents.searchBarHint ()) (fun x -> dispatch (FilterChanged x.NewTextValue)))
                        .Row(0)
                    View.CollectionView(
                        itemsLayout = itemsLayout,
                        selectionMode = SelectionMode.Single,
                        items = items)
                        .Row(1)
                ])
            )