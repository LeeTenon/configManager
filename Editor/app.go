package main

import (
    "context"
    "myproject/common/logx"
    configManager "myproject/configManage"
)

type App struct {
    ctx       context.Context
    configMgr *configManager.ConfigManager
}

func NewApp() *App {
    return &App{
        configMgr: configManager.NewConfigManager(),
    }
}

func (a *App) startup(ctx context.Context) {
    logx.InitLogger(ctx)
}

func (a *App) LoadConfigTemplate() *Response {
    data, err := a.configMgr.LoadTemplate(a.ctx)
    if err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{Data: data}
}

func (a *App) LoadConfigCache() *Response {
    data, err := a.configMgr.LoadCache(a.ctx)
    if err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{Data: data}
}

func (a *App) SaveConfig(in string) *Response {
    if err := a.configMgr.Save(a.ctx, in); err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{}
}

func (a *App) GenConfig(in map[string]string) *Response {
    if err := a.configMgr.Generate(in); err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{}
}

func (a *App) SyncCsv() *Response {
    if err := a.configMgr.SyncCsv(a.ctx); err != nil {
        return &Response{Error: err.Error()}
    }
    return &Response{}
}

type Response struct {
    Error string
    Data  interface{}
}
