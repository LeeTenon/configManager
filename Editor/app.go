package main

import (
    "context"
    "myproject/common/logx"
    configManager "myproject/configManage"
)

// App struct
type App struct {
    ctx context.Context
}

// NewApp creates a new App application struct
func NewApp() *App {
    return &App{}
}

// startup is called at application startup
func (a *App) startup(ctx context.Context) {
    logx.InitLogger(ctx)
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

func (a *App) GenConfig(in map[string]string) string {
    if err := configManager.Manager.Generate(in); err != nil {
        return err.Error()
    }
    return ""
}

func (a *App) SyncCsv() string {
    if err := configManager.Manager.SyncCsv(a.ctx); err != nil {
        return err.Error()
    }
    return ""
}
