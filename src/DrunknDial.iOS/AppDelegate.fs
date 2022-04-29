// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace DrunknDial.iOS

open System
open Akavache
open MediaManager
open UIKit
open Foundation
open Xamarin.Forms
open Xamarin.Forms.Platform.iOS

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()

    override this.FinishedLaunching (app, options) =
        Forms.Init()
        Registrations.Start("DrunknDial")
        CrossMediaManager.Current.Init();
        let appcore = new DrunknDial.App()
        this.LoadApplication (appcore)
        base.FinishedLaunching(app, options)

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main(args, null, "AppDelegate")
        0

