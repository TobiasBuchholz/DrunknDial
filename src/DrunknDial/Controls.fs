namespace DrunknDial

open DrunknDial.Models
open Fabulous.XamarinForms
open Xamarin.Essentials
open Xamarin.Forms

module Controls =
    let avatar contact size =
        View.Frame(
            height = size,
            width = size,
            cornerRadius = size/2.,
            padding = Thickness 0.,
            verticalOptions = LayoutOptions.Center,
            horizontalOptions = LayoutOptions.Center,
            hasShadow = false,
            backgroundColor = Color.FromHex(contact.AvatarColorHex),
            content = 
                View.Label(
                    verticalOptions = LayoutOptions.Center,
                    horizontalOptions = LayoutOptions.Center,
                    fontSize = InputTypes.FontSize.fromValue (size * 0.4),
                    text = StringUtils.getInitials contact.DisplayName,
                    textColor = Colors.primaryText)
        )

    let searchBar placeholder textChanged =
        View.Frame(
            verticalOptions = LayoutOptions.Fill,
            margin = Thickness (12., 6.),
            padding = Thickness 0.,
            cornerRadius = 10.,
            hasShadow = false,
            backgroundColor = (if DeviceInfo.Platform = DevicePlatform.Android then Colors.primary else Color.Transparent),
            content =
                View.SearchBar(
                    height = 50.,
                    placeholder = placeholder,
                    textColor = Colors.primaryText,
                    placeholderColor = Colors.entryPlaceholder,
                    backgroundColor = Color.Transparent,
                    cancelButtonColor = Colors.primaryText,
                    textChanged = textChanged)
        )

    let iconButton icon title backgroundColor command =
        View.Frame(
            cornerRadius = 20.,
            hasShadow = false,
            backgroundColor = backgroundColor,
            padding = Thickness 8.,
            verticalOptions = LayoutOptions.End,
            content =
                View.Grid(
                    coldefs = [ Dimension.Auto; Dimension.Auto; Dimension.Star; ],
                    columnSpacing = 12.,
                    children = [
                        View.Image(
                            width = 32.0,
                            height = 32.0,
                            margin = Thickness (8., 0., 0., 0.),
                            source = Image.fromPath icon)
                            .Column(0)
                        View.Rectangle(
                            width = 1.5,
                            verticalOptions = LayoutOptions.Fill,
                            backgroundColor = Colors.primaryText)
                            .Column(1)
                        View.Label(
                            verticalOptions = LayoutOptions.Center,
                            horizontalOptions = LayoutOptions.Center,
                            text = title,
                            textColor = Colors.primaryText,
                            fontAttributes = FontAttributes.Bold,
                            fontSize = InputTypes.FontSize.fromValue 18.)
                            .Column(2)
                        View.Button(
                            backgroundColor = Color.Transparent,
                            command = command)
                            .ColumnSpan(3)
                    ])
                )
