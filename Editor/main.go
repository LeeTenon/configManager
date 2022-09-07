package main

import (
    "embed"
    "flag"
    "github.com/wailsapp/wails/v2"
    "github.com/wailsapp/wails/v2/pkg/logger"
    "github.com/wailsapp/wails/v2/pkg/options"
    "github.com/wailsapp/wails/v2/pkg/options/windows"
    "io"
    "log"
)

//go:embed frontend/dist
var assets embed.FS

func main() {
    flag.CommandLine.SetOutput(io.Discard)
    // 功能实例
    app := NewApp()

    err := wails.Run(&options.App{
        Title:  "",
        Width:  1024,
        Height: 768,
        // MinWidth:          720,
        // MinHeight:         570,
        // MaxWidth:          1280,
        // MaxHeight:         740,
        DisableResize:     false,
        Fullscreen:        false,
        Frameless:         true,
        StartHidden:       false,
        HideWindowOnClose: false,
        BackgroundColour:  &options.RGBA{R: 255, G: 255, B: 255, A: 255},
        Assets:            assets,
        LogLevel:          logger.DEBUG,
        OnStartup:         app.startup,
        //OnDomReady:        app.domReady,
        //OnShutdown:        app.shutdown,
        Bind: []interface{}{
            app,
        },
        // Windows platform specific options
        Windows: &windows.Options{
            WebviewIsTransparent: false,
            WindowIsTranslucent:  false,
            DisableWindowIcon:    true,
        },
        Experimental: &options.Experimental{
            UseCSSDrag: true,
        },
    })
    if err != nil {
        log.Fatal(err)
    }
}
