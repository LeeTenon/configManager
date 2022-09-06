package main

import (
    "context"
    configManager "myproject/configManage"
    "myproject/pkg/logx"
)

// App struct
type App struct {
    ctx context.Context
}

// NewApp creates a new App application struct
func NewApp() *App {
    return &App{}
}

func (a *App) LoadConfigTemplate() interface{} {
    data, _ := configManager.Manager.LoadTemplate(a.ctx)
    return data
}

func (a *App) LoadConfigCache() interface{} {
    data, _ := configManager.Manager.LoadCache(a.ctx)
    return data
}

func (a *App) SaveConfig(in string) string {
    if err := configManager.Manager.Save(a.ctx, in); err != nil {
        return err.Error()
    }
    return ""
}

func (a *App) GenConfig(mode string) string {
    if err := configManager.Manager.Generate(mode); err != nil {
        return err.Error()
    }
    return ""
}

func (a *App) SyncCsv() string {
    if err := configManager.Manager.SyncCsv(); err != nil {
        return err.Error()
    }
    return ""
}

func (a *App) GetLog() string {
    return logx.GloLog.GetLogs()
}
